//*******************************************************************************************************
//  Cipher.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/04/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Crypto).
//  02/28/2007 - J. Ritchie Carroll
//       Changed string-based encrypt and decrypt functions to return null if
//       input string to be encrypted or decrypted was null or empty.
//  10/11/2007 - J. Ritchie Carroll
//       Added Obfuscate and Deobfuscate functions that perform data obfuscation
//       based upon simple bit-rotation algorithms.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C# - basic encryption/decryption extends string, byte[], and Stream.
//  08/10/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/05/2009 - J. Ritchie Carroll
//       Switched to AES only encryption/decryption using machine specific local key cache.
//  07/13/2010 - Stephen C. Wills
//       Added a file watcher to reload the KeyIV table when an external
//       process modifies the cache file.
//  03/17/2011 - J. Ritchie Carroll
//       Modified key and initialization vector cache to be able to operate in a non-elevated mode.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;
using TVA.IO;

namespace TVA.Security.Cryptography
{
    #region [ Enumerations ]

    /// <summary>
    /// Cryptographic strength enumeration.
    /// </summary>
    public enum CipherStrength
    {
        /// <summary>Uses no encryption.</summary>
        None = 0,
        /// <summary>Uses AES 128-bit encryption.</summary>
        Aes128 = 128,
        /// <summary>Uses AES 256-bit encryption.</summary>
        Aes256 = 256
    }

    #endregion

    /// <summary>
    /// Provides general use cryptographic functions.
    /// </summary>
    /// <remarks>
    /// This class exists to simplify usage of basic cryptography functionality.
    /// </remarks>
    public static class Cipher
    {
        // Constants
        private const int KeyIndex = 0;
        private const int IVIndex = 1;

        // Default key and initialization vector cache file name
        private const string DefaultCacheFileName = "KeyIVCache.bin";

        // Default maximum retry attempts allowed for loading cryptographic key and initialization vector cache
        private const int DefaultMaximumRetryAttempts = 10;

        // Default wait interval, in milliseconds, before retrying load of cryptographic key and initialization vector cache
        private const double DefaultRetryDelayInterval = 200.0D;

        // The standard settings category for cryptography information
        private const string CryptoServicesSettingsCategory = "CryptographyServices";

        /// <summary>
        /// Represents an interprocess serializable cryptographic key and initialization vector cache.
        /// </summary>
        private class KeyIVCache : InterprocessFile
        {
            #region [ Members ]

            // Internal key and initialization vector table
            private Dictionary<string, byte[][]> m_keyIVTable = new Dictionary<string, byte[][]>();

            // Wait handle used so that system will wait for valid key/IV cache load before any pending ciphers
            private ManualResetEventSlim m_keyIVTableIsReady = new ManualResetEventSlim(false);

            // Class disposed flag
            private bool m_disposed;

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets a copy of the internal key and initialization vector table.
            /// </summary>
            public Dictionary<string, byte[][]> KeyIVTable
            {
                get
                {
                    Dictionary<string, byte[][]> keyIVTable;

                    // We wait until the key and IV cache is loaded before attempting to access it
                    if (!m_keyIVTableIsReady.IsSet && !m_keyIVTableIsReady.Wait((int)(RetryDelayInterval * MaximumRetryAttempts)))
                        throw new UnauthorizedAccessException("Cryptographic key access failure: timeout while attempting to load cryptographic key cache.");

                    // Wait for thread level lock on key table
                    lock (m_keyIVTable)
                    {
                        // Make a copy of the keyIV table for external use
                        keyIVTable = new Dictionary<string, byte[][]>(m_keyIVTable);
                    }

                    return keyIVTable;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="KeyIVCache"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (m_keyIVTableIsReady != null)
                                m_keyIVTableIsReady.Dispose();

                            m_keyIVTableIsReady = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true;          // Prevent duplicate dispose.
                        base.Dispose(disposing);    // Call base class Dispose().
                    }
                }
            }

