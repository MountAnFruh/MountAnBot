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
        private CommandService service;

        public TerminModule(CommandService service)
        {
            this.service = service;
        }

        [Command("termin help")]
        [Summary("Damit bekommst du deine Hilfe die du brauchst")]
        public async Task TerminHelp()
        {
            EmbedBuilder embedbuilder = new EmbedBuilder();
            embedbuilder.Color = new Color(255, 255, 0);
            embedbuilder.Title = "Alle Termin-Commands:";
            embedbuilder.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username
            };

            string message = "";
            foreach (CommandInfo info in service.Commands)
            {
                if(info.Name.StartsWith("termin"))
                {
                    message += "\n" + info.Name + " -> " + info.Summary;
                }
            }
            embedbuilder.Description = message;
            await ReplyAsync("", false, embedbuilder);
        }

        [Command("termin list")]
        [Summary("Zeigt alle zukünftigen Termine an")]
        public async Task TerminList()
        {
            EmbedBuilder embedbuilder = new EmbedBuilder();
            embedbuilder.Color = new Color(255, 255, 0);
            embedbuilder.Title = "Terminliste:";
            embedbuilder.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username
            };

            string message = "";
            foreach (Termin termin in dba.getAllZukTermine())
            {
                message += "\n" + termin.ToString();
            }
            embedbuilder.Description = message;
            await ReplyAsync("", false, embedbuilder);
        }

        [Command("termin next")]
        [Summary("Zeigt den nächsten Termin an")]
        public async Task TerminNext()
        {
            EmbedBuilder embedbuilder = new EmbedBuilder();
            embedbuilder.Color = new Color(255, 255, 0);
            embedbuilder.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username
            };

            string message = "";
            List<Termin> termine = dba.getNextTermine();
            embedbuilder.Title = termine.Count > 1 ? "Nächste Termine:" : "Nächster Termin:";

            foreach (Termin termin in termine)
            {
                message += "\n" + termin.ToString();
            }

            embedbuilder.Description = message;
            await ReplyAsync("", false, embedbuilder);
        }

        [Command("termin list all")]
        [Summary("Zeigt alle Termine an")]
        public async Task TerminListAll()
        {
            EmbedBuilder embedbuilder = new EmbedBuilder();
            embedbuilder.Color = new Color(255, 255, 0);
            embedbuilder.Title = "Terminliste:";
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
            await ReplyAsync("", false, embedbuilder);
        }

        [Command("termin remove")]
        [Summary("Entfernt einen Termin")]
        public async Task TerminRemove(params string[] input)
        {
            if(input.Length >= 2 && input.Length <= 3)
            {
                Termin termin;
                if(input.Length == 2)
                {
                    termin = new Termin(input[0], DateTime.Parse(input[1]));
                }
                else
                {
                    termin = new Termin(input[0], DateTime.Parse(input[1]), DateTime.Parse(input[2]));
                }
                bool success = dba.removeTermin(termin);
                if(success)
                {
                    await ReplyAsync("Termin wurde erfolgreich entfernt!");
                }
                else
                {
                    await ReplyAsync("Es gibt keinen Termin mit diesen Daten!");
                }
            }
            else
            {
                await ReplyAsync("=> !termin remove [Bezeichnung] [Startdatum] {Enddatum}");
            }
        }

        [Command("termin add")]
        [Summary("Fügt einen neuen Termin hinzu")]
        public async Task TerminAdd(params string[] input)
        {
            if(input.Length >= 2 && input.Length <= 3)
            {
                Termin termin;
                if (input.Length == 2)
                {
                    termin = new Termin(input[0], DateTime.Parse(input[1]));
                }
                else
                {
                    termin = new Termin(input[0], DateTime.Parse(input[1]), DateTime.Parse(input[2]));
                }
                bool success = dba.addTermin(termin);
                if (success)
                {
                    await ReplyAsync("Termin wurde erfolgreich hinzugefügt!");
                }
                else
                {
                    await ReplyAsync("Es existiert schon ein Termin mit diesen Daten!");
                }
            }
            else
            {
                await ReplyAsync("=> !termin add [Bezeichnung] [Startdatum] {Enddatum}");
            }
        }
    }
}
