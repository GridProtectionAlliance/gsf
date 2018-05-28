using System;
using System.Windows.Forms;
using GSF.Security.Cryptography.X509;

namespace GSF.Security.CertificateGenerator
{
    internal class CertificateOption
    {
        public string CommonName { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public CertificateSigningMode KeyType { get; set; }
        public short SignatureBit { get; set; }

        public CertificateOption()
        {
            KeyType = CertificateSigningMode.RSA_2048_SHA2_256;
            CommonName = Environment.MachineName;
            EndDate = DateTime.Now.Date.AddYears(3);
            StartDate = DateTime.Now.Date.AddDays(-1);
        }

        public bool Validate()
        {
            return ValidateStartEndDate() && ValidateCommonName();
        }

        private bool ValidateCommonName()
        {
            if (string.IsNullOrEmpty(CommonName))
            {
                MessageBox.Show("Common name is empty.");
                return false;
            }
            return true;
        }


        private bool ValidateStartEndDate()
        {
            if (EndDate < StartDate)
            {
                MessageBox.Show("Start date is before end date.");
                return false;
            }
            return true;
        }

    }

}