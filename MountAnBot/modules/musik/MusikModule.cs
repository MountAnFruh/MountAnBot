using Discord;
using Discord.Commands;
using MountAnBot.core;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules.musik
{
    public class MusikModule : ModuleBase
    {
        private readonly AudioService service = new AudioService();

        private DBAccess dba = DBAccess.getInstance();
        private CommandService commandservice;

        private Random rand = new Random();

        private bool loop;
        private bool mute;
        private string lastSong;

        public MusikModule(AudioService service, CommandService commandservice)
        {
            this.commandservice = commandservice;
            this.service = service;
        }

        public async Task Play(bool _loop, bool random, string parfilename)
        {
            this.loop = _loop;
            if (service.Client != null)
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es wird gerade schon ein Song abgespielt"));
                return;
            }

            IVoiceChannel voiceChan = (Context.User as IVoiceState).VoiceChannel;
            if (voiceChan == null)
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Du bist noch nicht mal in einem Voice-Channel drinnen. Pffft"));
                return;
            }

            await service.JoinAudio(voiceChan);

            do
            {
                List<string> rightFiles = new List<string>();

                string[] files = Directory.GetFiles(service.Musicdirectory);
                string[] parts = null;
                foreach (string file in files)
                {
                    parts = file.Split(Path.DirectorySeparatorChar);
                    if (parts[parts.Length - 1].ToLower().Replace("_"," ").Contains(parfilename.ToLower()))
                    {
                        rightFiles.Add(file);
                    }
                }

                if (random)
                {
                    int index = rand.Next(0, rightFiles.Count);
                    List<string> rightFile = new List<string>();
                    rightFile.Add(rightFiles[index]);
                    rightFiles = rightFile;
                }

                if (rightFiles.Count > 1)
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es gibt mehrere Dateien die diesen Namen beinhalten!"));
                    await service.LeaveAudio();
                    return;
                }
                else if (rightFiles.Count == 0)
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es gibt keine Dateien die diesen Namen beinhalten!"));
                    await service.LeaveAudio();
                    return;
                }

                string[] newParts = rightFiles[0].Split(Path.DirectorySeparatorChar);
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song " + newParts[newParts.Length - 1] + " wird abgespielt..."));
                lastSong = newParts[newParts.Length - 1];

                do
                {
                    try
                    {
                        await service.SendAudioAsync(Context.Channel, rightFiles[0]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                } while (loop && !random);
            } while (loop && random);

            await service.LeaveAudio();

            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song(s) fertig abgespielt"));
        }

        [Command("song help"), Alias("song")]
        [Summary("Gibt dir die Hilfe die du brauchst")]
        public async Task SongHelp()
        {
            string message = "";
            foreach (CommandInfo info in commandservice.Commands)
            {
                if (info.Name.StartsWith("song"))
                {
                    message += "\n**" + info.Name + "** -> " + info.Summary;
                }
            }
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "Alle Song-Commands:", message));
        }

        [Command("song play", RunMode = RunMode.Async)]
        [Summary("Spielt einen Song ab")]
        public async Task SongPlay(params string[] input)
        {
            if(input.Length == 1)
            {
                await Play(false, false, input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "","=> !song play [Name]"));
            }
        }

        [Command("song randomloop", RunMode = RunMode.Async)]
        [Summary("Wiederholt bestimmte Songs, die einen bestimmten Namen beinhalten")]
        public async Task SongRandomLoop(params string[] input)
        {
            if (input.Length <= 1)
            {
                await Play(true, true, input.Length == 0 ? "" : input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song randomloop [Name]"));
            }
        }

        [Command("song random", RunMode = RunMode.Async)]
        [Summary("Wiederholt bestimmte zufällige Songs, die einen bestimmten Namen beinhalten")]
        public async Task SongRandom(params string[] input)
        {
            if (input.Length <= 1)
            {
                await Play(false, true, input.Length == 0 ? "" : input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song random [Name]"));
            }
        }

        [Command("song loop", RunMode = RunMode.Async)]
        [Summary("Wiederholt einen bestimmten Song, der einen bestimmten Namen beinhaltet")]
        public async Task SongLoop(params string[] input)
        {
            if (input.Length == 1)
            {
                await Play(true, false, input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song loop [Name]"));
            }
        }

        [Command("song stop")]
        [Summary("Stoppt den Song")]
        public async Task SongStop()
        {
            loop = false;
            service.StopAudio();
            await service.LeaveAudio();
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song wurde gestoppt"));
        }

        [Command("song skip")]
        [Summary("Überspringt einen Song")]
        public async Task SongSkip()
        {
            service.StopAudio();
            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song wurde übersprungen"));
        }

        [Command("song mute")]
        [Summary("Schaltet die Textausgabe des Songs vom Bot aus")]
        public async Task SongMute()
        {
            if(!mute)
            {
                mute = true;
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Textausgabe ausgeschalten"));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Textausgabe ist schon ausgeschalten"));
            }
        }

        [Command("song unmute")]
        [Summary("Schaltet die Textausgabe des Songs vom Bot ein")]
        public async Task SongUnmute()
        {
            if (mute)
            {
                mute = false;
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Textausgabe eingeschalten"));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Textausgabe ist schon eingeschalten"));
            }
        }

        [Command("song last")]
        [Summary("Zeigt den letzten gespielten Song an")]
        public async Task SongLast()
        {
            if (!lastSong.Equals(""))
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Der letzte Song der abgespielt worden ist, ist " + lastSong));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es gibt keinen zuletzt abgespielten Song"));
            }
        }
    }
}
