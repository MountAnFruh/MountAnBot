using System;
using System.Collections.Generic;
using System.Text;

namespace MountAnBot.beans
{
    public class Youtubeurl
    {
        public string Title { get { return title; } }
        public string Url { get { return "https://www.youtube.com/watch?v=" + id; } }

        private string title;
        private string id;

        public Youtubeurl(string title, string id, string url)
        {
            this.title = title;
            this.id = id;
        }
    }
}
