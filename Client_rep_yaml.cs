using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lombard
{
    internal class Client_rep_yaml
    {
        private List<Client> _clients;
        private readonly string _filePath;
        private int _nextId;

        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public Client_rep_yaml(string filePath)
        {
            _filePath = filePath;
            _clients = new List<Client>();
            _nextId = 1;

            // Настраиваем сериализатор YAML
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithIndentedSequences()
                .Build();

            // Настраиваем десериализатор YAML
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            LoadFromFile();
        }

        #region Основные методы

        // Чтение всех значений из файла
        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _clients = new List<Client>();
                    return;
                }

                var yamlContent = File.ReadAllText(_filePath, Encoding.UTF8);

                // Если файл пустой или содержит только пробелы
                if (string.IsNullOrWhiteSpace(yamlContent))
                {
                    _clients = new List<Client>();
                    return;
                }

                var loadedClients = _deserializer.Deserialize<List<Client>>(yamlContent);
                _clients = loadedClients ?? new List<Client>();

                // Обновляем следующий ID
                _nextId = _clients.Count > 0 ? _clients.Max(c => c.Id) + 1 : 1;
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new Exception($"Ошибка при чтении YAML файла: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при чтении файла: {ex.Message}", ex);
            }
        }

        // Запись всех значений в файл
        public void SaveToFile()
        {
            try
            {
                var yamlContent = _serializer.Serialize(_clients);
                File.WriteAllText(_filePath, yamlContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении файла: {ex.Message}", ex);
            }
        }

        // Получить объект по ID
        public Client GetById(int id)
        {
            return _clients.FirstOrDefault(c => c.Id == id)
                ?? throw new ArgumentException($"Клиент с ID {id} не найден");
        }

        // Получить список k по счету n объектов класса ClientShort
        public List<ClientShort> GetShortList(int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentException("Смещение не может быть отрицательным");

            if (count <= 0)
                throw new ArgumentException("Количество элементов должно быть больше 0");

            var shortClients = _clients
                .Skip(offset)
                .Take(count)
                .Select(client => new ClientShort(
                    client.Id,
                    client.LastName,
                    client.FirstName,
                    client.PassportSeries,
                    client.PassportNumber,
                    client.PhoneNumber,
                    client.Patronymic))
                .ToList();

            return shortClients;
        }

        // Сортировать элементы по выбранному полю
        public void SortBy(Func<Client, IComparable> keySelector, bool ascending = true)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (ascending)
            {
                _clients = _clients.OrderBy(keySelector).ToList();
            }
            else
            {
                _clients = _clients.OrderByDescending(keySelector).ToList();
            }
        }

        // Добавить объект в список (с формированием нового ID)
        public Client Add(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            // Проверяем валидность клиента
            if (!client.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            // Проверяем уникальность паспортных данных
            if (_clients.Any(c =>
                c.PassportSeries == client.PassportSeries &&
                c.PassportNumber == client.PassportNumber))
            {
                throw new ArgumentException("Клиент с такими паспортными данными уже существует!");
            }

            // Проверяем уникальность номера телефона
            if (_clients.Any(c =>
                c.PhoneNumber == client.PhoneNumber))
            {
                throw new ArgumentException("Клиент с таким номером телефона уже существует!");
            }

            // Проверяем уникальность Email
            if (_clients.Any(c =>
                c.Email == client.Email))
            {
                throw new ArgumentException("Клиент с таким Email уже существует!");
            }

            // Присваиваем новый ID
            var clientWithId = new Client(
                _nextId,
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

            _clients.Add(clientWithId);
            _nextId++;

            SaveToFile();
            return clientWithId;
        }

        // Заменить элемент списка по ID
        public Client Update(int id, Client updatedClient)
        {
            if (updatedClient == null)
                throw new ArgumentNullException(nameof(updatedClient));

            // Проверяем валидность клиента
            if (!updatedClient.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            var existingClient = GetById(id);
            var index = _clients.IndexOf(existingClient);

            // Проверяем уникальность паспортных данных (исключая текущего клиента)
            if (_clients.Any(c =>
                c.Id != id &&
                c.PassportSeries == updatedClient.PassportSeries &&
                c.PassportNumber == updatedClient.PassportNumber))
            {
                throw new ArgumentException("Клиент с такими паспортными данными уже существует");
            }

            // Проверяем уникальность номера телефона (исключая текущего клиента)
            if (_clients.Any(c =>
                c.Id != id &&
                c.PhoneNumber == updatedClient.PhoneNumber))
            {
                throw new ArgumentException("Клиент с таким номером телефона уже существует!");
            }

            // Проверяем уникальность Email (исключая текущего клиента)
            if (_clients.Any(c =>
                c.Id != id &&
                c.Email == updatedClient.Email))
            {
                throw new ArgumentException("Клиент с таким Email уже существует!");
            }

            // Создаем клиента с сохранением ID
            var clientWithId = new Client(
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

            _clients[index] = clientWithId;
            SaveToFile();

            return clientWithId;
        }

        // Удалить элемент списка по ID
        public bool Delete(int id)
        {
            var client = _clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
                return false;

            bool removed = _clients.Remove(client);
            if (removed)
            {
                SaveToFile();
            }
            return removed;
        }

        // Получить количество элементов
        public int GetCount()
        {
            return _clients.Count;
        }

        #endregion

        #region Дополнительные методы

        // Получить всех клиентов
        public List<Client> GetAll()
        {
            return new List<Client>(_clients);
        }

        // Получить всех клиентов в коротком формате
        public List<ClientShort> GetAllShort()
        {
            return _clients
                .Select(client => new ClientShort(
                    client.Id,
                    client.LastName,
                    client.FirstName,
                    client.PassportSeries,
                    client.PassportNumber,
                    client.PhoneNumber,
                    client.Patronymic))
                .ToList();
        }

        // Поиск по фамилии
        public List<Client> SearchByLastName(string lastName)
        {
            return _clients
                .Where(c => c.LastName.Contains(lastName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Поиск по номеру телефона
        public List<Client> SearchByPhone(string phoneNumber)
        {
            string cleanPhone = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");
            return _clients
                .Where(c => System.Text.RegularExpressions.Regex.Replace(c.PhoneNumber, @"\D", "") == cleanPhone)
                .ToList();
        }

        // Очистить все данные
        public void Clear()
        {
            _clients.Clear();
            _nextId = 1;
            SaveToFile();
        }

        // Экспорт в YAML строку (для отладки или передачи)
        public string ExportToYamlString()
        {
            return _serializer.Serialize(_clients);
        }

        // Импорт из YAML строки
        public void ImportFromYamlString(string yamlContent)
        {
            try
            {
                var importedClients = _deserializer.Deserialize<List<Client>>(yamlContent);
                if (importedClients != null)
                {
                    _clients = importedClients;
                    _nextId = _clients.Count > 0 ? _clients.Max(c => c.Id) + 1 : 1;
                    SaveToFile();
                }
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new Exception($"Ошибка при импорте YAML: {ex.Message}", ex);
            }
        }

        #endregion

        #region Сортировки

        public void SortByLastName(bool ascending = true)
        {
            SortBy(c => c.LastName, ascending);
        }

        public void SortByBirthDate(bool ascending = true)
        {
            SortBy(c => c.BirthDate, ascending);
        }

        public void SortById(bool ascending = true)
        {
            SortBy(c => c.Id, ascending);
        }

        public void SortByAge(bool ascending = true)
        {
            if (ascending)
            {
                _clients = _clients.OrderBy(c => c.Age).ToList();
            }
            else
            {
                _clients = _clients.OrderByDescending(c => c.Age).ToList();
            }
        }

        #endregion
    }
}