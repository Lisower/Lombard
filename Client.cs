namespace Lombard
{
    public class Client
    {
        #region Поля
        private int _id;
        private string _lastName;
        private string _firstName;
        private string _patronymic;
        private string _passportSeries;
        private string _passportNumber;
        private string _phoneNumber;
        private string _email;
        private DateTime _birthDate;
        private Genders _gender;
        #endregion

        private const string NamePattern = @"^[a-zA-Zа-яА-ЯёЁ\- ]+$";
        private const string NamePatternOptional = @"^[a-zA-Zа-яА-ЯёЁ\- ]*$";

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

        public Genders Gender
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
            Genders gender,
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
            Gender = gender;
            Email = email;
        }
        #endregion

        #region Переопределённый конструктор

        public Client(string serializedData, SerializationFormat format)
        {
            switch (format)
            {
                case SerializationFormat.Json:
                    LoadFromJson(serializedData);
                    break;
                case SerializationFormat.Xml:
                    LoadFromXml(serializedData);
                    break;
                default:
                    throw new ArgumentException($"Неподдерживаемый формат: {format}");
            }

            if (!IsValid())
                throw new ArgumentException("Некорректные данные клиента после десериализации");
        }

        private void LoadFromJson(string json)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var client = System.Text.Json.JsonSerializer.Deserialize<Client>(json, options);
            CopyFrom(client);
        }

        private void LoadFromXml(string xml)
        {
            using var reader = new System.IO.StringReader(xml);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Client));
            var client = (Client)serializer.Deserialize(reader);
            CopyFrom(client);
        }

        private void CopyFrom(Client source)
        {
            if (source == null)
                throw new ArgumentException("Некорректные данные");

            var type = typeof(Client);
            var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = field.GetValue(source);
                field.SetValue(this, value);
            }
        }

        // Конструктор без параметров для десериализации
        public Client()
        {
            _lastName = string.Empty;
            _firstName = string.Empty;
            _patronymic = string.Empty;
            _passportSeries = string.Empty;
            _passportNumber = string.Empty;
            _phoneNumber = string.Empty;
            _email = string.Empty;
            _birthDate = DateTime.MinValue;
            _gender = Genders.Male;
        }
        #endregion

        #region Перечисления
        public enum Genders
        {
            Male,
            Female
        }

        public enum SerializationFormat
        {
            Json,
            Xml,
            Yaml
        }
        #endregion

        #region Статические методы валидации
        private static void ValidateStringLength(string value, string fieldName, int minLength = 2, int maxLength = 255)
        {
            if (value.Length < minLength)
                throw new ArgumentException($"{fieldName} должна содержать минимум {minLength} символа");

            if (value.Length > maxLength)
                throw new ArgumentException($"{fieldName} не может превышать {maxLength} символов");
        }

        private static void ValidateStringFormat(string value, string pattern, string fieldName, string errorMessage)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                throw new ArgumentException($"{fieldName} {errorMessage}");
        }

        private static void ValidateNameField(string value, string fieldName, bool isRequired = true)
        {
            if (isRequired && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{fieldName} не может быть пустым");

            if (!isRequired && string.IsNullOrEmpty(value))
                return;

            ValidateStringFormat(value, isRequired ? NamePattern : NamePatternOptional,
                fieldName, "может содержать только буквы, дефисы и пробелы");

            ValidateStringLength(value, fieldName, 2, 255);
        }
        public static bool ValidateLastName(string lastName)
        {
            ValidateNameField(lastName, "Фамилия");
            return true;
        }

        public static bool ValidateFirstName(string firstName)
        {
            ValidateNameField(firstName, "Имя");
            return true;
        }

        public static bool ValidatePatronymic(string patronymic)
        {
            ValidateNameField(patronymic, "Отчество", false);
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

            ValidateStringLength(email, "Email", 0, 255);

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

        public static bool ValidateGender(Genders gender)
        {
            if (!Enum.IsDefined(typeof(Genders), gender))
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
            return $"{ToString()}, пол: {Gender}, паспорт: {GetFullPassportData()}, тел.: {PhoneNumber}, email: {Email}, возраст: {Age}";
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