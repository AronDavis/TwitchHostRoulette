using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using TwitchApi;
using TwitchApi.ResponseModels.Chatters;
using TwitchApi.ResponseModels.Streams;
using TwitchHostRoulette.Models.Follows;
using TwitchIrcBot;

namespace TwitchHostRoulette
{
    class Program
    {
        private static IrcClient _ircClient;
        static void Main(string[] args)
        {
            var username = ConfigurationManager.AppSettings["username"];
            var userId = int.Parse(ConfigurationManager.AppSettings["user-id"]);
            string oauthToken = ConfigurationManager.AppSettings["api-oauth-token"];
            string clientId = ConfigurationManager.AppSettings["api-client-id"];

            TwitchApiClient twitchApiClient = new TwitchApiClient();

            Random random = new Random();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press Q to quit.");
            do
            {
                ChattersModel chattersData = twitchApiClient.GetChatters(username).Result;

                List<FollowDataModel> followersData = _getFollowers(
                    twitchApiClient: twitchApiClient,
                    userId: userId,
                    oauthToken: oauthToken,
                    clientId: clientId
                    );

                string[] chatters = chattersData.Chatters.VIPs
                    .Concat(chattersData.Chatters.Moderators)
                    .Concat(chattersData.Chatters.Staff)
                    .Concat(chattersData.Chatters.Admins)
                    .Concat(chattersData.Chatters.GlobalMods)
                    .Concat(chattersData.Chatters.Viewers)
                    .Select(c => c.ToLower())
                    .ToArray();

                string[] followers = followersData.Select(f => f.FromName.ToLower()).ToArray();


                List<string> followersInChat = chatters.Intersect(followers).Distinct().ToList();

                if(followersInChat.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("There are no followers in chat!");
                    continue;
                }

                bool isHostChosen = false;
                while (!isHostChosen)
                {
                    int index = random.Next(followersInChat.Count);

                    string chosenFollower = followersInChat[index];

                    List<StreamData> streamDataList = twitchApiClient.GetStreams(1, (s) => true, chosenFollower, "512710").Result; //CoD ID

                    if (streamDataList.Count > 0)
                    {
                        isHostChosen = true;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(chosenFollower);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{chosenFollower} was chosen, but is not streaming the game.");
                        //remove because they're not streaming
                        followersInChat.Remove(chosenFollower);

                        if(followersInChat.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Could not find a follower in chat that is streaming the game!");
                            break;
                        }
                    }
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);


            /*
            string password = ConfigurationManager.AppSettings["oauth"];

            _ircClient = new IrcClient("irc.chat.twitch.tv", 6667, username, password); //password from www.twitchapps.com/tmi

            Thread ircThread = new Thread(_handleIrc);
            ircThread.IsBackground = true;
            ircThread.Start();

            _ircClient.JoinRoom(username);
            _ircClient.RequestNames(username);

            var apiClient = new TwitchApiClient();

            while(true)
            {
                Thread.Sleep(500);
            }
            */
        }

        private static List<FollowDataModel> _getFollowers(TwitchApiClient twitchApiClient, int userId, string oauthToken, string clientId)
        {
            FollowsModel followers = twitchApiClient.GetFollows(userId, oauthToken, clientId).Result;

            List<FollowDataModel> followersData = new List<FollowDataModel>(followers.Data);

            while (followers.Pagination.Cursor != null)
            {
                followers = twitchApiClient.GetFollows(userId, oauthToken, clientId, followers.Pagination.Cursor).Result;
                followersData.AddRange(followers.Data);
            }

            return followersData;
        }

        private static void _handleIrc()
        {
            while (true)
            {
                string incoming = _ircClient.ReadMessage();
                if (incoming == null || incoming.Length == 0)
                    continue;

                //Console.WriteLine(incoming);
                Message message = new Message(incoming);

                switch (message.MessageType)
                {
                    case MessageTypeEnum.Unknown:
                        _updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.Welcome:
                    case MessageTypeEnum.YourHost:
                    case MessageTypeEnum.Created:
                    case MessageTypeEnum.MyInfo:
                    case MessageTypeEnum.MessageOfTheDay:
                    case MessageTypeEnum.Capability:
                    case MessageTypeEnum.Notice:
                    case MessageTypeEnum.UnknownCommand:
                        _updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.Join:
                        _updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.Part:
                        _updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.PrivateMessage:
                        _updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.Ping:
                        _ircClient.SendIrcMessage("PONG");
                        break;
                }
            }
        }

        private static void _updateChatDisplay(Message message)
        {
            if (message.Username != null)
            {

                if (message.MessageType == MessageTypeEnum.PrivateMessage)
                {
                    Console.WriteLine($"#{message.Channel} {message.Username}: {message.Text}");
                }
                else
                {
                    Console.WriteLine($"{message.Text}");
                }
            }
            else
            {
                Console.WriteLine(message.Text);
            }
        }
    }
}
