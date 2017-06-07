using Discord;
using Discord.Commands;
using MountAnBot.core;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules
{
    [Name("Settings")]
    public class SettingsModule : ModuleBase
    {
        private DBAccess dba = DBAccess.getInstance();
        private CommandService commandservice;

        public SettingsModule(CommandService commandservice)
        {
            this.commandservice = commandservice;
        }

        [Command("settings update")]
        [Summary("Ändert die Einstellung | Nur für Autorisierte!")]
        public async Task SettingsUpdate(params string[] input)
        {
            if (dba.isAuthorized("authorizedsettingsroles", (IGuildUser)Context.User))
            {
                if(input.Length == 2)
                {
                    string description = input[0];
                    string oldValue = dba.getSetting(description);
                    if(oldValue != null)
                    {
                        dba.setSetting(description, input[1]);
                        await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Die Einstellungen wurden geändert!"));
                    }
                    else
                    {
                        await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255,0,0), Context.User, "", "Es gibt keine Einstellung mit dieser Beschreibung!"));
                    }
                }
                else
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !settings update [Beschreibung] [Wert]"));
                }
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.WEAKMESSAGE));
            }
        }

        [Command("settings list")]
        [Summary("Liefert alle Beschreibungen mit deren Werten zurück | Nur für Autorisierte!")]
        public async Task SettingsDescriptions()
        {
            if (dba.isAuthorized("authorizedsettingsroles", (IGuildUser)Context.User))
            {
                string message = "";
                Dictionary<string, string> dict = dba.getSettings();
                foreach (string key in dict.Keys)
                {
                    message += "\n+ " + key + " = " + dict[key];
                }
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "Settingsliste:", message));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.WEAKMESSAGE));
            }
        }

        [Command("settings help"), Alias("settings")]
        [Summary("Gibt dir die Hilfe die du brauchst | Nur für Autorisierte!")]
        public async Task SongHelp()
        {
            if (dba.isAuthorized("authorizedsettingsroles", (IGuildUser)Context.User))
            {
                string message = "";
                foreach (CommandInfo info in commandservice.Commands)
                {
                    if (info.Name.StartsWith("settings"))
                    {
                        message += "\n**" + info.Name + "** -> " + info.Summary;
                    }
                }
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "Alle Settings-Commands:", message));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.WEAKMESSAGE));
            }
        }

    }
}
