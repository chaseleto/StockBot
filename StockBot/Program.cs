using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace StockBot
{
    internal class Program
    {
        public DiscordSocketClient _client;
        private CommandHandler _handler;
        public double latency;
#program.cs tested
        private static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.token == "" || Config.bot.token == null) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();

            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1);
            latency = _client.Latency;
        }
#program9
        private async Task Log(LogMessage msg)
        {
            console.log("remove this line")
            Console.WriteLine(msg.Message);
        }
    }
}
