using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using StockBot.Modules;
namespace StockBot
{
    public class CommandHandler
    {
        public DiscordSocketClient _client;
        private CommandService _service;
        private IServiceProvider services;
        public int latency;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            #Hacker code
            #hacking code 12
                #ha
            _service = new CommandService();
            services = new ServiceCollection()
               .AddSingleton(_client)
               .AddSingleton<InteractiveService>()
               .BuildServiceProvider();
            await _service.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
            _client.MessageReceived += HandleCommandAsync;
            latency = _client.Latency;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            Config cfg = new Config();
            //Stock stock = new Stock(_service);
            //Admin admin = new Admin(_service);
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var ctx = new SocketCommandContext(_client, msg);
            int argPos = 0;
            
            /*if(msg.Author.Username.Contains("owen") && msg.Content.StartsWith('c'))
            {
                String test = msg.Content;
                test = admin.reverse(test);
                test = test.Substring(5);
                test = admin.reverse(test).Substring(2);
                
            }*/

            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(ctx, argPos, services: services);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}
