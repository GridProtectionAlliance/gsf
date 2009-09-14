//*******************************************************************************************************
//  LogFile.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/09/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/21/2008 - Pinal C. Patel
//       Edited code comments.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  07/02/2009 - Pinal C. Patel
//       Modified state alterning properties to reopen the file when changed.
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
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;

namespace TVA.IO
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the operation to be performed on the <see cref="LogFile"/> when it is full.
    /// </summary>
    public enum LogFileFullOperation
    {
        /// <summary>
        /// Truncates the existing entries in the <see cref="LogFile"/> to make space for new entries.
        /// </summary>
        Truncate,
        /// <summary>
        /// Rolls over to a new <see cref="LogFile"/>, and keeps the full <see cref="LogFile"/> for reference.
        /// </summary>
        Rollover
    }

    #endregion

    /// <summary>
    /// Represents a file that can be used for logging messages in real-time.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="LogFile"/> for logging messages:
    /// <code>
    /// using System;
    /// using TVA.IO;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         LogFile log = new LogFile();
    ///         log.Initialize();                       // Initialize the log file.
    ///         log.Open();                             // Open the log file.
    ///         log.WriteTimestampedLine("Test entry"); // Write message to the log file.
    ///         log.Flush();                            // Flush message to the log file.
    ///         log.Close();                            // Close the log file.
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    [ToolboxBitmap(typeof(LogFile))]
    public class LogFile : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the minimum size for a <see cref="LogFile"/>.
        /// </summary>
        public const int MinFileSize = 1;

        /// <summary>
        /// Specifies the maximum size for a <see cref="LogFile"/>.
        /// </summary>
        public const int MaxFileSize = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "LogFile.txt";

        /// <summary>
        /// Specifies the default value for the <see cref="FileSize"/> property.
        /// </summary>
        public const int DefaultFileSize = 3;

        /// <summary>
        /// Specifies the default value for the <see cref="FileFullOperation"/> property.
        /// </summary>
        public const LogFileFullOperation DefaultFileFullOperation = LogFileFullOperation.Truncate;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "LogFile";

        // Events

        /// <summary>
        /// Occurs when the <see cref="LogFile"/> is full.
        /// </summary>
        [Description("Occurs when the LogFile is full.")]
        public event EventHandler FileFull;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while writing entries to the <see cref="LogFile"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered during writing of log entries.
        /// </remarks>
        [Description("Occurs when an Exception is encountered while writing entries to the LogFile.")]
        public event EventHandler<EventArgs<Exception>> LogException;

        // Fields
        private string m_fileName;
        private int m_fileSize;
        private LogFileFullOperation m_fileFullOperation;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private FileStream m_fileStream;
        private ManualResetEvent m_operationWaitHandle;
        private ProcessQueue<string> m_logEntryQueue;
        private Encoding m_textEncoding;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        public LogFile()
            : base()
        {
            m_fileName = DefaultFileName;
            m_fileSize = DefaultFileSize;
            m_fileFullOperation = DefaultFileFullOperation;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_textEncoding = Encoding.Default;
            m_operationWaitHandle = new ManualResetEvent(true);
            m_logEntryQueue = ProcessQueue<string>.CreateSynchronousQueue(WriteLogEntries);

            this.FileFull += LogFile_FileFull;
            m_logEntryQueue.ProcessException += ProcessExceptionHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="LogFile"/>.</param>
        public LogFile(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="LogFile"/>, including the file extension.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Settings"),
        DefaultValue(DefaultFileName),
        Description("Name of the LogFile, including the file extension.")]
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
        /// Gets or sets the size of the <see cref="LogFile"/> in MB.
        /// </summary>
        ///<exception cref="ArgumentOutOfRangeException">The value being assigned outside the <see cref="MinFileSize"/> and <see cref="MaxFileSize"/> range.</exception>
        [Category("Settings"),
        DefaultValue(DefaultFileSize),
        Description("Size of the LogFile in MB.")]
        public int FileSize
        {
            get
            {
                return m_fileSize;
            }
            set
            {
                if (value < MinFileSize || value > MaxFileSize)
                    throw new ArgumentOutOfRangeException("value", string.Format("Value must be between {0} and {1}.", MinFileSize, MaxFileSize));

                m_fileSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of operation to be performed when the <see cref="LogFile"/> is full.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultFileFullOperation),
        Description("Type of operation to be performed when the LogFile is full.")]
        public LogFileFullOperation FileFullOperation
        {
            get
            {
                return m_fileFullOperation;
            }
            set
            {
                m_fileFullOperation = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="LogFile"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of LogFile object are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="LogFile"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of LogFile object are to be saved to the config file if the PersistSettings property is set to true.")]
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
        /// Gets or sets the <see cref="Encoding"/> to be used to encode the messages being logged.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                if (value == null)
                    m_textEncoding = Encoding.Default;
                else
                    m_textEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="LogFile"/> object is currently enabled.
        /// </summary>
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
                if (value && !Enabled)
                    Open();
                else if (!value && Enabled)
                    Close();
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="LogFile"/> is open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileStream != null);
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="LogFile"/> object.
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
        /// Gets the descriptive status of the <see cref="LogFile"/> object.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("     Configuration section: ");
                status.Append(m_settingsCategory);
                status.AppendLine();
                status.Append("       Maximum export size: ");
                status.Append(m_fileSize.ToString());
                status.Append(" MB");
                status.AppendLine();
                status.Append("       File full operation: ");
                status.Append(m_fileFullOperation);
                status.AppendLine();

                if (m_logEntryQueue != null)
                    status.Append(m_logEntryQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="LogFile"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="LogFile"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="LogFile"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="LogFile"/> object is consumed through the designer surface of the IDE.
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
        /// Performs necessary operations after the <see cref="LogFile"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="LogFile"/> object is consumed through the designer surface of the IDE.
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
        /// Saves settings for the <see cref="LogFile"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
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
                element = settings["FileSize", true];
                element.Update(m_fileSize, element.Description, element.Encrypted);
                element = settings["FileFullOperation", true];
                element.Update(m_fileFullOperation, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="LogFile"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
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
                settings.Add("FileName", m_fileName, "Name of the log file including its path.");
                settings.Add("FileSize", m_fileSize, "Maximum size of the log file in MB.");
                settings.Add("FileFullOperation", m_fileFullOperation, "Operation (Truncate; Rollover) that is to be performed on the file when it is full.");
                FileName = settings["FileName"].ValueAs(m_fileName);
                FileSize = settings["FileSize"].ValueAs(m_fileSize);
                FileFullOperation = settings["FileFullOperation"].ValueAs(m_fileFullOperation);
            }
        }

        /// <summary>
        /// Opens the <see cref="LogFile"/> for use if it is closed.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                // Initialize if uninitialized.
                Initialize();

                // Get the absolute file path if a relative path is specified.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                // Create the folder in which the log file will reside it, if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                // Open the log file (if it exists) or creates it (if it does not exist).
                m_fileStream = new FileStream(m_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // Scrolls to the end of the file so that existing data is not overwritten.
                m_fileStream.Seek(0, SeekOrigin.End);

                // If this is a new log file, set its creation date to current date. This is done to prevent historic
                // log files (when FileFullOperation = Rollover) from having the same start time in their filename.
                if (m_fileStream.Length == 0)
                    (new FileInfo(m_fileName)).CreationTime = DateTime.Now;

                // Start the queue to which log entries are going to be added.
                m_logEntryQueue.Start();
            }
        }

        /// <summary>
        /// Closes the <see cref="LogFile"/> if it is open.
        /// </summary>
        /// <remarks>
        /// Forces queued log entries to be flushed to the <see cref="LogFile"/>.
        /// </remarks>
        public void Close()
        {
            Close(true);
        }

        /// <summary>
        /// Closes the <see cref="LogFile"/> if it is open.
        /// </summary>
        /// <param name="flushQueuedEntries">true, if queued log entries are to be written to the <see cref="LogFile"/>; otherwise, false.</param>
        public void Close(bool flushQueuedEntries)
        {
            if (IsOpen)
            {
                if (flushQueuedEntries)
                    // Writes all queued log entries to the file.
                    Flush();
                else
                    // Stops processing the queued log entries.
                    m_logEntryQueue.Stop();

                if (m_fileStream != null)
                {
                    // Closes the log file.
                    m_fileStream.Close();
                    m_fileStream = null;
                }
            }
        }

        /// <summary>
        /// Forces queued log entries to be written to the <see cref="LogFile"/>.
        /// </summary>
        public void Flush()
        {
            if (IsOpen)
                m_logEntryQueue.Flush();
        }

        /// <summary>
        /// Queues the text for writing to the <see cref="LogFile"/>.
        /// </summary>
        /// <param name="text">The text to be written to the <see cref="LogFile"/>.</param>
        public void Write(string text)
        {
            // Yield to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (IsOpen)
                // Queue the text for writing to the log file.
                m_logEntryQueue.Add(text);
            else
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
        }

        /// <summary>
        /// Queues the text for writing to the <see cref="LogFile"/>.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>
        /// In addition to the specified text, a "newline" character will be appended to the text.
        /// </remarks>
        public void WriteLine(string text)
        {
            Write(text + "\r\n");
        }

        /// <summary>
        /// Queues the text for writing to the log file.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>
        /// In addition to the specified text, a timestamp will be prepended, and a "newline" character will appended to the text.
        /// </remarks>
        public void WriteTimestampedLine(string text)
        {
            Write("[" + DateTime.Now.ToString() + "] " + text + "\r\n");
        }

        /// <summary>
        /// Reads and returns the text from the <see cref="LogFile"/>.
        /// </summary>
        /// <returns>The text read from the <see cref="LogFile"/>.</returns>
        public string ReadText()
        {
            // Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (IsOpen)
            {
                byte[] buffer = null;

                lock (m_fileStream)
                {
                    buffer = new byte[m_fileStream.Length];
                    m_fileStream.Seek(0, SeekOrigin.Begin);
                    m_fileStream.Read(buffer, 0, buffer.Length);
                }

                return m_textEncoding.GetString(buffer);
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads text from the <see cref="LogFile"/> and returns a list of lines created by seperating the text by the "newline"
        /// characters if and where present.
        /// </summary>
        /// <returns>A list of lines from the text read from the <see cref="LogFile"/>.</returns>
        public List<string> ReadLines()
        {
            return new List<string>(ReadText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        }

        /// <summary>
        /// Raises the <see cref="FileFull"/> event.
        /// </summary>
        protected virtual void OnFileFull()
        {
            if (FileFull != null)
                FileFull(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="LogException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="LogException"/> event.</param>
        protected virtual void OnLogException(Exception ex)
        {
            if (LogException != null)
                LogException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LogFile"/> object and optionally releases the managed resources.
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

                        this.FileFull -= LogFile_FileFull;

                        if (m_operationWaitHandle != null)
                            m_operationWaitHandle.Close();

                        if (m_fileStream != null)
                            m_fileStream.Dispose();

                        if (m_logEntryQueue != null)
                        {
                            m_logEntryQueue.ProcessException -= ProcessExceptionHandler;
                            m_logEntryQueue.Dispose();
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

        private void WriteLogEntries(string[] items)
        {
            long currentFileSize = 0;
            long maximumFileSize = (long)(m_fileSize * 1048576);

            lock (m_fileStream)
            {
                currentFileSize = m_fileStream.Length;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (!string.IsNullOrEmpty(items[i]))
                {
                    // Write entries with text.
                    byte[] buffer = m_textEncoding.GetBytes(items[i]);

                    if (currentFileSize + buffer.Length <= maximumFileSize)
                    {
                        // Writes the entry.
                        lock (m_fileStream)
                        {
                            m_fileStream.Write(buffer, 0, buffer.Length);
                        }

                        currentFileSize += buffer.Length;
                    }
                    else
                    {
                        // Either truncates the file or rolls over to a new file because the current file is full.
                        // Prior to acting, it requeues the entries that have not been written to the file.
                        for (int j = items.Length - 1; j >= i; j--)
                        {
                            m_logEntryQueue.Insert(0, items[j]);
                        }

                        // Truncates file or roll over to new file.
                        OnFileFull();

                        return;
                    }
                }
            }
        }

        private void LogFile_FileFull(object sender, System.EventArgs e)
        {
            // Signals that the "file full operation" is in progress.
            m_operationWaitHandle.Reset();

            switch (m_fileFullOperation)
            {
                case LogFileFullOperation.Truncate:
                    // Deletes the existing log entries, and makes way from new ones.
                    try
                    {
                        Close(false);
                        File.Delete(m_fileName);
                    }
                    finally
                    {
                        Open();
                    }
                    break;
                case LogFileFullOperation.Rollover:
                    // Rolls over to a new log file, and keeps the current file for history.
                    try
                    {
                        Close(false);
                        File.Move(m_fileName, FilePath.GetDirectoryName(m_fileName) + 
                                              FilePath.GetFileNameWithoutExtension(m_fileName) + "_" + 
                                              File.GetCreationTime(m_fileName).ToString("yyyy-MM-dd hh!mm!ss") + "_to_" +
                                              File.GetLastWriteTime(m_fileName).ToString("yyyy-MM-dd hh!mm!ss") + FilePath.GetExtension(m_fileName));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Open();
                    }
                    break;
            }

            // Signals that the "file full operation" is complete.
            m_operationWaitHandle.Set();
        }

        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnLogException(e.Argument);
        }

        #endregion
    }
}