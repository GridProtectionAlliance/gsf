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
    public partial class RelatedTypesFilter : Form
    {
        public string SelectedItems;

        public RelatedTypesFilter(PublisherTypeDefinition types)
        {
            InitializeComponent();
            listBox1.Items.Add(types.TypeName);
            listBox1.Items.AddRange(types.RelatedTypes.Cast<object>().ToArray());
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectedItems = (string)listBox1.SelectedItem;
            if (SelectedItems == null)
            {
                MessageBox.Show("Select an item");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

        }
    }
}
