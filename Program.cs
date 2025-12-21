namespace Lombard
{
    public class Program
    {
        private static readonly Random _random = new Random();
        private static readonly string[] _maleFirstNames = { "Александр", "Михаил", "Иван", "Дмитрий", "Андрей", "Сергей", "Алексей", "Владимир", "Евгений", "Павел" };
        private static readonly string[] _femaleFirstNames = { "Елена", "Ольга", "Наталья", "Ирина", "Мария", "Светлана", "Анна", "Татьяна", "Екатерина", "Юлия" };
        private static readonly string[] _lastNames = { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Новиков", "Федоров", "Морозов" };
        private static readonly string[] _malePatronymics = { "Александрович", "Михайлович", "Иванович", "Дмитриевич", "Андреевич", "Сергеевич", "Алексеевич", "Владимирович", "Евгеньевич", "Павлович" };
        private static readonly string[] _femalePatronymics = { "Александровна", "Михайловна", "Ивановна", "Дмитриевна", "Андреевна", "Сергеевна", "Алексеевна", "Владимировна", "Евгеньевна", "Павловна" };
        private static readonly string[] _domains = { "gmail.com", "mail.ru", "yandex.ru", "outlook.com", "yahoo.com" };

        public static void Main(string[] args)
        {
            // Строка подключения к PostgreSQL
            string connectionString = "Host=localhost;Port=5432;Database=Lombard;Username=postgres;Password=your_password";

            try
            {
                // Создание репозитория
                var repo = new Client_rep_DB(connectionString);

                Console.WriteLine("Начинаем заполнение таблицы случайными клиентами...");
                Console.WriteLine("==================================================");

                // Проверяем, есть ли уже клиенты в базе
                int existingCount = repo.GetCount();
                Console.WriteLine($"Текущее количество клиентов в БД: {existingCount}");

                // Запрашиваем количество клиентов для добавления
                int clientsToGenerate = 25;
                Console.WriteLine($"Будет сгенерировано {clientsToGenerate} клиентов");

                // Цикл генерации и добавления клиентов
                for (int i = 1; i <= clientsToGenerate; i++)
                {
                    try
                    {
                        // Генерируем случайного клиента
                        Client randomClient = GenerateRandomClient();

                        // Добавляем клиента в БД
                        var addedClient = repo.Add(randomClient);

                        Console.WriteLine($"[{i}/{clientsToGenerate}] Добавлен клиент: {addedClient.LastName} {addedClient.FirstName} {addedClient.Patronymic} (ID: {addedClient.Id})");

                        // Небольшая задержка для имитации реальной работы
                        Thread.Sleep(50);
                    }
                    catch (ArgumentException ex)
                    {
                        // Если клиент с такими данными уже существует, пропускаем
                        Console.WriteLine($"[{i}/{clientsToGenerate}] Ошибка: {ex.Message}. Пропускаем...");
                        i--; // Повторяем попытку для этого номера
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{i}/{clientsToGenerate}] Ошибка: {ex.Message}");
                    }
                }

                // Выводим итоговую статистику
                Console.WriteLine("\n==================================================");
                Console.WriteLine("Заполнение завершено!");
                Console.WriteLine($"Итоговое количество клиентов в БД: {repo.GetCount()}");

                // Демонстрация работы других методов
                DemonstrateRepositoryFeatures(repo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                Console.WriteLine("Проверьте строку подключения и убедитесь, что:");
                Console.WriteLine("1. PostgreSQL запущен");
                Console.WriteLine("2. База данных 'Lombard' существует");
                Console.WriteLine("3. Таблица 'Clients' создана");
                Console.WriteLine("4. Указаны правильные учетные данные");
            }
        }

        private static Client GenerateRandomClient()
        {
            // Случайно выбираем пол
            bool isMale = _random.Next(2) == 0;
            Client.Genders gender = isMale ? Client.Genders.Male : Client.Genders.Female;

            // Выбираем имя в зависимости от пола
            string firstName = isMale
                ? _maleFirstNames[_random.Next(_maleFirstNames.Length)]
                : _femaleFirstNames[_random.Next(_femaleFirstNames.Length)];

            // Выбираем фамилию (можем добавить женские окончания)
            string lastName = _lastNames[_random.Next(_lastNames.Length)];
            if (!isMale)
            {
                lastName += "а"; // Простое добавление женского окончания
            }

            // Выбираем отчество
            string patronymic = isMale
                ? _malePatronymics[_random.Next(_malePatronymics.Length)]
                : _femalePatronymics[_random.Next(_femalePatronymics.Length)];

            // Генерируем паспортные данные
            string passportSeries = _random.Next(1000, 9999).ToString();
            string passportNumber = _random.Next(100000, 999999).ToString();

            // Генерируем номер телефона
            string phoneNumber = "79" + _random.Next(100000000, 999999999).ToString();

            // Генерируем email
            string email = $"{firstName.ToLower()}.{lastName.ToLower()}@{_domains[_random.Next(_domains.Length)]}";

            // Генерируем дату рождения (от 18 до 80 лет)
            DateTime currentDate = DateTime.Now;
            int age = _random.Next(18, 81);
            int birthYear = currentDate.Year - age;
            int birthMonth = _random.Next(1, 13);
            int birthDay = _random.Next(1, DateTime.DaysInMonth(birthYear, birthMonth) + 1);
            DateTime birthDate = new DateTime(birthYear, birthMonth, birthDay);

            // Создаем объект клиента
            return new Client(
                0, // ID будет сгенерирован БД
                lastName,
                firstName,
                passportSeries,
                passportNumber,
                birthDate,
                phoneNumber,
                gender,
                patronymic,
                email
            );
        }

        private static void DemonstrateRepositoryFeatures(Client_rep_DB repo)
        {
            Console.WriteLine("\nДемонстрация работы репозитория:");
            Console.WriteLine("==================================");

            try
            {
                // 1. Получаем количество клиентов
                int totalCount = repo.GetCount();
                Console.WriteLine($"1. Всего клиентов в базе: {totalCount}");

                // 2. Получаем первую страницу (первые 10 клиентов)
                Console.WriteLine("\n2. Первые 10 клиентов (пагинация):");
                var firstPage = repo.GetShortList(0, 10);
                foreach (var client in firstPage)
                {
                    Console.WriteLine($"   {client.Id}: {client.LastName} {client.FirstName} {client.Patronymic}");
                }

                // 3. Поиск по фамилии
                Console.WriteLine("\n3. Поиск клиентов с фамилией, содержащей 'ов':");
                var searchResults = repo.SearchByLastName("ов");
                if (searchResults.Any())
                {
                    foreach (var client in searchResults.Take(5))
                    {
                        Console.WriteLine($"   {client.Id}: {client.LastName} {client.FirstName}");
                    }
                    if (searchResults.Count > 5)
                    {
                        Console.WriteLine($"   ... и еще {searchResults.Count - 5} клиентов");
                    }
                }
                else
                {
                    Console.WriteLine("   Клиенты не найдены");
                }

                // 4. Обновление первого клиента
                Console.WriteLine("\n4. Обновление данных клиента:");
                if (firstPage.Any())
                {
                    var firstClient = repo.GetById(firstPage[0].Id);
                    Console.WriteLine($"   До обновления: {firstClient.LastName} {firstClient.FirstName}");

                    // Изменяем email
                    var updatedClient = new Client(
                        firstClient.Id,
                        firstClient.LastName,
                        firstClient.FirstName,
                        firstClient.PassportSeries,
                        firstClient.PassportNumber,
                        firstClient.BirthDate,
                        firstClient.PhoneNumber,
                        firstClient.Gender,
                        firstClient.Patronymic,
                        "updated.email@example.com"
                    );

                    var result = repo.Update(firstClient.Id, updatedClient);
                    Console.WriteLine($"   После обновления: Email изменен на {result.Email}");
                }

                // 5. Добавление еще одного тестового клиента
                Console.WriteLine("\n5. Добавление тестового клиента:");
                var testClient = new Client(
                    0,
                    "Тестовый",
                    "Клиент",
                    "9999",
                    "999999",
                    new DateTime(1985, 1, 1),
                    "79123456789",
                    Client.Genders.Male,
                    "Тестович",
                    "test@example.com"
                );

                var addedTestClient = repo.Add(testClient);
                Console.WriteLine($"   Добавлен: {addedTestClient.LastName} {addedTestClient.FirstName} (ID: {addedTestClient.Id})");

                // 6. Удаление тестового клиента
                Console.WriteLine("\n6. Удаление тестового клиента:");
                bool deleted = repo.Delete(addedTestClient.Id);
                Console.WriteLine($"   Клиент удален: {deleted}");

                // 7. Итоговое количество
                Console.WriteLine("\n7. Итоговое количество клиентов:");
                Console.WriteLine($"   Всего клиентов после операций: {repo.GetCount()}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при демонстрации: {ex.Message}");
            }
        }
    }
}