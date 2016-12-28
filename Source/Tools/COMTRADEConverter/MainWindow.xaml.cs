using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace COMTRADEConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        private COMTRADEConverterViewModel m_viewModel;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            m_viewModel = new COMTRADEConverterViewModel();
            m_viewModel.AddFiles( new string[] { "Drag and drop files here or use the add files button", "Double click on a file to remove it." } );
            DataContext = m_viewModel;
        }

        #region EventHandlers

        private void AddFilesClicked(object sender, RoutedEventArgs e)
        {
            string filter = "Disturbance Files|*.dat;*.d00;*.rcd;*.rcl;*.pqd;*.eve;*.sel|"
                + "COMTRADE Files|*.dat;*.d00|EMAX Files|*.rcd;*.rcl|PQDIF Files|*.pqd|SEL Files|*.eve;*.sel|All Files|*.*";
            OpenFileDialog dialog = new OpenFileDialog() { Multiselect = true, Filter = filter };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            m_viewModel.AddFiles(dialog.FileNames);
        }

        private void FileListDrop(object sender, System.Windows.DragEventArgs e)
        {
            string[] newFiles = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);
            m_viewModel.AddFiles(newFiles);
        }

        private void GoButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(m_viewModel.ExportPath))
                BrowseButtonClick(sender, e);

            m_viewModel.ProcessFiles();
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog() { ShowNewFolderButton = true, Description = "Choose a folder to export files to." };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                m_viewModel.ExportPath = dialog.SelectedPath;
        }

        private void ClearFilesClicked(object sender, RoutedEventArgs e)
        {
            m_viewModel.ClearFileList();
        }

        private void FileListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListBox fileList = (System.Windows.Controls.ListBox)sender;
            if (fileList.SelectedItem != null)
                m_viewModel.Files.Remove(fileList.SelectedItem.ToString());
        }

        #endregion
    }
}
