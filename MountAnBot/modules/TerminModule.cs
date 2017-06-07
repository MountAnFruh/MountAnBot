using Discord;
using Discord.Commands;
using MountAnBot.beans;
using MountAnBot.core;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        [Command("termin help"), Alias("termin")]
        [Summary("Damit bekommst du deine Hilfe die du brauchst")]
        public async Task TerminHelp()
        {
            string message = "";
            foreach (CommandInfo info in service.Commands)
            {
                if(info.Name.StartsWith("termin"))
                {
                    message += "\n**" + info.Name + "** -> " + info.Summary;
                }
            }
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "Alle Termin-Commands:", message));
        }

        [Command("termin list")]
        [Summary("Zeigt alle zukünftigen Termine an")]
        public async Task TerminList()
        {
            string message = "";
            foreach (Termin termin in dba.getAllZukTermine())
            {
                message += "\n" + termin.ToString();
            }
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255,255,0),Context.User,"Terminliste:",message, "diff"));
        }

        [Command("termin next")]
        [Summary("Zeigt den nächsten Termin an")]
        public async Task TerminNext()
        {
            string message = "";
            List<Termin> termine = dba.getNextTermine();
            string title = termine.Count > 1 ? "Nächste Termine:" : "Nächster Termin:";

            foreach (Termin termin in termine)
            {
                message += "\n" + termin.ToString();
            }
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255,255,0),Context.User,title,message,"diff"));
        }

        [Command("termin list all")]
        [Summary("Zeigt alle Termine an")]
        public async Task TerminListAll()
        {
            string message = "";
            foreach (Termin termin in dba.getAllTermine())
            {
                message += "\n" + termin.ToString();
            }
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255,255,0),Context.User,"Terminliste:",message, "diff"));
        }

        [Command("termin remove")]
        [Summary("Entfernt einen Termin | Nur für Autorisierte!")]
        public async Task TerminRemove(params string[] input)
        {
            if (dba.isAuthorized("authorizedterminroles", (IGuildUser)Context.User))
            {
                Color color = new Color(255, 255, 0);
                string message = "ERROR";

                if (input.Length >= 2 && input.Length <= 3)
                {
                    Termin termin;
                    if (input.Length == 2)
                    {
                        termin = new Termin(input[0], input[1]);
                    }
                    else
                    {
                        termin = new Termin(input[0], input[1], input[2]);
                    }
                    bool success = dba.removeTermin(termin);
                    if (success)
                    {
                        message = "Termin wurde erfolgreich entfernt!";
                    }
                    else
                    {
                        color = new Color(255, 0, 0);
                        message = "Es gibt keinen Termin mit diesen Daten!";
                    }
                }
                else
                {
                    color = new Color(0, 255, 0);
                    message = "=> !termin remove [Bezeichnung] [Startdatum] {Enddatum}";
                }
                await ReplyAsync("", false, MountEmbedBuilder.create(color, Context.User, "", message));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.WEAKMESSAGE));
            }
        }

        [Command("termin add")]
        [Summary("Fügt einen neuen Termin hinzu | Nur für Autorisierte!")]
        public async Task TerminAdd(params string[] input)
        {
            if (dba.isAuthorized("authorizedterminroles", (IGuildUser)Context.User))
            {
                Color color = new Color(255, 255, 0);
                string message = "ERROR";

                try
                {
                    if (input.Length >= 2 && input.Length <= 3)
                    {
                        Termin termin;
                        if (input.Length == 2)
                        {
                            termin = new Termin(input[0], input[1]);
                        }
                        else
                        {
                            termin = new Termin(input[0], input[1], input[2]);
                        }
                        bool success = dba.addTermin(termin);
                        if (success)
                        {
                            message = "Termin wurde erfolgreich hinzugefügt!";
                        }
                        else
                        {
                            color = new Color(255, 0, 0);
                            message = "Es existiert schon ein Termin mit diesen Daten!";
                        }
                    }
                    else
                    {
                        color = new Color(0, 255, 0);
                        message = "=> !termin add [Bezeichnung] [Startdatum] {Enddatum}";
                    }
                }
                catch (FormatException)
                {
                    color = new Color(255, 0, 0);
                    message = "**FORMATEXCEPTION:** Das Format des Datums ist falsch!";
                }
                await ReplyAsync("", false, MountEmbedBuilder.create(color, Context.User, "", message));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.WEAKMESSAGE));
            }
        }
    }
}
