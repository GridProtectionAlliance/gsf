//*******************************************************************************************************
//  IsamDataFileBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/08/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/28/2008 - Pinal C. Patel
//       Edited code comments.
//  12/04/2008 - J. Ritchie Carroll
//       Modified class an example to use new ISupportBinaryImage.
//  05/12/2009 - Pinal C. Patel
//       Optimized Read() for better memory management by using "yield return".
//  05/19/2009 - Pinal C. Patel
//       Implemented the IProvideStatus interface.
//  07/02/2009 - Pinal C. Patel
//       Modified state alterning properties to reopen the file when changed.
//  08/10/2009 - Pinal C. Patel
//       Modified Write() to write empty intermediate records that are missing to avoid garbage data 
//       for the missing intermediate records when records are being written to disk directly.
//  9/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using TVA.Configuration;
using TVA.Parsing;

namespace TVA.IO
{
    /// <summary>
    /// An abstract class that defines the read/write capabilities for ISAM (Indexed Sequential Access Method) file.
    /// </summary>
    /// <typeparam name="T">
    /// <see cref="Type"/> of the records the file contains. This <see cref="Type"/> must implement the <see cref="ISupportBinaryImage"/> interface.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This ISAM implementation keeps all the records in memory, so it may not be suitable for very large files. Since data is stored
    /// in memory using a list, the maximum number of possible supported records will be 2,147,483,647 (i.e., Int32.MaxValue).
    /// </para>
    /// <para>
    /// See <a href="http://en.wikipedia.org/wiki/ISAM" target="_blank">http://en.wikipedia.org/wiki/ISAM</a> for more information on ISAM files.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows a sample implementation of <see cref="IsamDataFileBase{T}"/>:
    /// <code>
    /// using System;
    /// using System.Text;
    /// using TVA;
    /// using TVA.IO;
    /// using TVA.Parsing;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Create a few test records.
    ///         TestIsamFileRecord r1 = new TestIsamFileRecord(1);
    ///         r1.Name = "TestRecord1";
    ///         r1.Value = double.MinValue;
    ///         r1.Description = "Test record with minimum double value";
    ///         TestIsamFileRecord r2 = new TestIsamFileRecord(2);
    ///         r2.Name = "TestRecord2";
    ///         r2.Value = double.MaxValue;
    ///         r2.Description = "Test record with maximum double value";
    /// 
    ///         // Open ISAM file.
    ///         TestIsamFile testFile = new TestIsamFile();
    ///         testFile.FileName = "TestIsamFile.dat";
    ///         testFile.Open();
    /// 
    ///         // Write test records.
    ///         testFile.Write(r1.Index, r1);
    ///         testFile.Write(r2.Index, r2);
    /// 
    ///         // Read test records.
    ///         Console.WriteLine(testFile.Read(1));
    ///         Console.WriteLine(testFile.Read(2));
    /// 
    ///         // Close ISAM file.
    ///         testFile.Close();
    /// 
    ///         Console.ReadLine();
    ///     }
    /// }
    /// 
    /// class TestIsamFile : IsamDataFileBase&lt;TestIsamFileRecord&gt;
    /// {
    ///     /// <summary>
    ///     /// Size of a single file record.
    ///     /// </summary>
    ///     protected override int GetRecordSize()
    ///     {
    ///         return TestIsamFileRecord.RecordLength;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Creates a new empty file record.
    ///     /// </summary>
    ///     protected override TestIsamFileRecord CreateNewRecord(int id)
    ///     {
    ///         return new TestIsamFileRecord(id);
    ///     }
    /// }
    /// 
    /// class TestIsamFileRecord : ISupportBinaryImage
    /// {
    ///     private int m_index;
    ///     private string m_name;                  // 20  * 1 =  20
    ///     private double m_value;                 // 1   * 8 =   8
    ///     private string m_description;           // 100 * 1 = 100
    ///     
    ///     public const int RecordLength = 128;    // Total   = 128
    /// 
    ///     public TestIsamFileRecord(int recordIndex)
    ///     {
    ///         m_index = recordIndex;
    ///         
    ///         Name = string.Empty;
    ///         Value = double.NaN;
    ///         Description = string.Empty;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// 1-based index of the record.
    ///     /// </summary>
    ///     public int Index
    ///     {
    ///         get { return m_index; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Name of the record.
    ///     /// </summary>
    ///     public string Name
    ///     {
    ///         get { return m_name; }
    ///         set { m_name = value.TruncateRight(20).PadRight(20); }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Value of the record.
    ///     /// </summary>
    ///     public double Value
    ///     {
    ///         get { return m_value; }
    ///         set { m_value = value; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Description of the record.
    ///     /// </summary>
    ///     public string Description
    ///     {
    ///         get { return m_description; }
    ///         set { m_description = value.TruncateRight(100).PadRight(100); }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Serialized record length.
    ///     /// </summary>
    ///     public int BinaryLength
    ///     {
    ///         get { return RecordLength; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Serialized record data.
    ///     /// </summary>
    ///     public byte[] BinaryImage
    ///     {
    ///         get
    ///         {
    ///             // Serialize TestIsamFileRecord into byte array.
    ///             byte[] image = new byte[RecordLength];
    ///             Buffer.BlockCopy(Encoding.ASCII.GetBytes(Name), 0, image, 0, 20);
    ///             Buffer.BlockCopy(BitConverter.GetBytes(Value), 0, image, 20, 8);
    ///             Buffer.BlockCopy(Encoding.ASCII.GetBytes(Description), 0, image, 28, 100);
    /// 
    ///             return image;
    ///         }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Deserializes the record.
    ///     /// </summary>
    ///     public int Initialize(byte[] binaryImage, int startIndex, int length)
    ///     {
    ///         if (length &gt;= RecordLength)
    ///         {
    ///             // Deserialize byte array into TestIsamFileRecord.
    ///             Name = Encoding.ASCII.GetString(binaryImage, startIndex, 20);
    ///             Value = BitConverter.ToDouble(binaryImage, startIndex + 20);
    ///             Description = Encoding.ASCII.GetString(binaryImage, startIndex + 28, 100);
    ///         }
    ///         else
    ///             throw new InvalidOperationException("Invalid record size, not enough data to deserialize record."); 
    /// 
    ///         return RecordLength;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// String representation of the record.
    ///     /// </summary>
    ///     public override string ToString()
    ///     {
    ///         return string.Format("Name: {0}, Value: {1}, Description: {2}", Name, Value, Description);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class IsamDataFileBase<T> : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings where T : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "IsamDataFile.dat";

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoSaveInterval"/> property.
        /// </summary>
        public const int DefaultAutoSaveInterval = -1;

