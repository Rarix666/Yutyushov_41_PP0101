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
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public Profile()
        {
            InitializeComponent();
            ProfileInformation();
        }

        public void ProfileInformation()
        {
            LabelName.Content = AppState.CurrentUser.name;
            LabelDivision.Content = AppState.CurrentUser.division;
            LabelRole.Content = AppState.CurrentUser.role;
            LabelPhone.Content = AppState.CurrentUser.phone;
            LabelAdress.Content = "В разработке";
            LabelEmail.Content = AppState.CurrentUser.email;
        }

        public void ButtonExit_Click (object sender, RoutedEventArgs e)
        {
            if (AppState.CurrentUser.role == "Commander")
            {
                WindowCommander windowCommander = new WindowCommander();
                windowCommander.Show();
                this.Close();
            }
            if (AppState.CurrentUser.role != "Commander")
            {
                WindowNext windowNext = new WindowNext();
                this.Close();
                windowNext.Show();
            }
            
        }
    }
}
