namespace Lombard
{
    public class Client
    {
        #region Поля
        private readonly int _id;
        private string _lastName;
        private string _firstName;
        private string _patronymic;
        private string _passportSeries;
        private string _passportNumber;
        private string _phoneNumber;
        private DateTime _birthDate;
        private Gender _gender;
        #endregion

        #region Свойства
        public int Id => _id;

        public string LastName
        {
            get => _lastName;
            set => _lastName = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Фамилия не может быть пустой");
        }

        public string FirstName
        {
            get => _firstName;
            set => _firstName = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Имя не может быть пустым");
        }

        public string Patronymic
        {
            get => _patronymic;
            set => _patronymic = value ?? string.Empty;
        }

        public string PassportSeries
        {
            get => _passportSeries;
            set => _passportSeries = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Серия паспорта не может быть пустой");
        }

        public string PassportNumber
        {
            get => _passportNumber;
            set => _passportNumber = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Номер паспорта не может быть пустым");
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Номер телефона не может быть пустым");
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set => _birthDate = value <= DateTime.Now ? value : throw new ArgumentException("Дата рождения не может быть в будущем");
        }

        public Gender ClientGender
        {
            get => _gender;
            set => _gender = value;
        }

        public int Age => DateTime.Now.Year - _birthDate.Year - (DateTime.Now.DayOfYear < _birthDate.DayOfYear ? 1 : 0);
        #endregion

        #region Конструктор
        public Client(
            string lastName,
            string firstName,
            string passportSeries,
            string passportNumber,
            DateTime birthDate,
            string phoneNumber,
            Gender gender,
            string patronymic = null,
            int id = 0
        )
        {
            _id = id;
            LastName = lastName;
            FirstName = firstName;
            Patronymic = patronymic;
            PassportSeries = passportSeries;
            PassportNumber = passportNumber;
            BirthDate = birthDate;
            PhoneNumber = phoneNumber;
            _gender = gender;
        }
        #endregion

        #region Перечисления
        public enum Gender
        {
            Male,
            Female
        }
        #endregion

        #region Методы
        public override string ToString()
        {
            return $"{LastName} {FirstName} {Patronymic}".Trim();
        }

        public string GetFullPassportData()
        {
            return $"{PassportSeries} {PassportNumber}";
        }

        public string GetFullInfo()
        {
            return $"{ToString()}, паспорт: {GetFullPassportData()}, тел.: {PhoneNumber}, возраст: {Age}";
        }
        #endregion
    }

    #region Проверка класса
    // Дополнительный класс с точкой входа
    public class Program
    {
        public static void Main(string[] args)
        {
            // Теперь этот код находится внутри метода Main
            Client client = new Client(
                lastName: "Иванов",
                firstName: "Петр",
                passportSeries: "4501",
                passportNumber: "123456",
                birthDate: new DateTime(1990, 5, 15),
                phoneNumber: "+7-912-345-67-89",
                gender: Client.Gender.Male
            );

            // Пример использования
            Console.WriteLine(client.GetFullInfo());
        }
    }
    #endregion
}