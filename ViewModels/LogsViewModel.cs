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
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;

        private ObservableCollection<LogEntry> _logs;
        private readonly Window _owner;
        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        // Фильтры
        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        private string _selectedLevel;
        public string SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }

        private string _userLogin;
        public string UserLogin
        {
            get => _userLogin;
            set => SetProperty(ref _userLogin, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<string> LevelList { get; } = new ObservableCollection<string>
        {
            "INFO", "WARN", "ERROR"
        }; //Уровни логов для выпадающего списка

        public ObservableCollection<int> PageSizeList { get; } = new ObservableCollection<int> { 50, 100, 200, 500 }; //Установка размера страницы лога

        private int _page = 1;
        public int Page
        {
            get => _page;
            set
            {
                if (SetProperty(ref _page, value))
                    _ = LoadLogsAsync();
            }
        }

        private int _pageSize = 100; //Количество выводимых записей логов на одной странице
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                    Page = 1;
                    _ = LoadLogsAsync();
                }
            }
        }

        private int _totalCount;
        public int TotalCount //Общее количество записей
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value))
                    OnPropertyChanged(nameof(TotalPages)); 
            }
        }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize; //Определение количества страниц с записями

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand ApplyFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public RelayCommand ExitCommand { get; }

        public LogsViewModel()
        {
            Logs = new ObservableCollection<LogEntry>();
            ExitCommand = new RelayCommand(CloseWindow);

            ApplyFiltersCommand = new AsyncRelayCommand(ApplyFiltersAsync);
            NextPageCommand = new RelayCommand(() => { if (Page < TotalPages) Page++; }, () => Page < TotalPages);
            PrevPageCommand = new RelayCommand(() => { if (Page > 1) Page--; }, () => Page > 1);

            _ = LoadLogsAsync();
        }

        private void CloseWindow()
        {
            foreach (var window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.LogsWindow logsWindow && logsWindow.DataContext == this)
                {
                    logsWindow.Close();
                    break;
                }
            }
        }

        private async Task ApplyFiltersAsync()
        {
            Page = 1;
            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var (logs, total) = await _supabase.GetLogsAsync(
                    p_page: Page,
                    p_page_size: PageSize,
                    p_from_date: FromDate,
                    p_to_date: ToDate,
                    level: SelectedLevel,
                    search: SearchText,
                    userLogin: UserLogin
                );

                Logs.Clear();
                foreach (var log in logs)
                    Logs.Add(log);
                TotalCount = total;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка загрузки логов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
