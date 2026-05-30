using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class AdminPanelViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;
        public AsyncRelayCommand LockCommand { get; }
        public AsyncRelayCommand UnlockCommand { get; }

        public string AdminName => AppState.CurrentUser?.name ?? "";

        private ObservableCollection<AdminData> _users;
        public ObservableCollection<AdminData> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private AdminData _selectedUser;
        public AdminData SelectedUser //Выбор пользователя в DataGrid при помощи мыши
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    if (value != null)
                    {
                        Login = value.login;
                        Name = value.name;
                        SelectedRole = value.role;
                        FlashSerial = value.flash_serial;
                        if (Divisions != null)
                            SelectedDivision = Divisions.FirstOrDefault(d => d.id.ToString() == value.division);
                        if (Units != null)
                            SelectedUnit = Units.FirstOrDefault(u => u.number == value.unit);
                        Password = "";
                        UpdateLockVisibility();
                    }
                    else
                    {
                        ClearForm();
                    }
                }
            }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private ObservableCollection<Divisions> _divisions;
        public ObservableCollection<Divisions> Divisions
        {
            get => _divisions;
            set => SetProperty(ref _divisions, value);
        }

        private ObservableCollection<Units> _units;
        public ObservableCollection<Units> Units
        {
            get => _units;
            set => SetProperty(ref _units, value);
        }

        private Divisions _selectedDivision;
        public Divisions SelectedDivision
        {
            get => _selectedDivision;
            set => SetProperty(ref _selectedDivision, value);
        }

        private Units _selectedUnit;
        public Units SelectedUnit
        {
            get => _selectedUnit;
            set => SetProperty(ref _selectedUnit, value);
        }

        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        private string _selectedFlashSerial;
        public string FlashSerial
        {
            get => _selectedFlashSerial;
            set => SetProperty(ref _selectedFlashSerial, value);
        }

        public ObservableCollection<FlashDriveInfo> FlashDrives { get; } = new ObservableCollection<FlashDriveInfo>();

        private FlashDriveInfo _selectedFlashDrive;
        public FlashDriveInfo SelectedFlashDrive
        {
            get => _selectedFlashDrive;
            set
            {
                if (SetProperty(ref _selectedFlashDrive, value))
                {
                    if (value != null)
                    {
                        FlashSerial = value.SerialNumber;
                    }
                }
            }
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand OpenLogsCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand RefreshFlashDrivesCommand { get; }

        public AdminPanelViewModel(Window owner)
        {
            _owner = owner;
            Users = new ObservableCollection<AdminData>();
            Divisions = new ObservableCollection<Divisions>();
            Units = new ObservableCollection<Units>();

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            SaveCommand = new AsyncRelayCommand(SaveUserAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteUserAsync);
            ExitCommand = new RelayCommand(Exit);
            OpenLogsCommand = new RelayCommand(OpenLogs);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            RefreshFlashDrivesCommand = new RelayCommand(RefreshFlashDrives);
            LockCommand = new AsyncRelayCommand(LockUserAsync, () => SelectedUser != null && !SelectedUser.is_locked);
            UnlockCommand = new AsyncRelayCommand(UnlockUserAsync, () => SelectedUser != null && SelectedUser.is_locked);

            _ = LoadReferencesAsync();
            _ = LoadUsersAsync();
            RefreshFlashDrives();
        }

        private Visibility _showLock = Visibility.Collapsed;
        public Visibility ShowLock
        {
            get => _showLock;
            set => SetProperty(ref _showLock, value);
        }

        private Visibility _showUnlock = Visibility.Collapsed;
        public Visibility ShowUnlock
        {
            get => _showUnlock;
            set => SetProperty(ref _showUnlock, value);
        }

        private async Task LockUserAsync() //Блокировка аккаунта
        {
            if (SelectedUser == null) return;
            bool success = await _supabase.SetUserLockStatus(SelectedUser.id, true);
            if (success)
            {
                await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} заблокировал пользователя {SelectedUser.login}");
                SelectedUser.is_locked = true;
                await LoadUsersAsync();
                UpdateLockVisibility();
                WpfMessageBox.Show("Пользователь заблокирован.");
            }
            else WpfMessageBox.Show("Ошибка блокировки.");
        }

        private async Task UnlockUserAsync() //Разблокировка аккаунта
        {
            if (SelectedUser == null) return;
            bool success = await _supabase.SetUserLockStatus(SelectedUser.id, false);
            if (success)
            {
                await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} разблокировал пользователя {SelectedUser.login}");
                SelectedUser.is_locked = false;
                await LoadUsersAsync();
                UpdateLockVisibility();
                WpfMessageBox.Show("Пользователь разблокирован.");
            }
            else WpfMessageBox.Show("Ошибка разблокировки.");
        }

        private void UpdateLockVisibility() //Обновление текста изменения статуса блокировки на кнопке в контекстном меню
        {
            if (SelectedUser != null)
            {
                ShowLock = SelectedUser.is_locked ? Visibility.Collapsed : Visibility.Visible;
                ShowUnlock = SelectedUser.is_locked ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                ShowLock = Visibility.Collapsed;
                ShowUnlock = Visibility.Collapsed;
            }
        }

        private async Task LoadReferencesAsync() //Добавление названий подразделений и частей в combobox в админ панели
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);

            await AppState.LoadUnitsAsync();
            Units.Clear();
            foreach (var unit in AppState.units)
                Units.Add(unit);
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                await Task.Delay(200);
                bool success = await _supabase.AdminInformation();
                if (!success || AppState.AdminDataUsers == null)
                {
                    WpfMessageBox.Show("Ошибка загрузки данных");
                    return;
                }

                var list = AppState.AdminDataUsers
                    .Where(w => w.role != "admin")
                    .Select(w => new AdminData
                    {
                        id = w.id,
                        email = w.email,
                        role = w.role,
                        login = w.login,
                        name = w.name,
                        division = w.division,
                        unit = w.unit,
                        is_locked = w.is_locked,
                        flash_serial = w.flash_serial
                    })
                    .ToList();

                Users.Clear();
                foreach (var user in list)
                    Users.Add(user);
            }
            catch (Exception ex)
            {
                await AppState.Logger.Error(ex);
                WpfMessageBox.Show($"Ошибка: {ex}");
            }
        }

        private async Task SaveUserAsync() //Добавление или обновление данных пользователя
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Name) || SelectedDivision == null ||
                SelectedUnit == null || string.IsNullOrWhiteSpace(SelectedRole))
            {
                WpfMessageBox.Show("Заполните все поля!");
                return;
            }

            int? divisionName = SelectedDivision.id;
            int? unitNumber = SelectedUnit.id;
            bool result;

            if (SelectedUser != null)
            {
                await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} обновил данные пользователя {Login}");
                result = await _supabase.UpdateUser(SelectedUser.id, Login, Password, Name, divisionName, unitNumber, SelectedRole, FlashSerial);
                if (result) WpfMessageBox.Show("Пользователь обновлён!");
            }
            else
            {
                await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} добавил пользователя {Login}");
                result = await _supabase.CreateUser(Login, Password, Name, divisionName, unitNumber, SelectedRole, FlashSerial);
                if (result) WpfMessageBox.Show("Пользователь успешно добавлен!");
            }

            if (result)
            {
                await LoadUsersAsync();
                ClearForm();
                SelectedUser = null;
            }
            else
            {
                WpfMessageBox.Show(SelectedUser != null ? "Ошибка при обновлении." : "Такой пользователь уже существует.");
            }
        }

        private async Task DeleteUserAsync() //Удаление выбранного пользователя
        {
            if (SelectedUser == null)
            {
                WpfMessageBox.Show("Выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = WpfMessageBox.Show($"Удалить пользователя {SelectedUser.login} (ID: {SelectedUser.id})?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            bool success = await _supabase.DeleteUser(SelectedUser.id);
            if (success)
            {
                await AppState.Logger.Info($"Администратор {AppState.CurrentUser.name} удалил пользователя {SelectedUser.login}");
                WpfMessageBox.Show("Пользователь удалён.");
                await LoadUsersAsync();
                ClearForm();
                SelectedUser = null;
            }
            else
            {
                WpfMessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshFlashDrives() //Обновление списка доступных флешек и автоподбор если флешка одна
        {
            var drives = AppState.UsbAuth.GetAllAuthFlashDrives(); //Получаем все флешки содержащие нужный документ
            FlashDrives.Clear();
            foreach (var d in drives)
                FlashDrives.Add(d);

            //Автовыбор флешки, если она одна
            if (FlashDrives.Count == 1)
                SelectedFlashDrive = FlashDrives[0]; 
        }

        private void Exit() //Выход из админ панели
        {
            MainWindow main = new MainWindow();
            main.Show();
            _owner.Hide();
        }

        private void ClearSelection() //Очистка выбранного пользователя
        {
            SelectedUser = null;
            ClearForm();
        }

        private void OpenLogs() //Переход на форму логов
        {
            var logsWindow = new Views.LogsWindow();
            logsWindow.Owner = _owner;
            logsWindow.ShowDialog();
        }

        private void ClearForm() //Метод очистки формы
        {
            Login = "";
            Password = "";
            Name = "";
            FlashSerial = "";
            SelectedDivision = null;
            SelectedUnit = null;
            SelectedRole = "";
        }
    }
}
