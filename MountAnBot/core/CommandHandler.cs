﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MountAnBot.database;
using MountAnBot.modules.musik;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MountAnBot.core
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IDependencyMap map;
        private DBAccess dba = DBAccess.getInstance();
        private AudioService audioService;

        public async Task Install(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();

            audioService = new AudioService();
            _map.Add(audioService);

            //_map.Add(commands);
            map = _map;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
            client.UserVoiceStateUpdated += HandleVoiceState;

            Console.WriteLine("CommandHandler erfolgreich installiert!");
        }

        public async Task HandleVoiceState(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter)
        {

            IGuildUser guildUser = stateBefore.VoiceChannel.Guild.GetUser(client.CurrentUser.Id);
            int count = (guildUser.VoiceChannel as SocketVoiceChannel).Users.Count;
            if(count == 1)
            {
                audioService.StopAudio();
                await audioService.LeaveAudio();
            }
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            SocketUserMessage message = parameterMessage as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos)
               || message.HasCharPrefix('!', ref argPos))) return;

            CommandContext context = new CommandContext(client, message);

            IResult result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await message.Channel.SendMessageAsync("**ERROR:** " + result.ErrorReason);
            }
        }
    }
}
