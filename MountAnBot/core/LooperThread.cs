using Discord;
using Discord.WebSocket;
using MountAnBot.database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MountAnBot.core
{
    public class LooperThread
    {
        private Thread thread;

        private DiscordSocketClient client;

        private DateTime currDate = DateTime.Now;
        private DBAccess dba = DBAccess.getInstance();

        public LooperThread(DiscordSocketClient client)
        {
            this.client = client;
        }

        public void Stop()
        {
            thread.Interrupt();
            Console.WriteLine("LooperThread wurde ausgeschalten!");
        }

        public void Start()
        {
            thread = new Thread(this.Loop);
            thread.Start();
            Console.WriteLine("LooperThread wurde eingeschalten!");
        }

        private void Loop()
        {
            Thread.Sleep(5000);
            while(thread.IsAlive)
            {

                if(currDate.Date != DateTime.Now.Date)
                {
                    // DO SOMETHING WHEN NEXT DAY
                    if(client.ConnectionState == Discord.ConnectionState.Connected)
                    {
                        SocketChannel channel = client.GetChannel(ulong.Parse(dba.getSetting("terminchannel")));
                        IMessageChannel messChannel = (IMessageChannel) channel;
                        messChannel.SendMessageAsync("!termin next");
                    }

                    currDate = DateTime.Now.Date;
                }

                Thread.Sleep(500);

            }
        }
    }
}
