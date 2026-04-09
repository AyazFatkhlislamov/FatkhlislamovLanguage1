using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FatkhlislamovLanguage;

namespace SavelyevLanguage
{
    public partial class ClientPage : Page
    {
        private int _currentPage = 1;

        public ClientPage()
        {
            InitializeComponent();
            UpdateClients();
        }

        private void UpdateClients()
        {
            var currentClients = FatkhlislamovLanguageEntities.GetContext().Client.ToList();

            // Общее количество всех записей в БД
            int totalRecords = FatkhlislamovLanguageEntities.GetContext().Client.Count();

            // Запоминаем количество ДО разбиения на страницы (чтобы выводило 100 из 100)
            int filteredRecords = currentClients.Count;

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
                }
                else
                {
                    _currentPage = 1;
                }
            }

            ClientListView.ItemsSource = currentClients;

            // Вывод "100 из 100"
            if (TextBlockCount != null)
            {
                TextBlockCount.Text = $"{filteredRecords} из {totalRecords}";
            }

            // Обновляем цифру между синими кнопками
            if (PageNumberTextBlock != null)
            {
                PageNumberTextBlock.Text = _currentPage.ToString();
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
            var totalRecords = FatkhlislamovLanguageEntities.GetContext().Client.Count();

            if (ComboType.SelectedIndex >= 0)
            {
                string selectedText = ((TextBlock)ComboType.SelectedItem).Text;
                if (selectedText != "Все")
                {
                    int pageSize = Convert.ToInt32(selectedText);
                    int maxPages = (int)Math.Ceiling((double)totalRecords / pageSize);

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

            // Проверка из 3 пункта методички
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
