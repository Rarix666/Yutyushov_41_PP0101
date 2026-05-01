using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class WindowOrderViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;

        // Список подразделений для ComboBox
        private ObservableCollection<dynamic> _divisions;
        public ObservableCollection<dynamic> Divisions
        {
            get => _divisions;
            set => SetProperty(ref _divisions, value);
        }

        // Выбранное подразделение
        private dynamic _selectedDivision;
        public dynamic SelectedDivision
        {
            get => _selectedDivision;
            set
            {
                if (SetProperty(ref _selectedDivision, value))
                {
                    _ = LoadDocumentsAsync();
                }
            }
        }

        // Список документов для DataGrid
        private ObservableCollection<Documents> _documents;
        public ObservableCollection<Documents> Documents
        {
            get => _documents;
            set => SetProperty(ref _documents, value);
        }

        // Выбранный документ (для контекстного меню)
        private Documents _selectedDocument;
        public Documents SelectedDocument
        {
            get => _selectedDocument;
            set => SetProperty(ref _selectedDocument, value);
        }

        // Команды
        public AsyncRelayCommand LoadDivisionsCommand { get; }
        public AsyncRelayCommand LoadDocumentsCommand { get; }
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand BackCommand { get; }

        public WindowOrderViewModel(Window owner)
        {
            _owner = owner;
            Divisions = new ObservableCollection<dynamic>();
            Documents = new ObservableCollection<Documents>();

            LoadDivisionsCommand = new AsyncRelayCommand(LoadDivisionsAsync);
            LoadDocumentsCommand = new AsyncRelayCommand(LoadDocumentsAsync);
            OpenDocumentCommand = new RelayCommand(OpenDocument, () => SelectedDocument != null);
            BackCommand = new RelayCommand(Back);

            _ = LoadDivisionsAsync();
        }

        private async Task LoadDivisionsAsync()
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);

            if (Divisions.Any())
                SelectedDivision = Divisions.First();
        }

        private async Task LoadDocumentsAsync()
        {
            try
            {
                if (SelectedDivision == null)
                {
                    Documents.Clear();
                    return;
                }

                await Task.Delay(200);
                bool success = await _supabase.DocsInformation();
                if (!success || AppState.Documentation == null)
                {
                    WpfMessageBox.Show("Ошибка загрузки данных");
                    return;
                }

                var docs = AppState.Documentation
                    .Where(w => w.unit == AppState.CurrentUser.unit && w.Division == SelectedDivision.name)
                    .Select(w => new Documents
                    {
                        Name = w.Name,
                        DateDispatch = w.DateDispatch,
                        DueDate = w.DueDate,
                        Status = w.Status,
                        Description = w.Description,
                        file_url = w.file_url
                    })
                    .ToList();

                Documents.Clear();
                foreach (var doc in docs)
                    Documents.Add(doc);
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void OpenDocument()
        {
            if (SelectedDocument == null)
            {
                WpfMessageBox.Show("Не выбрана запись для открытия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detailWindow = new DescriptionOrder(SelectedDocument);
            detailWindow.Owner = _owner;
            detailWindow.ShowDialog();
        }

        private void Back()
        {
            // Возврат к окну командира
            WindowCommander commander = new WindowCommander();
            commander.Show();
            _owner.Close();
        }
    }
}
