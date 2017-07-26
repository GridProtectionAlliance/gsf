using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace OGE.Core.Security.CertificateGenerator
{
    public partial class FormCertificateGenerator : Form
    {
        CertificateOption m_opt = new CertificateOption();

        public FormCertificateGenerator()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetBindings();
        }

        private void SetBindings()
        {
            textBoxCommonName.DataBindings.Add("Text", m_opt, "CommonName", false, DataSourceUpdateMode.OnPropertyChanged);

            dateTimePickerStartDate.DataBindings.Add("Value", m_opt, "StartDate", false, DataSourceUpdateMode.OnPropertyChanged);
            dateTimePickerEndDate.DataBindings.Add("Value", m_opt, "EndDate", false, DataSourceUpdateMode.OnPropertyChanged);

            comboBoxSignatureBits.DataSource = new BindingSource { DataSource = new BindingList<short>(m_opt.SignatureBits.ToList()) };
            comboBoxSignatureBits.DataBindings.Add("SelectedItem", m_opt, "SignatureBit", false, DataSourceUpdateMode.OnPropertyChanged);

            CmbKeyStrengths.DataSource = new BindingSource { DataSource = new BindingList<int>(m_opt.KeyStrengths.ToList()) };
            CmbKeyStrengths.DataBindings.Add("SelectedItem", m_opt, "KeyStrength", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_opt.Validate())
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "Key File|*.pfx";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CertificateMaker.GenerateSelfSignedCertificate(string.Format("CN={0}", m_opt.CommonName), m_opt.StartDate, m_opt.EndDate, m_opt.SignatureBit, m_opt.KeyStrength, txtPassword.Text, dlg.FileName);
                        MessageBox.Show("Done!");
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }

}