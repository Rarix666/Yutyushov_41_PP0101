using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

namespace AISDisciplineDesc
{
    /// <summary>
    /// Логика взаимодействия для AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            AdminName();
            LoadAdminDataUsers();
            Loaded += Division_Loaded;
            Loaded += Unit_Loaded;
        }

        private async void Division_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadDivisionsAsync();
            TextDiv.ItemsSource = AppState.divisions;
        }

        private async void Unit_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadUnitsAsync();
            TextUnit.ItemsSource = AppState.units;
        }

        public void AdminName()
        {
            LabelNameAdmin.Content = AppState.CurrentUser.name;
        }
        private void AdminDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = AdminDataGrid.SelectedItem as AdminData; // убедитесь, что тип правильный
            if (selected == null) return;

            // Заполняем текстовые поля
            TextLogin.Text = selected.login;
            TextName.Text = selected.name;
            TextRole.Text = selected.role;
            // Пароль не выводим
            TextPassword.Text = "";

            // Подразделение (TextDiv)
            if (TextDiv.ItemsSource != null)
            {
                var division = TextDiv.ItemsSource.Cast<dynamic>().FirstOrDefault(d => d.name == selected.division);
                TextDiv.SelectedItem = division;
            }

            // Часть (TextUnit)
            if (TextUnit.ItemsSource != null)
            {
                var unit = TextUnit.ItemsSource.Cast<dynamic>().FirstOrDefault(u => u.number == selected.unit);
                TextUnit.SelectedItem = unit;
            }
        }

        private async void LoadAdminDataUsers()
        {
            try
            {
                bool success = await AppState.Supabase.AdminInformation();
                if (!success || AppState.AdminDataUsers == null)
                {
                    MessageBox.Show("Ошибка загрузки данных");
                    return;
                }

                var AdminData = AppState.AdminDataUsers
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

                AdminDataGrid.ItemsSource = AdminData;
                AdminDataGrid.UnselectAll();
                if (AdminDataGrid.Items.Count > 0)
                    AdminDataGrid.ScrollIntoView(AdminDataGrid.Items[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = AdminDataGrid.SelectedItem as AdminData;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {selectedUser.login} (ID: {selectedUser.id})?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            bool success = await AppState.Supabase.DeleteUser(selectedUser.id);
            if (success)
            {
                MessageBox.Show("Пользователь удалён.");
                LoadAdminDataUsers(); 
            }
            else
            {
                MessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveUser_Click(object sender, EventArgs e)
        {
            string clogin = TextLogin.Text;
            string cpassword = TextPassword.Text;
            string cname = TextName.Text;
            string cdivision = (TextDiv.SelectedItem as dynamic)?.name ?? TextDiv.Text;
            string cunit = (TextUnit.SelectedItem as dynamic)?.number ?? TextUnit.Text;
            string crole = TextRole.Text;

            if (clogin == "" || cpassword == "" || cname == "" || cdivision == "" || cunit == "" || crole == "" )
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            var selectedUser = AdminDataGrid.SelectedItem as AdminData;
            bool result;

            if (selectedUser != null)
            {
                result = await AppState.Supabase.UpdateUser(selectedUser.id, clogin, cpassword, cname, cdivision, cunit, crole);
                if (result) MessageBox.Show("Пользователь обновлён!");
            }
            else
            {
                result = await AppState.Supabase.CreateUser(clogin, cpassword, cname, cdivision, cunit, crole);
                if (result) MessageBox.Show("Пользователь успешно добавлен!");
            }

            if (result)
            {
                LoadAdminDataUsers();
                ClearBox();
                TextDiv.SelectedItem = null;
                TextUnit.SelectedItem = null;
                AdminDataGrid.SelectedItem = null;
            }
            else
            {
                MessageBox.Show(selectedUser != null ? "Ошибка при обновлении." : "Такой пользователь уже существует.");
            }
        }

        private void AdminDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest(AdminDataGrid, e.GetPosition(AdminDataGrid));
            if (hit == null || hit.VisualHit is not DataGridRow)
            {
                AdminDataGrid.UnselectAll();
                ClearBox();
                TextDiv.SelectedItem = null;
                TextUnit.SelectedItem = null;
            }
        }

        public void ClearBox()
        {
            TextLogin.Text = "";
            TextPassword.Text = "";
            TextName.Text = "";
            TextRole.Text = "";
        }

        public void ButtonExit_Click(object sender, EventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }

    }
}
