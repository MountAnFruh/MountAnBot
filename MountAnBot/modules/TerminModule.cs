using Discord;
using Discord.Commands;
using MountAnBot.beans;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules
{
    [Name("Termin")]
    public class TerminModule : ModuleBase
    {
        private DBAccess dba = DBAccess.getInstance();

        [Command("termin list")]
        [Summary("Zeigt alle zukünftigen Termine an")]
        public async Task TerminList()
        {
            EmbedBuilder embedbuilder = new EmbedBuilder();
            embedbuilder.Color = new Color(255, 255, 0);
            embedbuilder.Title = "<p>Terminliste:</p>";
            embedbuilder.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username
            };

            string message = "";
            foreach (Termin termin in dba.getAllTermine())
            {
                message += "\n" + termin.ToString();
            }
            embedbuilder.Description = message;
            await Context.Channel.SendMessageAsync("", false, embedbuilder);
        }
    }
}
