using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MountAnBot.database;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MountAnBot.core
{
    public class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    return false;
            }
        }

        public static void Main(string[] args)
        {
            new Program().Start().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;
        private DBAccess dba = DBAccess.getInstance();
        private CommandHandler handler;

        public async Task Start()
        {
            try
            {
                dba.connect();

                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);

                client = new DiscordSocketClient();

                await client.LoginAsync(TokenType.Bot, dba.getSetting("bottoken"));
                await client.StartAsync();

                Console.WriteLine("Bot ist online!");

                //await client.SetGameAsync("Rainbow Six Siege");

                DependencyMap map = new DependencyMap();
                map.Add(client);

                handler = new CommandHandler();
                await handler.Install(map);

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ProcessExit(object sender, EventArgs x)
        {
            dba.disconnect();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}