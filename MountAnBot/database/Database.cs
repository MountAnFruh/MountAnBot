using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountAnBot.database
{
    public class Database
    {
        public NpgsqlConnection Connection { get { return connection; } }
        public string Name { get { return name; } }

        private static Database instance;
        private NpgsqlConnection connection;
        private string name;

        public static Database getInstance()
        {
            if (instance == null)
            {
                instance = new Database();
            }

            return instance;
        }

        private Database()
        {
            name = "discordbotdb";
        }

        public void connect()
        {
            connection = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres;Password=postgres;Database=" + name + ";");
            connection.Open();
        }

        public void disconnect()
        {
            connection.Close();
        }
    }
}
