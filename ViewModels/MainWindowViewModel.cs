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
    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly Window _owner;

        private string _login = "";
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public AsyncRelayCommand LoginCommand { get; }

        public MainWindowViewModel(Window owner)
        {
            _owner = owner;

            if (AppState.Supabase == null)
                AppState.Supabase = new SupabaseClient();

            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                string login = Login;
                string password = Password;

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }

                bool success = await AppState.Supabase.AuthenticateUser(login, password);
                if (success)
                {
                    if (AppState.CurrentUser.role == "admin")
                    {
                        MessageBox.Show("Вы авторизованы как администратор");
                        AdminPanel admin = new AdminPanel();
                        admin.Show();
                        _owner.Hide();
                    }
                    else if (AppState.CurrentUser.role == "Командир части")
                    {
                        MessageBox.Show("Вы авторизованы как командир части");
                        WindowCommander commander = new WindowCommander();
                        commander.Show();
                        _owner.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Авторизация прошла успешно!");
                        WindowNext window = new WindowNext();
                        window.Show();
                        _owner.Hide();
                    }
                }
                else
                {
                    MessageBox.Show("Данного пользователя не существует");
                }
            }
            catch
            {
                MessageBox.Show("Ошибка авторизации");
            }
        }

        public void UpdatePassword(string newPassword)
        {
            Password = newPassword;
        }
    }
}
