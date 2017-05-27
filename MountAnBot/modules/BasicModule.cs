using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules
{
    public class BasicModule : ModuleBase
    {
        [Command("kevin")]
        [Summary("Kevin")]
        public async Task Kevin()
        {
            MessageProperties props = new MessageProperties();
            IUserMessage msg = await Context.Channel.SendMessageAsync("<:zugfuhrer:278803990068592641>");

            await Task.Delay(200);
            await msg.ModifyAsync(x => { x.Content = "<:zugfuhrerlll:311395154718097409>"; });

            await Task.Delay(200);
            await msg.ModifyAsync(x => { x.Content = "<:zugfuhrerll:311394867039174657>"; });

            await Task.Delay(200);
            await msg.ModifyAsync(x => { x.Content = "<:zugfuhrerl:311394726823723008>"; });

            await Task.Delay(200);
            await msg.ModifyAsync(x => { x.Content = "<:zugfuhrer:278803990068592641>"; });
        }

        [Command("swag")]
        [Summary("Einfach nur swag")]
        public async Task Swag()
        {
            IUserMessage msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1500);
            await msg.ModifyAsync(x => { x.Content = "( ͡⌐■ ͜ʖ ͡-■)"; });
        }
    }
}
