namespace Lombard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Демонстрация работы с YAML репозиторием клиентов ===\n");

            try
            {
                // 1. Инициализация репозитория
                Console.WriteLine("1. Инициализация репозитория...");
                var repo = new Client_rep_yaml("clients.yaml");
                Console.WriteLine($"Репозиторий создан. Файл: clients.yaml");
                Console.WriteLine($"Текущее количество клиентов: {repo.GetCount()}\n");

                // 2. Добавление новых клиентов
                Console.WriteLine("2. Добавление новых клиентов...");

                var client1 = new Client(
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

                var client2 = new Client(
                    0,
                    "Петрова",
                    "Мария",
                    "4321",
                    "098765",
                    new DateTime(1985, 10, 20),
                    "+7-999-987-65-43",
                    Client.Genders.Female,
                    "Сергеевна",
                    "petrova@example.com"
                );

                var client3 = new Client(
                    0,
                    "Сидоров",
                    "Алексей",
                    "5555",
                    "111111",
                    new DateTime(1978, 3, 8),
                    "+7-999-555-55-55",
                    Client.Genders.Male,
                    "Петрович",
                    "sidorov@example.com"
                );

                var addedClient1 = repo.Add(client1);
                Console.WriteLine($"Добавлен клиент: {addedClient1.LastName} {addedClient1.FirstName} (ID: {addedClient1.Id})");

                var addedClient2 = repo.Add(client2);
                Console.WriteLine($"Добавлен клиент: {addedClient2.LastName} {addedClient2.FirstName} (ID: {addedClient2.Id})");

                var addedClient3 = repo.Add(client3);
                Console.WriteLine($"Добавлен клиент: {addedClient3.LastName} {addedClient3.FirstName} (ID: {addedClient3.Id})");

                Console.WriteLine($"Всего клиентов: {repo.GetCount()}\n");

                // 3. Получение клиента по ID
                Console.WriteLine("3. Получение клиента по ID...");
                try
                {
                    var retrievedClient = repo.GetById(addedClient1.Id);
                    Console.WriteLine($"Найден клиент: {retrievedClient.LastName} {retrievedClient.FirstName}");
                    Console.WriteLine($"Паспорт: {retrievedClient.PassportSeries} {retrievedClient.PassportNumber}");
                    Console.WriteLine($"Телефон: {retrievedClient.PhoneNumber}");
                    Console.WriteLine($"Email: {retrievedClient.Email}");
                    Console.WriteLine($"Возраст: {retrievedClient.Age}\n");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}\n");
                }

                // 4. Получение короткого списка клиентов
                Console.WriteLine("4. Получение короткого списка клиентов (первые 2)...");
                var shortList = repo.GetShortList(0, 2);
                foreach (var clientShort in shortList)
                {
                    Console.WriteLine($"{clientShort.Id}. {clientShort.LastName} {clientShort.FirstName} " +
                                     $"{clientShort.Patronymic} - {clientShort.PhoneNumber}");
                }
                Console.WriteLine();

                // 5. Получение всех клиентов в коротком формате
                Console.WriteLine("5. Все клиенты в коротком формате:");
                var allShortClients = repo.GetAllShort();
                foreach (var clientShort in allShortClients)
                {
                    Console.WriteLine($"{clientShort.Id}. {clientShort.LastName} {clientShort.FirstName} " +
                                     $"{clientShort.Patronymic}");
                }
                Console.WriteLine();

                // 6. Сортировка клиентов
                Console.WriteLine("6. Сортировка клиентов по фамилии (по возрастанию)...");
                repo.SortByLastName(ascending: true);
                var sortedClients = repo.GetAllShort();
                foreach (var client in sortedClients)
                {
                    Console.WriteLine($"{client.LastName} {client.FirstName}");
                }
                Console.WriteLine();

                // 7. Сортировка по возрасту (по убыванию)
                Console.WriteLine("7. Сортировка клиентов по возрасту (по убыванию)...");
                repo.SortByAge(ascending: false);
                var ageSortedClients = repo.GetAll();
                foreach (var client in ageSortedClients)
                {
                    Console.WriteLine($"{client.LastName} {client.FirstName} - Возраст: {client.Age}");
                }
                Console.WriteLine();

                // 8. Поиск клиентов
                Console.WriteLine("8. Поиск клиентов по фамилии 'Иванов'...");
                var searchResults = repo.SearchByLastName("Иванов");
                if (searchResults.Any())
                {
                    foreach (var client in searchResults)
                    {
                        Console.WriteLine($"Найден: {client.LastName} {client.FirstName}");
                    }
                }
                else
                {
                    Console.WriteLine("Клиенты не найдены");
                }
                Console.WriteLine();

                // 9. Поиск по номеру телефона
                Console.WriteLine("9. Поиск по номеру телефона '999-987-65-43'...");
                var phoneResults = repo.SearchByPhone("999-987-65-43");
                if (phoneResults.Any())
                {
                    foreach (var client in phoneResults)
                    {
                        Console.WriteLine($"Найден: {client.LastName} {client.FirstName} - {client.PhoneNumber}");
                    }
                }
                else
                {
                    Console.WriteLine("Клиенты не найдены");
                }
                Console.WriteLine();

                // 10. Обновление данных клиента
                Console.WriteLine("10. Обновление данных клиента...");
                try
                {
                    var clientToUpdate = repo.GetById(addedClient1.Id);
                    var updatedClientData = new Client(
                        0, // ID будет заменен на существующий
                        "Иванов",
                        "Иван",
                        "1234",
                        "567890",
                        clientToUpdate.BirthDate,
                        "+7-999-123-45-67", // Телефон тот же
                        Client.Genders.Male,
                        "Иванович",
                        "new-ivanov@example.com" // Новый email
                    );

                    var updatedClient = repo.Update(addedClient1.Id, updatedClientData);
                    Console.WriteLine($"Клиент обновлен. Новый email: {updatedClient.Email}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка при обновлении: {ex.Message}");
                }
                Console.WriteLine();

                // 11. Попытка добавить клиента с существующими данными (должна вызвать ошибку)
                Console.WriteLine("11. Попытка добавить клиента с существующими паспортными данными...");
                try
                {
                    var duplicateClient = new Client(
                        0,
                        "Другой",
                        "Клиент",
                        "1234", // Такие же паспортные данные
                        "567890",
                        new DateTime(2000, 1, 1),
                        "+7-999-000-00-00",
                        Client.Genders.Male,
                        "Олегович",
                        "other@example.com"
                    );

                    repo.Add(duplicateClient);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка (ожидаемая): {ex.Message}");
                }
                Console.WriteLine();

                // 12. Экспорт в YAML строку
                Console.WriteLine("12. Экспорт данных в YAML строку (первые 100 символов)...");
                try
                {
                    var yamlString = repo.ExportToYamlString();
                    var preview = yamlString.Length > 100 ? yamlString.Substring(0, 100) + "..." : yamlString;
                    Console.WriteLine(preview);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при экспорте: {ex.Message}");
                }

                // 13. Удаление клиента
                Console.WriteLine("13. Удаление клиента с ID 2...");
                bool deleted = repo.Delete(2);
                if (deleted)
                {
                    Console.WriteLine("Клиент успешно удален");
                    Console.WriteLine($"Осталось клиентов: {repo.GetCount()}");
                }
                else
                {
                    Console.WriteLine("Клиент не найден");
                }
                Console.WriteLine();

                // 14. Получение всех клиентов после изменений
                Console.WriteLine("14. Все клиенты после изменений:");
                var finalClients = repo.GetAll();
                if (finalClients.Any())
                {
                    foreach (var client in finalClients)
                    {
                        Console.WriteLine($"{client.Id}. {client.LastName} {client.FirstName} " +
                                         $"- {client.Email} - {client.PhoneNumber}");
                    }
                }
                else
                {
                    Console.WriteLine("Клиентов нет");
                }

                // 15. Создание нового репозитория для демонстрации загрузки из файла
                Console.WriteLine("\n15. Создание нового репозитория и загрузка из сохраненного файла...");
                var newRepo = new Client_rep_yaml("clients.yaml");
                Console.WriteLine($"Загружено клиентов: {newRepo.GetCount()}");
                var loadedClients = newRepo.GetAllShort();
                foreach (var client in loadedClients)
                {
                    Console.WriteLine($"{client.Id}. {client.LastName} {client.FirstName}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Детали: {ex.StackTrace}");
            }

            Console.WriteLine("\n=== Демонстрация завершена ===");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}