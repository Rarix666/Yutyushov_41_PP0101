using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace AISDisciplineDesc.ViewModels
{
    public class DescriptionOrderViewModel : ViewModelBase
    {
        private readonly Window _owner;

        private string _documentName;
        public string DocumentName
        {
            get => _documentName;
            set => SetProperty(ref _documentName, value);
        }

        private string _documentText;
        public string DocumentText
        {
            get => _documentText;
            set => SetProperty(ref _documentText, value);
        }

        private bool _isPdf;
        public bool IsPdf
        {
            get => _isPdf;
            set => SetProperty(ref _isPdf, value);
        }

        private byte[] _pdfData;
        public byte[] PdfData
        {
            get => _pdfData;
            set => SetProperty(ref _pdfData, value);
        }

        public RelayCommand CloseCommand { get; }

        public DescriptionOrderViewModel(Window owner, Documents document)
        {
            _owner = owner;
            DocumentName = document.Name;
            // Проверяем, является ли документ PDF
            if (!string.IsNullOrEmpty(document.file_url) &&
                document.file_url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                IsPdf = true;
                // Загружаем PDF асинхронно
                _ = LoadPdfAsync(document.file_url);
            }
            else
            {
                IsPdf = false;
                DocumentText = document.Description ?? "(Нет описания)";
            }

            CloseCommand = new RelayCommand(() => _owner.Close());
        }

        private async Task LoadPdfAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(url);
                PdfData = bytes;
            }
            catch (Exception ex)
            {
                IsPdf = false;
                DocumentText = $"Ошибка загрузки PDF: {ex.Message}";
            }
        }
    }
}