        /// <summary>
        /// Specifes the default value for the <see cref="MinimumRecordCount"/> property.
        /// </summary>
        public const int DefaultMinimumRecordCount = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="LoadOnOpen"/> property.
        /// </summary>
        public const bool DefaultLoadOnOpen = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SaveOnClose"/> property.
        /// </summary>
        public const bool DefaultSaveOnClose = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReloadOnModify"/> property.
        /// </summary>
        public const bool DefaultReloadOnModify = false;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "IsamDataFile";

        // Events

        /// <summary>
        /// Occurs when data is being read from disk into memory.
        /// </summary>
        [Description("Occurs when data is being read from disk into memory.")]
        public event EventHandler DataLoading;

        /// <summary>
        /// Occurs when data has been read from disk into memory.
        /// </summary>
        [Description("Occurs when data has been read from disk into memory.")]
        public event EventHandler DataLoaded;

        /// <summary>
        /// Occurs when data is being saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data is being saved from memory onto disk.")]
        public event EventHandler DataSaving;

        /// <summary>
        /// Occurs when data has been saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data has been saved from memory onto disk.")]
        public event EventHandler DataSaved;

        /// <summary>
        /// Occurs when file data on the disk is modified.
        /// </summary>
        [Description("Occurs when file data on the disk is modified.")]
        public event EventHandler FileModified;

