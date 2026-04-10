using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SavelyevLanguage;

namespace FatkhlislamovLanguage
{
    public partial class AddEditPage : Page
    {
        private Client _currentClient = new Client();

        public AddEditPage(Client selectedClient)
        {
            InitializeComponent();

            if (selectedClient != null)
            {
                _currentClient = selectedClient;
            }
            else
            {
                // Скрываем ID при добавлении
                TextBlockId.Visibility = Visibility.Collapsed;
                TBoxId.Visibility = Visibility.Collapsed;

                _currentClient.RegistrationDate = DateTime.Now;
            }

            DataContext = _currentClient;

            if (_currentClient.GenderCode == "м") RbMale.IsChecked = true;
            else if (_currentClient.GenderCode == "ж") RbFemale.IsChecked = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            string fioPattern = @"^[a-zA-Zа-яА-ЯёЁ\s\-]+$";

            // 1. Проверка Фамилии (в БД это FirstName)
            if (string.IsNullOrWhiteSpace(_currentClient.FirstName))
                errors.AppendLine("Укажите фамилию.");
            else if (_currentClient.FirstName.Length > 50)
                errors.AppendLine("Фамилия не может быть длиннее 50 символов.");
            else if (!Regex.IsMatch(_currentClient.FirstName, fioPattern))
                errors.AppendLine("Фамилия может содержать только буквы, пробел и дефис.");

            // 2. Проверка Имени (в БД это LastName)
            if (string.IsNullOrWhiteSpace(_currentClient.LastName))
                errors.AppendLine("Укажите имя.");
            else if (_currentClient.LastName.Length > 50)
                errors.AppendLine("Имя не может быть длиннее 50 символов.");
            else if (!Regex.IsMatch(_currentClient.LastName, fioPattern))
                errors.AppendLine("Имя может содержать только буквы, пробел и дефис.");

            // 3. Проверка Отчества
            if (!string.IsNullOrWhiteSpace(_currentClient.Patronymic))
            {
                if (_currentClient.Patronymic.Length > 50)
                    errors.AppendLine("Отчество не может быть длиннее 50 символов.");
                else if (!Regex.IsMatch(_currentClient.Patronymic, fioPattern))
                    errors.AppendLine("Отчество может содержать только буквы, пробел и дефис.");
            }

            // 4. Проверка Email (СТРОГАЯ ПРОВЕРКА)
            string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";
            if (string.IsNullOrWhiteSpace(_currentClient.Email) || !Regex.IsMatch(_currentClient.Email, emailPattern))
                errors.AppendLine("Укажите корректный Email (только латинские буквы, цифры и символы @ . - _).");

            // 5. Проверка Телефона (ПРОДВИНУТАЯ)
            if (string.IsNullOrWhiteSpace(_currentClient.Phone))
            {
                errors.AppendLine("Укажите номер телефона.");
            }
            else
            {
                string phone = _currentClient.Phone.Trim();

                // Проверка на допустимые символы
                if (!Regex.IsMatch(phone, @"^[\d\+\-\(\)\s]+$"))
                    errors.AppendLine("Телефон может содержать только цифры, +, -, (, ) и пробелы.");

                // Проверка: не должен начинаться со скобки
                if (phone.StartsWith("("))
                    errors.AppendLine("Номер телефона не должен начинаться со скобки.");

                // Проверка: минимум 11 цифр (считаем только цифры, игнорируя скобки и плюсы)
                int digitCount = phone.Count(char.IsDigit);
                if (digitCount < 11)
                    errors.AppendLine("Номер телефона должен содержать минимум 11 цифр.");
            }

            // 6. Проверка Даты рождения
            if (_currentClient.Birthday == null || _currentClient.Birthday == DateTime.MinValue)
                errors.AppendLine("Выберите дату рождения.");

            // 7. Проверка Пола
            if (RbMale.IsChecked == true) _currentClient.GenderCode = "м";
            else if (RbFemale.IsChecked == true) _currentClient.GenderCode = "ж";
            else errors.AppendLine("Выберите пол клиента.");

            // Если есть ошибки - выводим и прерываем сохранение
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // --- ЗАГЛУШКА ДЛЯ ФОТО ---
            // Если пользователь не выбрал фото, прописываем путь к заглушке, чтобы БД не ругалась на пустоту.
            if (string.IsNullOrWhiteSpace(_currentClient.PhotoPath))
            {
                _currentClient.PhotoPath = "res/picture.png";
            }

            // Если новый - добавляем
            if (_currentClient.ID == 0)
            {
                FatkhlislamovLanguageEntities.GetContext().Client.Add(_currentClient);
            }

            try
            {
                FatkhlislamovLanguageEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _currentClient.PhotoPath = openFileDialog.FileName;
                    ClientImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки фото: " + ex.Message);
                }
            }
        }
    }
}
