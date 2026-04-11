using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Логика взаимодействия для WindowCommander.xaml
    /// </summary>
    public partial class WindowCommander : Window
    {
        public WindowCommander()
        {
            InitializeComponent();
            ProfName();
            Loaded += Division_Loaded;
        }

        public void ProfName()
        {
            LabelName.Content = AppState.CurrentUser.name;
        }

        private async void Division_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadDivisionsAsync();
            DivisionComboBox.ItemsSource = AppState.divisions;
        }

        public async void ButtonCreateOrder_Click (object sender, RoutedEventArgs e)
        {
            string cunit = AppState.CurrentUser.unit;
            string cdivision = DivisionComboBox.Text;
            TextRange range = new TextRange(RichTextOrder.Document.ContentStart, RichTextOrder.Document.ContentEnd);
            string cdescription = range.Text;
            string cname = NameOrder.Text;
            string inputdate = TextDueDate.Text;
            DateTime cduedate = DateTime.ParseExact(inputdate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            DateTime cdatedispatch = DateTime.Now;

            if (cunit == "" || cdivision == "" || cdescription == "" || cname == "" || inputdate == "" || inputdate == "")
            {
                MessageBox.Show("Заполните все поля!");
            }

            bool result = await AppState.Supabase.CreateOrder(cunit, cdivision, cdescription, cname, cduedate, cdatedispatch);
            if (result)
            {
                MessageBox.Show("Приказ отправлен");
                ClearBox();
                range.Text = "";
            }
            else
            {
                MessageBox.Show("Ошибка отправления");
            }
        }

        public void ClearBox()
        {
            DivisionComboBox.Text = "";
            NameOrder.Text = "";
            TextDueDate.Text = "";
        }
        public void LabelProf_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile();
            this.Hide();
            profile.Show();
        }

        private void ButtonOrderForm_Click(object sender, RoutedEventArgs e)
        {
            WindowOrder order = new WindowOrder();
            order.Show();
            this.Hide();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
