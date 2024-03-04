using System;
using System.Collections.Generic;
using System.Text;
using Discord.Addons.Interactive;
using Discord.Commands;
using System.Threading.Tasks;
using ServiceStack.Script;

namespace StockBot.Modules
{
    public class Admin : InteractiveBase
    {
        private CommandService _service;

        public Admin(CommandService service)
        {
            _service = service;
        }
        [Command("GetID", RunMode = RunMode.Async), Summary("Gets channel ID"),]
        public async Task GetID()
        {
            ulong id = Context.Channel.Id;
            string text = id.ToString();
            await Context.Channel.SendMessageAsync(text.ToString());
        }
        public String reverse(String s)
        {
            char[] array = s.ToCharArray();
            Array.Reverse(array);
            console.log("remove this")
            return new string(array);
        }
    }
}
