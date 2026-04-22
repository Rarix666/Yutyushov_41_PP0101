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
                        Division = value.division ?? "";
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

        private string _division;
        public string Division
        {
            get => _division;
            set => SetProperty(ref _division, value);
        }

        public AsyncRelayCommand LoadPersonnelCommand { get; }
        public AsyncRelayCommand UpdateCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand CloseCommand { get; }

        public DivisionCommanderViewModel(Window owner)
        {
            _owner = owner;
            PersonnelList = new ObservableCollection<PersonnelData>();

            LoadPersonnelCommand = new AsyncRelayCommand(LoadPersonnelAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            ClearCommand = new RelayCommand(() => SelectedPersonnel = null);
            CloseCommand = new RelayCommand(Close);

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
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedPersonnel == null)
            {
                MessageBox.Show("Выберите сотрудника для обновления.");
                return;
            }

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
                MessageBox.Show("Данные обновлены.");
                SelectedPersonnel = null;
                ClearForm();
            }
            else
            {
                MessageBox.Show("Ошибка обновления.");
            }
        }

        private void ClearForm()
        {
            Phone = "";
            Email = "";
            Address = "";
            Division = "";
        }

        private void Close()
        {
            WindowCommander windowCommander = new WindowCommander();
            windowCommander.Show();
            _owner.Close();
        }
    }
}
