namespace Lombard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Создание репозитория
            var repo = new Client_rep_json("clients.json");

            // i. Получить количество элементов
            Console.WriteLine($"Всего клиентов: {repo.GetCount()}");

            // d. Получить первые 10 элементов в коротком формате
            var shortList = repo.GetShortList(0, 10);
            foreach (var client in shortList)
            {
                Console.WriteLine(client.GetShortInfo());
            }

            // e. Сортировка по фамилии
            repo.SortByLastName();

            // c. Получить клиента по ID
            try
            {
                var client = repo.GetById(1);
                Console.WriteLine(client.GetFullInfo());
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // f. Добавить нового клиента
            var newClient = new Client(
                10, // ID будет сгенерирован автоматически
                "Иванов",
                "Иван",
                "1234",
                "567890",
                new DateTime(1990, 5, 15),
                "+79161234567",
                Client.Genders.Male,
                "Иванович",
                "ivanov@mail.ru"
            );

            var addedClient = repo.Add(newClient);
            Console.WriteLine($"Добавлен клиент с ID: {addedClient.Id}");

            // g. Обновить клиента
            var updatedClient = new Client(
                0, // ID будет заменен
                "Иванов",
                "Петр", // Изменили имя
                "1234",
                "567890",
                new DateTime(1990, 5, 15),
                "+79161234567",
                Client.Genders.Male,
                "Иванович",
                "petr@mail.ru"
            );

            repo.Update(addedClient.Id, updatedClient);

            // h. Удалить клиента
            bool deleted = repo.Delete(addedClient.Id);
            if (deleted)
            {
                Console.WriteLine("Клиент удален");
            }
        }
    }
}