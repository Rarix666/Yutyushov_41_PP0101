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

namespace AISDisciplineDesc
{
    /// <summary>
    /// Логика взаимодействия для WindowOrder.xaml
    /// </summary>
    public partial class WindowOrder : Window
    {
        public WindowOrder()
        {
            InitializeComponent();
            Loaded += Division_Loaded;
            LoadDocumentData();
            DivCombobox.SelectionChanged += Div_SelectChanged; 
        }

        public void Div_SelectChanged (object sender, SelectionChangedEventArgs e)
        {
            if (DivCombobox.SelectedItem != null)
            {
                LoadDocumentData();
            }
            else
            {
                DataBaseGrid.ItemsSource = null;
            }
        }

        private async void Division_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadDivisionsAsync();
            DivCombobox.ItemsSource = AppState.divisions;
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
                    .Where(w => w.unit == AppState.CurrentUser.unit && w.Division == DivCombobox.Text)
                    .Select(w => new Documents
                    {
                        Name = w.Name,
                        DateDispatch = w.DateDispatch,
                        DueDate = w.DueDate,
                        Status = w.Status,
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

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            WindowCommander commander = new WindowCommander();
            commander.Show();
            this.Close();
        }
    }
}
