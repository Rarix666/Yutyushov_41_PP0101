using AISDisciplineDesc.Models;
using AISDisciplineDesc.Services;
using AISDisciplineDesc.ViewModels;
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
            DataContext = new DivisionCommanderViewModel(this);
        }

        private void txtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
