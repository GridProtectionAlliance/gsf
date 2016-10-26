using System;
using System.Windows.Forms;

namespace OGE.Core.GSF.Diagnostics.UI
{
    public partial class FrmErrorFilterText : Form
    {
        public string ErrorText;
        public FrmErrorFilterText(string errorText)
        {
            InitializeComponent();
            ErrorText = errorText;
            TxtErrorName.Text = errorText;
        }

        private void TxtErrorName_TextChanged(object sender, EventArgs e)
        {
            ErrorText = TxtErrorName.Text;
        }


    }
}
