using Discord;
using Discord.Audio;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.modules.musik
{
    public class AudioService
    {
        public IAudioClient Client { get { return client; } }
        public string Musicdirectory { get { return musicdirectory; } }
        public string Ffmpegsource { get { return ffmpegsource; } }

        private IAudioClient client;
        private DBAccess dba = DBAccess.getInstance();

        private string musicdirectory, ffmpegsource;

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
                Console.WriteLine($"Starting playback of {path}");
                Stream output = CreateStream(path).StandardOutput.BaseStream;
                AudioOutStream stream = client.CreatePCMStream(AudioApplication.Music, 1920);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = ffmpegsource,
                Arguments = $"-i \"{path}\" -af \"volume=0.15\" -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
