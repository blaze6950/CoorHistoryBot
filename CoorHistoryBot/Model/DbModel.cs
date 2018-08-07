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

        public void AddNewPlace(Place newPlace)
        {
            InsertAddress(newPlace.Location, newPlace.Caption, newPlace.User);
            InsertPhotos(newPlace.Photos, GetAddressId(newPlace));
        }

        private long GetAddressId(Place newPlace)
        {
            long id = -1;
            string commandText = $"SELECT Id FROM Addresses WHERE Longitude = '{(double)(newPlace.Location.Longitude)}' AND Latitude = '{(double)(newPlace.Location.Latitude)}' AND Caption = '{newPlace.Caption}'";
            DbDataReader reader = null;
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    id = (long)reader["Id"];
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
            if (id == -1)
            {
                throw new Exception("Could not get id address");
            }
            return id;
        }

        private void InsertPhotos(List<byte[]> newPlacePhotos, long addressId)
        {
            byte[] lastPhotoBytes = newPlacePhotos[newPlacePhotos.Count - 1];
            newPlacePhotos.Remove(lastPhotoBytes);

            var command = _factory.CreateCommand();
            command.Connection = _connection;

            command.CommandText = $"INSERT INTO Photos(Photo, Address_Id) VALUES ('{lastPhotoBytes}', '{addressId}')";
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (newPlacePhotos.Count > 0)
            {
                InsertPhotos(newPlacePhotos, addressId);
            }
        }

        private void InsertAddress(Location newPlaceLocation, StringBuilder newPlaceCaption, User newPlaceUser)
        {
            long userId = GetUserId(newPlaceUser.Username);


            var command = _factory.CreateCommand();
            command.Connection = _connection;

            command.CommandText = $"INSERT INTO Addresses(Latitude, Longitude, Caption, User_Id) VALUES ('{(double)(newPlaceLocation.Latitude)}', '{(double)(newPlaceLocation.Longitude)}', '{newPlaceCaption.ToString()}', '{userId}')";
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private long GetUserId(string username)
        {
            long id = -1;
            string commandText = $"SELECT Id FROM Users WHERE Username = '{username}'";
            DbDataReader reader = null;
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    id = (long) reader["Id"];
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
            if (id == -1)
            {
                AddNewUser(username);
                id = GetUserId(username);
            }
            return id;
        }

        private void AddNewUser(string username)
        {
            var command = _factory.CreateCommand();
            command.Connection = _connection;

            command.CommandText = $"INSERT INTO Users VALUES ('' ,'{username}')";
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
