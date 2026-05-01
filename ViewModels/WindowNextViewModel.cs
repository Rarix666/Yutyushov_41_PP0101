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
using WpfApplication = System.Windows.Application;

namespace AISDisciplineDesc.ViewModels
{
    public class WindowNextViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;

        private ObservableCollection<Documents> _documents;
        public ObservableCollection<Documents> Documents
        {
            get => _documents;
            set => SetProperty(ref _documents, value);
        }

        private Documents? _selectedDocument;
        public Documents? SelectedDocument
        {
            get => _selectedDocument;
            set => SetProperty(ref _selectedDocument, value);
        }

        private string? _avatarUrl;
        public string? AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        public AsyncRelayCommand LoadDocumentsCommand { get; }
        public AsyncRelayCommand<Documents> UpdateStatusCommand { get; }
        public RelayCommand OpenProfileCommand { get; }
        public RelayCommand ExitCommand { get; }
        public AsyncRelayCommand<Documents> OpenOrderCommand { get; }

        public WindowNextViewModel()
        {
            Documents = new ObservableCollection<Documents>();
            AvatarUrl = AppState.CurrentUser?.avatar_url;

            LoadDocumentsCommand = new AsyncRelayCommand(LoadDocumentsAsync);
            UpdateStatusCommand = new AsyncRelayCommand<Documents>(UpdateStatusAsync);
            OpenProfileCommand = new RelayCommand(OpenProfile);
            ExitCommand = new RelayCommand(Exit);
            OpenOrderCommand = new AsyncRelayCommand<Documents>(OpenOrderAsync);

            _ = LoadDocumentsAsync();
        }

        private async Task LoadDocumentsAsync()
        {
            try
            {
                bool success = await _supabase.DocsInformation();
                if (!success || AppState.Documentation == null)
                {
                    WpfMessageBox.Show("Ошибка загрузки данных");
                    return;
                }

                var docs = AppState.Documentation
                    .Where(w => w.Division == AppState.CurrentUser.division &&
                                w.unit == AppState.CurrentUser.unit &&
                                w.Status != "Выполнено")
                    .Select(w => new Documents
                    {
                        id = w.id,
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

        private async Task UpdateStatusAsync(Documents? document)
        {
            if (document == null)
            {
                WpfMessageBox.Show("Выберите приказ для обновления.", "Внимание");
                return;
            }

            var result = WpfMessageBox.Show($"Обновить статус документа {document.Name}?",
                                         "Подтверждение обновления статуса",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            bool success = await _supabase.UpdateStatusOrder(document.id, "Выполнено");
            if (success)
            {
                WpfMessageBox.Show("Статус обновлён");
                await LoadDocumentsAsync();
            }
            else
            {
                WpfMessageBox.Show("Ошибка при обновлении.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenProfile()
        {
            Profile profile = new Profile();
            profile.Show();
            
            var current = WpfApplication.Current.Windows.OfType<WindowNext>().FirstOrDefault();
            current?.Close();
        }

        private void Exit()
        {
            MainWindow main = new MainWindow();
            main.Show();

            var current = WpfApplication.Current.Windows.OfType<WindowNext>().FirstOrDefault();
            current?.Close();
        }

        private async Task OpenOrderAsync(Documents? documents)
        {
            if (documents == null)
            {
                WpfMessageBox.Show("Не выбрана запись для открытия.", "Ошибка");
                return;
            }

            var detailWindow = new DescriptionOrder(documents);
            detailWindow.Owner = WpfApplication.Current.Windows[0] as Window;
            detailWindow.ShowDialog();
        }
    }
}
