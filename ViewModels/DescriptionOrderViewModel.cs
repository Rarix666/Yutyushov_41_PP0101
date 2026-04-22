using AISDisciplineDesc.Core;
using AISDisciplineDesc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace AISDisciplineDesc.ViewModels
{
    internal class DescriptionOrderViewModel : ViewModelBase
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

        public RelayCommand CloseCommand { get; }

        public DescriptionOrderViewModel(Window owner, Documents document)
        {
            _owner = owner;
            DocumentName = document.Name;
            DocumentText = document.Description ?? "";
            CloseCommand = new RelayCommand(() => _owner.Close());
        }
    }
}
