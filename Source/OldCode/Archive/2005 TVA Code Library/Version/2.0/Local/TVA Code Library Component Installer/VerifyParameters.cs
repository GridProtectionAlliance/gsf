using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TVA
{
    public partial class VerifyParameters : Form
    {
        public VerifyParameters()
        {
            InitializeComponent();
        }

        private void ButtonBrowseForAssemblyPath_Click(object sender, EventArgs e)
        {
            FolderBrowser.SelectedPath = TextBoxAssemblyPath.Text;
            
            if (FolderBrowser.ShowDialog(this) == DialogResult.OK)
                TextBoxAssemblyPath.Text = FolderBrowser.SelectedPath;
        }
    }
}