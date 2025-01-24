using System;
using System.Collections.Generic;
using System.Linq;

namespace Azure.AI.Details.Common.CLI
{
    public class FastTranscriptionCommandParser : Parser
    {
        public FastTranscriptionCommandParser(ICommandValues values) : base(values)
        {
            _values = values;
        }

        public override void Parse()
        {
            var args = _values.GetRemainingArgs();
            if (args.Count == 0)
            {
                throw new ArgumentException("Audio file path is required.");
            }

            _values["audioFile"] = args[0];

            // Parse locale or locales
            if (_values.ContainsKey("locale"))
            {
                _values["locale"] = _values.GetString("locale");
            }
            else if (_values.ContainsKey("locales"))
            {
                var locales = _values.GetString("locales")
                    .Split(',')
                    .Select(l => l.Trim())
                    .ToList();
                _values["locales"] = locales;
            }

            // Parse diarization options
            if (_values.ContainsKey("diarization"))
            {
                _values["diarization"] = true;
                if (_values.ContainsKey("max-speakers"))
                {
                    if (!int.TryParse(_values.GetString("max-speakers"), out int maxSpeakers))
                    {
                        throw new ArgumentException("max-speakers must be a valid integer");
                    }
                    _values["max-speakers"] = maxSpeakers;
                }
            }

            // Parse channels
            if (_values.ContainsKey("channels"))
            {
                var channels = _values.GetString("channels")
                    .Split(',')
                    .Select(c => 
                    {
                        if (!int.TryParse(c.Trim(), out int channel))
                        {
                            throw new ArgumentException($"Invalid channel value: {c}");
                        }
                        return channel;
                    })
                    .ToList();
                _values["channels"] = channels;
            }

            // Parse profanity filter mode
            if (_values.ContainsKey("profanity"))
            {
                var profanity = _values.GetString("profanity");
                if (!new[] { "None", "Masked", "Removed", "Tags" }.Contains(profanity))
                {
                    throw new ArgumentException("Invalid profanity filter mode. Valid values are: None, Masked, Removed, Tags");
                }
                _values["profanity"] = profanity;
            }

            // Parse output format
            var outputFormat = _values.GetOrDefault("output", "text");
            if (!new[] { "json", "text" }.Contains(outputFormat))
            {
                throw new ArgumentException("Invalid output format. Valid values are: json, text");
            }
            _values["output"] = outputFormat;

            // Validate required parameters
            if (!_values.ContainsKey("key"))
            {
                throw new ArgumentException("Speech service key is required. Use --key option.");
            }

            if (!_values.ContainsKey("region"))
            {
                throw new ArgumentException("Speech service region is required. Use --region option.");
            }
        }

        private readonly ICommandValues _values;
    }
}