using MountAnBot.beans;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MountAnBot.database
{
    public class DBAccess
    {
        private static DBAccess instance;
        private Database database;

        public static DBAccess getInstance()
        {
            if (instance == null)
            {
                instance = new DBAccess();
            }

            return instance;
        }

        private DBAccess()
        {
            database = Database.getInstance();
        }

        public void connect()
        {
            database.connect();
            Console.WriteLine("Connected to database " + database.Name + "!");
        }

        public void disconnect()
        {
            database.disconnect();
            Console.WriteLine("Disconnected from database " + database.Name + "!");
        }

        public string getSetting(string description)
        {
            string sqlString = "SELECT MAX(value) FROM settings WHERE description = '" + description + "';";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            string value = command.ExecuteScalar().ToString();
            return value;
        }

        public void setSetting(string description, string value)
        {
            string sqlString = "UPDATE settings SET value = '" + value + "' WHERE description = '" + description + "';";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            int rows = command.ExecuteNonQuery();
            if (rows == 0)
            {
                sqlString = "INSERT INTO settings (description, value) VALUES ('" + description + "', '" + value + "';";
                command = new NpgsqlCommand(sqlString, database.Connection);
                command.ExecuteNonQuery();
            }
        }

        public List<Termin> getAllZukTermine()
        {
            List<Termin> termine = new List<Termin>();
            string sqlString = "SELECT * FROM termin WHERE vondatum > current_date ORDER BY vondatum;";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            NpgsqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                Termin termin = new Termin("" + dr[0], DateTime.Parse("" + dr[1]), DateTime.Parse("" + dr[2]));
                termine.Add(termin);
            }
            dr.Close();
            return termine;
        }

        public List<Termin> getAllTermine()
        {
            List<Termin> termine = new List<Termin>();
            string sqlString = "SELECT * FROM termin ORDER BY vondatum;";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            NpgsqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                Termin termin = new Termin("" + dr[0], DateTime.Parse("" + dr[1]), DateTime.Parse("" + dr[2]));
                termine.Add(termin);
            }
            dr.Close();
            return termine;
        }

        public List<Termin> getNextTermine()
        {
            List<Termin> termine = new List<Termin>();
            string sqlString = "SELECT * FROM termin WHERE vondatum >= current_date AND (vondatum <= current_date + interval '5' day OR vondatum <= current_date + interval '5' day) ORDER BY vondatum";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            NpgsqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                Termin termin = new Termin("" + dr[0], DateTime.Parse("" + dr[1]), DateTime.Parse("" + dr[2]));
                termine.Add(termin);
            }
            dr.Close();
            return termine;
        }

        public bool removeTermin(Termin termin)
        {
            string sqlString = "DELETE FROM termin WHERE bezeichnung = '" + termin.Bezeichnung + "' AND vondatum = '" +
                                termin.Vondate.ToString("yyyy-MM-dd") + "' AND bisdatum = '" +
                                termin.Bisdate.ToString("yyyy-MM-dd") + "';";
            NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
            int rows = command.ExecuteNonQuery();
            if(rows == 0)
            {
                return false;
            }
            return true;
        }

        public bool addTermin(Termin termin)
        {
            try
            {
                string sqlString = "INSERT INTO termin(bezeichnung, vondatum, bisdatum) VALUES ('" + termin.Bezeichnung + "','" +
                                    termin.Vondate.ToString("yyyy-MM-dd") + "','" +
                                    termin.Bisdate.ToString("yyyy-MM-dd") + "');";
                NpgsqlCommand command = new NpgsqlCommand(sqlString, database.Connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch(NpgsqlException ex)
            {
                if(ex.ErrorCode == -2147467259)
                {
                    return false;
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
