using System.Windows;
using Autodesk.Revit.UI;
using HicasDemoMEP.ViewModels;

namespace HicasDemoMEP.Views
{
    public partial class Window1 : Window
    {
        public Window1(UIDocument uidoc)
        {
            InitializeComponent();

            // Set DataContext = ViewModel, đồng thời truyền hàm ẩn/hiện cửa sổ
            this.DataContext = new MainViewModel(uidoc,
                hideWindow: () => this.Hide(),
                showWindow: () => this.ShowDialog());
        }
    }
}