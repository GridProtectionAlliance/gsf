using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TVA
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            LabelProgressMessage.Text = "";
        }

        public void UpdateProgress(string progressMessage, int step, int total)
        {
            LabelProgressMessage.Text = progressMessage;

            if (ProgressBar.Maximum != total)
                ProgressBar.Maximum = total;

            ProgressBar.Value = step;
            Application.DoEvents();
        }
    }
}