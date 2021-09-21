using System;
using Xunit;

namespace IrcMessageParser.Tests
{
    public class MessageContentTests
    {
        [Fact]
        public void NoCtcp()
        {
            var raw = "text";
            var content = MessageContent.Parse(raw);

            Assert.Equal(raw, content.Text);
            Assert.Null(content.Ctcp);
            Assert.Equal(raw, content);
        }

        [Fact]
        public void Ctcp()
        {
            var raw = "\u0001ACTION text\u0001";
            var content = MessageContent.Parse(raw);

            Assert.Equal("text", content.Text);
            Assert.Equal("ACTION", content.Ctcp);
            Assert.Equal(raw, content);
        }

        [Fact]
        public void CtcpMissingEndDelimiter()
        {
            var raw = "\u0001ACTION text";
            var content = MessageContent.Parse(raw);

            Assert.Equal("text", content.Text);
            Assert.Equal("ACTION", content.Ctcp);
            Assert.Equal(raw + "\u0001", content);
        }

        [Fact]
        public void CtcpMissingEnding_Throws()
        {
            Assert.Throws<FormatException>(() => MessageContent.Parse("\u0001ACTION"));
        }
    }
}
