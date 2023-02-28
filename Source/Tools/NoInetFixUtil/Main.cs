//******************************************************************************************************
//  Main.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  12/20/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

// ReSharper disable InconsistentNaming
namespace NoInetFixUtil
{
    public partial class Main : Form
    {
        #region [ Members ]

        // Nested Types
        private class Product
        {
            public string Name;
            public string InstallPath;
            public string ServiceOID;
            public int DisableGeneratePublisherEvidence;
        }

        // Constants
        private static readonly string[] ClientOIDs = { "2.5.29.31", "1.3.6.1.5.5.7.1.1" };

        // Fields
        private List<Product> m_products;
        private bool m_checkedEventsEnabled;
        private readonly bool m_applyAll;
        #endregion

        #region [ Constructors ]

        public Main(bool applyAll)
        {
            InitializeComponent();
            m_applyAll = applyAll;
        }

        #endregion

        #region [ Methods ]

        // Sets the initial state of the checkboxes at startup.
        private void Main_Load(object sender, EventArgs e)
        {
            StatusTextBox.AppendText("Detecting GPA products that are installed on the system... ");

            using RegistryKey gpaKey = Registry.LocalMachine.OpenSubKey(@"Software\Grid Protection Alliance");

            if (gpaKey is not null)
            {
                // Populate the list of GPA products installed on the system
                m_products = gpaKey.GetSubKeyNames()
                    .Select(productName => ConvertToProductAndDispose(gpaKey.OpenSubKey(productName)))
                    .ToList();

                // Display the list of products that will be affected by the fixes that this tool provides
                foreach (Product product in m_products)
                {
                    GPAProductsTextBox.AppendText(Environment.NewLine);
                    GPAProductsTextBox.AppendText(product.Name);
                }

                // Determine whether the service OIDs are already registered for any products in the GPA product list
                if (m_products.Any(product => product.ServiceOID is not null))
                {
                    ServiceOIDCheckBox.Checked = true;

                    // Determine whether any GPA services have been installed or uninstalled since the last time the fix was applied
                    if (m_products.Any(product => product.ServiceOID is null))
                    {
                        AppendStatusMessage("NoInetFixUtil has detected that one or more GPA" +
                                            " products have been installed since the service OID fix was last applied." +
                                            " To correct this, double-click the \"Register OIDs used by GSF services\" checkbox.");
                    }
                    else if (m_products.Any(product => product.ServiceOID is not null && product.InstallPath is null))
                    {
                        AppendStatusMessage("NoInetFixUtil has detected that the service OID fix" +
                                            " has been applied to one or more GPA products that have since been uninstalled." +
                                            " To correct this, double-click the \"Register OIDs used by GSF services\" checkbox.");
                    }
                }

                // Determine whether the publisher evidence fix is already registered for any products in the GPA product list
                if (m_products.Any(product => product.DisableGeneratePublisherEvidence != 0))
                {
                    PublisherEvidenceCheckBox.Checked = true;

                    // Determine whether any GPA services have been installed or uninstalled since the last time the fix was applied
                    if (m_products.Any(product => product.DisableGeneratePublisherEvidence == 0))
                    {
                        AppendStatusMessage("NoInetFixUtil has detected that one or more GPA" +
                                            " products have been installed since the publisher evidence fix was last applied." +
                                            " To correct this, double-click the \"Disable publisher evidence generation\" checkbox.");
                    }
                }
            }
            else
            {
                // No GPA products are installed
                m_products = new List<Product>();
            }

            AppendStatusMessage("Done.");

            // Determine if the client OIDs are already registered on this system
            if (ClientOIDs.All(IsRegistered))
                ClientOIDCheckBox.Checked = true;

            // Determine if the root certificate list auto update feature is already disabled on this system
            using (RegistryKey authRootKey = Registry.LocalMachine.OpenSubKey(@"Software\Policies\Microsoft\SystemCertificates\AuthRoot"))
            {
                object disableRootAutoUpdate = authRootKey?.GetValue("DisableRootAutoUpdate");

                if (disableRootAutoUpdate != null && !Equals(disableRootAutoUpdate, 0))
                    RootCertificateListCheckBox.Checked = true;
            }

            // Enabled checkbox checked events
            m_checkedEventsEnabled = true;

            if (!m_applyAll)
                return;

            [DllImport("wininet.dll", SetLastError = true)]
            static extern bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);

