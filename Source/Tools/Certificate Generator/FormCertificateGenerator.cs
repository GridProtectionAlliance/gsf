using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using GSF.Security.Cryptography.X509;

namespace GSF.Security.CertificateGenerator
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

            CmbKeyStrengths.DataSource = new BindingSource { DataSource = Enum.GetValues(typeof(CertificateSigningMode)) };
            CmbKeyStrengths.DataBindings.Add("SelectedItem", m_opt, "KeyType", false, DataSourceUpdateMode.OnPropertyChanged);
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
                        using (var cert = CertificateMaker.GenerateSelfSignedCertificate(m_opt.KeyType, m_opt.CommonName, m_opt.StartDate, m_opt.EndDate))
                        {
                            File.WriteAllBytes(dlg.FileName, cert.Export(X509ContentType.Pkcs12, txtPassword.Text));

                            using (var dlg2 = new SaveFileDialog())
                            {
                                dlg2.Filter = "Certificate File|*.cer";
                                dlg2.FileName = Path.ChangeExtension(dlg.FileName, ".cer");
                                if (dlg2.ShowDialog() == DialogResult.OK)
                                {
                                    File.WriteAllBytes(dlg2.FileName, cert.Export(X509ContentType.Cert));
                                }
                            }
                        }
                    }
                }
               
                MessageBox.Show("Done!");

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