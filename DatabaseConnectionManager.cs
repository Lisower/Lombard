using Npgsql;
using System.Data;

namespace Lombard
{
    internal sealed class DatabaseConnectionManager
    {
        private static DatabaseConnectionManager _instance;
        private static readonly object _lock = new object();

        private readonly string _connectionString;
        private NpgsqlConnection _connection;

        private DatabaseConnectionManager(string connectionString)
        {
            _connectionString = connectionString;
            InitializeConnection();
        }

        public static DatabaseConnectionManager GetInstance(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock) // Для потокобезопасности
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnectionManager(connectionString);
                    }
                }
            }
            return _instance;
        }

        public static DatabaseConnectionManager GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Экземпляр еще не создан. Сначала вызовите GetInstance(string connectionString)");
            }
            return _instance;
        }

        private void InitializeConnection()
        {
            _connection = new NpgsqlConnection(_connectionString);
        }

        #region Основные методы работы с подключением

        public void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public void DisposeConnection()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public NpgsqlConnection GetConnection()
        {
            return _connection;
        }

        public ConnectionState GetConnectionState()
        {
            return _connection.State;
        }

        #endregion

        #region Методы для выполнения запросов

        public NpgsqlDataReader ExecuteReader(string query, params NpgsqlParameter[] parameters)
        {
            using (var command = new NpgsqlCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteReader();
            }
        }

        public int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var command = new NpgsqlCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
            using (var command = new NpgsqlCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }

        public void ExecuteTransaction(Action<NpgsqlTransaction> action)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    action(transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Вспомогательные методы

        public bool TestConnection()
        {
            try
            {
                OpenConnection();
                using (var command = new NpgsqlCommand("SELECT 1", _connection))
                {
                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeConnection();
                }
                _disposed = true;
            }
        }

        ~DatabaseConnectionManager()
        {
            Dispose(false);
        }

        #endregion
    }
}