using System.Text.Json;

namespace Lombard
{
    internal class Client_rep_json : ClientRepositoryBase
    {
        public Client_rep_json(string filePath) : base(filePath)
        {
            LoadFromFile();
        }

        // Реализация абстрактных методов

        public override void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _clients = new List<Client>();
                    return;
                }

                var json = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var loadedClients = JsonSerializer.Deserialize<List<Client>>(json, options);
                _clients = loadedClients ?? new List<Client>();

                // Обновляем следующий ID
                _nextId = _clients.Count > 0 ? _clients.Max(c => c.Id) + 1 : 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при чтении файла: {ex.Message}", ex);
            }
        }

        public override void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_clients, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении файла: {ex.Message}", ex);
            }
        }
    }
}