            /// <summary>
            /// Gets the crypto key and initialization vector for the given user password.
            /// </summary>
            /// <param name="password">User password used for key lookup.</param>
            /// <param name="keySize">Specifies the desired key size.</param>
            /// <returns>Crypto key, index 0, and initialization vector, index 1, for the given user password.</returns>
            public byte[][] GetCryptoKeyIV(string password, int keySize)
            {
                string hash = GetPasswordHash(password, keySize);
                byte[][] keyIV;
                byte[] key, iv;
                bool addedKey = false;

                // We wait until the key and IV cache is loaded before attempting to access it
                if (!m_keyIVTableIsReady.IsSet && !m_keyIVTableIsReady.Wait((int)(RetryDelayInterval * MaximumRetryAttempts)))
                    throw new UnauthorizedAccessException("Cryptographic cipher failure: timeout while attempting to load cryptographic key cache.");

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    // Lookup crypto key based on password hash in persisted key table
                    if (!m_keyIVTable.TryGetValue(hash, out keyIV))
                    {
                        // Key for password hash doesn't exist, create a new one
                        AesManaged symmetricAlgorithm = new AesManaged();

                        symmetricAlgorithm.KeySize = keySize;
                        symmetricAlgorithm.GenerateKey();
                        symmetricAlgorithm.GenerateIV();

                        key = symmetricAlgorithm.Key;
                        iv = symmetricAlgorithm.IV;
                        keyIV = new byte[][] { key, iv };

                        // Add new crypto key to key table
                        m_keyIVTable.Add(hash, keyIV);
                        addedKey = true;
                    }
                }

                // Queue up a serialization for any newly added key
                if (addedKey)
                    Save();

                return keyIV;
            }

            /// <summary>
            /// Imports a key and initialization vector into the local system key cache.
            /// </summary>
            /// <param name="password">User password used for key lookups.</param>
            /// <param name="keySize">Specifies the desired key size.</param>
            /// <param name="keyIVText">Text based key and initialization vector to import into local key cache.</param>
            /// <remarks>
            /// This method is used to manually import a key created on another computer.
            /// </remarks>
            public void ImportKeyIV(string password, int keySize, string keyIVText)
            {
                string hash = GetPasswordHash(password, keySize);

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    string[] keyIV = keyIVText.Split('|');
                    byte[] key = Convert.FromBase64String(keyIV[KeyIndex]);
                    byte[] iv = Convert.FromBase64String(keyIV[IVIndex]);

                    // Assign new crypto key to key table
                    m_keyIVTable[hash] = new byte[][] { key, iv };
                }