            static bool InternetConnectionDetected()
            {
                try
                {
                    return InternetGetConnectedState(out int _, 0);
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                if (!ServiceOIDCheckBox.Checked)
                    ServiceOIDCheckBox.Checked = true;

                if (!PublisherEvidenceCheckBox.Checked)
                    PublisherEvidenceCheckBox.Checked = true;

                if (!ClientOIDCheckBox.Checked)
                    ClientOIDCheckBox.Checked = true;

                // Only auto-disable root certificate list auto update if the system is not connected to the Internet
                if (!RootCertificateListCheckBox.Checked && !InternetConnectionDetected())
                    RootCertificateListCheckBox.Checked = true;
            }
            catch
            {
                // ignored
            }

            this.Close();
        }

        // Registers or unregisters the OIDs used by GPA services based on user selection.
        private void ServiceOIDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_checkedEventsEnabled)
                return;

            if (ServiceOIDCheckBox.Checked)
            {
                foreach (Product product in m_products)
                    RegisterServiceOID(product);
            }
            else
            {
                foreach (Product product in m_products)
                    UnregisterServiceOID(product);
            }
        }

        // Registers or unregisters the OIDs used by GPA client applications based on user selection.
        private void ClientOIDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_checkedEventsEnabled)
                return;

            if (ClientOIDCheckBox.Checked)
            {
                StatusTextBox.AppendText("Registering client OIDs... ");

                foreach (string clientOID in ClientOIDs)
                    RegisterOID(clientOID);
            }
            else
            {
                StatusTextBox.AppendText("Unregistring client OIDs... ");

                foreach (string clientOID in ClientOIDs)
                    UnregisterOID(clientOID);
            }

            AppendStatusMessage("Done.");
        }

        // Sets the registry key to enable or disable the automatic root certificate list updates.
        private void RootCertificateListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_checkedEventsEnabled)
                return;

            StatusTextBox.AppendText($"{(RootCertificateListCheckBox.Checked ? "Disabling" : "Enabling")} automatic root certificate list update through Windows Update... ");

            using RegistryKey authRootKey = Registry.LocalMachine.CreateSubKey(@"Software\Policies\Microsoft\SystemCertificates\AuthRoot");

            if (authRootKey is not null)
            {
                if (RootCertificateListCheckBox.Checked)
                    authRootKey.SetValue("DisableRootAutoUpdate", 1);
                else
                    authRootKey.DeleteValue("DisableRootAutoUpdate");

                AppendStatusMessage("Done.");
            }
            else
            {
                AppendStatusMessage("Failed. Unable to update the registry key.");
                m_checkedEventsEnabled = false;
                RootCertificateListCheckBox.Checked = !RootCertificateListCheckBox.Checked;
                m_checkedEventsEnabled = true;
            }
        }

        // Enables or disables publisher evidence generation.
        private void PublisherEvidenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_checkedEventsEnabled)
                return;

            foreach (Product product in m_products)
                SetDisableGeneratePublisherEvidence(product, PublisherEvidenceCheckBox.Checked);
        }

        // Registers the service OID for the given product.
        private void RegisterServiceOID(Product product)
        {
            // If the service is already registered, don't need to do anything
            if (product.ServiceOID is not null)
                return;

            // Get the path to the certificate used to obtain the OID for this fix
            string certificatePath = Path.Combine(product.InstallPath, product.Name + ".cer");

            if (!File.Exists(certificatePath))
                return;

            StatusTextBox.AppendText($"Registering service OID for {product.Name}... ");

            // Get the key algorithm of the certificate,
            // which is the OID used by the service
            X509Certificate2 certificate = new(certificatePath);
            string keyAlgorithm = certificate.GetKeyAlgorithm();

            // Determine which other products are sharing this service OID
            List<Product> productsUsingThisOID = m_products
                .Where(p => p.ServiceOID == keyAlgorithm)
                .ToList();

            // Set service OID to the key algorithm of the certificate
            product.ServiceOID = keyAlgorithm;

            // Store the OID of that certificate in case we need to unregister it later
            using (RegistryKey productKey = Registry.LocalMachine.CreateSubKey($@"Software\Grid Protection Alliance\{product.Name}"))
                productKey?.SetValue("ServiceOID", keyAlgorithm);

            switch (productsUsingThisOID.Count)
            {
                case 0:
                    RegisterOID(keyAlgorithm);
                    AppendStatusMessage("Done.");
                    break;
                case 1:
                    AppendStatusMessage($"Service OID already registered for {productsUsingThisOID[0].Name}.");
                    break;
                default:
                    AppendStatusMessage($"Service OID already registered for {productsUsingThisOID.Count} other products.");
                    break;
            }
        }

        // Unregisters the service OID for the given product.
        private void UnregisterServiceOID(Product product)
        {
            if (product.ServiceOID is null)
                return;

            StatusTextBox.AppendText($"Unregistering service OID for {product.Name}... ");
                
            // Set service OID to null
            string serviceOID = product.ServiceOID;
            product.ServiceOID = null;

            // Delete the service OID key in the registry
            using (RegistryKey productKey = Registry.LocalMachine.OpenSubKey($@"Software\Grid Protection Alliance\{product.Name}", true))
                productKey?.DeleteValue("ServiceOID");

            // Determine which other products were sharing this service OID
            List<Product> productsUsingThisOID = m_products
                .Where(p => p.ServiceOID == serviceOID)
                .ToList();

            switch (productsUsingThisOID.Count)
            {
                case 0:
                    // Unregister the service OID
                    UnregisterOID(serviceOID);
                    AppendStatusMessage("Done.");
                    break;
                case 1:
                    AppendStatusMessage($"Service OID still in use by {productsUsingThisOID[0].Name}.");
                    break;
                default:
                    AppendStatusMessage($"Service OID still in use by {productsUsingThisOID.Count} other services.");
                    break;
            }
        }

        // Appends the given status message to the text box.
        private void AppendStatusMessage(string message)
        {
            StatusTextBox.AppendText(message);
            StatusTextBox.AppendText(Environment.NewLine);
        }

        // Disables publisher evidence generation for the given product.
        private static void SetDisableGeneratePublisherEvidence(Product product, bool value)
        {
            string xmlValue = value ? "false" : "true";
            int registryValue = value ? 1 : 0;

            foreach (string executablePath in EnumerateFiles(product.InstallPath, "*.exe", SearchOption.TopDirectoryOnly))
            {
                string configPath = Path.Combine(executablePath + ".config");

                XDocument document = File.Exists(configPath) ? XDocument.Load(configPath) : new XDocument();
                XElement configuration = document.Root ?? new XElement("configuration");
                XElement runtime = configuration.Element("runtime") ?? new XElement("runtime");
                XElement generatePublisherEvidence = runtime.Element("generatePublisherEvidence") ?? new XElement("generatePublisherEvidence");
                XAttribute enabled = generatePublisherEvidence.Attribute("enabled") ?? new XAttribute("enabled", "");

                if (StringComparer.OrdinalIgnoreCase.Equals((string)enabled, xmlValue))
                    continue;

                if (configuration.Document != document)
                    document.Add(configuration);

                if (runtime.Document != document)
                    configuration.Add(runtime);

                if (generatePublisherEvidence.Document != document)
                    runtime.Add(generatePublisherEvidence);

                if (enabled.Document != document)
                    generatePublisherEvidence.Add(enabled);

                enabled.SetValue(xmlValue);
                document.Save(configPath);
            }

            // Set publisher evidence flag
            product.DisableGeneratePublisherEvidence = registryValue;

            // Store the OID of that certificate in case we need to unregister it later
            using RegistryKey productKey = Registry.LocalMachine.CreateSubKey($@"Software\Grid Protection Alliance\{product.Name}");
            productKey?.SetValue("DisableGeneratePublisherEvidence", registryValue);
        }

        // Registers the given OID.
        private static void RegisterOID(string oid)
        {
            WindowsApi.CRYPT_OID_INFO oidInfo;

            // Look up the OID
            IntPtr info = WindowsApi.CryptFindOIDInfo(WindowsApi.CRYPT_OID_INFO_OID_KEY, oid, WindowsApi.CRYPT_OID_DISABLE_SEARCH_DS_FLAG);

            if (info.Equals(IntPtr.Zero))
                return;

            // Register the OID
            oidInfo = new WindowsApi.CRYPT_OID_INFO();
            Marshal.PtrToStructure(info, oidInfo);
            WindowsApi.CryptRegisterOIDInfo(info, WindowsApi.CRYPT_INSTALL_OID_INFO_BEFORE_FLAG);

            // Add the name of the OID to the registry since the .NET libraries lookup by name
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey($@"Software\Wow6432Node\Microsoft\Cryptography\OID\EncodingType 0\CryptDllFindOIDInfo\{oidInfo.pszOID}!{oidInfo.dwGroupId}", true))
                registryKey?.SetValue("Name", oidInfo.pszOID);

            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey($@"Software\Microsoft\Cryptography\OID\EncodingType 0\CryptDllFindOIDInfo\{oidInfo.pszOID}!{oidInfo.dwGroupId}", true))
                registryKey?.SetValue("Name", oidInfo.pszOID);
        }

        // Unregisters the given OID.
        private static void UnregisterOID(string oid)
        {
            IntPtr info = WindowsApi.CryptFindOIDInfo(WindowsApi.CRYPT_OID_INFO_OID_KEY, oid, WindowsApi.CRYPT_OID_DISABLE_SEARCH_DS_FLAG);

            if (!info.Equals(IntPtr.Zero))
                WindowsApi.CryptUnregisterOIDInfo(info);
        }

        // Converts the given registry key into the product that it is associated with and disposes the registry key.
        private static Product ConvertToProductAndDispose(RegistryKey productKey)
        {
            if (productKey is null)
                return new Product();

            using (productKey)
            {
                return new Product()
                {
                    Name = Path.GetFileName(productKey.Name),
                    InstallPath = GetInstallPath(productKey),
                    ServiceOID = GetServiceOID(productKey),
                    DisableGeneratePublisherEvidence = GetDisableGeneratePublisherEvidence(productKey)
                };
            }
        }

        // Uses the given registry key to get the install path of the product.
        private static string GetInstallPath(RegistryKey productKey) => 
            (string)productKey?.GetValue("InstallPath");

        // Uses the given registry key to get the certificate OID of the product.
        private static string GetServiceOID(RegistryKey productKey) => 
            (string)productKey?.GetValue("ServiceOID");

        // Uses the given registry key to get the certificate OID of the product.
        private static int GetDisableGeneratePublisherEvidence(RegistryKey productKey) => 
            productKey is not null ? (int)(productKey.GetValue("DisableGeneratePublisherEvidence") ?? 0) : 0;

        // Determines whether the given OID is already registered.
        private static bool IsRegistered(string oid)
        {
            WindowsApi.CRYPT_OID_INFO oidInfo;

            IntPtr info = WindowsApi.CryptFindOIDInfo(WindowsApi.CRYPT_OID_INFO_OID_KEY, oid, WindowsApi.CRYPT_OID_DISABLE_SEARCH_DS_FLAG);

            if (info.Equals(IntPtr.Zero))
                return false;

            oidInfo = new WindowsApi.CRYPT_OID_INFO();
            Marshal.PtrToStructure(info, oidInfo);

            using (RegistryKey oidKey = Registry.LocalMachine.OpenSubKey($@"Software\Microsoft\Cryptography\OID\EncodingType 0\CryptDllFindOIDInfo\{oidInfo.pszOID}!{oidInfo.dwGroupId}"))
            {
                if (oidKey is not null)
                    return true;
            }

            using (RegistryKey oidKey = Registry.LocalMachine.OpenSubKey($@"Software\Wow6432Node\Microsoft\Cryptography\OID\EncodingType 0\CryptDllFindOIDInfo\{oidInfo.pszOID}!{oidInfo.dwGroupId}"))
            {
                if (oidKey is not null)
                    return true;
            }

            return false;
        }

        // Returns an enumerable collection of file names that match a search pattern in a specified path, and optionally searches subdirectories.
        private static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories, Action<Exception> exceptionHandler = null)
        {
            try
            {
                return (searchOption == SearchOption.TopDirectoryOnly)
                    ? Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly)
                    : Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly)
                        .Concat(Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly)
                            .SelectMany(directory => EnumerateFiles(directory, searchPattern, searchOption, exceptionHandler)));
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
            }

            return Enumerable.Empty<string>();
        }

        #endregion
    }
}
