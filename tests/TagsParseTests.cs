using Xunit;

namespace IrcMessageParser.Tests
{
    public class TagsParseTests
    {

        [Fact]
        public void TagsParseTest()
        {
            var raw = @"+example=raw+:=,escaped\:\s\\;semicolon=\:;space=\s;backslash=\\;cr=\r;lf=\n;random=1\:\s\\\r\n2;flag;trailing=;overwrite=first;overwrite=last";
            var tags = MessageTags.Parse(raw);

            Assert.Equal(10, tags.Count);
            Assert.Equal(@"raw+:=,escaped; \", tags["+example"]);
            Assert.Equal(@";", tags["semicolon"]);
            Assert.Equal(@" ", tags["space"]);
            Assert.Equal("\\", tags["backslash"]);
            Assert.Equal("\r", tags["cr"]);
            Assert.Equal("\n", tags["lf"]);
            Assert.Equal("1; \\\r\n2", tags["random"]);
            Assert.Equal("", tags["flag"]);
            Assert.Equal("", tags["trailing"]);
            Assert.Equal("last", tags["overwrite"]);
        }
    }
}
