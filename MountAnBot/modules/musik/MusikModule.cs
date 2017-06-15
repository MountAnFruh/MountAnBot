using Discord;
using Discord.Commands;
using MountAnBot.beans;
using MountAnBot.core;
using MountAnBot.database;
using Newtonsoft.Json;
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

        public async Task Stream(String url)
        {
            try
            {
                await service.SendAudioAsync(Context.Channel, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task PlayYT(String input, bool _loop)
        {
            if (service.Process != null)
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es wird gerade schon ein Song abgespielt"));
                return;
            }

            IVoiceChannel voiceChan = (Context.User as IVoiceState).VoiceChannel;
            if (voiceChan == null)
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", MountEmbedBuilder.NOAUDIOCHANNEL));
                return;
            }

            await service.JoinAudio(voiceChan);

            string youtubedlPath = dba.getSetting("youtubedlsource");
            string youtubeCmdOuter = "-j --flat-playlist \"" + input + "\"";

            Process processOuter = Process.Start(new ProcessStartInfo
            {
                FileName = youtubedlPath,
                Arguments = youtubeCmdOuter,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            while (!processOuter.StandardOutput.EndOfStream)
            {
                string youtubeUrl = processOuter.StandardOutput.ReadLine();
                Youtubeurl yturl = JsonConvert.DeserializeObject<Youtubeurl>(youtubeUrl);
                string youtubeCmd = "-f bestaudio -g \"" + yturl.Url + "\"";
                Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = youtubedlPath,
                    Arguments = youtubeCmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                service.Lastsong = yturl.Title;
                loop = _loop;

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    do
                    {
                        if (!service.Mute)
                        {
                            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Es wird versucht den Youtube-Song \"" + yturl.Title + "\" abzuspielen ..."));
                        }

                        await Stream(line);
                    } while (loop);
                }

                if (!process.StandardError.EndOfStream)
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Inner-Error: " + process.StandardError.ReadToEnd()));
                }

            }

            if(!processOuter.StandardError.EndOfStream)
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Outer-Error: " + processOuter.StandardError.ReadToEnd()));
            }

            await service.LeaveAudio();
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

        [Command("song youtube playlist", RunMode = RunMode.Async)]
        [Summary("Spielt eine Youtube-Playlist ab")]
        public async Task SongYoutubePlaylist(params string[] input)
        {
            if (input.Length == 1)
            {
                if (input[0].StartsWith("https://www.youtube.com/") || input[0].StartsWith("https://youtu.be/"))
                {
                    await PlayYT(input[0], false);
                }
                else
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Ernsthaft, das ist keine Youtube-Playlist-URL! Geh nach Hause!"));
                }
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song youtube playlist [Youtube-Playlist-URL]"));
            }
        }

        [Command("song youtube play", RunMode = RunMode.Async)]
        [Summary("Spielt einen Youtube-Song ab (keine Streams!)")]
        public async Task SongYoutubePlay(params string[] input)
        {
            if (input.Length == 1)
            {
                if (input[0].StartsWith("https://www.youtube.com/") || input[0].StartsWith("https://youtu.be/"))
                {
                    await PlayYT(input[0], false);
                }
                else
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Ernsthaft, das ist keine Youtube-URL! Geh nach Hause!"));
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
                if (input[0].StartsWith("https://www.youtube.com/") || input[0].StartsWith("https://youtu.be/"))
                {
                    await PlayYT(input[0], true);
                }
                else
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Ernsthaft, das ist keine Youtube-URL! Geh nach Hause!"));
                }
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song youtube loop [Youtube-URL]"));
            }
        }

        [Command("song stream")]
        [Summary("Spielt eine Stream-URL ab")]
        public async Task SongStream(params string[] input)
        {
            if (input.Length == 1)
            {
                await Stream(input[0]);
            }
            else
            {
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(0, 255, 0), Context.User, "", "=> !song stream [Stream-URL]"));
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
            if (!service.Mute)
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
