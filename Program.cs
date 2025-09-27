namespace Lombard
{
    #region Проверка класса
    // Дополнительный класс с точкой входа
    public class Program
    {
        public static void Main(string[] args)
        {
            // Теперь этот код находится внутри метода Main
            Client client = new Client(
                lastName: "Иванов11",
                firstName: "Петр13",
                passportSeries: "4501А",
                passportNumber: "1234567",
                birthDate: new DateTime(2055, 5, 15),
                phoneNumber: "+7-912-345-67-89123",
                gender: Client.Gender.Male
            );

            // Пример использования
            Console.WriteLine(client.GetFullInfo());
        }
    }
    #endregion
}