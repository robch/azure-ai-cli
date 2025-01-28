using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommonForCli.Details.Commands;
using Microsoft.CommonForCli.Details.NamedValues;

namespace Microsoft.AI.CLI.Commands
{
    public class FastTranscriptionCommand : Command
    {
        private readonly string _audioFile;
        private readonly IEnumerable<NamedValueToken> _tokens;

        public FastTranscriptionCommand(string audioFile, IEnumerable<NamedValueToken> tokens)
        {
            _audioFile = audioFile;
            _tokens = tokens;
        }

        public override async Task<int> RunAsync()
        {
            try
            {
                if (!File.Exists(_audioFile))
                {
                    throw new FileNotFoundException($"Audio file not found: {_audioFile}");
                }

                var key = GetRequiredTokenValue("key");
                var region = GetRequiredTokenValue("region");
                var endpoint = $"https://{region}.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions";

                var requestBody = new
                {
                    contentUrls = new[] { _audioFile },
                    properties = new
                    {
                        diarizationEnabled = GetTokenValue("diarization") == "true",
                        maxSpeakerCount = GetTokenValue("max-speakers") != null ? int.Parse(GetTokenValue("max-speakers")) : (int?)null,
                        profanityFilterMode = GetTokenValue("profanity") ?? "raw",
                        channels = GetTokenValue("channels") != null ? int.Parse(GetTokenValue("channels")) : 1,
                        language = GetTokenValue("locale"),
                        multilingualConfig = GetTokenValue("locales") != null ? new
                        {
                            languages = GetTokenValue("locales").Split(',')
                        } : null
                    }
                };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to start transcription: {error}");
                }

                var result = await response.Content.ReadAsStringAsync();
                var outputFormat = GetTokenValue("output") ?? "text";

                if (outputFormat == "json")
                {
                    Console.WriteLine(result);
                }
                else
                {
                    var transcription = JsonSerializer.Deserialize<TranscriptionResponse>(result);
                    Console.WriteLine(transcription.Text);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private string GetRequiredTokenValue(string name)
        {
            var token = _tokens.FirstOrDefault(t => t.Name == name);
            if (token == null || !token.HasValue)
            {
                throw new ArgumentException($"Required parameter --{name} is missing");
            }
            return token.Value;
        }

        private string GetTokenValue(string name)
        {
            var token = _tokens.FirstOrDefault(t => t.Name == name);
            return token?.Value;
        }

        private class TranscriptionResponse
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Text { get; set; }
        }
    }
}