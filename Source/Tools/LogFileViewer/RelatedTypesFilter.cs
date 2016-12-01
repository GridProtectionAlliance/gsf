using System;
using System.Linq;
using System.Windows.Forms;
using GSF.Diagnostics;

namespace LogFileViewer
{
    public partial class RelatedTypesFilter : Form
    {
        public string[] SelectedItems;

        public RelatedTypesFilter(PublisherTypeDefinition types)
        {
            InitializeComponent();
            checkedListBox1.Items.Add(types.TypeName);
            checkedListBox1.Items.AddRange(types.RelatedTypes.Cast<object>().ToArray());
            checkedListBox1.Sorted = true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectedItems = checkedListBox1.CheckedItems.Cast<string>().ToArray();
            if (SelectedItems.Length == 0)
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
