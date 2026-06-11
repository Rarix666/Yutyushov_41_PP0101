using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfMessageBox = System.Windows.MessageBox;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class WindowCommanderViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;

        public string UserName => AppState.CurrentUser?.name ?? "";

        public int? Unit => AppState.CurrentUser?.unit ?? null;

        private ObservableCollection<dynamic> _divisions;
        public ObservableCollection<dynamic> Divisions
        {
            get => _divisions;
            set => SetProperty(ref _divisions, value);
        }

        private dynamic _selectedDivision;
        public dynamic SelectedDivision
        {
            get => _selectedDivision;
            set => SetProperty(ref _selectedDivision, value);
        }

        private string _orderName;
        public string OrderName
        {
            get => _orderName;
            set => SetProperty(ref _orderName, value);
        }

        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }
        public DateTime MinDueDate => DateTime.Today;

        //Свойства для PDF и сертификатов

        private string _pdfFilePath;
        public string PdfFilePath
        {
            get => _pdfFilePath;
            set => SetProperty(ref _pdfFilePath, value);
        }

        private bool _isSignEnabled = true;
        public bool IsSignEnabled
        {
            get => _isSignEnabled;
            set => SetProperty(ref _isSignEnabled, value);
        }

        private string? _avatarUrl;
        public string? AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        public AsyncRelayCommand LoadDivisionsCommand { get; }
        public AsyncRelayCommand<object> CreateOrderCommand { get; }
        public RelayCommand OpenProfileCommand { get; }
        public RelayCommand OpenOrdersCommand { get; }
        public RelayCommand OpenPersonnelCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand SelectPdfCommand { get; }

        public WindowCommanderViewModel(Window owner)
        {
            _owner = owner;
            Divisions = new ObservableCollection<dynamic>();
            AvatarUrl = AppState.CurrentUser?.avatar_url;

            LoadDivisionsCommand = new AsyncRelayCommand(LoadDivisionsAsync);
            CreateOrderCommand = new AsyncRelayCommand<object>(ExecuteCreateOrder);
            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenOrdersCommand = new RelayCommand(OpenOrders);
            OpenPersonnelCommand = new RelayCommand(OpenPersonnel);
            ExitCommand = new RelayCommand(Exit);
            SelectPdfCommand = new RelayCommand(SelectPdfFile);

            _ = LoadDivisionsAsync();
        }

        private async Task LoadDivisionsAsync() //Загрузка данных о подразделениях в combobox
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);
        }

        private async Task ExecuteCreateOrder(object parameter) //Публикация документа
        {
            if (parameter is not WpfRichTextBox richTextBox)
            {
                WpfMessageBox.Show("Ошибка получения описания документа");
                return;
            }

            int? cunit = Unit;
            int? cdivision = SelectedDivision?.id ?? "";
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string cdescription = range.Text;
            string cname = OrderName;
            string uploadedFileUrl = null;

            if (cunit == null || cdivision ==null ||
                string.IsNullOrWhiteSpace(cdescription) || string.IsNullOrWhiteSpace(cname))
            {
                WpfMessageBox.Show("Заполните все поля!");
                return;
            }

            if (!DueDate.HasValue)
            {
                WpfMessageBox.Show("Укажите срок исполнения.");
                return;
            }

            DateTime cduedate = DueDate.Value;
            DateTime cdatedispatch = DateTime.Now;

            if (!string.IsNullOrEmpty(PdfFilePath))
            {
                // Читаем исходный PDF
                byte[] originalPdf = await File.ReadAllBytesAsync(PdfFilePath);

                if (IsSignEnabled) //Проверка наличия сертификата для подписи
                {
                    try
                    {
                        string pfxPath = AppState.UsbAuth.GetPfxFilePath();
                        if (string.IsNullOrEmpty(pfxPath))
                        {
                            WpfMessageBox.Show("Вставьте флешку с сертификатом подписи.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var signingService = new PdfSigningService();
                        originalPdf = signingService.SignPdf(originalPdf, pfxPath, AppSettings.PfxPassword);
                    }
                    catch (Exception ex)
                    {
                        WpfMessageBox.Show($"Ошибка подписи PDF:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Шифруем общим ключом
                byte[] encryptedPdf = AppState.Encryption.Encrypt(originalPdf);
                // Генерируем уникальное имя файла
                string fileName = Guid.NewGuid().ToString() + ".pdf";
                // Загружаем зашифрованные данные (используем вторую перегрузку)
                uploadedFileUrl = await _supabase.UploadDocumentFile(encryptedPdf, fileName);
                if (uploadedFileUrl == null)
                {
                    WpfMessageBox.Show("Не удалось загрузить PDF-файл. Проверьте подключение.");
                    return;
                }
            }

            bool result = await _supabase.CreateOrder(cunit, cdivision, cdescription, cname, cduedate, cdatedispatch, uploadedFileUrl);
            if (result)
            {
                WpfMessageBox.Show("Приказ отправлен", "Отпрвление приказа", MessageBoxButton.OK, MessageBoxImage.Information);
                OrderName = "";
                DueDate = null;
                PdfFilePath = "";
                richTextBox.Document.Blocks.Clear();
                SelectedDivision = null;
            }
            else
            {
                WpfMessageBox.Show("Ошибка отправления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenProfile() //Переход в окно профиля
        {
            Profile profile = new Profile();
            profile.Show();
            _owner.Hide();
        }

        private void OpenOrders() //Переход в окно приказов
        {
            WindowOrder order = new WindowOrder();
            order.Show();
            _owner.Hide();
        }

        private void OpenPersonnel() //Переход в окно управления личными делами
        {
            DivisionCommander division = new DivisionCommander();
            division.Show();
            _owner.Hide();
        }

        private void Exit() //Выход из главного окна командира части
        {
            MainWindow main = new MainWindow();
            main.Show();
            _owner.Hide();
        }

        private void SelectPdfFile() //Выбор PDF для отправки
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf";
            if (dialog.ShowDialog() == true)
            {
                PdfFilePath = dialog.FileName;
            }
        }
    }
}
