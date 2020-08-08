using System.Collections.Concurrent;
using System.Net.Sockets;
using System.IO;
using System;

namespace TwitchIrcBot
{
    public class IrcClient
    {
        private string _username;
        private ConcurrentDictionary<string, string> _channels;


        private readonly object _inputLock = new object();
        private readonly object _outputLock = new object();

        private TcpClient _tcpClient;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;
        private ConcurrentQueue<string> _consoleInput = new ConcurrentQueue<string>();

        public IrcClient(string ip, int port, string username, string password)
        {
            this._username = username;
            _tcpClient = new TcpClient(ip, port);
            _inputStream = new StreamReader(_tcpClient.GetStream());
            _outputStream = new StreamWriter(_tcpClient.GetStream());

            _channels = new ConcurrentDictionary<string, string>();

            //this is just what the server is expecting
            //creates the connection
            //connects to server, but not any specific channel yet
            _outputStream.WriteLine($"PASS {password}");
            _outputStream.WriteLine($"NICK {username}");
            _outputStream.WriteLine($"USER {username} 0 *:{username}");
            _outputStream.Flush();

            _outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            _outputStream.Flush();

        }

        /// <summary>
        /// Use this for testing.
        /// </summary>
        public IrcClient()
        {
            _inputStream = new StreamReader(new MemoryStream());
            _outputStream = new StreamWriter(new MemoryStream());
        }

        public void JoinRoom(string channel)
        {
            if (_channels.ContainsKey(channel))
                throw new Exception($"Already in channel \"{channel}\"");

            if (!_channels.TryAdd(channel, channel))
                throw new Exception("Failed to join channel.");

            lock (_outputLock)
            {
                _outputStream.WriteLine($"JOIN #{channel}");
                _outputStream.Flush();
            }
        }

        public void LeaveRoom(string channel)
        {
            if (!_channels.ContainsKey(channel))
                throw new Exception($"Not in channel \"{channel}\"");

            if (!_channels.TryRemove(channel, out var outValue))
                throw new Exception("Failed to leave channel.");

            lock (_outputLock)
            {
                _outputStream.WriteLine($"PART #{channel}");
                _outputStream.Flush();
            }
        }

        public void RequestNames()
        {
            foreach (var channel in _channels.Keys)
                RequestNames(channel);
        }

        public void RequestNames(string channel)
        {
            if (!_channels.ContainsKey(channel))
                throw new Exception($"Not in channel \"{channel}\"");

            lock (_outputLock)
            {
                _outputStream.WriteLine($"NAMES #{channel}");
                _outputStream.Flush();
            }
        }

        public void SendIrcMessage(string message)
        {
            lock (_outputLock)
            {
                _outputStream.WriteLine(message);
                _outputStream.Flush();
            }
        }

        public void SendChatMessage(string channel, string message)
        {
            SendIrcMessage(GenerateChatMessage(_username, channel, message));
        }

        public string GenerateChatMessage(string username, string channel, string message)
        {
            if (!_channels.ContainsKey(channel))
                throw new Exception($"Not in channel \"{channel}\"");

            return $":{username}!{username}@{username}.tmi.twitch.tv PRIVMSG #{channel} :{message}";
        }

        public void ConsoleInput(string message)
        {
            _consoleInput.Enqueue(message);
        }

        public string ReadMessage()
        {
            if (_consoleInput.Count > 0)
            {
                if (_consoleInput.TryDequeue(out string result))
                    return result;

                throw new Exception("Could not read from console.");

            }

            lock (_inputLock)
            {
                return _inputStream.ReadLine(); //NOTE: gets error when not connected to internet?
            }
        }
    }
}
