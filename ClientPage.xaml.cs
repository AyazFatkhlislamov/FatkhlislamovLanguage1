using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FatkhlislamovLanguage;

namespace SavelyevLanguage
{
    public partial class ClientPage : Page
    {
        private int _currentPage = 1;

        public ClientPage()
        {
            InitializeComponent();

            // Загружаем пол в КомбоБокс с добавлением "Все"
            var allGenders = FatkhlislamovLanguageEntities.GetContext().Gender.ToList();
            allGenders.Insert(0, new Gender { Name = "Все" });
            ComboGender.ItemsSource = allGenders;
            ComboGender.DisplayMemberPath = "Name";
            ComboGender.SelectedIndex = 0;

            UpdateClients();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null)); // Передаем null для добавления
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var client = button.DataContext as Client;
            if (client == null) return;

            Manager.MainFrame.Navigate(new AddEditPage(client)); // Передаем клиента для редактирования
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Обновление данных при возврате на страницу
            if (Visibility == Visibility.Visible)
            {
                FatkhlislamovLanguageEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                UpdateClients();
            }
        }


        private void UpdateClients()
        {
            if (ClientListView == null) return;

            var currentClients = FatkhlislamovLanguageEntities.GetContext().Client.ToList();
            int totalRecords = FatkhlislamovLanguageEntities.GetContext().Client.Count(); // Изменено для корректного отображения "из 100"

            // 1. ПОИСК (ФИО, email, телефон)
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                string searchString = TBoxSearch.Text.ToLower();
                // Очистка от спецсимволов для умного поиска по номеру
                string searchPhone = searchString.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace("+", "");

                currentClients = currentClients.Where(p =>
                    (p.FullName != null && p.FullName.ToLower().Contains(searchString)) ||
                    (p.Email != null && p.Email.ToLower().Contains(searchString)) ||
                    (p.Phone != null && p.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace("+", "").Contains(searchPhone))
                ).ToList();
            }

            // 2. ФИЛЬТРАЦИЯ (по полу)
            if (ComboGender.SelectedIndex > 0)
            {
                var selectedGender = ComboGender.SelectedItem as Gender;
                currentClients = currentClients.Where(p => p.GenderCode == selectedGender.Code).ToList();
            }

            // 3. СОРТИРОВКА
            if (ComboSort != null)
            {
                switch (ComboSort.SelectedIndex)
                {
                    case 1: // По фамилии (от А до Я)
                        // Поскольку в вашей БД фамилии сохранены в столбце FirstName, 
                        // мы сортируем именно по нему
                        currentClients = currentClients.OrderBy(p => p.FirstName).ToList();
                        break;
                    case 2: // Последнее посещение (новые к старым - убывание)
                        currentClients = currentClients.OrderByDescending(p => p.LastVisitDate).ToList();
                        break;
                    case 3: // Количество посещений (от большего к меньшему - убывание)
                        currentClients = currentClients.OrderByDescending(p => p.VisitCount).ToList();
                        break;
                }
            }

            // Количество записей ПОСЛЕ фильтрации (для надписи внизу)
            int filteredRecords = currentClients.Count;

            // ПАГИНАЦИЯ
            if (ComboType != null && ComboType.SelectedIndex >= 0)
            {
                string selectedText = ((TextBlock)ComboType.SelectedItem).Text;

                if (selectedText != "Все")
                {
                    int pageSize = Convert.ToInt32(selectedText);
                    int maxPages = (int)Math.Ceiling((double)filteredRecords / pageSize);

                    if (_currentPage > maxPages && maxPages > 0)
                        _currentPage = maxPages;
                    if (_currentPage < 1)
                        _currentPage = 1;

                    currentClients = currentClients.Skip((_currentPage - 1) * pageSize).Take(pageSize).ToList();
                    GeneratePageNumbers(maxPages);
                }
                else
                {
                    _currentPage = 1;
                    GeneratePageNumbers(1);
                }
            }

            ClientListView.ItemsSource = currentClients;

            if (TextBlockCount != null)
            {
                TextBlockCount.Text = $"{filteredRecords} из {totalRecords}";
            }
        }

        // Вызывается каждый раз, когда мы вводим текст в поиск или меняем сортировку/фильтр
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            _currentPage = 1; // Скидываем на первую страницу при любом поиске
            UpdateClients();
        }

        private void GeneratePageNumbers(int maxPages)
        {
            if (PageNumbersStackPanel == null) return;

            PageNumbersStackPanel.Children.Clear();

            for (int i = 1; i <= maxPages; i++)
            {
                int pageNumber = i;

                TextBlock tb = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = Cursors.Hand
                };

                tb.MouseLeftButtonDown += (sender, e) =>
                {
                    _currentPage = pageNumber;
                    UpdateClients();
                };

                if (i == _currentPage)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(5, 0, 5, 0),
                        Margin = new Thickness(2, 0, 2, 0),
                        Child = tb
                    };
                    PageNumbersStackPanel.Children.Add(border);
                }
                else
                {
                    tb.Margin = new Thickness(5, 0, 5, 0);
                    PageNumbersStackPanel.Children.Add(tb);
                }
            }
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentPage = 1;
            UpdateClients();
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdateClients();
            }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            // Берем кол-во записей именно с учетом фильтров, чтобы стрелочка вправо не листала пустые страницы
            var currentClients = FatkhlislamovLanguageEntities.GetContext().Client.ToList();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                string searchString = TBoxSearch.Text.ToLower();
                string searchPhone = searchString.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace("+", "");

                currentClients = currentClients.Where(p =>
                    (p.FullName != null && p.FullName.ToLower().Contains(searchString)) ||
                    (p.Email != null && p.Email.ToLower().Contains(searchString)) ||
                    (p.Phone != null && p.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace("+", "").Contains(searchPhone))
                ).ToList();
            }

            if (ComboGender.SelectedIndex > 0)
            {
                var selectedGender = ComboGender.SelectedItem as Gender;
                currentClients = currentClients.Where(p => p.GenderCode == selectedGender.Code).ToList();
            }

            int filteredRecords = currentClients.Count;

            if (ComboType.SelectedIndex >= 0)
            {
                string selectedText = ((TextBlock)ComboType.SelectedItem).Text;
                if (selectedText != "Все")
                {
                    int pageSize = Convert.ToInt32(selectedText);
                    int maxPages = (int)Math.Ceiling((double)filteredRecords / pageSize);

                    if (_currentPage < maxPages)
                    {
                        _currentPage++;
                        UpdateClients();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var client = button.DataContext as Client;
            if (client == null) return;

            if (client.ClientService.Count > 0)
            {
                MessageBox.Show("Удаление запрещено! У выбранного клиента есть информация о посещениях.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить клиента: {client.FullName}?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    FatkhlislamovLanguageEntities.GetContext().Client.Remove(client);
                    FatkhlislamovLanguageEntities.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");

                    UpdateClients();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
    }
}
