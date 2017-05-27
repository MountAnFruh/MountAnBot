﻿using Discord;
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

        private Random rand = new Random();

        public MusikModule(AudioService service)
        {
            this.service = service;
        }

        public async Task Play(bool loop, bool random, string parfilename)
        {
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

            do
            {
                List<string> rightFiles = new List<string>();

                string[] files = Directory.GetFiles(service.Musicdirectory);
                string[] parts = null;
                foreach (string file in files)
                {
                    parts = file.Split(Path.DirectorySeparatorChar);
                    if (parts[parts.Length - 1].ToLower().Contains(parfilename.ToLower()))
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
                    return;
                }
                else if (rightFiles.Count == 0)
                {
                    await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 0, 0), Context.User, "", "Es gibt keine Dateien die diesen Namen beinhalten!"));
                    return;
                }

                string[] newParts = rightFiles[0].Split(Path.DirectorySeparatorChar);
                await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song " + newParts[newParts.Length - 1] + " wird abgespielt..."));

                await service.JoinAudio(voiceChan);

                do
                {
                    await service.SendAudioAsync(Context.Channel, rightFiles[0]);
                } while (loop && !random);
            } while (loop && random);

            await service.LeaveAudio();

            await ReplyAsync("", false, MountEmbedBuilder.create(new Color(255, 255, 0), Context.User, "", "Song(s) fertig abgespielt"));
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
            await service.LeaveAudio();
        }

        [Command("song skip")]
        [Summary("Überspringt einen Song")]
        public async Task SongSkip()
        {
            await service.LeaveAudio();
        }
    }
}
