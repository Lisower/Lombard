namespace Lombard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Использование полиморфизма
            ClientRepositoryBase repository;

            // Выбор формата хранения
            string format = "json"; // или "yaml"
            string filePath = "clients.dat";

            if (format.ToLower() == "yaml")
            {
                repository = new Client_rep_yaml(filePath);
            }
            else
            {
                repository = new Client_rep_json(filePath);
            }

            // Все общие методы работают одинаково
            var allClients = repository.GetAll();
            var count = repository.GetCount();
            repository.SortByLastName();

            // Добавление нового клиента
            var newClient = new Client(
                0, // ID будет сгенерирован автоматически
                "Иванов",
                "Иван",
                "1234",
                "567890",
                new DateTime(1990, 5, 15),
                "+7-999-123-45-67",
                Client.Genders.Male,
                "Иванович",
                "ivanov@example.com"
            );

            var addedClient = repository.Add(newClient);

            // Если нужны специфичные методы YAML
            if (repository is Client_rep_yaml yamlRepo)
            {
                string yamlString = yamlRepo.ExportToYamlString();
                Console.WriteLine(yamlString);
            }
        }
    }
}