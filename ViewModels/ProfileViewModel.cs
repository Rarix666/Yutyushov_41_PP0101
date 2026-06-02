using AISDisciplineDesc.Core;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public RelayCommand BackCommand { get; }

        public ProfileViewModel(Window owner)
        {
            AvatarUrl = AppState.CurrentUser?.avatar_url;

            _owner = owner;
            BackCommand = new RelayCommand(GoBack);
        }

        private string? _avatarUrl;
        public string? AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

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
