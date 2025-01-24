using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Azure.AI.Details.Common.CLI.Tests
{
    public class FastTranscriptionCommandTests
    {
        [Fact]
        public void TestParserValidatesRequiredParameters()
        {
            var values = new CommandValues();
            values.AddRemainingArg("test.wav");
            var parser = new FastTranscriptionCommandParser(values);

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse());
            Assert.Contains("Speech service key is required", ex.Message);

            values["key"] = "test-key";
            ex = Assert.Throws<ArgumentException>(() => parser.Parse());
            Assert.Contains("Speech service region is required", ex.Message);
        }

        [Fact]
        public void TestParserParsesLocales()
        {
            var values = new CommandValues();
            values.AddRemainingArg("test.wav");
            values["key"] = "test-key";
            values["region"] = "westus";
            values["locales"] = "en-US,ja-JP";

            var parser = new FastTranscriptionCommandParser(values);
            parser.Parse();

            var locales = values.Get<List<string>>("locales");
            Assert.Equal(2, locales.Count);
            Assert.Equal("en-US", locales[0]);
            Assert.Equal("ja-JP", locales[1]);
        }

        [Fact]
        public void TestParserParsesDiarization()
        {
            var values = new CommandValues();
            values.AddRemainingArg("test.wav");
            values["key"] = "test-key";
            values["region"] = "westus";
            values["diarization"] = "";
            values["max-speakers"] = "3";

            var parser = new FastTranscriptionCommandParser(values);
            parser.Parse();

            Assert.True(values.Get<bool>("diarization"));
            Assert.Equal(3, values.Get<int>("max-speakers"));
        }

        [Fact]
        public void TestParserValidatesOutputFormat()
        {
            var values = new CommandValues();
            values.AddRemainingArg("test.wav");
            values["key"] = "test-key";
            values["region"] = "westus";
            values["output"] = "invalid";

            var parser = new FastTranscriptionCommandParser(values);
            var ex = Assert.Throws<ArgumentException>(() => parser.Parse());
            Assert.Contains("Invalid output format", ex.Message);
        }

        [Fact]
        public void TestParserValidatesProfanityMode()
        {
            var values = new CommandValues();
            values.AddRemainingArg("test.wav");
            values["key"] = "test-key";
            values["region"] = "westus";
            values["profanity"] = "invalid";

            var parser = new FastTranscriptionCommandParser(values);
            var ex = Assert.Throws<ArgumentException>(() => parser.Parse());
            Assert.Contains("Invalid profanity filter mode", ex.Message);
        }
    }
}