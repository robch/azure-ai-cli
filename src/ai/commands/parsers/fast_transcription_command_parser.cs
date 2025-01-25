using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommonForCli.Details.Commands.Parsers;
using Microsoft.CommonForCli.Details.NamedValues;
using Microsoft.CommonForCli.Details.NamedValues.Parsers;

namespace Microsoft.AI.CLI.Commands.Parsers
{
    public class FastTranscriptionCommandParser : CommandParser
    {
        public FastTranscriptionCommandParser()
        {
            AddTokenParser(new RequiredValidValueNamedValueTokenParser("key", "The subscription key for Azure Speech service"));
            AddTokenParser(new RequiredValidValueNamedValueTokenParser("region", "The Azure region where your Speech resource is deployed"));
            
            AddTokenParser(new OptionalValidValueNamedValueTokenParser("locale", "The language code for single-language transcription"));
            AddTokenParser(new OptionalValidValueNamedValueTokenParser("locales", "Comma-separated list of language codes for multi-language transcription"));
            
            AddTokenParser(new TrueFalseNamedValueTokenParser("diarization", "Enable speaker diarization"));
            AddTokenParser(new OptionalValidValueNamedValueTokenParser("max-speakers", "Maximum number of speakers to identify (2-10)"));
            
            AddTokenParser(new OptionalValidValueNamedValueTokenParser("channels", "Number of audio channels"));
            
            var profanityValues = new[] { "raw", "remove", "mask" };
            AddTokenParser(new Any1ValueNamedValueTokenParser("profanity", "Profanity filtering mode", profanityValues));
            
            var outputValues = new[] { "text", "json" };
            AddTokenParser(new Any1ValueNamedValueTokenParser("output", "Output format", outputValues));
        }

        public override void ValidateTokens(IEnumerable<NamedValueToken> tokens)
        {
            base.ValidateTokens(tokens);

            var hasLocale = tokens.Any(t => t.Name == "locale" && t.HasValue);
            var hasLocales = tokens.Any(t => t.Name == "locales" && t.HasValue);
            
            if (hasLocale && hasLocales)
            {
                throw new ArgumentException("Cannot specify both --locale and --locales");
            }

            var hasDiarization = tokens.Any(t => t.Name == "diarization" && t.Value == "true");
            var maxSpeakersToken = tokens.FirstOrDefault(t => t.Name == "max-speakers");
            
            if (hasDiarization && (maxSpeakersToken == null || !maxSpeakersToken.HasValue))
            {
                throw new ArgumentException("--max-speakers is required when using --diarization");
            }

            if (maxSpeakersToken != null && maxSpeakersToken.HasValue)
            {
                if (!int.TryParse(maxSpeakersToken.Value, out int maxSpeakers) || maxSpeakers < 2 || maxSpeakers > 10)
                {
                    throw new ArgumentException("--max-speakers must be a number between 2 and 10");
                }
            }

            var channelsToken = tokens.FirstOrDefault(t => t.Name == "channels");
            if (channelsToken != null && channelsToken.HasValue)
            {
                if (!int.TryParse(channelsToken.Value, out int channels) || channels < 1)
                {
                    throw new ArgumentException("--channels must be a positive number");
                }
            }
        }
    }
}