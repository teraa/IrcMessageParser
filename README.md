# IRC Message Parser

[![NuGet](https://img.shields.io/nuget/v/Teraa.IrcMessageParser?label=NuGet&logo=nuget&color=blue)](https://www.nuget.org/packages/Teraa.IrcMessageParser/)

## Description
Library containing types and parsers for IRC message, as specified in [RFC 1459](https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1).
Compatible with [Twitch Messaging Interface (TMI)](https://dev.twitch.tv/docs/irc).

Library provides multiple implementations of parsers, depending on the needs.
### `ITagsParser` implementations
- `TagsParser` - default implementation, validates and parses all of the message tags (both keys and values) in a dictionary.
This allocates most memory.

- `LazyTagsParser` - recommended if messages are expected to contain many tags, and tags are expected to be used only via the indexer.
Any other behaviour of `LazyTags` will fall back to the default implementation and allocate whole dictionary containing all the tags.
Format of the input is not validated apart from empty/null check.

### `ICommandParser` implementations

- `CommandParser` - default but slower implementation.
Allows for case insensitive commands (e.g. `PRIVMSG`, `privmsg`, etc), as per the spec.

- `FastCommandParser` - faster implementation using the [source generated](https://github.com/andrewlock/NetEscapades.EnumGenerators) parser for the `Command` enum.
The parsing is case-sensitive, and only all-uppercase command names or 3 digit numerics are accepted.

## Example
```cs
using System.Diagnostics;
using Teraa.Irc;
using Teraa.Irc.Parsing;

// Recommended for TMI
var parser = new MessageParser
{
    TagsParser = new LazyTagsParser(),
    CommandParser = new FastCommandParser(),
};

// Parse Example
string rawMessage = "@emote-only=0;followers-only=-1;r9k=0;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #channel";
IMessage message = parser.Parse(rawMessage);

Debug.Assert(message.Tags is not null);
Debug.Assert(message.Prefix is not null);
Debug.Assert(message.Arg is not null);

Console.WriteLine(message.Tags["followers-only"]); // -1
Console.WriteLine(message.Prefix.Name); // tmi.twitch.tv
Console.WriteLine(message.Command); // ROOMSTATE
Console.WriteLine(message.Arg); // #channel

// ToString Example
message = new Message(
    Command: Command.PRIVMSG,
    Prefix: new Prefix(
        Name: "nick",
        User: "user",
        Host: "host"),
    Arg: "#channel",
    Content: new Content(
        Text: "Hello"));

rawMessage = parser.ToString(message);

Console.WriteLine(rawMessage); // :nick!user@host PRIVMSG #channel :Hello

```

## References
- [RFC 1459](https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1) - Message format
- [IRCv3 spec](https://ircv3.net/specs/extensions/message-tags#format) - Message tags format
- [CTCP protocol format](https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html)
