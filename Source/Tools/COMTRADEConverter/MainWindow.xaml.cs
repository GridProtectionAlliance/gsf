using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace COMTRADEConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        private COMTRADEConverterViewModel m_converter;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            m_converter = new COMTRADEConverterViewModel();
        }

        private void OpenClicked(object sender, RoutedEventArgs e)
        {
            m_converter.OpenFiles();
            FileListBox.Text = "";
            foreach (string file in m_converter.Files)
            {
                FileListBox.Text += file + "\n";
            }
        }

        private void ProcessFilesClicked(object sender, RoutedEventArgs e)
        {
            m_converter.ProcessFiles();
        }
    }
}
