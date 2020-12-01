using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using NoSqlUploader.ViewModel;

namespace NoSqlUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent(); 
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}