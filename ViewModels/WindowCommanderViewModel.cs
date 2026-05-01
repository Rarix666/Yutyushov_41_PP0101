using AISDisciplineDesc.Core;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class WindowCommanderViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;

        public string UserName => AppState.CurrentUser?.name ?? "";

        public string Unit => AppState.CurrentUser?.unit ?? "";

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

        private string _dueDate;
        public string DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        private string _pdfFilePath;
        public string PdfFilePath
        {
            get => _pdfFilePath;
            set => SetProperty(ref _pdfFilePath, value);
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

            LoadDivisionsCommand = new AsyncRelayCommand(LoadDivisionsAsync);
            CreateOrderCommand = new AsyncRelayCommand<object>(ExecuteCreateOrder);
            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenOrdersCommand = new RelayCommand(OpenOrders);
            OpenPersonnelCommand = new RelayCommand(OpenPersonnel);
            ExitCommand = new RelayCommand(Exit);
            SelectPdfCommand = new RelayCommand(SelectPdfFile);

            _ = LoadDivisionsAsync();
        }

        private async Task LoadDivisionsAsync()
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);
        }

        private async Task ExecuteCreateOrder(object parameter)
        {
            if (parameter is not WpfRichTextBox richTextBox)
            {
                WpfMessageBox.Show("Ошибка получения описания документа");
                return;
            }

            string cunit = Unit;
            string cdivision = SelectedDivision?.name ?? "";
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string cdescription = range.Text;
            string cname = OrderName;
            string inputdate = DueDate;
            string uploadedFileUrl = null;

            if (string.IsNullOrWhiteSpace(cunit) || string.IsNullOrWhiteSpace(cdivision) ||
                string.IsNullOrWhiteSpace(cdescription) || string.IsNullOrWhiteSpace(cname) ||
                string.IsNullOrWhiteSpace(inputdate))
            {
                WpfMessageBox.Show("Заполните все поля!");
                return;
            }

            if (!DateTime.TryParseExact(inputdate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime cduedate))
            {
                WpfMessageBox.Show("Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                return;
            }
            DateTime cdatedispatch = DateTime.Now;

            if (!string.IsNullOrEmpty(PdfFilePath))
            {
                uploadedFileUrl = await _supabase.UploadDocumentFile(PdfFilePath);
                if (uploadedFileUrl == null)
                {
                    WpfMessageBox.Show("Не удалось загрузить PDF-файл. Проверьте подключение и настройки бакета.");
                    return;
                }
            }

            bool result = await _supabase.CreateOrder(cunit, cdivision, cdescription, cname, cduedate, cdatedispatch, uploadedFileUrl);
            if (result)
            {
                WpfMessageBox.Show("Приказ отправлен");
                OrderName = "";
                DueDate = "";
                PdfFilePath = "";
                richTextBox.Document.Blocks.Clear();
                SelectedDivision = null;
            }
            else
            {
                WpfMessageBox.Show("Ошибка отправления");
            }
        }

        private void OpenProfile()
        {
            Profile profile = new Profile();
            profile.Show();
            _owner.Hide();
        }

        private void OpenOrders()
        {
            WindowOrder order = new WindowOrder();
            order.Show();
            _owner.Hide();
        }

        private void OpenPersonnel()
        {
            DivisionCommander division = new DivisionCommander();
            division.Show();
            _owner.Hide();
        }

        private void Exit()
        {
            MainWindow main = new MainWindow();
            main.Show();
            _owner.Close();
        }

        private void SelectPdfFile()
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
