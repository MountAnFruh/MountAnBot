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
            builder.Title = title.Equals("") ? "" : "**" + title + "**";
            builder.Description = description;
            builder.Footer = new EmbedFooterBuilder()
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy - hh:mm:ss")
            };
            return builder;
        }

        public static EmbedBuilder create(Color color, IUser user, string title, string description, string codeLanguage)
        {
            EmbedBuilder builder = MountEmbedBuilder.create(color, user, title, "```" + codeLanguage + "\n" + description + "```");
            return builder;
        }

    }
}
