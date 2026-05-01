using AISDisciplineDesc.Models;
using AISDisciplineDesc.ViewModels;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
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
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc
{
    /// <summary>
    /// Логика взаимодействия для DescriptionOrder.xaml
    /// </summary>
    public partial class DescriptionOrder : Window
    {
        private readonly DescriptionOrderViewModel _vm;
        private PdfViewer _pdfViewer;
        private MemoryStream _pdfStream;

        public DescriptionOrder(Documents document)
        {
            InitializeComponent();
            _vm = new DescriptionOrderViewModel(this, document);
            DataContext = _vm;

            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_vm.PdfData) && _vm.PdfData != null)
                {
                    LoadPdf(_vm.PdfData);
                }
            };
        }

        private void LoadPdf(byte[] pdfData)
        {
            try
            {
                // Освобождаем старые ресурсы, если окно использовалось повторно
                _pdfViewer?.Document?.Dispose();
                _pdfViewer?.Dispose();
                _pdfStream?.Dispose();

                // Создаём поток (не используем using!)
                _pdfStream = new MemoryStream(pdfData);
                var document = PdfDocument.Load(_pdfStream);

                _pdfViewer = new PdfViewer();
                _pdfViewer.Document = document;
                PdfHost.Child = _pdfViewer;
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ошибка отображения PDF: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _pdfViewer?.Document?.Dispose();
            _pdfViewer?.Dispose();
            base.OnClosed(e);
        }
    }
}
