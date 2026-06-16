using AISDisciplineDesc.Core;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class ProfileViewModel : ViewModelBase
    {
        private readonly Window _owner;

        public string Name => AppState.CurrentUser?.name ?? "";
        public string Division => AppState.CurrentUser?.division_name ?? "";
        public string Role => AppState.CurrentUser?.role ?? "";
        public string Phone => AppState.CurrentUser?.phone ?? "";
        public string Address => AppState.CurrentUser?.address ?? "";
        public string Email => AppState.CurrentUser?.email ?? "";

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public AsyncRelayCommand ChangeAvatarCommand { get; }
        public RelayCommand BackCommand { get; }

        public ProfileViewModel(Window owner)
        {
            AvatarUrl = AppState.CurrentUser?.avatar_url;

            _owner = owner;
            BackCommand = new RelayCommand(GoBack);
            ChangeAvatarCommand = new AsyncRelayCommand(ChangeAvatarAsync);
        }

        private string? _avatarUrl;
        public string? AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        private async Task ChangeAvatarAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Выберите новое фото профиля"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                IsBusy = true; // опционально: показать индикатор загрузки

                byte[] imageBytes = await File.ReadAllBytesAsync(dialog.FileName);
                string fileName = $"{AppState.CurrentUser.id}_{Guid.NewGuid()}{Path.GetExtension(dialog.FileName)}";

                string publicUrl = await AppState.Supabase.UploadAvatar(imageBytes, fileName);
                if (publicUrl == null)
                {
                    WpfMessageBox.Show("Ошибка загрузки аватара на сервер.");
                    return;
                }

                bool updated = await AppState.Supabase.UpdateUserAvatar(AppState.CurrentUser.id, publicUrl);
                if (!updated)
                {
                    WpfMessageBox.Show("Не удалось обновить аватар в базе данных.");
                    return;
                }

                AppState.CurrentUser.avatar_url = publicUrl;
                AvatarUrl = publicUrl;
                OnPropertyChanged(nameof(AvatarUrl));

                WpfMessageBox.Show("Аватар успешно обновлён.");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка при смене аватара: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        } //Обновление фото профиля

        private void GoBack() //Переход в главное окно в зависимости от роли пользователя
        {
            if (AppState.CurrentUser?.role == "Командир части")
            {
                WindowCommander windowCommander = new WindowCommander();
                windowCommander.Show();
            }
            else
            {
                WindowNext windowNext = new WindowNext();
                windowNext.Show();
            }
            _owner.Hide();
        }
    }
}
