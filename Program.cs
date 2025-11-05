namespace Lombard
{
    #region Проверка класса
    public class Program
    {
        public static void Main(string[] args)
        {
            Client client = new Client(
                lastName: "Иванов",
                firstName: "Петр",
                passportSeries: "4501",
                passportNumber: "123456",
                birthDate: new DateTime(2005, 5, 15),
                phoneNumber: "+7-912-345-67-89",
                email: "example@example.com",
                gender: Client.Gender.Male
            );

            Console.WriteLine(client.GetFullInfo());
        }
    }
    #endregion
}