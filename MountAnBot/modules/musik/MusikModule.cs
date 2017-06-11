using Discord;
using Discord.Commands;
using MountAnBot.core;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules.musik
{
    [Name("Music")]
    public class MusikModule : ModuleBase
    {
        private readonly AudioService service = new AudioService();

        private DBAccess dba = DBAccess.getInstance();
        private CommandService commandservice;

        private Random rand = new Random();

        private bool loop;

        public MusikModule(AudioService service, CommandService commandservice)
        {
            this.commandservice = commandservice;
            this.service = service;
        }

        public async Task Stream(bool _loop, string url)
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

            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Es wird versucht den Stream abzuspielen ..."));
            do
            {
                try {
                    await service.SendAudioAsync(Context.Channel, url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (loop);
            await service.LeaveAudio();
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

                string[] files = Directory.GetFiles(dba.getSetting("musicdirectory"));
                string[] parts = null;
                foreach (string file in files)
                {
                    parts = file.Split(Path.DirectorySeparatorChar);
                    if (parts[parts.Length - 1].ToLower().Replace("_", " ").Contains(parfilename.ToLower()))
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
                if (!service.Mute)
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song " + newParts[newParts.Length - 1] + " wird abgespielt..."));
                }

                service.Lastsong = newParts[newParts.Length - 1];

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

        [Command("song youtube play", RunMode = RunMode.Async)]
        [Summary("Spielt einen Youtube-Song ab")]
        public async Task SongYoutubePlay(params string[] input)
        {
            if(input.Length == 1)
            {
                string youtubedlPath = dba.getSetting("youtubedlsource");
                string youtubeCmd = "-f bestaudio -g \"" + input[0] + "\"";
                Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = youtubedlPath,
                    Arguments = youtubeCmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });
                while (!process.StandardOutput.EndOfStream)
                {
                    service.Lastsong = input[0];
                    string line = process.StandardOutput.ReadLine();
                    await Stream(false,line);
                }
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song youtube play [Youtube-URL]"));
            }
        }

        [Command("song youtube loop", RunMode = RunMode.Async)]
        [Summary("Wiederholt einen Youtube-Song")]
        public async Task SongYoutubeLoop(params string[] input)
        {
            if (input.Length == 1)
            {
                //string youtubedlPath = dba.getSetting("youtubedlsource");
                //string youtubeCmd = "-f bestaudio -g \"" + input[0] + "\"";
                //Process process = Process.Start(new ProcessStartInfo
                //{
                //    FileName = youtubedlPath,
                //    Arguments = youtubeCmd,
                //    UseShellExecute = false,
                //    RedirectStandardOutput = true
                //});
                //loop = true;
                //while (!process.StandardOutput.EndOfStream)
                //{
                //    string line = process.StandardOutput.ReadLine();
                //    await Stream(true, line);
                //}
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "Funktioniert noch nich :(("));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song youtube random [Youtube-URL]"));
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

        [Command("song stream", RunMode = RunMode.Async)]
        [Summary("Spielt einen Stream ab (wenn möglich)")]
        public async Task SongStream(params string[] input)
        {
            if (input.Length == 1)
            {
                service.Lastsong = input[0];
                await Stream(false, input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song stream [URL/File/etc]"));
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
            if(!service.Mute)
            {
                service.Mute = true;
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Textausgabe des Songs ausgeschalten"));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Textausgabe des Songs ist schon ausgeschalten"));
            }
        }

        [Command("song unmute")]
        [Summary("Schaltet die Textausgabe des Songs vom Bot ein")]
        public async Task SongUnmute()
        {
            if (service.Mute)
            {
                service.Mute = false;
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Textausgabe des Songs eingeschalten"));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Textausgabe des Songs ist schon eingeschalten"));
            }
        }

        [Command("song last")]
        [Summary("Zeigt den letzten gespielten Song an")]
        public async Task SongLast()
        {
            if (!service.Lastsong.Equals(""))
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Der letzte Song der abgespielt worden ist, ist \"" + service.Lastsong + "\""));
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es gibt keinen zuletzt abgespielten Song"));
            }
        }
    }
}
