using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;

namespace STRHM.Tests
{
    /// <summary>
    /// Singleton
    /// </summary>
    public class RedisConnection
    {
        private readonly object _connectionInitLock = new object();
        private ConnectionMultiplexer _connection;
        private readonly string _connectionString;

        public RedisConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                if (_connection == null || !_connection.IsConnected)
                {
                    lock (_connectionInitLock)
                    {
                            _connection?.Close();
                            _connection = ConnectionMultiplexer.Connect(_connectionString);
                    }
                }

                return _connection;
            }
        }
    }
}
