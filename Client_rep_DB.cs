using Npgsql;

namespace Lombard
{
    internal class Client_rep_DB
    {
        private readonly DatabaseConnectionManager _dbManager;

        public Client_rep_DB(string connectionString)
        {
            // Получаем Singleton экземпляр менеджера подключений
            _dbManager = DatabaseConnectionManager.GetInstance(connectionString);

            // Открываем соединение при создании репозитория
            _dbManager.OpenConnection();
        }

        // Альтернативный конструктор для использования существующего экземпляра
        public Client_rep_DB()
        {
            _dbManager = DatabaseConnectionManager.GetInstance();
            _dbManager.OpenConnection();
        }

        #region Основные методы (с использованием делегирования)

        // Получить объект по ID
        public Client GetById(int id)
        {
            var query = @"
                SELECT 
                    client_id, last_name, first_name, patronymic, 
                    passport_series, passport_number, phone_number, 
                    email, birth_date, gender
                FROM Clients 
                WHERE client_id = @id";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", id)
            };

            using (var reader = _dbManager.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    return MapToClient(reader);
                }
                else
                {
                    throw new ArgumentException($"Клиент с ID {id} не найден");
                }
            }
        }

        // Получить список k по счету n объектов класса short
        public List<ClientShort> GetShortList(int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentException("Смещение не может быть отрицательным");

            if (count <= 0)
                throw new ArgumentException("Количество элементов должно быть больше 0");

            var clients = new List<ClientShort>();

            var query = @"
                SELECT 
                    client_id, last_name, first_name, 
                    passport_series, passport_number, phone_number,
                    patronymic
                FROM Clients 
                ORDER BY client_id
                LIMIT @count OFFSET @offset";

            var parameters = new[]
            {
                new NpgsqlParameter("@count", count),
                new NpgsqlParameter("@offset", offset)
            };

            using (var reader = _dbManager.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    var clientShort = new ClientShort(
                        reader.GetInt32(0),       // client_id
                        reader.GetString(1),      // last_name
                        reader.GetString(2),      // first_name
                        reader.GetString(3),      // passport_series
                        reader.GetString(4),      // passport_number
                        reader.GetString(5),      // phone_number
                        reader.IsDBNull(6) ? null : reader.GetString(6)  // patronymic
                    );
                    clients.Add(clientShort);
                }
            }

            return clients;
        }

        // Добавить объект в список (при добавлении сформировать новый ID)
        public Client Add(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (!client.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            CheckUniqueness(client, null);

            var query = @"
                INSERT INTO Clients (
                    last_name, first_name, patronymic, 
                    passport_series, passport_number, phone_number, 
                    email, birth_date, gender
                ) 
                VALUES (
                    @last_name, @first_name, @patronymic, 
                    @passport_series, @passport_number, @phone_number, 
                    @email, @birth_date, @gender
                ) 
                RETURNING client_id";

            var parameters = CreateClientParameters(client);

            // Выполняем вставку и получаем сгенерированный ID
            var newId = (int)_dbManager.ExecuteScalar(query, parameters);

            // Создаем клиента с присвоенным ID
            return new Client(
                newId,
                client.LastName,
                client.FirstName,
                client.PassportSeries,
                client.PassportNumber,
                client.BirthDate,
                client.PhoneNumber,
                client.Gender,
                client.Patronymic,
                client.Email
            );
        }

        // Заменить элемент списка по ID
        public Client Update(int id, Client updatedClient)
        {
            if (updatedClient == null)
                throw new ArgumentNullException(nameof(updatedClient));

            if (!updatedClient.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            // Проверяем существование клиента
            var existing = GetById(id);

            CheckUniqueness(updatedClient, id);

            var query = @"
                UPDATE Clients 
                SET 
                    last_name = @last_name,
                    first_name = @first_name,
                    patronymic = @patronymic,
                    passport_series = @passport_series,
                    passport_number = @passport_number,
                    phone_number = @phone_number,
                    email = @email,
                    birth_date = @birth_date,
                    gender = @gender
                WHERE client_id = @id";

            var parameters = CreateClientParameters(updatedClient);
            // Добавляем параметр ID в начало массива
            var allParameters = new NpgsqlParameter[parameters.Length + 1];
            allParameters[0] = new NpgsqlParameter("@id", id);
            Array.Copy(parameters, 0, allParameters, 1, parameters.Length);

            int rowsAffected = _dbManager.ExecuteNonQuery(query, allParameters);

            if (rowsAffected == 0)
            {
                throw new ArgumentException($"Клиент с ID {id} не найден");
            }

            return new Client(
                id,
                updatedClient.LastName,
                updatedClient.FirstName,
                updatedClient.PassportSeries,
                updatedClient.PassportNumber,
                updatedClient.BirthDate,
                updatedClient.PhoneNumber,
                updatedClient.Gender,
                updatedClient.Patronymic,
                updatedClient.Email
            );
        }

        // Удалить элемент списка по ID
        public bool Delete(int id)
        {
            var query = "DELETE FROM Clients WHERE client_id = @id";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", id)
            };

            int rowsAffected = _dbManager.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // Получить количество элементов
        public int GetCount()
        {
            var query = "SELECT COUNT(*) FROM Clients";

            var result = _dbManager.ExecuteScalar(query);
            return Convert.ToInt32(result);
        }

        #endregion

        #region Вспомогательные методы

        private Client MapToClient(NpgsqlDataReader reader)
        {
            return new Client(
                reader.GetInt32(0),           // client_id
                reader.GetString(1),          // last_name
                reader.GetString(2),          // first_name
                reader.GetString(4),          // passport_series
                reader.GetString(5),          // passport_number
                reader.GetDateTime(8),        // birth_date
                reader.GetString(6),          // phone_number
                reader.GetString(9)[0] == 'M' ? Client.Genders.Male : Client.Genders.Female, // gender
                reader.IsDBNull(3) ? null : reader.GetString(3),  // patronymic
                reader.IsDBNull(7) ? null : reader.GetString(7)   // email
            );
        }

        private NpgsqlParameter[] CreateClientParameters(Client client)
        {
            return new[]
            {
                new NpgsqlParameter("@last_name", client.LastName),
                new NpgsqlParameter("@first_name", client.FirstName),
                new NpgsqlParameter("@patronymic", (object)client.Patronymic ?? DBNull.Value),
                new NpgsqlParameter("@passport_series", client.PassportSeries),
                new NpgsqlParameter("@passport_number", client.PassportNumber),
                new NpgsqlParameter("@phone_number", client.PhoneNumber),
                new NpgsqlParameter("@email", (object)client.Email ?? DBNull.Value),
                new NpgsqlParameter("@birth_date", client.BirthDate),
                new NpgsqlParameter("@gender", client.Gender == Client.Genders.Male ? 'M' : (client.Gender == Client.Genders.Female ? 'F' : DBNull.Value))
            };
        }

        private void CheckUniqueness(Client client, int? excludeId)
        {
            // Проверка паспортных данных
            var passportQuery = @"
                SELECT COUNT(*) 
                FROM Clients 
                WHERE passport_series = @series 
                  AND passport_number = @number";

            if (excludeId.HasValue)
            {
                passportQuery += " AND client_id != @excludeId";
            }

            var passportParams = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@series", client.PassportSeries),
                new NpgsqlParameter("@number", client.PassportNumber)
            };

            if (excludeId.HasValue)
            {
                passportParams.Add(new NpgsqlParameter("@excludeId", excludeId.Value));
            }

            var passportCount = Convert.ToInt32(_dbManager.ExecuteScalar(passportQuery, passportParams.ToArray()));
            if (passportCount > 0)
            {
                throw new ArgumentException("Клиент с такими паспортными данными уже существует!");
            }

            // Проверка номера телефона
            var phoneQuery = "SELECT COUNT(*) FROM Clients WHERE phone_number = @phone";

            if (excludeId.HasValue)
            {
                phoneQuery += " AND client_id != @excludeId";
            }

            var phoneParams = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@phone", client.PhoneNumber)
            };

            if (excludeId.HasValue)
            {
                phoneParams.Add(new NpgsqlParameter("@excludeId", excludeId.Value));
            }

            var phoneCount = Convert.ToInt32(_dbManager.ExecuteScalar(phoneQuery, phoneParams.ToArray()));
            if (phoneCount > 0)
            {
                throw new ArgumentException("Клиент с таким номером телефона уже существует!");
            }

            // Проверка email (если указан)
            if (!string.IsNullOrWhiteSpace(client.Email))
            {
                var emailQuery = "SELECT COUNT(*) FROM Clients WHERE email = @email";

                if (excludeId.HasValue)
                {
                    emailQuery += " AND client_id != @excludeId";
                }

                var emailParams = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@email", client.Email)
                };

                if (excludeId.HasValue)
                {
                    emailParams.Add(new NpgsqlParameter("@excludeId", excludeId.Value));
                }

                var emailCount = Convert.ToInt32(_dbManager.ExecuteScalar(emailQuery, emailParams.ToArray()));
                if (emailCount > 0)
                {
                    throw new ArgumentException("Клиент с таким Email уже существует!");
                }
            }
        }

        #endregion

        #region Дополнительные методы

        public List<Client> GetAll()
        {
            var clients = new List<Client>();

            var query = @"
                SELECT 
                    client_id, last_name, first_name, patronymic, 
                    passport_series, passport_number, phone_number, 
                    email, birth_date, gender
                FROM Clients 
                ORDER BY client_id";

            using (var reader = _dbManager.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    clients.Add(MapToClient(reader));
                }
            }

            return clients;
        }

        public List<Client> SearchByLastName(string lastName)
        {
            var clients = new List<Client>();

            var query = @"
                SELECT 
                    client_id, last_name, first_name, patronymic, 
                    passport_series, passport_number, phone_number, 
                    email, birth_date, gender
                FROM Clients 
                WHERE LOWER(last_name) LIKE LOWER(@pattern)
                ORDER BY last_name, first_name";

            var parameters = new[]
            {
                new NpgsqlParameter("@pattern", $"%{lastName}%")
            };

            using (var reader = _dbManager.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    clients.Add(MapToClient(reader));
                }
            }

            return clients;
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            // Не закрываем соединение полностью, так как это Singleton
            // Просто освобождаем ресурсы этого репозитория
        }

        #endregion
    }
}