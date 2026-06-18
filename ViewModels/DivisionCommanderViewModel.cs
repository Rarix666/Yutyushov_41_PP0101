using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.ViewModels
{
    internal class DivisionCommanderViewModel : ViewModelBase
    {
        private readonly SupabaseClient _supabase = AppState.Supabase;
        private readonly Window _owner;

        private ObservableCollection<PersonnelData> _personnelList;
        public ObservableCollection<PersonnelData> PersonnelList
        {
            get => _personnelList;
            set => SetProperty(ref _personnelList, value);
        }

        private PersonnelData _selectedPersonnel;
        public PersonnelData SelectedPersonnel //Вывод данных выбранного коммандира в отдельные textbox
        {
            get => _selectedPersonnel;
            set
            {
                if (SetProperty(ref _selectedPersonnel, value))
                {
                    if (value != null)
                    {
                        Phone = value.phone ?? "";
                        Email = value.email ?? "";
                        Address = value.address ?? "";
                        if (Divisions != null)
                            SelectedDivision = Divisions.FirstOrDefault(d => d.id == value.division);
                    }
                    else
                    {
                        ClearForm();
                    }
                }
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    IsEmailInvalid = false;
                }
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

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
            set => SetProperty(ref _selectedDivision, value);
        }

        // ----Настройка стиля полей----

        private bool _isEmailInvalid;
        public bool IsEmailInvalid
        {
            get => _isEmailInvalid;
            set => SetProperty(ref _isEmailInvalid, value);
        }

        public AsyncRelayCommand ChangeAvatarCommand { get; }
        public AsyncRelayCommand LoadPersonnelCommand { get; }
        public AsyncRelayCommand UpdateCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand CloseCommand { get; }

        public DivisionCommanderViewModel(Window owner)
        {
            _owner = owner;
            PersonnelList = new ObservableCollection<PersonnelData>();
            Divisions = new ObservableCollection<Divisions>();

            LoadPersonnelCommand = new AsyncRelayCommand(LoadPersonnelAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            ClearCommand = new RelayCommand(() => SelectedPersonnel = null);
            CloseCommand = new RelayCommand(Close);
            ChangeAvatarCommand = new AsyncRelayCommand(ChangeAvatarAsync);

            _ = LoadReferencesAsync();
            _ = LoadPersonnelAsync();
        }

        private async Task LoadPersonnelAsync()
        {
            try
            {
                var all = await _supabase.GetPersonnelList();
                var filtered = all.Where(p => p.unit == AppState.CurrentUser.unit && p.id != AppState.CurrentUser.id && p.role != "admin").ToList();
                PersonnelList.Clear();
                foreach (var p in filtered)
                    PersonnelList.Add(p);
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        } //Загрузка данных в datagrid с учётом роли пользователей

        private async Task LoadReferencesAsync()
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);
        } //Загрузка данных подразделений в combobox

        private async Task UpdateAsync()
        {
            if (SelectedPersonnel == null)
            {
                WpfMessageBox.Show("Выберите аккаунт для обновления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEmailInvalid = false; // Сброс красной подсветки email

            if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains("@")) //Проверка правильного заполнения поля email
            {
                IsEmailInvalid = true;
                return;
            }

            int? Division = SelectedDivision.id;

            bool success = await _supabase.UpdateUserProfile(
                SelectedPersonnel.id,
                Phone,
                Email,
                Address,
                Division
            );

            if (success)
            {
                string personalName = SelectedPersonnel.name;
                int personalId = SelectedPersonnel.id;
                SelectedPersonnel.phone = Phone;
                SelectedPersonnel.email = Email;
                SelectedPersonnel.address = Address;
                SelectedPersonnel.division = Division;
                int index = PersonnelList.IndexOf(SelectedPersonnel);
                if (index >= 0)
                    PersonnelList[index] = SelectedPersonnel;
                WpfMessageBox.Show("Данные обновлены.", "Обновление личного дела", MessageBoxButton.OK, MessageBoxImage.Information);

                await AppState.Logger.Info($"Пользователь {AppState.CurrentUser.login} обновил данные: ФИО: {personalName}, ID: {personalId}");

                SelectedPersonnel = null;
                _ = LoadPersonnelAsync();
                ClearForm();
            }
            else
            {
                WpfMessageBox.Show("Ошибка обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } //Обновлнение данных командира в панели управления личными делами

        private async Task ChangeAvatarAsync()
        {
            string personalName = SelectedPersonnel.name;
            int personalId = SelectedPersonnel.id;
            if (SelectedPersonnel == null)
            {
                WpfMessageBox.Show("Выберите командира подразделения для смены фото!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Выберите новое фото"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                byte[] imageBytes = await File.ReadAllBytesAsync(dialog.FileName);
                string fileName = $"{SelectedPersonnel.id}_{Guid.NewGuid()}{Path.GetExtension(dialog.FileName)}";

                string publicUrl = await AppState.Supabase.UploadAvatar(imageBytes, fileName);
                if (publicUrl == null)
                {
                    WpfMessageBox.Show("Ошибка загрузки фото на сервер");
                    return;
                }

                bool updated = await AppState.Supabase.UpdateUserAvatar(SelectedPersonnel.id, publicUrl);
                if (!updated)
                {
                    WpfMessageBox.Show("Не удалось обновить фото в базе данных");
                    return;
                }

                SelectedPersonnel.avatar_url = publicUrl;

                int index = PersonnelList.IndexOf(SelectedPersonnel);
                if (index >= 0)
                    PersonnelList[index] = SelectedPersonnel;

                WpfMessageBox.Show("Фото сотрудника обновлено");
                await AppState.Logger.Info($"Командир {AppState.CurrentUser.login} сменил фото в личном деле {personalName} (id={personalId})");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка при смене фото: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            Phone = "";
            Email = "";
            Address = "";
            SelectedDivision = null;
        } //Очистка данных на форме

        private void Close()
        {
            WindowCommander windowCommander = new WindowCommander();
            windowCommander.Show();
            _owner.Hide();
        }
    }
}
