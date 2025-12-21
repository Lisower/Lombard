using System.Text.RegularExpressions;

namespace Lombard
{
    internal abstract class ClientRepositoryBase
    {
        protected List<Client> _clients;
        protected readonly string _filePath;
        protected int _nextId;

        protected ClientRepositoryBase(string filePath)
        {
            _filePath = filePath;
            _clients = new List<Client>();
            _nextId = 1;
        }

        #region Абстрактные методы (реализуются в наследниках)

        // Чтение всех значений из файла
        public abstract void LoadFromFile();

        // Запись всех значений в файл
        public abstract void SaveToFile();

        #endregion

        #region Основные методы (реализация в базовом классе)

        // Получить объект по ID
        public virtual Client GetById(int id)
        {
            return _clients.FirstOrDefault(c => c.Id == id)
                ?? throw new ArgumentException($"Клиент с ID {id} не найден");
        }

        // Получить список k по счету n объектов класса ClientShort
        public virtual List<ClientShort> GetShortList(int offset, int count)
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
        public virtual void SortBy(Func<Client, IComparable> keySelector, bool ascending = true)
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
        public virtual Client Add(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            // Проверяем валидность клиента
            if (!client.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            // Проверяем уникальность данных
            ValidateUniqueness(client, null);

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
        public virtual Client Update(int id, Client updatedClient)
        {
            if (updatedClient == null)
                throw new ArgumentNullException(nameof(updatedClient));

            // Проверяем валидность клиента
            if (!updatedClient.IsValid())
                throw new ArgumentException("Некорректные данные клиента");

            var existingClient = GetById(id);
            var index = _clients.IndexOf(existingClient);

            // Проверяем уникальность данных (исключая текущего клиента)
            ValidateUniqueness(updatedClient, id);

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
        public virtual bool Delete(int id)
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
        public virtual int GetCount()
        {
            return _clients.Count;
        }

        #endregion

        #region Дополнительные методы

        // Получить всех клиентов
        public virtual List<Client> GetAll()
        {
            return new List<Client>(_clients);
        }

        // Получить всех клиентов в коротком формате
        public virtual List<ClientShort> GetAllShort()
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
        public virtual List<Client> SearchByLastName(string lastName)
        {
            return _clients
                .Where(c => c.LastName.Contains(lastName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Поиск по номеру телефона
        public virtual List<Client> SearchByPhone(string phoneNumber)
        {
            string cleanPhone = Regex.Replace(phoneNumber, @"\D", "");
            return _clients
                .Where(c => Regex.Replace(c.PhoneNumber, @"\D", "") == cleanPhone)
                .ToList();
        }

        // Очистить все данные
        public virtual void Clear()
        {
            _clients.Clear();
            _nextId = 1;
            SaveToFile();
        }

        #endregion

        #region Сортировки

        public virtual void SortByLastName(bool ascending = true)
        {
            SortBy(c => c.LastName, ascending);
        }

        public virtual void SortByBirthDate(bool ascending = true)
        {
            SortBy(c => c.BirthDate, ascending);
        }

        public virtual void SortById(bool ascending = true)
        {
            SortBy(c => c.Id, ascending);
        }

        public virtual void SortByAge(bool ascending = true)
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

        #region Защищённые методы

        // Проверка уникальности данных клиента
        protected virtual void ValidateUniqueness(Client client, int? excludeId = null)
        {
            // Проверяем уникальность паспортных данных
            if (_clients.Any(c =>
                c.Id != excludeId &&
                c.PassportSeries == client.PassportSeries &&
                c.PassportNumber == client.PassportNumber))
            {
                throw new ArgumentException("Клиент с такими паспортными данными уже существует!");
            }

            // Проверяем уникальность номера телефона
            if (_clients.Any(c =>
                c.Id != excludeId &&
                c.PhoneNumber == client.PhoneNumber))
            {
                throw new ArgumentException("Клиент с таким номером телефона уже существует!");
            }

            // Проверяем уникальность Email (если указан)
            if (!string.IsNullOrWhiteSpace(client.Email) &&
                _clients.Any(c =>
                    c.Id != excludeId &&
                    c.Email == client.Email))
            {
                throw new ArgumentException("Клиент с таким Email уже существует!");
            }
        }

        #endregion
    }
}