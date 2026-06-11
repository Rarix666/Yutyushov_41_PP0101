using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

        // Подразделения
        private ObservableCollection<Divisions> _divisions;
        public ObservableCollection<Divisions> Divisions
        {
            get => _divisions;
            set => SetProperty(ref _divisions, value);
        }

        private Divisions _selectedDivision;
        public Divisions SelectedDivision
        {
            get => _selectedDivision;
            set
            {
                if (SetProperty(ref _selectedDivision, value))
                    _ = LoadDocumentsAsync();
            }
        }

        // Документы
        private ObservableCollection<Documents> _documents;
        public ObservableCollection<Documents> Documents
        {
            get => _documents;
            set => SetProperty(ref _documents, value);
        }

        private Documents _selectedDocument;
        public Documents SelectedDocument
        {
            get => _selectedDocument;
            set => SetProperty(ref _selectedDocument, value);
        }

        // ========== Фильтры ==========
        private string _searchName = "";
        public string SearchName
        {
            get => _searchName;
            set
            {
                if (SetProperty(ref _searchName, value))
                    _ = LoadDocumentsAsync();
            }
        }

        private DateTime? _searchDateFrom;
        public DateTime? SearchDateFrom
        {
            get => _searchDateFrom;
            set
            {
                if (SetProperty(ref _searchDateFrom, value))
                    _ = LoadDocumentsAsync();
            }
        }

        private DateTime? _searchDateTo;
        public DateTime? SearchDateTo
        {
            get => _searchDateTo;
            set
            {
                if (SetProperty(ref _searchDateTo, value))
                    _ = LoadDocumentsAsync();
            }
        }

        // Список статусов
        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string>
        {
            "Выполнено",
            "Не выполнено",
            "Просрочено"
        };

        private string _selectedStatus = "Все";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    _ = LoadDocumentsAsync();
            }
        }

        // Команды
        public AsyncRelayCommand LoadDivisionsCommand { get; }
        public AsyncRelayCommand LoadDocumentsCommand { get; }
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }

        public WindowOrderViewModel(Window owner)
        {
            _owner = owner;
            Divisions = new ObservableCollection<Divisions>();
            Documents = new ObservableCollection<Documents>();

            LoadDivisionsCommand = new AsyncRelayCommand(LoadDivisionsAsync);
            LoadDocumentsCommand = new AsyncRelayCommand(LoadDocumentsAsync);
            OpenDocumentCommand = new RelayCommand(OpenDocument, () => SelectedDocument != null);
            BackCommand = new RelayCommand(Back);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

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

                var allDocs = AppState.Documentation
                    .Where(w => w.unit == AppState.CurrentUser.unit
                                && w.Division == SelectedDivision.id);

                // Фильтр по названию
                if (!string.IsNullOrWhiteSpace(SearchName))
                {
                    allDocs = allDocs.Where(w =>
                        w.Name.IndexOf(SearchName, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // Фильтр по дате регистрации
                if (SearchDateFrom.HasValue)
                {
                    allDocs = allDocs.Where(w => w.DateDispatch.Date >= SearchDateFrom.Value.Date);
                }

                if (SearchDateTo.HasValue)
                {
                    allDocs = allDocs.Where(w => w.DateDispatch.Date <= SearchDateTo.Value.Date);
                }

                // Фильтр по статусу
                if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "Все")
                {
                    allDocs = allDocs.Where(w => w.Status == SelectedStatus);
                }

                var docs = allDocs.Select(w => new Documents
                {
                    Name = w.Name,
                    DateDispatch = w.DateDispatch,
                    DueDate = w.DueDate,
                    Status = w.Status,
                    Description = w.Description,
                    file_url = w.file_url
                }).ToList();

                Documents.Clear();
                foreach (var doc in docs)
                    Documents.Add(doc);
            }
            catch (Exception ex)
            {
                await AppState.Logger.Error(ex);
                WpfMessageBox.Show($"Возникла техническая ошибка, обратитесь к администратору", "Техническая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenDocument() //Функция открытия документа
        {
            if (SelectedDocument == null)
            {
                WpfMessageBox.Show("Не выбрана запись для открытия.");
                return;
            }

            var detailWindow = new DescriptionOrder(SelectedDocument);
            detailWindow.Owner = _owner;
            detailWindow.ShowDialog();
        }

        private void Back() //Переход в главное окно
        {
            WindowCommander commander = new WindowCommander();
            commander.Show();
            _owner.Hide();
        }

        private void ClearFilters() //Очистка фильтров
        {
            SearchName = "";
            SearchDateFrom = null;
            SearchDateTo = null;
            SelectedStatus = "Все";
        }
    }
}
