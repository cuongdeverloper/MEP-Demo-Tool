using System.Windows;

namespace HicasDemoMEP.Views
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        public Window1(object viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void OnHelpClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Help documentation coming soon!", "Hicas MEP Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}