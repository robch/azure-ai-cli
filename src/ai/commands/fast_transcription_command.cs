using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.AI.Details.Common.CLI
{
    public class FastTranscriptionCommand : Command
    {
        internal FastTranscriptionCommand(ICommandValues values) : base(values)
        {
            _parser = new FastTranscriptionCommandParser(values);
        }

        internal bool RunCommand()
        {
            try
            {
                var audioFile = _values.GetString("audioFile");
                if (!File.Exists(audioFile))
                {
                    throw new FileNotFoundException($"Audio file not found: {audioFile}");
                }

                var result = TranscribeAudioAsync().GetAwaiter().GetResult();
                OutputResult(result);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteLineError($"\nERROR: {ex.Message}");
                return false;
            }
        }

        private async Task<string> TranscribeAudioAsync()
        {
            var region = _values.GetString("region");
            var key = _values.GetString("key");
            var endpoint = $"https://{region}.api.cognitive.microsoft.com/speechtotext/transcriptions:transcribe?api-version=2024-11-15";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            var content = new MultipartFormDataContent();

            // Add audio file
            var audioFile = _values.GetString("audioFile");
            var audioContent = new ByteArrayContent(File.ReadAllBytes(audioFile));
            content.Add(audioContent, "audio", Path.GetFileName(audioFile));

            // Build definition JSON
            var definition = new Dictionary<string, object>();
            
            // Handle locales
            var locales = _values.GetOrDefault("locales", new List<string>());
            if (locales.Count == 0 && _values.ContainsKey("locale"))
            {
                locales.Add(_values.GetString("locale"));
            }
            definition["locales"] = locales;

            // Handle diarization
            if (_values.GetOrDefault("diarization", false))
            {
                definition["diarization"] = new Dictionary<string, object>
                {
                    ["enabled"] = true,
                    ["maxSpeakers"] = _values.GetOrDefault("max-speakers", 2)
                };
            }

            // Handle channels
            var channels = _values.GetOrDefault("channels", new List<int>());
            if (channels.Count > 0)
            {
                definition["channels"] = channels;
            }

            // Handle profanity
            var profanity = _values.GetOrDefault("profanity", "Masked");
            definition["profanityFilterMode"] = profanity;

            var definitionJson = JsonSerializer.Serialize(definition);
            content.Add(new StringContent(definitionJson), "definition");

            var response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private void OutputResult(string jsonResponse)
        {
            var outputFormat = _values.GetOrDefault("output", "text");
            
            if (outputFormat == "json")
            {
                Console.WriteLine(jsonResponse);
                return;
            }

            // Parse JSON and output as text
            var result = JsonSerializer.Deserialize<TranscriptionResult>(jsonResponse);
            foreach (var phrase in result.CombinedPhrases)
            {
                Console.WriteLine(phrase.Text);
            }
        }

        private readonly FastTranscriptionCommandParser _parser;
    }

    public class TranscriptionResult
    {
        [JsonPropertyName("durationMilliseconds")]
        public long DurationMilliseconds { get; set; }

        [JsonPropertyName("combinedPhrases")]
        public List<CombinedPhrase> CombinedPhrases { get; set; }

        [JsonPropertyName("phrases")]
        public List<Phrase> Phrases { get; set; }
    }

    public class CombinedPhrase
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class Phrase
    {
        [JsonPropertyName("offsetMilliseconds")]
        public long OffsetMilliseconds { get; set; }

        [JsonPropertyName("durationMilliseconds")]
        public long DurationMilliseconds { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}