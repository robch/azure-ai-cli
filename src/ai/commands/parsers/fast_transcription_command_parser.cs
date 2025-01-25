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
        public static bool ParseCommand(INamedValueTokens tokens, ICommandValues values)
        {
            return ParseCommand("fast-transcribe", fastTranscriptionCommandParsers, tokens, values);
        }

        public static bool ParseCommandValues(INamedValueTokens tokens, ICommandValues values)
        {
            return ParseCommandValues("fast-transcribe", fastTranscriptionCommandParsers, tokens, values);
        }

        public static IEnumerable<INamedValueTokenParser> GetCommandParsers()
        {
            return fastTranscriptionCommandParsers;
        }

        #region private data

        private static INamedValueTokenParser[] fastTranscriptionCommandParsers = {
            // Command identifier
            new RequiredValidValueNamedValueTokenParser(null, "x.command", "11", "fast-transcribe"),

            // Common parsers
            new ExpectOutputTokenParser(),
            new DiagnosticLogTokenParser(),
            new CommonNamedValueTokenParsers(),

            // Required parameters
            new RequiredValidValueNamedValueTokenParser("--key", "service.config.key", "001", null),
            new RequiredValidValueNamedValueTokenParser("--region", "service.config.region", "001", null),

            // Language configuration
            new Any1ValueNamedValueTokenParser("--locale", "service.config.locale", "001"),
            new Any1ValueNamedValueTokenParser("--locales", "service.config.locales", "001"),

            // Diarization settings
            new TrueFalseNamedValueTokenParser("--diarization", "service.config.diarization.enabled", "001"),
            new Any1ValueNamedValueTokenParser("--max-speakers", "service.config.diarization.max.speakers", "001"),

            // Audio configuration
            new Any1ValueNamedValueTokenParser("--channels", "service.config.audio.channels", "001"),

            // Profanity filter
            new RequiredValidValueNamedValueTokenParser("--profanity", "service.config.profanity", "001", "raw;remove;mask"),

            // Output format
            new RequiredValidValueNamedValueTokenParser("--output", "service.config.output.format", "001", "text;json"),

            // Input file handling
            new ExpandFileNameNamedValueTokenParser(),
            new Any1ValueNamedValueTokenParser(null, "audio.input.file", "011"),
        };

        public override void ValidateTokens(IEnumerable<NamedValueToken> tokens)
        {
            base.ValidateTokens(tokens);

            var hasLocale = tokens.Any(t => t.Name == "service.config.locale" && t.HasValue);
            var hasLocales = tokens.Any(t => t.Name == "service.config.locales" && t.HasValue);
            
            if (hasLocale && hasLocales)
            {
                throw new ArgumentException("Cannot specify both --locale and --locales");
            }

            var hasDiarization = tokens.Any(t => t.Name == "service.config.diarization.enabled" && t.Value == "true");
            var maxSpeakersToken = tokens.FirstOrDefault(t => t.Name == "service.config.diarization.max.speakers");
            
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

            var channelsToken = tokens.FirstOrDefault(t => t.Name == "service.config.audio.channels");
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