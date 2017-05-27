using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountAnBot.core
{
    public class MountEmbedBuilder
    {
        public static EmbedBuilder create(Color color, IUser user, string title, string description)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = color;
            builder.Author = new EmbedAuthorBuilder()
            {
                Name = user.Username,
                IconUrl = user.GetAvatarUrl()
            };
            builder.Title = "**" + title + "**";
            builder.Description = description;
            return builder;
        }
    }
}
