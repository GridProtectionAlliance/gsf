using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GSF.Diagnostics;

namespace LogFileViewer
{
    public partial class StackDetailsFilter : Form
    {
        private class KVP
        {
            public KeyValuePair<string, string> Values;
            public KVP(KeyValuePair<string, string> values)
            {
                Values = values;
            }
            public override string ToString()
            {
                return Values.Key + "=" + Values.Value;
            }
        }

        public LogStackMessages SelectedItems;

        public StackDetailsFilter(LogStackMessages details)
        {
            InitializeComponent();

            for (int x = 0; x < details.Count; x++)
            {
                checkedListBox1.Items.Add(new KVP(details[x]));
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Select an item");
                return;
            }

            SelectedItems = new LogStackMessages(checkedListBox1.CheckedItems.Cast<KVP>().Select(x=>x.Values).ToList());
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
