using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogFileViewer
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public static string Show(string prompt, string title, string defaultValue)
        {
            using (var win = new InputBox())
            {
                win.Text = title;
                win.LblLabel.Text = prompt;
                win.TxtValue.Text = defaultValue;
                if (win.ShowDialog() == DialogResult.OK)
                {
                    return win.TxtValue.Text;
                }
                return "";
            }
        }
    }
}
