using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MountAnBot.core;
using MountAnBot.beans;

namespace MountAnBot.modules
{
    [Name("Game")]
    public class GameModule : ModuleBase
    {
        private CommandService commandservice;

        public GameModule(CommandService commandservice)
        {
            this.commandservice = commandservice;
        }

        //[Command("game help"), Alias("game")]
        //[Summary("Gibt dir die Hilfe die du brauchst")]
        //public async Task GameHelp()
        //{
        //    string message = "";
        //    foreach (CommandInfo info in commandservice.Commands)
        //    {
        //        if (info.Name.StartsWith("song"))
        //        {
        //            message += "\n**" + info.Name + "** -> " + info.Summary;
        //        }
        //    }
        //    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "Alle Game-Commands:", message));
        //}

        [Command("distance")]
        [Summary("Gibt dir die Entfernung von zwei Orten zurück")]
        public async Task Distance(params string[] input)
        {
            if (input.Length == 2) {
                Pfad entf = new Pfad(input[0], input[1]);
                string message;
                if(entf.Start_addresse.Equals("") || entf.End_adresse.Equals(""))
                {
                    message = "Route ist nicht verfügbar!";
                }
                else
                {
                    message = "Von: " + entf.Start_addresse + "\nBis: " + entf.End_adresse + "\nEntfernung: " + entf.Entfernung + " km";
                }
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "Entfernung:", message));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !distance [Ort 1] [Ort 2]"));
            }
        }

        //[Command("game start")]
        //[Summary("Startet das Spiel für dich")]
        //public async Task GameStart()
        //{

        //}
    }
}
