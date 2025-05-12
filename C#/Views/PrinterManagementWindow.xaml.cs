using System.Windows;
using XPRINT.ViewModels;

namespace XPRINT.Views
{
    public partial class PrinterManagementWindow : Window
    {
        public PrinterManagementWindow(PrinterManagementViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}