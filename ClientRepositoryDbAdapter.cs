using Npgsql;
using System.Data;

namespace Lombard
{
    internal class ClientRepositoryDbAdapter : ClientRepositoryBase
    {
        private readonly Client_rep_DB _dbRepository;

        // Конструктор принимает строку подключения
        public ClientRepositoryDbAdapter(string connectionString)
            : base(null) // Передаем null, так как для БД filePath не нужен
        {
            _dbRepository = new Client_rep_DB(connectionString);
        }

        #region Реализация абстрактных методов (адаптация)

        // Для БД этот метод не имеет смысла - данные всегда "загружены"
        public override void LoadFromFile()
        {

        }

        // Для БД этот метод не имеет смысла - данные сохраняются сразу
        public override void SaveToFile()
        {

        }

        #endregion

        #region Переопределение виртуальных методов (делегирование к Client_rep_DB)

        public override Client GetById(int id)
        {
            return _dbRepository.GetById(id);
        }

        public override List<ClientShort> GetShortList(int offset, int count)
        {
            return _dbRepository.GetShortList(offset, count);
        }

        public override Client Add(Client client)
        {
            var added = _dbRepository.Add(client);
            return added;
        }

        public override Client Update(int id, Client updatedClient)
        {
            return _dbRepository.Update(id, updatedClient);
        }

        public override bool Delete(int id)
        {
            return _dbRepository.Delete(id);
        }

        public override int GetCount()
        {
            return _dbRepository.GetCount();
        }

        public override List<Client> GetAll()
        {
            return _dbRepository.GetAll();
        }

        public override List<ClientShort> GetAllShort()
        {
            int count = GetCount();
            return GetShortList(0, count);
        }

        public override List<Client> SearchByLastName(string lastName)
        {
            return _dbRepository.SearchByLastName(lastName);
        }

        public override List<Client> SearchByPhone(string phoneNumber)
        {
            string cleanPhone = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");

            var allClients = _dbRepository.GetAll();
            return allClients
                .Where(c => System.Text.RegularExpressions.Regex.Replace(c.PhoneNumber, @"\D", "") == cleanPhone)
                .ToList();
        }

        public override void Clear()
        {

            var allClients = _dbRepository.GetAll();
            foreach (var client in allClients)
            {
                _dbRepository.Delete(client.Id);
            }

            _nextId = 1;
        }

        #endregion

        #region Дополнительные методы для работы с БД

        // Метод для проверки подключения
        public bool TestConnection()
        {
            try
            {
                GetCount();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