                // Queue up a serialization for this new key
                Save();
            }

            /// <summary>
            /// Exports a key and initialization vector from the local system key cache.
            /// </summary>
            /// <param name="password">User password used for key lookup.</param>
            /// <param name="keySize">Specifies the desired key size.</param>
            /// <returns>Text based key and initialization vector exported from local key cache.</returns>
            /// <remarks>
            /// This method is used to manually export a key to be installed on another computer. 
            /// </remarks>
            public string ExportKeyIV(string password, int keySize)
            {
                byte[][] keyIV = GetCryptoKeyIV(password, keySize);
                return string.Concat(Convert.ToBase64String(keyIV[KeyIndex]), "|", Convert.ToBase64String(keyIV[IVIndex]));
            }

            /// <summary>
            /// Merge keys and initialization vectors from another <see cref="KeyIVCache"/>, local cache taking precedence.
            /// </summary>
            /// <param name="other">Other <see cref="KeyIVCache"/> to merge with.</param>
            public void MergeLeft(KeyIVCache other)
            {
                // Merge other keys into local ones
                Dictionary<string, byte[][]> mergedKeyIVTable = KeyIVTable.Merge(other.KeyIVTable);

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    // Replace local key IV table with merged items
                    m_keyIVTable = mergedKeyIVTable;
                }

                // Queue up a serialization for any newly added keys
                Save();
            }

            /// <summary>
            /// Merge keys and initialization vectors from another <see cref="KeyIVCache"/>, other cache taking precedence.
            /// </summary>
            /// <param name="other">Other <see cref="KeyIVCache"/> to merge with.</param>
            public void MergeRight(KeyIVCache other)
            {
                // Merge other keys into local ones
                Dictionary<string, byte[][]> mergedKeyIVTable = other.KeyIVTable.Merge(KeyIVTable);

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    // Replace local key IV table with merged items
                    m_keyIVTable = mergedKeyIVTable;
                }

                // Queue up a serialization for any newly added keys
                Save();
            }

            /// <summary>
            /// Initiates interprocess synchronized save of key and initialization vector table.
            /// </summary>
            public override void Save()
            {
                byte[] serializedKeyIVTable;

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    serializedKeyIVTable = Serialization.GetBytes(m_keyIVTable);
                }

                // File data is the serialized Key/IV cache, assigmnent will initiate auto-save if needed
                FileData = serializedKeyIVTable;
            }

            /// <summary>
            /// Initiates interprocess synchronized load of key and initialization vector table.
            /// </summary>
            public override void Load()
            {
                // Hold any threads needing crypto keys
                m_keyIVTableIsReady.Reset();

                base.Load();
            }

            /// <summary>
            /// Handles serialization of file to disk; virtual method allows customization (e.g., pre-save encryption and/or data merge).
            /// </summary>
            /// <param name="fileStream"><see cref="FileStream"/> used to serialize data.</param>
            /// <param name="fileData">File data to be serialized.</param>
            /// <remarks>
            /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
            /// </remarks>
            protected override void SaveFileData(FileStream fileStream, byte[] fileData)
            {
                // Encrypt data local to this machine (this way user cannot copy key cache to another machine)
                base.SaveFileData(fileStream, ProtectedData.Protect(fileData, null, DataProtectionScope.LocalMachine));
            }

            /// <summary>
            /// Handles deserialization of file from disk; virtual method allows customization (e.g., pre-load decryption and/or data merge).
            /// </summary>
            /// <param name="fileStream"><see cref="FileStream"/> used to deserialize data.</param>
            /// <returns>Deserialized file data.</returns>
            /// <remarks>
            /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
            /// </remarks>
            protected override byte[] LoadFileData(FileStream fileStream)
            {
                // Decrypt data that was encrypted local to this machine
                byte[] serializedKeyIVTable = ProtectedData.Unprotect(fileStream.ReadStream(), null, DataProtectionScope.LocalMachine);
                Dictionary<string, byte[][]> keyIVTable = Serialization.GetObject<Dictionary<string, byte[][]>>(serializedKeyIVTable);

                // Wait for thread level lock on key table
                lock (m_keyIVTable)
                {
                    // Merge new and existing key tables since new keys may have been queued for serialization, but not saved yet
                    m_keyIVTable = keyIVTable.Merge(m_keyIVTable);
                }

                // Release any threads waiting for crypto keys
                m_keyIVTableIsReady.Set();

                return serializedKeyIVTable;
            }

            #endregion
        }

        // Primary cryptographic key and initialization vector cache.
        private static KeyIVCache s_keyIVCache;

        // Password hash table (run-time optimization)
        private static Dictionary<string, string> s_passwordHash = new Dictionary<string, string>();

        /// <summary>
        /// Static constructor for the <see cref="Cipher"/> class.
        /// </summary>
        static Cipher()
        {
            KeyIVCache localKeyIVCache;
            string localCacheFileName = DefaultCacheFileName;
            double retryDelayInterval = DefaultRetryDelayInterval;
            int maximumRetryAttempts = DefaultMaximumRetryAttempts;

            // Load cryptographic settings
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[CryptoServicesSettingsCategory];

            settings.Add("CryptoCache", localCacheFileName, "Path and file name of cryptographic key and initialization vector cache.");
            settings.Add("RetryDelayInterval", retryDelayInterval, "Wait interval, in milliseconds, before retrying load of cryptographic key and initialization vector cache.");
            settings.Add("MaximumRetryAttempts", maximumRetryAttempts, "Maximum retry attempts allowed for loading cryptographic key and initialization vector cache.");

            localCacheFileName = FilePath.GetAbsolutePath(settings["CryptoCache"].ValueAs(localCacheFileName));
            retryDelayInterval = settings["RetryDelayInterval"].ValueAs(retryDelayInterval);
            maximumRetryAttempts = settings["MaximumRetryAttempts"].ValueAs(maximumRetryAttempts);

            // Initialize local cryptographic key and initialization vector cache (application may only have read-only access to this cache)
            localKeyIVCache = new KeyIVCache()
            {
                FileName = localCacheFileName,
                RetryDelayInterval = retryDelayInterval,
                MaximumRetryAttempts = maximumRetryAttempts,
                ReloadOnChange = true,
                AutoSave = false
            };

            // Load initial keys
            localKeyIVCache.Load();

            try
            {
                // Validate that user has write access to the local cryptographic cache folder
                string tempFile = FilePath.GetDirectoryName(localCacheFileName) + Guid.NewGuid().ToString() + ".tmp";

                using (File.Create(tempFile))
                {
                }

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                // No access issues exist, use local cache as the primary cryptographic key and initialization vector cache
                s_keyIVCache = localKeyIVCache;
                s_keyIVCache.AutoSave = true;
            }
            catch (UnauthorizedAccessException)
            {
                // User does not have needed serialization access to common cryptographic cache folder, use a path where user will have rights
                string userCacheFolder = FilePath.AddPathSuffix(FilePath.GetApplicationDataFolder());
                string userCacheFileName = userCacheFolder + FilePath.GetFileName(localCacheFileName);

                // Make sure user directory exists
                if (!Directory.Exists(userCacheFolder))
                    Directory.CreateDirectory(userCacheFolder);

                // Copy existing common cryptographic cache if none exists
                if (File.Exists(localCacheFileName) && !File.Exists(userCacheFileName))
                    File.Copy(localCacheFileName, userCacheFileName);

                // Initialize primary cryptographic key and initialization vector cache within user folder
                s_keyIVCache = new KeyIVCache()
                {
                    FileName = userCacheFileName,
                    RetryDelayInterval = retryDelayInterval,
                    MaximumRetryAttempts = maximumRetryAttempts,
                    ReloadOnChange = true,
                    AutoSave = true
                };

                // Initiate a key reload
                s_keyIVCache.Load();

                // Merge new or updated keys, protected folder keys taking precendence over user keys
                s_keyIVCache.MergeRight(localKeyIVCache);
            }
        }

        /// <summary>
        /// Imports a key and initialization vector into the local system key cache.
        /// </summary>
        /// <param name="password">User password used for key lookups.</param>
        /// <param name="keySize">Specifies the desired key size.</param>
        /// <param name="keyIVText">Text based key and initialization vector to import into local key cache.</param>
        /// <remarks>
        /// This method is used to manually import a key created on another computer.
        /// </remarks>
        public static void ImportKeyIV(string password, int keySize, string keyIVText)
        {
            s_keyIVCache.ImportKeyIV(password, keySize, keyIVText);
        }

        /// <summary>
        /// Exports a key and initialization vector from the local system key cache.
        /// </summary>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="keySize">Specifies the desired key size.</param>
        /// <returns>Text based key and initialization vector exported from local key cache.</returns>
        /// <remarks>
        /// This method is used to manually export a key to be installed on another computer. 
        /// </remarks>
        public static string ExportKeyIV(string password, int keySize)
        {
            return s_keyIVCache.ExportKeyIV(password, keySize);
        }

        /// <summary>
        /// Gets the Base64 encoded SHA-2 hash of given user password.
        /// </summary>
        /// <param name="password">User password to get hash for.</param>
        /// <param name="keySize">Specifies the desired key size.</param>
        /// <returns>Base64 encoded SHA-2 hash of user password.</returns>
        public static string GetPasswordHash(string password, int keySize)
        {
            string hash;

            // Suffix password with key size since same password may be in use for different key sizes
            password += keySize.ToString();

            lock (s_passwordHash)
            {
                // Lookup SHA-2 hash of user password in run-time cache
                if (!s_passwordHash.TryGetValue(password, out hash))
                {
                    // Password hash doesn't exist, create one
                    hash = Convert.ToBase64String((new SHA256Managed()).ComputeHash(Encoding.Default.GetBytes(password)));
                    s_passwordHash.Add(password, hash);
                }
            }

            return hash;
        }

        /// <summary>
        /// Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        /// the given parameters.
        /// </summary>
        /// <param name="source">Source string to encrypt.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting string.</param>
        /// <returns>An encrypted version of the source string.</returns>
        public static string Encrypt(this string source, string password, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            if (strength == CipherStrength.None)
                return source;

            return Convert.ToBase64String(Encoding.Unicode.GetBytes(source).Encrypt(password, strength));
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, string password, CipherStrength strength)
        {
            return source.Encrypt(0, source.Length, password, strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to encrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, int startIndex, int length, string password, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            byte[][] keyIV = s_keyIVCache.GetCryptoKeyIV(password, (int)strength);

            return source.Encrypt(startIndex, length, keyIV[KeyIndex], keyIV[IVIndex], strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="iv">Initialization vector to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, byte[] key, byte[] iv, CipherStrength strength)
        {
            return source.Encrypt(0, source.Length, key, iv, strength);
        }

        /// <summary>
        /// Returns a binary array of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to encrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to encrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to encrypt data.</param>
        /// <param name="iv">Initialization vector to use to encrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting data.</param>
        /// <returns>An encrypted version of the source data.</returns>
        public static byte[] Encrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] iv, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            AesManaged symmetricAlgorithm = new AesManaged();

            symmetricAlgorithm.KeySize = (int)strength;

            return symmetricAlgorithm.Encrypt(source, startIndex, length, key, iv);
        }

        /// <summary>
        /// Returns a stream of encrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to encrypt.</param>
        /// <param name="key">Encryption key to use to encrypt stream.</param>
        /// <param name="iv">Initialization vector to use to encrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting stream.</param>
        /// <returns>An encrypted version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the encrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Encrypt(this Stream source, byte[] key, byte[] iv, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Encrypt(destination, key, iv, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Encrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to encrypt.</param>
        /// <param name="destination">Destination stream used to hold encrypted data.</param>
        /// <param name="key">Encryption key to use to encrypt stream.</param>
        /// <param name="iv">Initialization vector to use to encrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting stream.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for encrypting large streams.</param>
        public static void Encrypt(this Stream source, Stream destination, byte[] key, byte[] iv, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer = new byte[Standard.BufferSize];
            byte[] outBuffer, lengthBuffer;
            long total = 0;
            long length = -1;
            int read;

            // Sends initial progress event.
            if (progressHandler != null)
            {
                try
                {
                    if (source.CanSeek)
                        length = source.Length;
                }
                catch
                {
                    length = -1;
                }

                // Create a new progress handler to track encryption progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Encrypt", length);
                progress.Complete = 0;
            }

            // Reads initial buffer.
            read = source.Read(inBuffer, 0, Standard.BufferSize);

            while (read > 0)
            {
                // Encrypts buffer.
                outBuffer = inBuffer.BlockCopy(0, read).Encrypt(key, iv, strength);

                // The destination encryption stream length does not have to be same as the input stream length, so we
                // prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                // safely decrypt the stream in a "chunked" fashion later.
                lengthBuffer = BitConverter.GetBytes(outBuffer.Length);
                destination.Write(lengthBuffer, 0, lengthBuffer.Length);
                destination.Write(outBuffer, 0, outBuffer.Length);

                // Updates encryption progress.
                if (progressHandler != null)
                {
                    total += read;
                    progress.Complete = total;
                }

                // Reads next buffer.
                read = source.Read(inBuffer, 0, Standard.BufferSize);
            }
        }

        /// <summary>
        /// Creates an encrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when encrypting file.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for encrypting large files.</param>
        public static void EncryptFile(string sourceFileName, string destinationFileName, string password, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            byte[][] keyIV = s_keyIVCache.GetCryptoKeyIV(password, (int)strength);

            sourceFileStream.Encrypt(destFileStream, keyIV[KeyIndex], keyIV[IVIndex], strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }

        /// <summary>
        /// Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        /// parameters.
        /// </summary>
        /// <param name="source">Source string to decrypt.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting string.</param>
        /// <returns>A decrypted version of the source string.</returns>
        public static string Decrypt(this string source, string password, CipherStrength strength)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            if (strength == CipherStrength.None)
                return source;

            return Encoding.Unicode.GetString(Convert.FromBase64String(source).Decrypt(password, strength));
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, string password, CipherStrength strength)
        {
            return source.Decrypt(0, source.Length, password, strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, int startIndex, int length, string password, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            byte[][] keyIV = s_keyIVCache.GetCryptoKeyIV(password, (int)strength);

            return source.Decrypt(startIndex, length, keyIV[KeyIndex], keyIV[IVIndex], strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="iv">Initialization vector to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, byte[] key, byte[] iv, CipherStrength strength)
        {
            return source.Decrypt(0, source.Length, key, iv, strength);
        }

        /// <summary>
        /// Returns a binary array of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Binary array of data to decrypt.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Number of bytes in <paramref name="source"/> buffer to decrypt starting from <paramref name="startIndex"/> offset.</param>
        /// <param name="key">Encryption key to use to decrypt data.</param>
        /// <param name="iv">Initialization vector to use to decrypt data.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting data.</param>
        /// <returns>A decrypted version of the source data.</returns>
        public static byte[] Decrypt(this byte[] source, int startIndex, int length, byte[] key, byte[] iv, CipherStrength strength)
        {
            if (strength == CipherStrength.None)
                return source;

            AesManaged symmetricAlgorithm = new AesManaged();

            symmetricAlgorithm.KeySize = (int)strength;

            return symmetricAlgorithm.Decrypt(source, startIndex, length, key, iv);
        }

        /// <summary>
        /// Returns a stream of decrypted data for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to decrypt.</param>
        /// <param name="key">Encryption key to use to decrypt stream.</param>
        /// <param name="iv">Initialization vector to use to decrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting stream.</param>
        /// <returns>A decrypted version of the source stream.</returns>
        /// <remarks>
        /// This returns a memory stream of the decrypted results, if the incoming stream is
        /// very large this will consume a large amount of memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        public static MemoryStream Decrypt(this Stream source, byte[] key, byte[] iv, CipherStrength strength)
        {
            MemoryStream destination = new MemoryStream();

            source.Decrypt(destination, key, iv, strength, null);
            destination.Position = 0;

            return destination;
        }

        /// <summary>
        /// Decrypts input stream onto output stream for the given parameters.
        /// </summary>
        /// <param name="source">Source stream that contains data to decrypt.</param>
        /// <param name="destination">Destination stream used to hold decrypted data.</param>
        /// <param name="key">Encryption key to use to decrypt stream.</param>
        /// <param name="iv">Initialization vector to use to decrypt stream.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting stream.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for decrypting large streams.</param>
        public static void Decrypt(this Stream source, Stream destination, byte[] key, byte[] iv, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer, outBuffer;
            byte[] lengthBuffer = BitConverter.GetBytes((int)0);
            long total = 0;
            long length = -1;
            int size, read;

            // Sends initial progress event.
            if (progressHandler != null)
            {
                try
                {
                    if (source.CanSeek)
                        length = source.Length;
                }
                catch
                {
                    length = -1;
                }

                // Create a new progress handler to track decryption progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Decrypt", length);
                progress.Complete = 0;
            }

            // When the source stream was encrypted, it was known that the encrypted stream length did not have to be same as
            // the input stream length. We prepended the final size of the each encrypted buffer onto the destination
            // ouput stream (now the input stream to this function), so that we could safely decrypt the stream in a
            // "chunked" fashion, hence the following:

            // Reads the size of the next buffer from the stream.
            read = source.Read(lengthBuffer, 0, lengthBuffer.Length);

            while (read > 0)
            {
                // Converts the byte array containing the buffer size into an integer.
                size = BitConverter.ToInt32(lengthBuffer, 0);

                if (size > 0)
                {
                    // Creates and reads the next buffer.
                    inBuffer = new byte[size];
                    read = source.Read(inBuffer, 0, size);

                    if (read > 0)
                    {
                        // Decrypts buffer.
                        outBuffer = inBuffer.Decrypt(key, iv, strength);
                        destination.Write(outBuffer, 0, outBuffer.Length);

                        // Updates decryption progress.
                        if (progressHandler != null)
                        {
                            total += (read + lengthBuffer.Length);
                            progress.Complete = total;
                        }
                    }
                }

                // Reads the size of the next buffer from the stream.
                read = source.Read(lengthBuffer, 0, lengthBuffer.Length);
            }
        }

        /// <summary>
        /// Creates a decrypted file from source file data.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destinationFileName">Destination file name.</param>
        /// <param name="password">User password used for key lookup.</param>
        /// <param name="strength">Cryptographic strength to use when decrypting file.</param>
        /// <param name="progressHandler">Optional delegate to handle progress updates for decrypting large files.</param>
        public static void DecryptFile(string sourceFileName, string destinationFileName, string password, CipherStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            FileStream sourceFileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream destFileStream = File.Create(destinationFileName);

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            byte[][] keyIV = s_keyIVCache.GetCryptoKeyIV(password, (int)strength);

            sourceFileStream.Decrypt(destFileStream, keyIV[KeyIndex], keyIV[IVIndex], strength, progressHandler);

            destFileStream.Flush();
            destFileStream.Close();
            sourceFileStream.Close();
        }
    }
}