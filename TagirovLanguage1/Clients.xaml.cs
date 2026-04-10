using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TagirovLanguage1
{
    public partial class Clients : Page
    {
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;
        int PageSize = 10;

        List<Client> CurrentPageList = new List<Client>();
        List<Client> TableList = new List<Client>();

        public Clients()
        {
            InitializeComponent();
            Loaded += Clients_Loaded;
        }

        private void Clients_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Clients_Loaded;
            LoadClients();
        }

        private void LoadClients()
        {
            TableList = TagirovLanguage1Entities.GetContext()
                .Client
                .Include("Gender")
                .Include("ClientService")
                .ToList();

            ChangePage(0, 0);
        }

        private void ApplyFiltersAndSorting()
        {
            if (SearchTextBox == null || GenderFilterComboBox == null || SortComboBox == null || PageSizeComboBox == null || ClientsListView == null || PageListBox == null)
                return;

            var currentClients = TagirovLanguage1Entities.GetContext()
                .Client
                .Include("Gender")
                .Include("ClientService")
                .ToList();

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                string clearPhone = searchText.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");

                currentClients = currentClients.Where(x =>
                    (((x.LastName ?? "") + " " + (x.FirstName ?? "") + " " + (x.Patronymic ?? "")).ToLower().Contains(searchText)) ||
                    ((x.Email ?? "").ToLower().Contains(searchText)) ||
                    (((x.Phone ?? "").Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "")).Contains(clearPhone))
                ).ToList();
            }

            if (GenderFilterComboBox.SelectedItem != null)
            {
                string selectedGender = (GenderFilterComboBox.SelectedItem as ComboBoxItem).Tag.ToString();
                if (selectedGender != "All")
                    currentClients = currentClients.Where(x => x.Gender != null && x.Gender.Name == selectedGender).ToList();
            }

            if (SortComboBox.SelectedItem != null)
            {
                string selectedSort = (SortComboBox.SelectedItem as ComboBoxItem).Tag.ToString();

                switch (selectedSort)
                {
                    case "LastNameAsc":
                        currentClients = currentClients.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                        break;
                    case "LastVisitDesc":
                        currentClients = currentClients.OrderByDescending(x => x.LastVisitDate).ToList();
                        break;
                    case "VisitCountDesc":
                        currentClients = currentClients.OrderByDescending(x => x.VisitCount).ToList();
                        break;
                }
            }

            TableList = currentClients;
            CurrentPage = 0;
            ChangePage(0, 0);
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            if (PageListBox == null)
                return;

            CountRecords = TableList.Count;
            CountPage = PageSize == 0 ? 1 : (CountRecords == 0 ? 1 : (CountRecords + PageSize - 1) / PageSize);

            bool ifUpdate = true;

            if (selectedPage.HasValue)
            {
                if (selectedPage.Value >= 0 && selectedPage.Value < CountPage)
                    CurrentPage = selectedPage.Value;
                else
                    ifUpdate = false;
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (CurrentPage > 0)
                            CurrentPage--;
                        else
                            ifUpdate = false;
                        break;
                    case 2:
                        if (CurrentPage < CountPage - 1)
                            CurrentPage++;
                        else
                            ifUpdate = false;
                        break;
                }
            }

            if (!ifUpdate)
                return;

            CurrentPageList.Clear();

            if (PageSize == 0)
            {
                CurrentPageList.AddRange(TableList);
            }
            else
            {
                int start = CurrentPage * PageSize;
                int end = Math.Min(start + PageSize, CountRecords);

                for (int i = start; i < end; i++)
                    CurrentPageList.Add(TableList[i]);
            }

            PageListBox.Items.Clear();

            if (PageSize != 0)
            {
                for (int i = 1; i <= CountPage; i++)
                    PageListBox.Items.Add(i);

                PageListBox.SelectedIndex = CurrentPage;
            }

            ClientsListView.ItemsSource = null;
            ClientsListView.ItemsSource = CurrentPageList;

            TBCount.Text = CurrentPageList.Count.ToString();
            TBAllRecords.Text = "из " + CountRecords.ToString();

            LeftDirButton.Visibility = PageSize == 0 ? Visibility.Collapsed : Visibility.Visible;
            RightDirButton.Visibility = PageSize == 0 ? Visibility.Collapsed : Visibility.Visible;
            PageListBox.Visibility = PageSize == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void GenderFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox == null || PageSizeComboBox.SelectedItem == null)
                return;

            PageSize = Convert.ToInt32((PageSizeComboBox.SelectedItem as ComboBoxItem).Tag.ToString());
            CurrentPage = 0;
            ChangePage(0, 0);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem != null)
                ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            Client currentClient = (sender as Button).Tag as Client;
            Manager.MainFrame.Navigate(new AddEditPage(currentClient));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Client currentClient = (sender as Button).Tag as Client;
            if (currentClient == null)
                return;

            currentClient = TagirovLanguage1Entities.GetContext().Client
                .Include("ClientService")
                .FirstOrDefault(x => x.ID == currentClient.ID);

            if (currentClient == null)
                return;

            if (currentClient.ClientService.Count > 0)
            {
                MessageBox.Show("Нельзя удалить клиента, у которого есть посещения.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить клиента?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                TagirovLanguage1Entities.GetContext().Client.Remove(currentClient);
                TagirovLanguage1Entities.GetContext().SaveChanges();
                LoadClients();
                ApplyFiltersAndSorting();
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
                ApplyFiltersAndSorting();
        }
    }
}
