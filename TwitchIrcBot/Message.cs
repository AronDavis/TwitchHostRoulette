﻿using System.Collections.Generic;

namespace TwitchIrcBot
{
    public class Message
    {
        public string Username;
        public string Text;
        public string Channel;

        public MessageTypeEnum MessageType;

        private static readonly Dictionary<string, MessageTypeEnum> _messageTypes = new Dictionary<string, MessageTypeEnum>()
        {
            { "001", MessageTypeEnum.Welcome },
            { "002", MessageTypeEnum.YourHost },
            { "003", MessageTypeEnum.Created },
            { "004", MessageTypeEnum.MyInfo },
            { "353", MessageTypeEnum.NameReply },
            { "366", MessageTypeEnum.EndOfNames },
            { "372", MessageTypeEnum.MessageOfTheDay },
            { "375", MessageTypeEnum.MessageOfTheDayStart },
            { "376", MessageTypeEnum.MessageOfTheDayEnd },
            { "421", MessageTypeEnum.UnknownCommand},
            { "CAP", MessageTypeEnum.Capability },
            { "JOIN", MessageTypeEnum.Join },
            { "PART", MessageTypeEnum.Part },
            { "PRIVMSG", MessageTypeEnum.PrivateMessage},
            { "PING", MessageTypeEnum.Ping },
            { "NOTICE", MessageTypeEnum.Notice },
        };

        public Message(string incoming)
        {
            if (incoming.StartsWith("PING"))
            {
                MessageType = MessageTypeEnum.Ping;
                return;
            }

            string[] incomingSplit = incoming.Split(' ');

            if (incomingSplit.Length < 1)
                return;

            MessageType = _messageTypes[incomingSplit[1]];

            switch (MessageType)
            {
                case MessageTypeEnum.Welcome:
                case MessageTypeEnum.YourHost:
                case MessageTypeEnum.Created:
                case MessageTypeEnum.MyInfo:
                case MessageTypeEnum.NameReply:
                case MessageTypeEnum.EndOfNames:
                case MessageTypeEnum.MessageOfTheDay:
                case MessageTypeEnum.Notice:
                    Text = incoming.Substring(incoming.IndexOf(" :") + 2);
                    break;
                case MessageTypeEnum.PrivateMessage:
                    Username = incoming.Substring(1, incoming.IndexOf("!") - 1);
                    int textStartIndex = incoming.IndexOf(" :") + 2;
                    Text = incoming.Substring(textStartIndex);
                    int channelStartIndex = incoming.IndexOf("#") + 1;
                    Channel = incoming.Substring(channelStartIndex, textStartIndex - channelStartIndex - 2);
                    break;
                case MessageTypeEnum.Capability:
                    if(incomingSplit[2] == "*" && incomingSplit[3] == "ACK")
                        Text = $"Server acknowledged {incoming.Substring(incoming.IndexOf(" :") + 2)}";
                    break;
                case MessageTypeEnum.Join:
                    Username = incoming.Substring(1, incoming.IndexOf("!") - 1);
                    Channel = incoming.Substring(incoming.IndexOf("#") + 1);
                    Text = $"{Username} joined #{Channel}!";
                    break;
                case MessageTypeEnum.Part:
                    Username = incoming.Substring(1, incoming.IndexOf("!") - 1);
                    Channel = incoming.Substring(incoming.IndexOf("#") + 1);
                    Text = $"{Username} left #{Channel}!";
                    break;
                default:
                    Text = $"Unknown message type: {incoming}";
                    break;
            }
        }

        public override string ToString()
        {
            if (Username != null && MessageType == MessageTypeEnum.PrivateMessage)
            {
                return $"{Username}: {Text}";
            }

            return Text;
        }
    }
}