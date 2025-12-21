using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lombard
{
    internal class Client_rep_yaml : ClientRepositoryBase
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public Client_rep_yaml(string filePath) : base(filePath)
        {
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

        public override void SaveToFile()
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

        #region Специфичные для YAML методы

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
    }
}