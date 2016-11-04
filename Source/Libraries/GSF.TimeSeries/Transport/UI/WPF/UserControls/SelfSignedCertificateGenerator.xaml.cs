//******************************************************************************************************
//  SelfSignedCertificateGenerator.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/03/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.IO;
using GSF.TimeSeries.UI.Commands;
using Microsoft.Win32;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SelfSignedCertificateGenerator.xaml
    /// </summary>
    public partial class SelfSignedCertificateGenerator : UserControl
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event triggered when the <see cref="SelfSignedCertificateGenerator"/>
        /// encounters an exception while generating certificates.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private ICommand m_generateButtonCommand;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SelfSignedCertificateGenerator"/> class.
        /// </summary>
        public SelfSignedCertificateGenerator()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the generated certificate file.
        /// </summary>
        public string CertificateFile
        {
            get
            {
                return (string)GetValue(CertificateFileProperty);
            }
            set
            {
                SetValue(CertificateFileProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the command called when the certificate is successfully generated.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the common name stored in the certificate.
        /// </summary>
        public string CommonName
        {
            get
            {
                return (string)GetValue(CommonNameProperty);
            }
            set
            {
                SetValue(CommonNameProperty, value);
            }
        }

        /// <summary>
        /// Gets the command called when the user chooses to generate a certificate.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand GenerateButtonCommand
        {
            get
            {
                if ((object)m_generateButtonCommand == null)
                    m_generateButtonCommand = new RelayCommand(GenerateCertificate, () => CanGenerate());

                return m_generateButtonCommand;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a certificate with the given common name at the given path.
        /// </summary>
        /// <param name="commonName">The common name stored in the certificate.</param>
        /// <param name="certificateFile">The path to the certificate.</param>
        public void MakeCertificate(string commonName, string certificateFile)
        {
            string certificatePath = FilePath.GetAbsolutePath(certificateFile);
            string makeCertPath = FilePath.GetAbsolutePath("makecert.exe");

            ProcessStartInfo processInfo;

            if (Uri.CheckHostName(commonName) == UriHostNameType.Unknown)
                throw new ArgumentException(string.Format("Common name \"{0}\" is not a valid host name.", commonName), nameof(commonName));

            if (!File.Exists(makeCertPath))
                throw new FileNotFoundException("Unable to find makecert.exe", "makecert.exe");

            if (!FilePath.IsValidFileName(certificatePath))
                throw new InvalidOperationException(string.Format("Invalid file path: {0}", certificatePath));

            processInfo = new ProcessStartInfo(makeCertPath);
            processInfo.Arguments = string.Format("-r -pe -n \"CN={0}\" -ss My -sr LocalMachine \"{1}\"", commonName, certificatePath);
            processInfo.UseShellExecute = true;
            processInfo.Verb = "runas";

            Process.Start(processInfo).Dispose();
        }

        private void GenerateCertificate()
        {
            SaveFileDialog saveDialog;

            //if (string.IsNullOrWhiteSpace(CommonName))
            //{
            //    MessageBox.Show(Application.Current.MainWindow, "Please enter a common name first, then click Generate.", "Cannot generate certificate");
            //    return;
            //}

            saveDialog = new SaveFileDialog();
            saveDialog.FileName = CertificateFile;
            saveDialog.DefaultExt = ".cer";
            saveDialog.Filter = "Certificate files|*.cer|All Files|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                CertificateFile = saveDialog.FileName;

                if (TryMakeCertificate() && (object)Command != null)
                    Command.Execute(CertificateFile);
            }
        }

        private bool TryMakeCertificate()
        {
            try
            {
                MakeCertificate(CommonName, CertificateFile);
                return true;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
                return false;
            }
        }

        private bool CanGenerate()
        {
            return ((object)Command == null) || Command.CanExecute(null);
        }

        private void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Dependency property for the <see cref="CertificateFile"/> property.
        /// </summary>
        public static readonly DependencyProperty CertificateFileProperty = DependencyProperty.Register("CertificateFile", typeof(string), typeof(SelfSignedCertificateGenerator));

        /// <summary>
        /// Dependency property for the <see cref="Command"/> property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SelfSignedCertificateGenerator));

        /// <summary>
        /// Dependency property for the <see cref="CommonName"/> property.
        /// </summary>
        public static readonly DependencyProperty CommonNameProperty = DependencyProperty.Register("CommonName", typeof(string), typeof(SelfSignedCertificateGenerator));

        #endregion
    }
}