        // Fields
        private string m_fileName;
        private FileAccess m_fileAccessMode;
        private int m_autoSaveInterval;
        private int m_minimumRecordCount;
        private bool m_loadOnOpen;
        private bool m_saveOnClose;
        private bool m_reloadOnModify;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private List<T> m_fileRecords;
        private byte[] m_recordBuffer;
        private FileStream m_fileData;
        private ManualResetEvent m_loadWaitHandle;
        private ManualResetEvent m_saveWaitHandle;
        private System.Timers.Timer m_autoSaveTimer;
        private FileSystemWatcher m_fileWatcher;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamDataFileBase{T}"/> class.
        /// </summary>
        protected IsamDataFileBase()
        {
            m_fileName = DefaultFileName;
            m_fileAccessMode = DefaultFileAccessMode;
            m_autoSaveInterval = DefaultAutoSaveInterval;
            m_minimumRecordCount = DefaultMinimumRecordCount;
            m_loadOnOpen = DefaultLoadOnOpen;
            m_saveOnClose = DefaultSaveOnClose;
            m_reloadOnModify = DefaultReloadOnModify;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_loadWaitHandle = new ManualResetEvent(true);
            m_saveWaitHandle = new ManualResetEvent(true);

            m_autoSaveTimer = new System.Timers.Timer();
            m_autoSaveTimer.Elapsed += m_autoSaveTimer_Elapsed;
            m_fileWatcher = new FileSystemWatcher();
            m_fileWatcher.Changed += m_fileWatcher_Changed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="FileName"/> when the file is open will cause the file to be re-opend.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultFileName),
        Description("Name of the file.")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_fileName = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileAccess"/> value to use when opening the file.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultFileAccessMode),
        Description("The System.IO.FileAccess value to use when opening the file.")]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                m_fileAccessMode = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the interval in milliseconds at which the records loaded in memory are to be persisted to disk.
        /// </summary>
        /// <remarks>
        /// <see cref="AutoSaveInterval"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultAutoSaveInterval),
        Description("Interval in milliseconds at which the records loaded in memory are to be persisted to disk.")]
        public int AutoSaveInterval
        {
            get
            {
                return m_autoSaveInterval;
            }
            set
            {
                m_autoSaveInterval = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of records that the file will have when it is opened.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMinimumRecordCount),
        Description("Minimum number of records that the file will have when it is opened.")]
        public int MinimumRecordCount
        {
            get
            {
                return m_minimumRecordCount;
            }
            set
            {
                m_minimumRecordCount = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records are to be loaded automatically in memory when 
        /// the file is opened.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultLoadOnOpen),
        Description("Indicates whether records are to be loaded automatically in memory when the file is opened.")]
        public bool LoadOnOpen
        {
            get
            {
                return m_loadOnOpen;
            }
            set
            {
                m_loadOnOpen = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records loaded in memory are to be persisted to disk 
        /// when the file is closed.
        /// </summary>
        /// <remarks>
        /// <see cref="SaveOnClose"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Behavior"),
        DefaultValue(DefaultSaveOnClose),
        Description("Indicates whether records loaded in memory are to be persisted to disk when the file is closed.")]
        public bool SaveOnClose
        {
            get
            {
                return m_saveOnClose;
            }
            set
            {
                m_saveOnClose = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records loaded in memory are to be re-loaded when the 
        /// file is modified on disk.
        /// </summary>
        /// <remarks>
        /// <see cref="ReloadOnModify"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Behavior"),
        DefaultValue(DefaultReloadOnModify),
        Description("Indicates whether records loaded in memory are to be re-loaded when the file is modified on disk.")]
        public bool ReloadOnModify
        {
            get
            {
                return m_reloadOnModify;
            }
            set
            {
                m_reloadOnModify = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the file settings are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the file settings are to be saved to the config file.")]
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the file settings are to be saved to the config file if the 
        /// <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the file settings are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException("value"));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the file is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will open the file if it is closed, setting
        /// to false will close the file if it is open.
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return IsOpen;
            }
            set
            {
                if (value && !IsOpen)
                    Open();
                else if (!value && IsOpen)
                    Close();
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the file is open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileData != null);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the file data on disk is corrupt.
        /// </summary>
        [Browsable(false)]
        public bool IsCorrupt
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileData)
                    {
                        fileLength = m_fileData.Length;
                    }

                    return (fileLength % m_recordBuffer.Length != 0);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        /// <summary>
        /// Gets the approximate memory consumption (in KB) of the file.
        /// </summary>
        /// <remarks>
        /// <see cref="MemoryUsage"/> will be zero (0) unless records are loaded in memory.
        /// </remarks>
        [Browsable(false)]
        public long MemoryUsage
        {
            get
            {
                return RecordsInMemory * m_recordBuffer.Length / 1024;
            }
        }

        /// <summary>
        /// Gets the number of file records on the disk.
        /// </summary>
        [Browsable(false)]
        public int RecordsOnDisk
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileData)
                    {
                        fileLength = m_fileData.Length;
                    }

                    return (int)(fileLength / (long)m_recordBuffer.Length);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        /// <summary>
        /// Gets the number of file records loaded in memory.
        /// </summary>
        [Browsable(false)]
        public int RecordsInMemory
        {
            get
            {
                int recordCount = 0;

                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        recordCount = m_fileRecords.Count;
                    }
                }

                return recordCount;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the file.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the file.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("                 File name: ");
                status.Append(FilePath.TrimFileName(FileName, 30));
                status.AppendLine();
                status.Append("                File state: ");
                status.Append(IsOpen ? "Open" : "Closed");
                status.AppendLine();
                status.Append("          File access mode: ");
                status.Append(FileAccessMode);
                status.AppendLine();
                status.Append("             Data validity: ");
                status.Append(IsCorrupt ? "Invalid" : "Valid");
                status.AppendLine();
                status.Append("            Auto-save data: ");
                if (LoadOnOpen && AutoSaveInterval > 0 && !SaveOnClose)
                    status.AppendFormat("Every {0}ms", AutoSaveInterval);
                if (LoadOnOpen && AutoSaveInterval > 0 && SaveOnClose)
                    status.AppendFormat("Every {0}ms & File Close", AutoSaveInterval);
                if (!LoadOnOpen || (LoadOnOpen && AutoSaveInterval < 1 && !SaveOnClose))
                    status.Append("Never");
                status.AppendLine();
                status.Append("           Records on disk: ");
                status.Append(RecordsOnDisk);
                status.AppendLine();
                status.Append("         Records in memory: ");
                status.Append(RecordsInMemory);
                status.AppendLine();
                status.Append("    Memory usage (approx.): ");
                status.AppendFormat("{0} KB", MemoryUsage);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="FileStream"/> of the file.
        /// </summary>
        /// <remarks>
        /// Thread-safety Warning: A lock must be obtained on <see cref="FileData"/> before accessing it.
        /// </remarks>
        protected FileStream FileData
        {
            get
            {
                return m_fileData;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, gets the size of a record (in bytes).
        /// </summary>
        /// <returns>Size of a record in bytes.</returns>
        protected abstract int GetRecordSize();

        /// <summary>
        /// When overridden in a derived class, returns a new empty record.
        /// </summary>
        /// <param name="recordIndex">1-based index of the new record.</param>
        /// <returns>New empty record.</returns>
        protected abstract T CreateNewRecord(int recordIndex);

        #endregion

        /// <summary>
        /// Initializes the file.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the file is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();                             // Load settings from the config file.
                m_recordBuffer = new byte[GetRecordSize()]; // Create buffer for reading records.
                m_initialized = true;                       // Initialize only once.
            }
        }

        /// <summary>
        /// Performs necessary operations before the file properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the file is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            if (!DesignMode)
            {
                try
                {
                    // Nothing needs to be done before component is initialized.
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Performs necessary operations after the file properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the file is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings of the file to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["FileName", true];
                element.Update(m_fileName, element.Description, element.Encrypted);
                element = settings["FileAccessMode", true];
                element.Update(m_fileAccessMode, element.Description, element.Encrypted);
                element = settings["AutoSaveInterval", true];
                element.Update(m_autoSaveInterval, element.Description, element.Encrypted);
                element = settings["MinimumRecordCount", true];
                element.Update(m_minimumRecordCount, element.Description, element.Encrypted);
                element = settings["LoadOnOpen", true];
                element.Update(m_loadOnOpen, element.Description, element.Encrypted);
                element = settings["SaveOnClose", true];
                element.Update(m_saveOnClose, element.Description, element.Encrypted);
                element = settings["ReloadOnModify", true];
                element.Update(m_reloadOnModify, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings of the file from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("FileName", m_fileName, "Name of the file including its path.");
                settings.Add("FileAccessMode", m_fileAccessMode, "Access mode (Read; Write; ReadWrite) to be used when opening the file.");
                settings.Add("AutoSaveInterval", m_autoSaveInterval, "Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving.");
                settings.Add("MinimumRecordCount", m_minimumRecordCount, "Minimum number of records that the file must have when it is opened.");
                settings.Add("LoadOnOpen", m_loadOnOpen, "True if file records are to be loaded in memory when opened; otherwise False.");
                settings.Add("SaveOnClose", m_saveOnClose, "True if file records loaded in memory are to be saved to disk when file is closed; otherwise False.");
                settings.Add("ReloadOnModify", m_reloadOnModify, "True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False.");
                FileName = settings["FileName"].ValueAs(m_fileName);
                FileAccessMode = settings["FileAccessMode"].ValueAs(m_fileAccessMode);
                AutoSaveInterval = settings["AutoSaveInterval"].ValueAs(m_autoSaveInterval);
                MinimumRecordCount = settings["MinimumRecordCount"].ValueAs(m_minimumRecordCount);
                LoadOnOpen = settings["LoadOnOpen"].ValueAs(m_loadOnOpen);
                SaveOnClose = settings["SaveOnClose"].ValueAs(m_saveOnClose);
                ReloadOnModify = settings["ReloadOnModify"].ValueAs(m_reloadOnModify);
            }
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                // Initialize if uninitialized.
                Initialize();

                // Make the file path absolute if it is relative.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                // Create the file directory if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                // Open if file exists, or create it if it doesn't.
                m_fileData = new FileStream(m_fileName, FileMode.OpenOrCreate, m_fileAccessMode, FileShare.ReadWrite);

                // Load records into memory if specified to do so.
                if (m_loadOnOpen) Load();

                // Makes sure that we have the minimum number of records specified.
                for (int i = RecordsOnDisk + 1; i <= m_minimumRecordCount; i++)
                {
                    Write(i, CreateNewRecord(i));
                }

                if (m_reloadOnModify)
                {
                    // Watch for any modifications made to the file on disk.
                    m_fileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                    m_fileWatcher.Filter = FilePath.GetFileName(m_fileName);
                    m_fileWatcher.EnableRaisingEvents = true;
                }

                if (m_autoSaveInterval > 0)
                {
                    // Start timer for saving records loaded in memory automatically.
                    m_autoSaveTimer.Interval = m_autoSaveInterval;
                    m_autoSaveTimer.Start();
                }
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                // Stop the timer if it is ticking.
                m_autoSaveTimer.Stop();

                // Stop monitoring for changes to the file.
                m_fileWatcher.EnableRaisingEvents = false;

                // Save records back to the file if specified.
                if (m_saveOnClose) Save();

                // Close the file stream used for file I/O.
                if (m_fileData != null)
                {
                    lock (m_fileData)
                    {
                        m_fileData.Dispose();
                    }
                }
                m_fileData = null;

                // Clear the records loaded in memory.
                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                    }
                }
                m_fileRecords = null;
            }
        }

        /// <summary>
        /// Loads records from disk into memory.
        /// </summary>
        public void Load()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_loadWaitHandle.Reset();

                try
                {
                    OnDataLoading();

                    if (m_fileRecords == null)
                        m_fileRecords = new List<T>();

                    List<T> records = new List<T>(ReadFromDisk());
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }

                    OnDataLoaded();
                }
                finally
                {
                    m_loadWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Saves records loaded in memory to disk.
        /// </summary>
        public void Save()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_saveWaitHandle.Reset();

                try
                {
                    OnDataSaving();

                    // Saves (persists) records to the file, if present in memory.
                    if (m_fileRecords != null)
                    {
                        lock (m_fileRecords)
                        {
                            WriteToDisk(m_fileRecords);
                        }
                    }

                    OnDataSaved();
                }
                finally
                {
                    m_saveWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Writes specified records to disk if records were not loaded in memory otherwise updates the records in memory.
        /// </summary>
        /// <param name="records">Records to be written.</param>
        /// <remarks>
        /// This operation will causes existing records to be deleted and replaced with the ones specified.
        /// </remarks>
        public virtual void Write(IEnumerable<T> records)
        {
            if (IsOpen)
            {
                if (m_fileRecords == null)
                {
                    WriteToDisk(records);
                }
                else
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Writes specified record to disk if records were not loaded in memory otherwise updates the record in memory.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be written.</param>
        /// <param name="record">Record to be written.</param>
        public virtual void Write(int recordIndex, T record)
        {
            if (IsOpen)
            {
                if (record != null)
                {
                    int lastRecordIndex = m_fileRecords == null ? RecordsOnDisk : RecordsInMemory;
                    if (recordIndex > lastRecordIndex + 1)
                    {
                        // Write missing intermediate records.
                        for (int i = lastRecordIndex + 1; i < recordIndex; i++)
                        {
                            Write(i, CreateNewRecord(i));
                        }
                    }

                    if (m_fileRecords == null)
                    {
                        // Write directly to the file.
                        WriteToDisk(recordIndex, record);
                    }
                    else
                    { 
                        // Update in-memory record list.
                        lastRecordIndex = RecordsInMemory;
                        if (recordIndex == lastRecordIndex + 1)
                        {
                            // Add new record.
                            lock (m_fileRecords)
                            {
                                m_fileRecords.Add(record);
                            }
                        }
                        else if (recordIndex <= lastRecordIndex)
                        {
                            // Update existing record.
                            lock (m_fileRecords)
                            {
                                m_fileRecords[recordIndex - 1] = record;
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentNullException("record");
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads file records from disk if records were not loaded in memory otherwise returns the records in memory.
        /// </summary>
        /// <returns>Records of the file.</returns>
        public virtual IEnumerable<T> Read()
        {
            if (IsOpen)
            {
                if (m_fileRecords == null)
                {
                    // Reads persisted records if no records are in memory.
                    return ReadFromDisk();
                }
                else
                {
                    // Reads records in memory.
                    lock (m_fileRecords)
                    {
                        return m_fileRecords;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads specified file record from disk if records were not loaded in memory otherwise returns the record in memory.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be read.</param>
        /// <returns>Record with the specified ID if it exists; otherwise null.</returns>
        public virtual T Read(int recordIndex)
        {
            if (IsOpen)
            {
                T record = default(T);

                if (recordIndex > 0)
                {
                    // ID of the requested record is valid.
                    if (m_fileRecords == null && recordIndex <= RecordsOnDisk)
                    {
                        // Reads the requested record exists in the file.
                        record = ReadFromDisk(recordIndex);
                    }
                    else if ((m_fileRecords != null) && recordIndex <= RecordsInMemory)
                    {
                        // Uses the requested record from memory.
                        lock (m_fileRecords)
                        {
                            record = m_fileRecords[recordIndex - 1];
                        }
                    }
                }

                return record;
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Raises the <see cref="FileModified"/> event.
        /// </summary>
        protected virtual void OnFileModified()
        {
            if (FileModified != null)
                FileModified(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataLoading"/> event.
        /// </summary>
        protected virtual void OnDataLoading()
        {
            if (DataLoading != null)
                DataLoading(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataLoaded"/> event.
        /// </summary>
        protected virtual void OnDataLoaded()
        {
            if (DataLoaded != null)
                DataLoaded(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataSaving"/> event.
        /// </summary>
        protected virtual void OnDataSaving()
        {
            if (DataSaving != null)
                DataSaving(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="DataSaved"/> event.
        /// </summary>
        protected virtual void OnDataSaved()
        {
            if (DataSaved != null)
                DataSaved(this, EventArgs.Empty);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the file and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if (m_loadWaitHandle != null)
                            m_loadWaitHandle.Close();

                        if (m_saveWaitHandle != null)
                            m_saveWaitHandle.Close();

                        if (m_autoSaveTimer != null)
                        {
                            m_autoSaveTimer.Elapsed -= m_autoSaveTimer_Elapsed;
                            m_autoSaveTimer.Dispose();
                        }

                        if (m_fileWatcher != null)
                        {
                            m_fileWatcher.Changed -= m_fileWatcher_Changed;
                            m_fileWatcher.Dispose();
                        }
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Re-opens the file if currently open.
        /// </summary>
        private void ReOpen()
        {
            if (IsOpen)
            {
                Close();
                Open();
            }
        }

        /// <summary>
        /// Writes records to disk.
        /// </summary>
        /// <param name="records">Records to be written to disk.</param>
        private void WriteToDisk(IEnumerable<T> records)
        {
            // Write all records to disk.
            int index = 0;

            foreach (T item in records)
            {
                WriteToDisk(++index, item);
            }

            // Discard previously existing records that were not written.
            lock (m_fileData)
            {
                m_fileData.SetLength(index * m_recordBuffer.Length);
            }
        }

        /// <summary>
        /// Writes single record to disk.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be written to disk.</param>
        /// <param name="record">Record to be written to disk.</param>
        private void WriteToDisk(int recordIndex, T record)
        {
            lock (m_fileData)
            {
                m_fileData.Seek((recordIndex - 1) * record.BinaryLength, SeekOrigin.Begin);
                m_fileData.Write(record.BinaryImage, 0, record.BinaryLength);
            }
        }

        /// <summary>
        /// Reads all records from disk.
        /// </summary>
        /// <returns>Records from disk.</returns>
        private IEnumerable<T> ReadFromDisk()
        {
            for (int i = 1; i <= RecordsOnDisk; i++)
            {
                yield return ReadFromDisk(i);
            }
        }

        /// <summary>
        /// Read single record from disk.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be read.</param>
        /// <returns>Record from the disk.</returns>
        private T ReadFromDisk(int recordIndex)
        {
            lock (m_fileData)
            {
                m_fileData.Seek((recordIndex - 1) * m_recordBuffer.Length, SeekOrigin.Begin);
                m_fileData.Read(m_recordBuffer, 0, m_recordBuffer.Length);
            }

            T newRecord = CreateNewRecord(recordIndex);
            newRecord.Initialize(m_recordBuffer, 0, m_recordBuffer.Length);

            return newRecord;
        }

        private void m_autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Automatically save records to disk if loaded in memory.
            if ((m_fileRecords != null) && IsOpen) Save();
        }

        private void m_fileWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            OnFileModified();

            // Reload records if they have been loaded in memory and reloading is enabled.
            if ((m_fileRecords != null) && m_reloadOnModify) Load();
        }

        #endregion
    }
}
