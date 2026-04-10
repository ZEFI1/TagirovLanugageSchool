using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TagirovLanguage1
{
    public partial class AddEditPage : Page
    {
        private Client _currentClient = new Client();
        private bool _isNewClient;

        public AddEditPage(Client selectedClient = null)
        {
            InitializeComponent();

            if (selectedClient == null)
            {
                _isNewClient = true;
                _currentClient = new Client();
                _currentClient.PhotoPath = "images\\picture.png";
                IdLabel.Visibility = Visibility.Collapsed;
                IdTextBox.Visibility = Visibility.Collapsed;
                MaleRadioButton.IsChecked = true;
                BirthdayDatePicker.SelectedDate = DateTime.Now;
                UpdatePhotoPreview(_currentClient.PhotoPath);
            }
            else
            {
                _isNewClient = false;
                _currentClient = TagirovLanguage1Entities.GetContext().Client.FirstOrDefault(x => x.ID == selectedClient.ID);
                LoadClientData();
            }
        }

        private void LoadClientData()
        {
            if (_currentClient == null)
                return;

            IdTextBox.Text = _currentClient.ID.ToString();
            LastNameTextBox.Text = _currentClient.LastName;
            FirstNameTextBox.Text = _currentClient.FirstName;
            PatronymicTextBox.Text = _currentClient.Patronymic;
            EmailTextBox.Text = _currentClient.Email;
            PhoneTextBox.Text = _currentClient.Phone;
            BirthdayDatePicker.SelectedDate = _currentClient.Birthday;

            if (_currentClient.GenderCode == "ж")
                FemaleRadioButton.IsChecked = true;
            else
                MaleRadioButton.IsChecked = true;

            UpdatePhotoPreview(_currentClient.PhotoPath);
        }

        private BitmapImage GetImageSource(string photoPath)
        {
            string fileName;

            if (string.IsNullOrWhiteSpace(photoPath))
                fileName = "picture.png";
            else
                fileName = Path.GetFileName(photoPath.Trim().Replace("/", "\\"));

            string clientPhotoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", fileName);
            string placeholderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "picture.png");

            string finalPath = File.Exists(clientPhotoPath) ? clientPhotoPath : placeholderPath;

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(finalPath, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            return image;
        }

        private string ValidateClient()
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                return "Введите фамилию.";

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                return "Введите имя.";

            if (LastNameTextBox.Text.Length > 50 || FirstNameTextBox.Text.Length > 50 || PatronymicTextBox.Text.Length > 50)
                return "Фамилия, имя и отчество не должны быть длиннее 50 символов.";

            string fioPattern = @"^[А-Яа-яA-Za-z\-\s]+$";
            if (!Regex.IsMatch(LastNameTextBox.Text, fioPattern))
                return "Фамилия может содержать только буквы, пробел и дефис.";

            if (!Regex.IsMatch(FirstNameTextBox.Text, fioPattern))
                return "Имя может содержать только буквы, пробел и дефис.";

            if (!string.IsNullOrWhiteSpace(PatronymicTextBox.Text) && !Regex.IsMatch(PatronymicTextBox.Text, fioPattern))
                return "Отчество может содержать только буквы, пробел и дефис.";

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                return "Введите email.";

            if (!Regex.IsMatch(EmailTextBox.Text, @"^[A-Za-z0-9._\-@]+$"))
                return "Email должен содержать только английские буквы, цифры и символы . _ - @";

            if (!Regex.IsMatch(EmailTextBox.Text, @"^[A-Za-z0-9._\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$"))
                return "Введите корректный email.";

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                return "Укажите телефон агента";
            }
            else
            {
                string ph = PhoneTextBox.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "").Replace(" ", "");
                if (ph.Length < 11 || ph.Length > 12)
                   return "Укажите правильно телефон агента";
            }


            if (BirthdayDatePicker.SelectedDate == null)
                return "Выберите дату рождения.";

            return "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = ValidateClient();
            if (errorMessage != "")
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentClient.LastName = LastNameTextBox.Text.Trim();
            _currentClient.FirstName = FirstNameTextBox.Text.Trim();
            _currentClient.Patronymic = PatronymicTextBox.Text.Trim();
            _currentClient.Email = EmailTextBox.Text.Trim();
            _currentClient.Phone = PhoneTextBox.Text.Trim();
            _currentClient.Birthday = BirthdayDatePicker.SelectedDate.Value;
            _currentClient.GenderCode = FemaleRadioButton.IsChecked == true ? "ж" : "м";

            if (string.IsNullOrWhiteSpace(_currentClient.PhotoPath))
                _currentClient.PhotoPath = "images\\picture.png";

            if (_isNewClient)
            {
                _currentClient.RegistrationDate = DateTime.Now;
                TagirovLanguage1Entities.GetContext().Client.Add(_currentClient);
            }

            try
            {
                TagirovLanguage1Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные клиента сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты")
            };

            if (myOpenFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(myOpenFileDialog.FileName);

                

                string fileName = Path.GetFileName(myOpenFileDialog.FileName);
                _currentClient.PhotoPath = "Клиенты\\" + fileName;
                UpdatePhotoPreview(_currentClient.PhotoPath);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
        }

        private void UpdatePhotoPreview(string path)
        {
            try
            {
                ClientImage.Source = GetImageSource(path);
            }
            catch
            {
                ClientImage.Source = GetImageSource(null);
            }
        }
    }
}