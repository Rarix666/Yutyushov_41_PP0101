using AISDisciplineDesc.Services;
using AISDisciplineDesc.ViewModels;
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
            DataContext = new WindowCommanderViewModel(this);
        }
    }
}
