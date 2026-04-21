using AISDisciplineDesc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
    /// Логика взаимодействия для DescriptionOrder.xaml
    /// </summary>
    public partial class DescriptionOrder : Window
    {
        public DescriptionOrder(Documents document)
        {
            InitializeComponent();

            NameOrder.Content = document.Name;

            if (!string.IsNullOrEmpty(document.Description))
            {
                OrderDescriptionBox.Document.Blocks.Clear();
                OrderDescriptionBox.Document.Blocks.Add(new Paragraph(new Run(document.Description)));
            }
            ;
        }
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
