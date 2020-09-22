using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using TwitchApi;
using TwitchApi.ResponseModels.Streams;
using TwitchHostRoulette.Models.Follows;
using TwitchIrcBot;

namespace TwitchHostRoulette
{
    class Program
    {
        private static IrcClient _ircClient;
        private static ConcurrentDictionary<string, string> _usersJoined;

        private static RouletteStateEnum _rouletteState = RouletteStateEnum.WaitingToStart;

        static void Main(string[] args)
        {
            var username = ConfigurationManager.AppSettings["username"];
            var userId = int.Parse(ConfigurationManager.AppSettings["user-id"]);
            string oauthToken = ConfigurationManager.AppSettings["api-oauth-token"];
            string clientId = ConfigurationManager.AppSettings["api-client-id"];
            string password = ConfigurationManager.AppSettings["oauth"];

            _usersJoined = new ConcurrentDictionary<string, string>();

            _startIrc(username, password);

            //give IRC time to connect
            Thread.Sleep(500);

            TwitchApiClient twitchApiClient = new TwitchApiClient();

            Random random = new Random();

            while (_rouletteState != RouletteStateEnum.Quitting)
            {
                switch (_rouletteState)
                {
                    case RouletteStateEnum.WaitingToStart:
                        _handleWaitingToStart();
                        break;
                    case RouletteStateEnum.WaitingForJoins:
                        _handleWaitingForJoins();
                        break;
                    case RouletteStateEnum.PickingAWinner:
                        _handlePickingAWinner(
                            twitchApiClient: twitchApiClient,
                            username: username,
                            userId: userId,
                            oauthToken: oauthToken,
                            clientId: clientId,
                            random: random
                            );
                        break;
                }
            } 
        }

        private static void _startIrc(string username, string password)
        {
            _ircClient = new IrcClient("irc.chat.twitch.tv", 6667, username, password); //password from www.twitchapps.com/tmi

            Thread ircThread = new Thread(_handleIrc);
            ircThread.IsBackground = true;
            ircThread.Start();

            _ircClient.JoinRoom(username);
        }

        private static void _handleWaitingToStart()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Waiting to start.");
            Console.WriteLine("Press S to start or Q to quit.");
            Console.WriteLine();
            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.S:
                    _usersJoined.Clear();
                    _rouletteState = RouletteStateEnum.WaitingForJoins;
                    break;
                case ConsoleKey.Q:
                    _rouletteState = RouletteStateEnum.Quitting;
                    break;
            }
        }

        private static void _handleWaitingForJoins()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Waiting for joins.");
            Console.WriteLine("Press C to continue, B to go back, or Q to quit.");
            Console.WriteLine();
            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.C:
                    _rouletteState = RouletteStateEnum.PickingAWinner;
                    break;
                case ConsoleKey.B:
                    _rouletteState = RouletteStateEnum.WaitingToStart;
                    break;
                case ConsoleKey.Q:
                    _rouletteState = RouletteStateEnum.Quitting;
                    break;
            }
        }

        private static void _handlePickingAWinner(TwitchApiClient twitchApiClient, string username, int userId, string oauthToken, string clientId, Random random)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Picking a winner.");
            Console.WriteLine();

            //ChattersModel chattersData = twitchApiClient.GetChatters(username).Result;

            //string[] chatters = chattersData.Chatters.VIPs
            //    .Concat(chattersData.Chatters.Moderators)
            //    .Concat(chattersData.Chatters.Staff)
            //    .Concat(chattersData.Chatters.Admins)
            //    .Concat(chattersData.Chatters.GlobalMods)
            //    .Concat(chattersData.Chatters.Viewers)
            //    .Select(c => c.ToLower())
            //    .ToArray();

            List<FollowDataModel> followersData = _getFollowers(
                twitchApiClient: twitchApiClient,
                userId: userId,
                oauthToken: oauthToken,
                clientId: clientId
                );

            string[] followers = followersData.Select(f => f.FromName.ToLower()).ToArray();

            var usersJoined = _usersJoined.Keys.Distinct();

            List<string> participants = usersJoined.Intersect(followers).ToList();

            if (participants.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No followers have joined!");

                _rouletteState = RouletteStateEnum.WaitingToStart;
                return;
            }

            bool isHostChosen = false;
            while (!isHostChosen)
            {
                int index = random.Next(participants.Count);

                string chosenParticipant = participants[index];

                List<StreamData> streamDataList = twitchApiClient.GetStreams(1, (s) => true, chosenParticipant, "512710").Result; //CoD ID

                if (streamDataList.Count > 0)
                {
                    isHostChosen = true;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(chosenParticipant);
                    _rouletteState = RouletteStateEnum.WaitingToStart;
                    return;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{chosenParticipant} was chosen, but is not streaming the game.");
                    //remove because they're not streaming
                    participants.Remove(chosenParticipant);

                    if (participants.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Could not find a follower that has joined and is streaming the game.");
                        _rouletteState = RouletteStateEnum.WaitingToStart;
                        return;
                    }
                }
            }
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
                        if(_rouletteState == RouletteStateEnum.WaitingForJoins && message.Text.ToLower().StartsWith("!join"))
                        {
                            var usernameLower = message.Username.ToLower();
                            _usersJoined.TryAdd(usernameLower, usernameLower);
                        }
                        //_updateChatDisplay(message);
                        break;
                    case MessageTypeEnum.Ping:
                        _ircClient.SendIrcMessage("PONG");
                        break;
                }
            }
        }

        private static void _updateChatDisplay(Message message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
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
