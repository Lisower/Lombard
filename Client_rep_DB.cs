using Npgsql;
using System.Data;

namespace Lombard
{
    internal class Client_rep_DB
    {
        private readonly string _connectionString;

        public Client_rep_DB(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Основные методы

        // Получить объект по ID
        public Client GetById(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        client_id, last_name, first_name, patronymic, 
                        passport_series, passport_number, phone_number, 
                        email, birth_date, gender
                    FROM Clients 
                    WHERE client_id = @id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
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

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // В PostgreSQL используем LIMIT и OFFSET
                var query = @"
                    SELECT 
                        client_id, last_name, first_name, patronymic, 
                        passport_series, passport_number, phone_number
                    FROM Clients 
                    ORDER BY client_id
                    LIMIT @count OFFSET @offset";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@count", count);
                    command.Parameters.AddWithValue("@offset", count * (offset - 1));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var clientShort = new ClientShort(
                                reader.GetInt32(0),       // client_id
                                reader.GetString(1),      // last_name
                                reader.GetString(2),      // first_name
                                reader.GetString(4),      // passport_series
                                reader.GetString(5),      // passport_number
                                reader.GetString(6),      // phone_number
                                reader.IsDBNull(3) ? null : reader.GetString(3)  // patronymic
                            );
                            clients.Add(clientShort);
                        }
                    }
                }
            }

            return clients;
        }

        // Добавить объект в список
        public Client Add(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            // Проверяем валидность клиента
            if (!client.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            // Проверяем уникальность данных
            CheckUniqueness(client, null);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

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

                using (var command = new NpgsqlCommand(query, connection))
                {
                    AddClientParameters(command, client);

                    // Выполняем вставку и получаем сгенерированный ID
                    var newId = (int)command.ExecuteScalar();

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
            }
        }

        // Заменить элемент списка по ID
        public Client Update(int id, Client updatedClient)
        {
            if (updatedClient == null)
                throw new ArgumentNullException(nameof(updatedClient));

            // Проверяем валидность клиента
            if (!updatedClient.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            // Проверяем существование клиента
            var existing = GetById(id);

            // Проверяем уникальность данных (исключая текущего клиента)
            CheckUniqueness(updatedClient, id);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

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

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    AddClientParameters(command, updatedClient);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new ArgumentException($"Клиент с ID {id} не найден");
                    }
                }
            }

            // Возвращаем обновленного клиента
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = "DELETE FROM Clients WHERE client_id = @id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        // Получить количество элементов
        public int GetCount()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM Clients";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
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
                reader.GetString(9)[0] == 'M' ? Client.Genders.Male : Client.Genders.Female, // gender (первый символ)
                reader.IsDBNull(3) ? null : reader.GetString(3),  // patronymic
                reader.IsDBNull(7) ? null : reader.GetString(7)   // email
            );
        }

        private void AddClientParameters(NpgsqlCommand command, Client client)
        {
            command.Parameters.AddWithValue("@last_name", client.LastName);
            command.Parameters.AddWithValue("@first_name", client.FirstName);
            command.Parameters.AddWithValue("@passport_series", client.PassportSeries);
            command.Parameters.AddWithValue("@passport_number", client.PassportNumber);
            command.Parameters.AddWithValue("@phone_number", client.PhoneNumber);
            command.Parameters.AddWithValue("@birth_date", client.BirthDate);
            command.Parameters.AddWithValue("@gender", client.Gender == Client.Genders.Male ? 'M' : (client.Gender == Client.Genders.Female ? 'M' : DBNull.Value));

            // Обрабатываем nullable поля
            if (client.Patronymic != null)
                command.Parameters.AddWithValue("@patronymic", client.Patronymic);
            else
                command.Parameters.AddWithValue("@patronymic", DBNull.Value);

            if (client.Email != null)
                command.Parameters.AddWithValue("@email", client.Email);
            else
                command.Parameters.AddWithValue("@email", DBNull.Value);
        }

        private void CheckUniqueness(Client client, int? excludeId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

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

                using (var command = new NpgsqlCommand(passportQuery, connection))
                {
                    command.Parameters.AddWithValue("@series", client.PassportSeries);
                    command.Parameters.AddWithValue("@number", client.PassportNumber);

                    if (excludeId.HasValue)
                    {
                        command.Parameters.AddWithValue("@excludeId", excludeId.Value);
                    }

                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        throw new ArgumentException("Клиент с такими паспортными данными уже существует!");
                    }
                }

                // Проверка номера телефона
                var phoneQuery = "SELECT COUNT(*) FROM Clients WHERE phone_number = @phone";

                if (excludeId.HasValue)
                {
                    phoneQuery += " AND client_id != @excludeId";
                }

                using (var command = new NpgsqlCommand(phoneQuery, connection))
                {
                    command.Parameters.AddWithValue("@phone", client.PhoneNumber);

                    if (excludeId.HasValue)
                    {
                        command.Parameters.AddWithValue("@excludeId", excludeId.Value);
                    }

                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        throw new ArgumentException("Клиент с таким номером телефона уже существует!");
                    }
                }

                // Проверка email (если указан)
                if (!string.IsNullOrWhiteSpace(client.Email))
                {
                    var emailQuery = "SELECT COUNT(*) FROM Clients WHERE email = @email";

                    if (excludeId.HasValue)
                    {
                        emailQuery += " AND client_id != @excludeId";
                    }

                    using (var command = new NpgsqlCommand(emailQuery, connection))
                    {
                        command.Parameters.AddWithValue("@email", client.Email);

                        if (excludeId.HasValue)
                        {
                            command.Parameters.AddWithValue("@excludeId", excludeId.Value);
                        }

                        var count = Convert.ToInt32(command.ExecuteScalar());
                        if (count > 0)
                        {
                            throw new ArgumentException("Клиент с таким Email уже существует!");
                        }
                    }
                }
            }
        }

        #endregion

        #region Дополнительные методы

        public List<Client> GetAll()
        {
            var clients = new List<Client>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        client_id, last_name, first_name, patronymic, 
                        passport_series, passport_number, phone_number, 
                        email, birth_date, gender
                    FROM Clients 
                    ORDER BY client_id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(MapToClient(reader));
                        }
                    }
                }
            }

            return clients;
        }

        public List<Client> SearchByLastName(string lastName)
        {
            var clients = new List<Client>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        client_id, last_name, first_name, patronymic, 
                        passport_series, passport_number, phone_number, 
                        email, birth_date, gender
                    FROM Clients 
                    WHERE LOWER(last_name) LIKE LOWER(@pattern)
                    ORDER BY last_name, first_name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pattern", $"%{lastName}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(MapToClient(reader));
                        }
                    }
                }
            }

            return clients;
        }

        #endregion
    }
}