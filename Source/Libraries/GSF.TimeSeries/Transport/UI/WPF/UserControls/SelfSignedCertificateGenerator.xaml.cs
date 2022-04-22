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
using System.Text;
using System.Windows;
using System.Windows.Input;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries.UI.Commands;
using Microsoft.Win32;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SelfSignedCertificateGenerator.xaml
    /// </summary>
    public partial class SelfSignedCertificateGenerator
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
            get => (string)GetValue(CertificateFileProperty);
            set => SetValue(CertificateFileProperty, value);
        }

        /// <summary>
        /// Gets or sets the command called when the certificate is successfully generated.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the common name stored in the certificate.
        /// </summary>
        public string CommonName
        {
            get => (string)GetValue(CommonNameProperty);
            set => SetValue(CommonNameProperty, value);
        }

        /// <summary>
        /// Gets the command called when the user chooses to generate a certificate.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand GenerateButtonCommand
        {
            get
            {
                if (m_generateButtonCommand is null)
                    m_generateButtonCommand = new RelayCommand(GenerateCertificate, CanGenerate);

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
            int expirationYear = DateTime.UtcNow.Year + 20;

            if (Uri.CheckHostName(commonName) == UriHostNameType.Unknown)
                throw new ArgumentException($"Common name \"{commonName}\" is not a valid host name.", nameof(commonName));

            if (!FilePath.IsValidFileName(certificatePath))
                throw new InvalidOperationException($"Invalid file path: {certificatePath}");

            ProcessStartInfo processInfo;
            Process process;

            // Attempt to use PowerShell to create a new self-signed certificate (trusting PowerShell to be more up to date with X.509 implementations and extensions)
            try
            {
                string command = $"Invoke-Command {{$cert = New-SelfSignedCertificate -Type Custom -CertStoreLocation \"Cert:\\LocalMachine\\My\" -KeyAlgorithm RSA -KeyLength 4096 -HashAlgorithm SHA256 -KeyExportPolicy Exportable -NotAfter (Get-Date -Date \"{expirationYear}-12-31\") -Subject \"CN={commonName}\"; Export-Certificate -Cert $cert -FilePath \"{certificatePath}\"}}";

                processInfo = new ProcessStartInfo("powershell.exe")
                {
                    Arguments = $"-NoProfile -NonInteractive -WindowStyle hidden -ExecutionPolicy unrestricted -EncodedCommand {Convert.ToBase64String(Encoding.Unicode.GetBytes(command))}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (process = Process.Start(processInfo))
                    process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            if (File.Exists(certificatePath))
                return;

            // Fallback on attempting to use makecert to create a new self-signed certificate, makecert.exe is normally deployed with GSF applications
            string makeCertPath = FilePath.GetAbsolutePath("makecert.exe");
            bool useShellExecute = File.Exists(makeCertPath);
            
            if (!useShellExecute)
                makeCertPath = "makecert.exe";

            processInfo = new ProcessStartInfo(makeCertPath)
            {
                Arguments = $"-r -a sha256 -len 4096 -pe -e 12/31/{expirationYear} -n \"CN={commonName}\" -ss My -sr LocalMachine \"{certificatePath}\"",
                UseShellExecute = useShellExecute,
                CreateNoWindow = true,                   // Hides window when UseShellExecute is false
                WindowStyle = ProcessWindowStyle.Hidden  // Hides window when UseShellExecute is true
            };

            using (process = Process.Start(processInfo))
                process?.WaitForExit();
        }

        private void GenerateCertificate()
        {
            SaveFileDialog saveDialog;

            //if (string.IsNullOrWhiteSpace(CommonName))
            //{
            //    MessageBox.Show(Application.Current.MainWindow, "Please enter a common name first, then click Generate.", "Cannot generate certificate");
            //    return;
            //}

            saveDialog = new SaveFileDialog
            {
                FileName = CertificateFile,
                DefaultExt = ".cer",
                Filter = "Certificate files|*.cer|All Files|*.*"
            };

            if (!saveDialog.ShowDialog().GetValueOrDefault())
                return;

            CertificateFile = saveDialog.FileName;

            if (TryMakeCertificate() && Command is not null)
                Command.Execute(CertificateFile);
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

        private bool CanGenerate() =>
            Command is null || Command.CanExecute(null);

        private void OnProcessException(Exception ex) => 
            ProcessException?.Invoke(this, new EventArgs<Exception>(ex));

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
