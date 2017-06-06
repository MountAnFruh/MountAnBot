using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MountAnBot.database;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MountAnBot.core
{
    public class Program
    {

        public static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
            Program program = new Program();
            program.Start().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;
        private DBAccess dba = DBAccess.getInstance();
        private CommandHandler handler;

        public async Task Start()
        {
            try
            {
                dba.connect();

                client = new DiscordSocketClient();

                await client.LoginAsync(TokenType.Bot, dba.getSetting("bottoken"));
                await client.StartAsync();

                Console.WriteLine("Bot ist online!");

                //await client.SetGameAsync("Rainbow Six Siege");

                DependencyMap map = new DependencyMap();
                map.Add(client);

                handler = new CommandHandler();
                await handler.Install(map);

                LooperThread looper = new LooperThread(client);
                looper.Start();

                await Task.Delay(-1);

                looper.Stop();
                dba.disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //private void ProcessExit(object sender, EventArgs x)
        //{
        //    dba.disconnect();
        //}

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}