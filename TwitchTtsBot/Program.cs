using System;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using TwitchIrcBot;
using TwitchIrcBot.CommandManagerPackage;
using UberduckApi;

namespace TwitchTtsBot
{
    class Program
    {
        private static IrcClient _irc;
        private static BotCommandManager _commandManager;
        private static string _username;

        static void Main(string[] args)
        {
            _commandManager = new BotCommandManager();
            _username = "voxindie";
            string twitchOAuth = ConfigurationManager.AppSettings["twitchOAuth"]; //from www.twitchapps.com/tmi
            string channel = "voxindie";

            _irc = new IrcClient("irc.twitch.tv", 6667, _username, twitchOAuth);

            string uberduckApiKey = ConfigurationManager.AppSettings["uberduckApiKey"]; //from www.twitchapps.com/tmi
            string uberduckApiSecret = ConfigurationManager.AppSettings["uberduckApiSecret"]; //from www.twitchapps.com/tmi
            UberduckApiClient uberduckApiClient = new UberduckApiClient();
            uberduckApiClient.SetAuth(uberduckApiKey, uberduckApiSecret);

            //join channel
            _irc.JoinRoom(channel);

            //Add commands
            _commandManager.AddCommand("!tts", "Used to generate TTS messages!", (message) =>
            {
                try
                {
                    if (message.Text?.ToLower() == "!tts")
                    {
                        _irc.SendChatMessage(channel, $"{message.Username} what do you want me to TTS?");
                        return;
                    }

                    string text = message.Text.Substring("!tts".Length).Trim();

                    string voice = "biggie-smalls";


                    if (text.StartsWith("("))
                    {
                        int endVoice = text.IndexOf(")");

                        if (endVoice != -1)
                        {
                            voice = text.Substring(1, endVoice - 1);
                            text = text.Substring(endVoice + 1).Trim();
                        }
                    }

                    if (text.Length == 0)
                    {
                        _irc.SendChatMessage(channel, $"{message.Username} what do you want me to TTS?");
                    }

                    UberduckApi.ResponseModels.SpeakResponse speakResponse = uberduckApiClient.Speak(text, voice).Result;

                    if (speakResponse.ErrorMessage != null)
                    {
                        _irc.SendChatMessage(channel, $"{message.Username} {speakResponse.ErrorMessage}");
                        return;
                    }

                    bool complete = false;
                    while (!complete)
                    {
                        UberduckApi.ResponseModels.SpeakStatusResponse statusResponse = uberduckApiClient.SpeakStatus(speakResponse.Uuid).Result;

                        complete = statusResponse.FinishedAt != null;

                        if (complete)
                        {
                            Console.WriteLine(statusResponse.Path);
                            ///
                            string program = "vlc.exe";

                            ProcessStartInfo pi = new ProcessStartInfo()
                            {
                                Arguments = $"{statusResponse.Path} --play-and-exit",
                                UseShellExecute = true,
                                FileName = program,
                                Verb = "OPEN",
                                WindowStyle = ProcessWindowStyle.Hidden
                            };

                            Process p = new Process();
                            p.StartInfo = pi;
                            p.Start();
                            p.WaitForExit();
                            ///
                        }
                    };
                }
                catch(Exception ex)
                {
                    _irc.SendChatMessage(channel, $"{message.Username} I broked.");
                }
            });

            Thread mainThread = new Thread(new ThreadStart(_runMain));
            mainThread.Start();
        }

        private static void _runMain()
        {
            while (true)
            {
                string incoming = _irc.ReadMessage();
                if (incoming == null || incoming.Length == 0)
                    continue;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(incoming);
                Message message = new Message(incoming);

                switch (message.MessageType)
                {
                    case MessageTypeEnum.Unknown:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(message);
                        break;
                    case MessageTypeEnum.Welcome:
                    case MessageTypeEnum.YourHost:
                    case MessageTypeEnum.Created:
                    case MessageTypeEnum.MyInfo:
                    case MessageTypeEnum.MessageOfTheDay:
                    case MessageTypeEnum.Capability:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(message);
                        break;
                    case MessageTypeEnum.Join:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(message);
                        break;
                    case MessageTypeEnum.Part:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(message);
                        break;
                    case MessageTypeEnum.PrivateMessage:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(message);
                        _handleCommand(message);
                        break;
                    case MessageTypeEnum.Ping:
                        _irc.SendIrcMessage("PONG");
                        break;
                }
            }
        }

        /// <summary>
        /// Assumes that message starts with a command
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        private static void _handleCommand(Message message)
        {
            Regex r = new Regex(@"^!\w+");
            _commandManager.RunCommand(r.Match(message.Text).Value, message);
        }
    }
}
