using System;

namespace IrcMessageParser
{
    public record MessageContent
    {
        private const char CtcpDelimiter = '\u0001';

        public string Text { get; }
        public string? Ctcp { get; }

        public MessageContent(string text, string? ctcp = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Ctcp = ctcp;
        }

        public static implicit operator string(MessageContent content)
        {
            if (content.Ctcp is null)
                return content.Text;

            return $"{CtcpDelimiter}{content.Ctcp} {content.Text}{CtcpDelimiter}";
        }

        public static explicit operator MessageContent(string s)
            => Parse(s);

        public static MessageContent Parse(ReadOnlySpan<char> input)
        {
            string? ctcp;
            if (input[0] == CtcpDelimiter)
            {
                input = input[1..];
                if (input[^1] == CtcpDelimiter)
                    input = input[..^1];

                int i = input.IndexOf(' ');

                if (i == -1)
                    throw new FormatException("Missing CTCP ending");

                ctcp = input[..i].ToString();
                input = input[(i + 1)..];
            }
            else
            {
                ctcp = null;
            }

            var text = input.ToString();

            return new MessageContent(text, ctcp);
        }

        public override string ToString() => this;
    }
}
