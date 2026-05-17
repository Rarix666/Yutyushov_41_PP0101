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
        public PersonnelData SelectedPersonnel
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
            set => SetProperty(ref _email, value);
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
        }

        private async Task LoadReferencesAsync()
        {
            await AppState.LoadDivisionsAsync();
            Divisions.Clear();
            foreach (var div in AppState.divisions)
                Divisions.Add(div);
        }

        private async Task UpdateAsync()
        {
            if (SelectedPersonnel == null)
            {
                WpfMessageBox.Show("Выберите сотрудника для обновления.");
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
                SelectedPersonnel.phone = Phone;
                SelectedPersonnel.email = Email;
                SelectedPersonnel.address = Address;
                SelectedPersonnel.division = Division;
                int index = PersonnelList.IndexOf(SelectedPersonnel);
                if (index >= 0)
                    PersonnelList[index] = SelectedPersonnel;
                WpfMessageBox.Show("Данные обновлены.");
                SelectedPersonnel = null;
                _ = LoadPersonnelAsync();
                ClearForm();
            }
            else
            {
                WpfMessageBox.Show("Ошибка обновления.");
            }
        }

        private void ClearForm()
        {
            Phone = "";
            Email = "";
            Address = "";
            SelectedDivision = null;
        }

        private void Close()
        {
            WindowCommander windowCommander = new WindowCommander();
            windowCommander.Show();
            _owner.Hide();
        }
    }
}
