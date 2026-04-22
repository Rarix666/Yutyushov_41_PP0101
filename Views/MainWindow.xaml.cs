using AISDisciplineDesc.Services;
using AISDisciplineDesc.ViewModels;
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
            DataContext = new MainWindowViewModel(this);
        }
        private void textBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.UpdatePassword(textBox1.Password);
            }
        }
    }
}