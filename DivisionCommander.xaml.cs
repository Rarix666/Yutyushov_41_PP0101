using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AISDisciplineDesc
{
    /// <summary>
    /// Логика взаимодействия для DivisionCommander.xaml
    /// </summary>
    public partial class DivisionCommander : Window
    {
        private ObservableCollection<PersonnelData> _personnelList;

        public DivisionCommander()
        {
            InitializeComponent();
            _personnelList = new ObservableCollection<PersonnelData>();
            PersonnelDataGrid.ItemsSource = _personnelList;
        }

        private async void DivisionCommander_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPersonnel();
        }

        private async Task LoadPersonnel()
        {
            try
            {
                var all = await AppState.Supabase.GetPersonnelList();
                var filtered = all.Where(p => p.unit == AppState.CurrentUser.unit && p.id != AppState.CurrentUser.id && p.role != "admin").ToList();
                _personnelList.Clear();
                foreach (var p in filtered)
                    _personnelList.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void PersonnelDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = PersonnelDataGrid.SelectedItem as PersonnelData;
            if (selected == null)
            {
                ClearForm();
                return;
            }

            txtPhone.Text = selected.phone;
            txtEmail.Text = selected.email;
            txtAddress.Text = selected.address;
            txtDivision.Text = selected.division;
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonnelDataGrid.SelectedItem as PersonnelData;
            if (selected == null)
            {
                MessageBox.Show("Выберите сотрудника для обновления.");
                return;
            }

            bool success = await AppState.Supabase.UpdateUserProfile(
                selected.id,
                txtPhone.Text,
                txtEmail.Text,
                txtAddress.Text,
                txtDivision.Text
            );

            if (success)
            {
                selected.phone = txtPhone.Text;
                selected.email = txtEmail.Text;
                selected.address = txtAddress.Text;
                selected.division = txtDivision.Text;
                PersonnelDataGrid.Items.Refresh();
                MessageBox.Show("Данные обновлены.");
                ClearForm();
                PersonnelDataGrid.SelectedItem = null;
            }
            else
            {
                MessageBox.Show("Ошибка обновления.");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            PersonnelDataGrid.SelectedItem = null;
        }

        private void txtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void ClearForm()
        {
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtDivision.Text = "";
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            WindowCommander windowCommander = new WindowCommander();
            windowCommander.Show();
            this.Close();
        }
    }
}
