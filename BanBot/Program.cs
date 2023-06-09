using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Threading;
using TwitchApi.ResponseModels.Auth;

namespace BanBot
{
    public class Program
    {
        public static object LockObject = new object();
        public static UserAccessTokenModel UserAccessToken;

        public static void Main(string[] args)
        {
            Thread workEngineThread = new Thread(new ThreadStart(RunWorkEngine));
            workEngineThread.Start();

            Thread webServerThread = new Thread(new ThreadStart(RunWebServer));
            webServerThread.Start();
        }

        public static void RunWebServer()
        {
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }

        public static void RunWorkEngine()
        {


            //loop forever
            while(true)
            {
                lock (LockObject)
                {
                    if(UserAccessToken == null)
                    {
                        continue;
                    }

                    System.Console.WriteLine(UserAccessToken.UserAccessToken);
                }

                /*
                //if there's work in the queue
                if (WorkQueue.Count > 0)
                {
                    //try to pull the job out of the queue
                    if (WorkQueue.TryDequeue(out string job))
                    {
                        Interlocked.Exchange(ref WorkQueue, null);
                        System.Console.WriteLine(job);
                    }
                }
                */

                //sleep for a second so we don't burn out
                Thread.Sleep(1000);
            }
        }
    }
}
