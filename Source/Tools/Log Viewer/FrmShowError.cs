using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GSF.Diagnostics.UI
{
    internal partial class FrmShowError : Form
    {
        public FrmShowError()
        {
            InitializeComponent();
        }

        private void FrmShowError_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
