namespace Lombard
{
    #region Проверка класса
    public class Program
    {
        public static void Main(string[] args)
        {
            Client client = new Client(
                lastName: "Иванов",
                firstName: "Иван",
                patronymic: "Иванович",
                passportSeries: "4501",
                passportNumber: "123456",
                birthDate: new DateTime(2005, 5, 15),
                phoneNumber: "79123456789",
                email: "ivanov@example.com",
                gender: Client.Genders.Male
            );

            Console.WriteLine(client.GetFullInfo());

            // JSON пример
            var jsonData = @"{
                ""Id"": 1,
                ""LastName"": ""Петров"",
                ""FirstName"": ""Пётр"",
                ""Patronymic"": ""Петрович"",
                ""PassportSeries"": ""4321"",
                ""PassportNumber"": ""765931"",
                ""PhoneNumber"": ""79987654321"",
                ""Email"": ""petrov@example.com"",
                ""BirthDate"": ""1990-05-15T00:00:00""
            }";

            var clientFromJson = new Client(jsonData, Client.SerializationFormat.Json);
            Console.WriteLine(clientFromJson.GetFullInfo());

            // XML пример
            var xmlData = @"<?xml version=""1.0"" encoding=""utf-16""?>
            <Client>
                <Id>1</Id>
                <LastName>Сидоров</LastName>
                <FirstName>Силр</FirstName>
                <Patronymic>Сидорович</Patronymic>
                <PassportSeries>1234</PassportSeries>
                <PassportNumber>567890</PassportNumber>
                <PhoneNumber>79991234567</PhoneNumber>
                <Email>sidorov@example.com</Email>
                <BirthDate>1990-05-15T00:00:00</BirthDate>
            </Client>";

            var clientFromXml = new Client(xmlData, Client.SerializationFormat.Xml);
            Console.WriteLine(clientFromXml.GetFullInfo());
        }
    }
    #endregion
}