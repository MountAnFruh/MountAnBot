using System;
using System.Collections.Generic;
using System.Text;

namespace MountAnBot.beans
{
    public class Termin
    {
        public string Bezeichnung { get { return bezeichnung; } }
        public DateTime Vondate { get { return vondate; } }
        public DateTime Bisdate { get { return bisdate; } }

        private string bezeichnung;
        private DateTime vondate;
        private DateTime bisdate;

        public Termin(string bezeichnung, DateTime vondate, DateTime bisdate)
        {
            this.bezeichnung = bezeichnung;
            this.vondate = vondate;
            this.bisdate = bisdate;
        }

        public Termin(string bezeichnung, DateTime date)
        {
            this.bezeichnung = bezeichnung;
            this.vondate = date;
            this.bisdate = date;
        }

        public override string ToString()
        {
            string bisdateStr = "";
            if (!bisdate.Date.Equals(vondate.Date))
            {
                bisdateStr = " - " + bisdate.Day + "." + bisdate.Month + "." + bisdate.Year;
            }
            return "__**" + vondate.Day + "." + vondate.Month + "." + vondate.Year + bisdateStr + "**__: " + bezeichnung;
        }

        public int CompareTo(Termin ter)
        {
            return vondate.CompareTo(ter.vondate);
        }
    }
}
