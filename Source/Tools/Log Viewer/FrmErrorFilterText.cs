using System;
using System.Text.RegularExpressions;
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

        private void BtnDone_Click(object sender, EventArgs e)
        {
            if (rdoRegex.Checked)
            {
                try
                {
                    Regex r = new Regex(ErrorText);
                }
                catch (Exception)
                {
                    MessageBox.Show("Not a valid Regex");
                    return;
                }
            }

            DialogResult=DialogResult.OK;
        }
    }
}
