using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace GSF.Security.CertificateGenerator
{
    internal class CertificateOption
    {
        public string CommonName { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int KeyStrength { get; set; }
        public short SignatureBit { get; set; }

        public CertificateOption()
        {
            CommonName = Environment.MachineName;
            EndDate = DateTime.Now.Date.AddYears(3);
            StartDate = DateTime.Now.Date.AddDays(-1);
            KeyStrength = 2048;
            SignatureBit = 256;
        }

        public IEnumerable<int> KeyStrengths
        {
            get
            {
                yield return 1024;
                yield return 2048;
                yield return 3072;
                yield return 4096;
            }
        }


        public IEnumerable<short> SignatureBits
        {
            get
            {
                yield return 160;
                yield return 224;
                yield return 256;
                yield return 384;
                yield return 512;
            }
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