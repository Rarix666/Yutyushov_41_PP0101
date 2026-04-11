using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для WindowNext.xaml
    /// </summary>
    public partial class WindowNext : Window
    {
        public WindowNext()
        {
            InitializeComponent();
            ProfUser();
            LoadDocumentData();
        }
        
        public void ProfUser()
        {
            LabelProfile.Content = AppState.CurrentUser.name;
        }

        private async void LoadDocumentData()
        {
            try
            {
                bool success = await AppState.Supabase.DocsInformation();
                if (!success || AppState.Documentation == null)
                {
                    MessageBox.Show("Ошибка загрузки данных");
                    return;
                }

                var DocsData = AppState.Documentation
                    .Where(w => w.Division == AppState.CurrentUser.division && w.unit == AppState.CurrentUser.unit)
                    .Select(w => new Documents
                    {
                        id = w.id,
                        Name = w.Name,
                        DateDispatch = w.DateDispatch,
                        DueDate = w.DueDate,
                        Status = w.Status,
                        Description = w.Description
                    })
                    .ToList();

                DataBaseGrid.ItemsSource = DocsData;
                DataBaseGrid.UnselectAll();
                if (DataBaseGrid.Items.Count > 0)
                    DataBaseGrid.ScrollIntoView(DataBaseGrid.Items[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        
        public async void UpdateOrderStatus_Click(object sender, EventArgs e)
        {
            string status = "Выполнено";
            var selectedIdOrder = DataBaseGrid.SelectedItem as Documents;
            if (selectedIdOrder == null)
            {
                MessageBox.Show("Выберите приказ для обновления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = await AppState.Supabase.UpdateStatusOrder(selectedIdOrder.id, status);
            if (success)
            {
                MessageBox.Show("Статус обновлён");
                selectedIdOrder.Status = status; 
                DataBaseGrid.Items.Refresh();    
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenOrderText_Click(object sender, RoutedEventArgs e)
        {
            var selectedDocument = (Documents)DataBaseGrid.SelectedItem;
            if (selectedDocument == null)
            {
                MessageBox.Show("Не выбрана запись для открытия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detailWindow = new DescriptionOrder(selectedDocument);
            detailWindow.Owner = this;
            detailWindow.ShowDialog();
        }
        public void LabelProf_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile();
            this.Hide();
            profile.Show();
        }
        public void ButtonExit_Click (object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
