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

namespace AISDisciplineDesc.ViewModels
{
    internal class AdminPanelViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;
        public string AdminName => AppState.CurrentUser?.name ?? "";

        private ObservableCollection<AdminData> _users;
        public ObservableCollection<AdminData> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private AdminData _selectedUser;
        public AdminData SelectedUser
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
                        if (Divisions != null)
                            SelectedDivision = Divisions.FirstOrDefault(d => d.name == value.division);
                        if (Units != null)
                            SelectedUnit = Units.FirstOrDefault(u => u.number == value.unit);
                        Password = "";
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

        private ObservableCollection<dynamic> _divisions;
        public ObservableCollection<dynamic> Divisions
        {
            get => _divisions;
            set => SetProperty(ref _divisions, value);
        }

        private ObservableCollection<dynamic> _units;
        public ObservableCollection<dynamic> Units
        {
            get => _units;
            set => SetProperty(ref _units, value);
        }

        private dynamic _selectedDivision;
        public dynamic SelectedDivision
        {
            get => _selectedDivision;
            set => SetProperty(ref _selectedDivision, value);
        }

        private dynamic _selectedUnit;
        public dynamic SelectedUnit
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

        public ICommand LoadUsersCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        public AdminPanelViewModel(Window owner)
        {
            _owner = owner;
            Users = new ObservableCollection<AdminData>();
            Divisions = new ObservableCollection<dynamic>();
            Units = new ObservableCollection<dynamic>();

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            SaveCommand = new AsyncRelayCommand(SaveUserAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteUserAsync);
            ExitCommand = new RelayCommand(Exit);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            _ = LoadReferencesAsync();
            _ = LoadUsersAsync();
        }

        private async Task LoadReferencesAsync()
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
                bool success = await _supabase.AdminInformation();
                if (!success || AppState.AdminDataUsers == null)
                {
                    MessageBox.Show("Ошибка загрузки данных");
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
                        unit = w.unit
                    })
                    .ToList();

                Users.Clear();
                foreach (var user in list)
                    Users.Add(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task SaveUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Name) || SelectedDivision == null ||
                SelectedUnit == null || string.IsNullOrWhiteSpace(SelectedRole))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string divisionName = SelectedDivision.name;
            string unitNumber = SelectedUnit.number;
            bool result;

            if (SelectedUser != null)
            {
                // обновление существующего
                result = await _supabase.UpdateUser(SelectedUser.id, Login, Password, Name, divisionName, unitNumber, SelectedRole);
                if (result) MessageBox.Show("Пользователь обновлён!");
            }
            else
            {
                // добавление нового
                result = await _supabase.CreateUser(Login, Password, Name, divisionName, unitNumber, SelectedRole);
                if (result) MessageBox.Show("Пользователь успешно добавлен!");
            }

            if (result)
            {
                await LoadUsersAsync();
                ClearForm();
                SelectedUser = null;
            }
            else
            {
                MessageBox.Show(SelectedUser != null ? "Ошибка при обновлении." : "Такой пользователь уже существует.");
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {SelectedUser.login} (ID: {SelectedUser.id})?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            bool success = await _supabase.DeleteUser(SelectedUser.id);
            if (success)
            {
                MessageBox.Show("Пользователь удалён.");
                await LoadUsersAsync();
                ClearForm();
                SelectedUser = null;
            }
            else
            {
                MessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit()
        {
            MainWindow main = new MainWindow();
            main.Show();
            _owner.Close();
        }

        private void ClearSelection()
        {
            SelectedUser = null;
            ClearForm();
        }

        private void ClearForm()
        {
            Login = "";
            Password = "";
            Name = "";
            SelectedDivision = null;
            SelectedUnit = null;
            SelectedRole = "";
        }
    }
}
