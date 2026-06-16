using AISDisciplineDesc.Core;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

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

        private bool _isPasswordVisible = false;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        } //Отображение/Скрытие пароля

        public AsyncRelayCommand LoginCommand { get; }
        public RelayCommand TogglePasswordVisibilityCommand { get; } //Показ и скрытие пароля

        public MainWindowViewModel(Window owner)
        {
            _owner = owner;

            if (AppState.Supabase == null)
            {
                AppState.Supabase = new SupabaseClient();
            }
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);

            TogglePasswordVisibilityCommand = new RelayCommand(ShowPassword);
        }

        private void ShowPassword() //Изменение фото кнопки скрыть пароль
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task ExecuteLoginAsync() //Реализация системы авторизации
        {
            try
            {
                string login = Login;
                string password = Password;

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    WpfMessageBox.Show("Заполните все поля!");
                    return;
                }

                if (LoginAttemptTrackerService.IsLocked(login))
                {
                    WpfMessageBox.Show("Слишком много неудачных попыток. Аккаунт заблокирован на 1 минуту.",
                                       "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = await AppState.Supabase.AuthenticateUser(login, password);
                if (success)
                {
                    LoginAttemptTrackerService.Reset(login);
                    string expectedSerial = AppState.CurrentUser.flash_serial;
                    if (!AppState.UsbAuth.IsValidKeyPresent(expectedSerial))
                    {
                        WpfMessageBox.Show("Вставьте назначенную вам флешку для авторизации.");
                        return;
                    }

                    await AppState.Supabase.UpdateOverdueDocumentsAsync();

                    if (AppState.CurrentUser.role == "admin")
                    {
                        await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} вошёл в систему");
                        WpfMessageBox.Show("Вы авторизованы как администратор", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                        AdminPanel admin = new AdminPanel();
                        admin.Show();
                        _owner.Hide();
                    }
                    else if (AppState.CurrentUser.role == "Командир части")
                    {
                        await AppState.Logger.Info($"Командир части {AppState.CurrentUser.name} вошёл в систему");
                        WpfMessageBox.Show("Вы авторизованы как командир части", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                        WindowCommander commander = new WindowCommander();
                        commander.Show();
                        _owner.Hide();
                    }
                    else
                    {
                        await AppState.Logger.Info($"Командир подразделения {AppState.CurrentUser.name} вошёл в систему");
                        WpfMessageBox.Show("Авторизация прошла успешно!", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                        WindowNext window = new WindowNext();
                        window.Show();
                        _owner.Hide();
                    }
                }
                else
                {
                    LoginAttemptTrackerService.RecordFailedAttempt(login);

                    int remaining = LoginAttemptTrackerService.GetRemainingAttempts(login);
                    if (remaining > 0)
                    {
                        await AppState.Logger.Warn($"Попытка авторизации с неверными данными. Login: {login}");
                        WpfMessageBox.Show($"Неверный логин или пароль. Осталось попыток: {remaining}", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        await AppState.Logger.Warn($"Попытка авторизации с неверными данными. Login: {login}");
                        WpfMessageBox.Show("Аккаунт заблокирован на 1 минуту из-за превышения попыток.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex) 
            {
                await AppState.Logger.Error(ex);
                WpfMessageBox.Show("Ошибка авторизации");
            }
        }
    }
}
