using System;
using Xunit;

namespace IrcMessageParser.Tests
{
    public class MessagePrefixTests
    {
        [Fact]
        public void NameOnly_Parse()
        {
            var raw = "servername";
            var prefix = MessagePrefix.Parse(raw);

            Assert.Equal(raw, prefix.Name);
            Assert.Null(prefix.User);
            Assert.Null(prefix.Host);
        }

        [Fact]
        public void NickAndUser_Parse()
        {
            var raw = "nick!user";
            var prefix = MessagePrefix.Parse(raw);

            Assert.Equal("nick", prefix.Name);
            Assert.Equal("user", prefix.User);
            Assert.Null(prefix.Host);
        }

        [Fact]
        public void NickAndHost_Parse()
        {
            var raw = "nick@host";
            var prefix = MessagePrefix.Parse(raw);

            Assert.Equal("nick", prefix.Name);
            Assert.Null(prefix.User);
            Assert.Equal("host", prefix.Host);
        }

        [Fact]
        public void Full_Parse()
        {
            var raw = "nick!user@host";
            var prefix = MessagePrefix.Parse(raw);

            Assert.Equal("nick", prefix.Name);
            Assert.Equal("user", prefix.User);
            Assert.Equal("host", prefix.Host);
        }

        [Fact]
        public void NameOnly_ToString()
        {
            var prefix = new MessagePrefix(name: "servername", user: null, host: null);
            Assert.Equal("servername", prefix.ToString());
        }

        [Fact]
        public void NickAndUser_ToString()
        {
            var prefix = new MessagePrefix(name: "nick", user: "user", host: null);
            Assert.Equal("nick!user", prefix.ToString());
        }

        [Fact]
        public void NickAndHost_ToString()
        {
            var prefix = new MessagePrefix(name: "nick", user: null, host: "host");
            Assert.Equal("nick@host", prefix.ToString());
        }

        [Fact]
        public void Full_ToString()
        {
            var prefix = new MessagePrefix(name: "nick", user: "user", host: "host");
            Assert.Equal("nick!user@host", prefix.ToString());
        }

        [Theory]
        [InlineData("nick!@host")]
        [InlineData("nick!user@")]
        [InlineData("nick!@")]
        [InlineData("nick!")]
        [InlineData("nick@")]
        [InlineData("!")]
        [InlineData("@")]
        public void Parse_ThrowsFormatException(string input)
        {
            Assert.Throws<FormatException>(
                () => MessagePrefix.Parse(input)
            );
        }
    }
}
