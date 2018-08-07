using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public DbModel(DbProviderFactory factory)
        {
            _factory = factory;
            _connection = _factory.CreateConnection();
            _connection.ConnectionString = ConfigurationManager.ConnectionStrings["CityQDB"].ConnectionString;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
