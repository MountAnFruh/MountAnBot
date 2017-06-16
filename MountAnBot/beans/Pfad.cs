using MountAnBot.core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountAnBot.beans
{
    public class Pfad
    {

        public string Start_addresse { get { return start_adresse; } }
        public string End_adresse { get { return end_adresse; } }
        public double Entfernung { get { return entfernung; } }

        private string start_adresse;
        private string end_adresse;
        private double entfernung;

        public Pfad(string _start_adresse, string _end_adresse)
        {
            System.Threading.Thread.Sleep(1000);
            //string from = origin.Text;
            //string to = destination.Text;
            string url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + _start_adresse + "&destination=" + _end_adresse + "&sensor=false";
            string requesturl = url;
            //string requesturl = @"http://maps.googleapis.com/maps/api/directions/json?origin=" + from + "&alternatives=false&units=imperial&destination=" + to + "&sensor=false";
            string content = fileGetContents(requesturl);
            JObject o = JObject.Parse(content);
            try
            {
                entfernung = (double)o.SelectToken("routes[0].legs[0].distance.value") / 1000.0;
                start_adresse = (string)o.SelectToken("routes[0].legs[0].start_address");
                end_adresse = (string)o.SelectToken("routes[0].legs[0].end_address");
            }
            catch
            {
                entfernung = 0.0;
            }
        }

        private string fileGetContents(string fileName)
        {
            string sContents = string.Empty;
            string me = string.Empty;
            try
            {
                if (fileName.ToLower().IndexOf("http:") > -1)
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    byte[] response = wc.DownloadData(fileName);
                    sContents = System.Text.Encoding.ASCII.GetString(response);

                }
                else
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
                    sContents = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch { sContents = "unable to connect to server "; }
            return sContents;
        }
    }
}
