using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AISDisciplineDesc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppState.Supabase = new SupabaseClient();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = textBox2.Text;
                string password = textBox1.Password;

                if (login == "" || password == "")
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }
                bool loginSuccess = await AppState.Supabase.AuthenticateUser(login, password);
                if (loginSuccess)
                {
                    if (AppState.CurrentUser.role == "admin")
                    {
                        MessageBox.Show("Вы авторизованы как администратор");
                        AdminPanel admin = new AdminPanel();
                        admin.Show();
                        this.Hide();
                    }
                    if (AppState.CurrentUser.role == "Commander")
                    {
                        MessageBox.Show("Вы авторизованы как командир части");
                        WindowCommander commander = new WindowCommander();
                        commander.Show();
                        this.Hide();
                    }
                    if (AppState.CurrentUser.role != "admin" && AppState.CurrentUser.role != "Commander")
                    {
                        MessageBox.Show("Авторизация прошла успешно!");
                        this.Hide();
                        WindowNext window = new WindowNext();
                        window.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Данного пользователя не существует");
                }
            }
            catch { }
        }
    }
}