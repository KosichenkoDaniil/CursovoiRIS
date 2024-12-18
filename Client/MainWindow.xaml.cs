using Microsoft.Win32;
using System.ComponentModel;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage _sourseImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var tcpManager = new TcpManager();
                ImagePathTextBox.Text = openFileDialog.FileName;
                _sourseImage = new BitmapImage(new System.Uri(openFileDialog.FileName));
                ImageDisplay1.Source = _sourseImage;
                
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
            BrowseButton.IsEnabled = false;
            try
            {
                var tcpManager = new TcpManager();
                                
                var targetImage = await Task.Run(() => tcpManager.ApplyFilter(_sourseImage));
                                
                ImageDisplay2.Source = targetImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                BrowseButton.IsEnabled = true;
            }
        }

    }
}