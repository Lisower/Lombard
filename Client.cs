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
        private string _email;
        private DateTime _birthDate;
        private Gender _gender;
        #endregion

        #region Свойства
        public int Id => _id;

        public string LastName
        {
            get => _lastName;
            set
            {
                ValidateLastName(value);
                _lastName = value;
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                ValidateFirstName(value);
                _firstName = value;
            }
        }

        public string Patronymic
        {
            get => _patronymic;
            set
            {
                ValidatePatronymic(value);
                _patronymic = value ?? string.Empty;
            }
        }

        public string PassportSeries
        {
            get => _passportSeries;
            set
            {
                ValidatePassportSeries(value);
                _passportSeries = value;
            }
        }

        public string PassportNumber
        {
            get => _passportNumber;
            set
            {
                ValidatePassportNumber(value);
                _passportNumber = value;
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                ValidatePhoneNumber(value);
                _phoneNumber = value;
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                ValidateEmail(value);
                _email = value;
            }
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                ValidateBirthDate(value);
                _birthDate = value;
            }
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
            string email = null,
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
            ClientGender = gender;
            Email = email;
        }
        #endregion

        #region Перечисления
        public enum Gender
        {
            Male,
            Female,
        }
        #endregion

        #region Статические методы валидации
        public static bool ValidateLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Фамилия не может быть пустой");

            if (!System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-Zа-яА-ЯёЁ\- ]+$"))
                throw new ArgumentException("Фамилия может содержать только буквы, дефисы и пробелы");

            if (lastName.Length < 2)
                throw new ArgumentException("Фамилия должна содержать минимум 2 символа");

            if (lastName.Length > 255)
                throw new ArgumentException("Фамилия не может превышать 255 символов");

            return true;
        }

        public static bool ValidateFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Имя не может быть пустым");

            if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-Zа-яА-ЯёЁ\- ]+$"))
                throw new ArgumentException("Имя может содержать только буквы, дефисы и пробелы");

            if (firstName.Length < 2)
                throw new ArgumentException("Имя должно содержать минимум 2 символа");

            if (firstName.Length > 255)
                throw new ArgumentException("Имя не может превышать 255 символов");

            return true;
        }

        public static bool ValidatePatronymic(string patronymic)
        {
            if (patronymic == null) return true;

            if (!System.Text.RegularExpressions.Regex.IsMatch(patronymic, @"^[a-zA-Zа-яА-ЯёЁ\- ]*$"))
                throw new ArgumentException("Отчество может содержать только буквы, дефисы и пробелы");

            if (patronymic.Length > 255)
                throw new ArgumentException("Отчество не может превышать 255 символов");

            return true;
        }

        public static bool ValidatePassportSeries(string passportSeries)
        {
            if (string.IsNullOrWhiteSpace(passportSeries))
                throw new ArgumentException("Серия паспорта не может быть пустой");

            if (!System.Text.RegularExpressions.Regex.IsMatch(passportSeries, @"^\d{4}$"))
                throw new ArgumentException("Серия паспорта должна состоять из 4 цифр");

            return true;
        }

        public static bool ValidatePassportNumber(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                throw new ArgumentException("Номер паспорта не может быть пустым");

            if (!System.Text.RegularExpressions.Regex.IsMatch(passportNumber, @"^\d{6}$"))
                throw new ArgumentException("Номер паспорта должен состоять из 6 цифр");

            return true;
        }

        public static bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Номер телефона не может быть пустым");

            string cleanPhone = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");

            if (cleanPhone.Length != 11)
                throw new ArgumentException("Номер телефона должен содержать 11 цифр");

            return true;
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return true;

            if (email.Length > 255)
                throw new ArgumentException("Email не может превышать 255 символов");

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new ArgumentException("Некорректный формат email");
            }
            catch
            {
                throw new ArgumentException("Некорректный формат email");
            }

            return true;
        }

        public static bool ValidateBirthDate(DateTime birthDate)
        {
            if (birthDate > DateTime.Now)
                throw new ArgumentException("Дата рождения не может быть в будущем");

            if (birthDate < DateTime.Now.AddYears(-150))
                throw new ArgumentException("Дата рождения не может быть более 150 лет назад");

            int age = DateTime.Now.Year - birthDate.Year - (DateTime.Now.DayOfYear < birthDate.DayOfYear ? 1 : 0);
            if (age < 14)
                throw new ArgumentException("Клиент должен быть старше 14 лет");

            if (age > 150)
                throw new ArgumentException("Возраст клиента не может превышать 150 лет");

            return true;
        }

        public static bool ValidateGender(Gender gender)
        {
            if (!Enum.IsDefined(typeof(Gender), gender))
                throw new ArgumentException("Некорректное значение пола");

            return true;
        }

        public static bool ValidateId(int id)
        {
            if (id < 0)
                throw new ArgumentException("ID не может быть отрицательным");

            return true;
        }
        #endregion

        #region Обычные методы
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
            return $"{ToString()}, паспорт: {GetFullPassportData()}, тел.: {PhoneNumber}, email: {Email}, возраст: {Age}";
        }

        public bool IsValid()
        {
            try
            {
                ValidateLastName(_lastName);
                ValidateFirstName(_firstName);
                ValidatePatronymic(_patronymic);
                ValidatePassportSeries(_passportSeries);
                ValidatePassportNumber(_passportNumber);
                ValidatePhoneNumber(_phoneNumber);
                ValidateEmail(_email);
                ValidateBirthDate(_birthDate);
                ValidateGender(_gender);
                ValidateId(_id);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}