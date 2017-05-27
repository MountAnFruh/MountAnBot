using MountAnBot.beans;
using Npgsql;
using System;
using System.Collections.Generic;
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

        public IEnumerable<Termin> getAllTermine()
        {
            List<Termin> termine = new List<Termin>();
            string sqlString = "SELECT * FROM termin;";
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
    }
}
