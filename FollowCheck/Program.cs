using System;
using System.Collections.Generic;
using System.Linq;
using TwitchApi;
using TwitchHostRoulette.Models.Follows;

namespace FollowCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = "VOXindie";
            //to get oauth:
            //https://id.twitch.tv/oauth2/token?client_id=p4y1qamoqkv2o2gm8fnw642yhfdec8&client_secret=my0oh15famxbcg6y9dtdr6b9140r0v&grant_type=client_credentials
            string oauthToken = "i3kegkcw7zygtx45e1qlzbpnx3ghvx";
            string clientId = "p4y1qamoqkv2o2gm8fnw642yhfdec8";

            TwitchApiClient twitchApiClient = new TwitchApiClient();

            int userId = twitchApiClient.GetUserId(username, oauthToken, clientId).Result;

            List<FollowDataModel> followersData = _getFollowers(
                twitchApiClient: twitchApiClient,
                userId: userId,
                oauthToken: oauthToken,
                clientId: clientId
                );

            HashSet<string> followers = new HashSet<string>(followersData.Select(f => f.FromName.ToLower()).Distinct());

            List<FollowDataModel> peopleIFollowData = _getPeopleUserFollows(
              twitchApiClient: twitchApiClient,
              userId: userId,
              oauthToken: oauthToken,
              clientId: clientId
              );

            string[] peopleIFollow = peopleIFollowData.Select(f => f.ToName.ToLower()).Distinct().ToArray();

            for(int i = 0; i < peopleIFollow.Length; i++)
            {
                if(!followers.Contains(peopleIFollow[i]))
                {
                    Console.WriteLine($"{peopleIFollow[i]} is not following you back.");
                }
            }

            Console.WriteLine("DONE");
            Console.ReadKey();
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

        private static List<FollowDataModel> _getPeopleUserFollows(TwitchApiClient twitchApiClient, int userId, string oauthToken, string clientId)
        {
            FollowsModel followers = twitchApiClient.GetPeopleUserFollows(userId, oauthToken, clientId).Result;

            List<FollowDataModel> followersData = new List<FollowDataModel>(followers.Data);

            while (followers.Pagination.Cursor != null)
            {
                followers = twitchApiClient.GetPeopleUserFollows(userId, oauthToken, clientId, followers.Pagination.Cursor).Result;
                followersData.AddRange(followers.Data);
            }

            return followersData;
        }

        
    }
}
