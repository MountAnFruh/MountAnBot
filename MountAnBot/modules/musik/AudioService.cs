﻿using Discord;
using Discord.Audio;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MountAnBot.modules.musik
{
    public class AudioService
    {
        public IAudioClient Client { get { return client; } }
        public string Musicdirectory { get { return musicdirectory; } }
        public string Ffmpegsource { get { return ffmpegsource; } }
        public string Lastsong { get { return lastSong; } }
        public bool Mute { get { return mute; } set { mute = value; } }

        private CancellationTokenSource cancel = new CancellationTokenSource();
        private IAudioClient client;
        private DBAccess dba = DBAccess.getInstance();
        private Stream output;
        private Process process;

        private string musicdirectory, ffmpegsource;
        private string lastSong = "";
        private bool mute;

        public AudioService()
        {
            musicdirectory = dba.getSetting("musicdirectory");
            ffmpegsource = dba.getSetting("ffmpegsource");
        }

        public async Task JoinAudio(IVoiceChannel target)
        {
            if (client != null)
            {
                return;
            }

            try
            {
                client = await target.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (client != null)
            {
                Console.WriteLine("Connected to voice.");
            }
        }

        public async Task LeaveAudio()
        {
            if (client != null)
            {
                await client.StopAsync();
                client = null;
            }
        }

        public async Task SendAudioAsync(IMessageChannel channel, string path)
        {
            // Your task: Get a full path to the file if the value of 'path' is only a filename.

            if (client != null)
            {
                await Task.Delay(2000);
                string[] newParts = path.Split(Path.DirectorySeparatorChar);
                lastSong = newParts[newParts.Length - 1];
                Console.WriteLine($"Starting playback of {path}");
                process = CreateStream(path);
                output = process.StandardOutput.BaseStream;
                AudioOutStream stream = client.CreatePCMStream(AudioApplication.Music, 1920);
                try
                {
                    await output.CopyToAsync(stream, 81920, cancel.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Playback of {path} cancelled");
                }
                finally
                {
                    await stream.FlushAsync().ConfigureAwait(false);
                }
            }
        }

        public void StopAudio()
        {
            cancel.Cancel();
            cancel.Dispose();
            cancel = new CancellationTokenSource();
            if (process != null)
            {
                process.Kill();
                process = null;
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = ffmpegsource,
                Arguments = $"-loglevel panic -i \"{path}\" -ac 2 -af \"volume=0.15\" -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
