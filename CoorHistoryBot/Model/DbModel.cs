using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CoorHistoryBot.Model
{
    class DbModel : IDisposable
    {
        private DbConnection _connection;
        private DbProviderFactory _factory;

        public DbModel()
        {
            _factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings["CityQDB"].ProviderName);
            _connection = _factory.CreateConnection();
            _connection.ConnectionString = ConfigurationManager.ConnectionStrings["CityQDB"].ConnectionString;
            _connection.Open();
        }

        public DbModel(DbProviderFactory factory)
        {
            _factory = factory;
            _connection = _factory.CreateConnection();
            _connection.ConnectionString = ConfigurationManager.ConnectionStrings["CityQDB"].ConnectionString;
            _connection.Open();
        }

        public List<String> GetUserList()
        {
            List<String> newUserList = new List<string>();
            string commandText = "SELECT * FROM Users";
            DbDataReader reader = null;
            try
            {
                _connection.Open();
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newUserList.Add((string)reader["Username"]);
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            return newUserList;
        }

        public List<Place> GetPlaces()
        {
            List<Place> newPlaceList = new List<Place>();
            string commandText = "SELECT * FROM Addresses";
            DbDataReader reader = null;
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newPlaceList.Add(
                        new Place(
                            new Location()
                            {
                                Latitude = (float)((double)reader["Latitude"]),
                                Longitude = (float)((double)reader["Longitude"])
                            }, new StringBuilder((string)reader["Caption"]))
                        { Id = (long)reader["Id"] });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            return newPlaceList;
        }

        public List<byte[]> GetPhotoList(int idPlace)
        {
            List<byte[]> newPhotoList = new List<byte[]>();
            string commandText = $"SELECT * FROM Photos WHERE Address_Id = '{idPlace}'";
            DbDataReader reader = null;
            try
            {
                _connection.Open();
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newPhotoList.Add((byte[])reader["Photo"]);
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            return newPhotoList;
        }

        public List<Place> GetModPlaces()
        {
            List<Place> newPlaceList = new List<Place>();
            string commandText = "SELECT * FROM ModAddresses";
            DbDataReader reader = null;
            try
            {
                _connection.Open();
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newPlaceList.Add(new Place(new Location() { Latitude = (float)reader["Latitude"], Longitude = (float)reader["Longitude"] }, new StringBuilder((string)reader["Caption"])) { Id = (int)reader["Id"] });
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            return newPlaceList;
        }

        public List<byte[]> GetModPhotoList(int idPlace)
        {
            List<byte[]> newPhotoList = new List<byte[]>();
            string commandText = $"SELECT * FROM ModPhotos WHERE Address_Id = '{idPlace}'";
            DbDataReader reader = null;
            try
            {
                _connection.Open();
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newPhotoList.Add((byte[])reader["Photo"]);
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            return newPhotoList;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
