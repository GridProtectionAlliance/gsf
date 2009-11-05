//*******************************************************************************************************
//  ActionAdapterBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/29/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
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
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TVA.Collections;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the base class for action adapters.
    /// </summary>
    /// <remarks>
    /// This base class acts as a measurement concentrator which time aligns all incoming measurements for proper processing.
    /// Derived classes are expected to override <see cref="ConcentratorBase.PublishFrame"/> to handle time aligned measurements
    /// and call <see cref="OnNewMeasurements"/> for any new measurements that may get created.
    /// </remarks>
    [CLSCompliant(false)]
	public abstract class ActionAdapterBase : ConcentratorBase, IActionAdapter
	{
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Provides new measurements from action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private string m_name;
        private uint m_id;
        private string m_connectionString;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private bool m_initialized;
        private MeasurementKey[] m_inputMeasurementKeys;
        private List<MeasurementKey> m_inputMeasurementKeysHash;
        private IMeasurement[] m_outputMeasurements;
        private int m_minimumMeasurementsToUse;
        private ManualResetEvent m_initializeWaitHandle;
        private Regex m_filterExpression = new Regex("(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+)[ ]+ORDER[ ]+BY[ ]+(?<SortField>\\w+))|(FILTER[ ]+(?<TableName>\\w+)[ ]+WHERE[ ]+(?<Expression>.+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ActionAdapterBase"/>.
        /// </summary>
        protected ActionAdapterBase()
        {
            m_initializeWaitHandle = new ManualResetEvent(true);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets key/value pair connection information specific to action adapter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example connection string using manually defined measurements:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumMeasurementsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputMeasurementKeys={P1:1245;P1:1247;P2:1335};</c><br/>
        /// <c>outputMeasurements={P3:1345,60.0,1.0;P3:1346;P3:1347}</c><br/>
        /// When defined manually, elements in key:<br/>
        /// * inputMeasurementKeys are defined as "ArchiveSource:PointID"<br/>
        /// * outputMeasurements are defined as "ArchiveSource:PointID,Adder,Multiplier", the adder and multiplier are optional
        /// defaulting to 0.0 and 1.0 respectively.
        /// <br/>
        /// </para>
        /// <para>
        /// Example connection string using measurements defined in a <see cref="DataSource"/> table:<br/>
        /// <c>framesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumMeasurementsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;</c><br/>
        /// <c>inputMeasurementKeys={FILTER ActiveMeasurements WHERE Company='TVA' AND SignalType='FREQ' ORDER BY ID};</c><br/>
        /// <c>outputMeasurements={FILTER ActiveMeasurements WHERE SignalType IN ('IPHA','VPHA') AND Phase='+'}</c><br/>
        /// <br/>
        /// Basic filtering syntax is as follows:<br/>
        /// <br/>
        ///     {FILTER &lt;TableName&gt; WHERE &lt;Expression&gt; [ORDER BY &lt;SortField&gt;]}<br/>
        /// <br/>
        /// Source tables are expected to have at least the following fields:<br/>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Name</term>
        ///         <term>Type</term>
        ///         <description>Description.</description>
        ///     </listheader>
        ///     <item>
        ///         <term>ID</term>
        ///         <term>NVARCHAR</term>
        ///         <description>Measurement key formatted as: ArchiveSource:PointID.</description>
        ///     </item>
        ///     <item>
        ///         <term>PointTag</term>
        ///         <term>NVARCHAR</term>
        ///         <description>Point tag of measurement.</description>
        ///     </item>
        ///     <item>
        ///         <term>Adder</term>
        ///         <term>FLOAT</term>
        ///         <description>Adder to apply to value, if any (default to 0.0).</description>
        ///     </item>
        ///     <item>
        ///         <term>Multiplier</term>
        ///         <term>FLOAT</term>
        ///         <description>Multipler to apply to value, if any (default to 1.0).</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// Note that expectedFramesPerSecond, lagTime and leadTime are required parameters, all other parameters are optional.
        /// </para>
        /// </remarks>
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Preparse settings upon connection string assignment
                if (string.IsNullOrEmpty(m_connectionString))
                    m_settings = new Dictionary<string, string>();
                else
                    m_settings = m_connectionString.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        public virtual IMeasurement[] OutputMeasurements
        {
            get
            {
                return m_outputMeasurements;
            }
            set
            {
                m_outputMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the action adapter expects.
        /// </summary>
        public virtual MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return m_inputMeasurementKeys;
            }
            set
            {
                m_inputMeasurementKeys = value;

                // Update input key lookup hash table
                m_inputMeasurementKeysHash = new List<MeasurementKey>(value);
                m_inputMeasurementKeysHash.Sort();
            }
        }

        /// <summary>
        /// Gets or sets minimum number of input measurements required for adapter.  Set to -1 to require all.
        /// </summary>
        public virtual int MinimumMeasurementsToUse
        {
            get
            {
                // Default to all measurements if minimum is not specified
                if (m_minimumMeasurementsToUse < 1)
                    return InputMeasurementKeys.Length;
                
                return m_minimumMeasurementsToUse;
            }
            set
            {
                m_minimumMeasurementsToUse = value;
            }
        }

        /// <summary>
        /// Gets name of the action adapter.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets numeric ID associated with this action adapter.
        /// </summary>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the action adapter has been initialized successfully.
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                return m_initialized;
            }
            set
            {
                m_initialized = value;

                // When initialization is complete we send notification
                if (value)
                    m_initializeWaitHandle.Set();
                else
                    m_initializeWaitHandle.Reset();
            }
        }

        /// <summary>
        /// Gets settings <see cref="Dictionary{TKey,TValue}"/> parsed when <see cref="ConnectionString"/> was assigned.
        /// </summary>
        protected Dictionary<string, string> Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
			{
				const int MaxMeasurementsToShow = 4;
				
				StringBuilder status = new StringBuilder();

                status.AppendFormat("    Referenced data source: {0}, {1} tables", DataSource.DataSetName, DataSource.Tables.Count);
                status.AppendLine();
                status.AppendFormat("       Adapter initialized: {0}", Initialized);
                status.AppendLine();
                status.AppendFormat("         Operational state: {0}", Enabled ? "Running" : "Stopped");
                status.AppendLine();
                status.AppendFormat("                Adpater ID: {0}", ID);
                status.AppendLine();

                Dictionary<string, string> keyValuePairs = Settings;
                char[] keyChars;
                string value;

                status.AppendFormat("         Connection string: {0} key/value pairs", keyValuePairs.Count);
                //                            1         2         3         4         5         6         7
                //                   123456789012345678901234567890123456789012345678901234567890123456789012345678
                //                                         Key = Value
                //                                                        1         2         3         4         5
                //                                               12345678901234567890123456789012345678901234567890
                status.AppendLine();
                status.AppendLine();

                foreach (KeyValuePair<string, string> item in keyValuePairs)
                {
                    keyChars = item.Key.Trim().ToCharArray();
                    keyChars[0] = char.ToUpper(keyChars[0]);

                    value = item.Value.Trim();
                    if (value.Length > 50)
                        value = value.TruncateRight(47) + "...";

                    status.AppendFormat("{0} = {1}", (new string(keyChars)).TruncateRight(25).PadLeft(25), value.PadRight(50));
                    status.AppendLine();
                }

                status.AppendLine();

                if (OutputMeasurements != null)
                {
                    status.Append("   Output measurement ID\'s: ");

                    if (OutputMeasurements.Length <= MaxMeasurementsToShow)
                    {
                        status.Append(OutputMeasurements.ToDelimitedString(','));
                    }
                    else
                    {
                        IMeasurement[] outputMeasurements = new IMeasurement[MaxMeasurementsToShow];
                        Array.Copy(OutputMeasurements, 0, outputMeasurements, 0, MaxMeasurementsToShow);
                        status.Append(outputMeasurements.ToDelimitedString(','));
                        status.Append(",...");
                    }

                    status.AppendLine();
                }

                if (InputMeasurementKeys != null)
                {
                    status.Append("    Input measurement ID\'s: ");

                    if (InputMeasurementKeys.Length <= MaxMeasurementsToShow)
                    {
                        status.Append(InputMeasurementKeys.ToDelimitedString(','));
                    }
                    else
                    {
                        MeasurementKey[] inputMeasurements = new MeasurementKey[MaxMeasurementsToShow];
                        Array.Copy(InputMeasurementKeys, 0, inputMeasurements, 0, MaxMeasurementsToShow);
                        status.Append(inputMeasurements.ToDelimitedString(','));
                        status.Append(",...");
                    }

                    status.AppendLine();
                }

				status.Append(" Minimum measurements used: ");
				status.Append(MinimumMeasurementsToUse);
				status.AppendLine();
				status.Append(base.Status);

				return status.ToString();
			}
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ActionAdapterBase"/> object and optionally releases the managed resources.
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
                        if (m_initializeWaitHandle != null)
                            m_initializeWaitHandle.Close();

                        m_initializeWaitHandle = null;
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
        /// Initializes <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual void Initialize()
        {
            Initialized = false;

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load required parameters
            base.FramesPerSecond = int.Parse(settings["framesPerSecond"]);
            base.LagTime = double.Parse(settings["lagTime"]);
            base.LeadTime = double.Parse(settings["leadTime"]);

            // Load optional parameters
            if (settings.TryGetValue("useLocalClockAsRealTime", out setting))
                base.UseLocalClockAsRealTime = setting.ParseBoolean();

            if (settings.TryGetValue("allowSortsByArrival", out setting))
                base.AllowSortsByArrival = setting.ParseBoolean();

            if (settings.TryGetValue("inputMeasurementKeys", out setting))
                InputMeasurementKeys = ParseInputMeasurementKeys(setting);

            if (settings.TryGetValue("outputMeasurements", out setting))
                OutputMeasurements = ParseOutputMeasurements(setting);

            if (settings.TryGetValue("minimumMeasurementsToUse", out setting))
                MinimumMeasurementsToUse = int.Parse(setting);
        }

        /// <summary>
        /// Starts the <see cref="ActionAdapterBase"/>, if it is not already running.
        /// </summary>
        [AdapterCommand("Starts the action adapter, if it is not already running.")]
        public override void Start()
        {
            // Wait for adapter intialization to complete...
            m_initializeWaitHandle.WaitOne();
            base.Start();
        }

        /// <summary>
        /// Stops the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Stops the action adapter.")]
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Manually sets the intialized state of the <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="initialized">Desired initialized state.</param>
        [AdapterCommand("Manually sets the intialized state of the action adapter.")]
        public virtual void SetInitializedState(bool initialized)
        {
            this.Initialized = initialized;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            return string.Format("Total input measurements: {0}, total output measurements: {1}", (InputMeasurementKeys == null ? 0 : InputMeasurementKeys.Length), (OutputMeasurements == null ? 0 : OutputMeasurements.Length)).PadLeft(maxLength);
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        /// <remarks>
        /// Measurement is filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            // If this is an input measurement to this adapter, sort it!
            if (IsInputMeasurement(measurement.Key))
                SortMeasurement(measurement);
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            List<IMeasurement> inputMeasurements = new List<IMeasurement>();

            foreach (IMeasurement measurement in measurements)
            {
                if (IsInputMeasurement(measurement.Key))
                    inputMeasurements.Add(measurement);
            }

            if (inputMeasurements.Count > 0)
                SortMeasurements(inputMeasurements);
        }

        /// <summary>
        /// Determines if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="item">Primary key of measurement to find.</param>
        /// <returns>true if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.</returns>
        public virtual bool IsInputMeasurement(MeasurementKey item)
        {
            return (m_inputMeasurementKeysHash.BinarySearch(item) >= 0);
        }

        /// <summary>
        /// Attempts to retrieve the minimum needed number of measurements from the frame (as specified by MinimumMeasurementsToUse)
        /// </summary>
        /// <param name="frame">Source frame for the measurements</param>
        /// <param name="measurements">Return array of measurements</param>
        /// <returns>True if minimum needed number of measurements were returned in measurement array</returns>
        /// <remarks>
        /// <para>
        /// Remember this function will *only* return the minimum needed number of measurements, no more.  If you want to use
        /// all available measurements in your adapter you should just use Frame.Measurements.Values directly.
        /// </para>
        /// <para>
        /// Note that the measurements array parameter will be created if the reference is null, otherwise if caller creates
        /// array it must be sized to MinimumMeasurementsToUse
        /// </para>
        /// </remarks>
        protected virtual bool TryGetMinimumNeededMeasurements(IFrame frame, ref IMeasurement[] measurements)
        {
            int index = 0, minNeeded = MinimumMeasurementsToUse;
            IDictionary<MeasurementKey, IMeasurement> frameMeasurements = frame.Measurements;
            MeasurementKey[] measurementKeys = InputMeasurementKeys;
            IMeasurement measurement;

            if (measurements == null || measurements.Length < minNeeded)
                measurements = new IMeasurement[minNeeded];

            // Loop through all input measurements to see if they exist in this frame
            for (int x = 0; x < measurementKeys.Length; x++)
            {
                if (frameMeasurements.TryGetValue(measurementKeys[x], out measurement))
                {
                    measurements[index++] = measurement;
                    if (index == minNeeded)
                        break;
                }
            }

            return (index == minNeeded);
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(status));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convienence.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
        }

        // Input keys can use DataSource for filtering desired set of input or output measurements
        // based on any table and fields in the data set by using a filter expression instead of
        // a list of measurement ID's. The format is as follows:

        //  FILTER <TableName> WHERE <Expression> [ORDER BY <SortField>]

        // Source tables are expected to have at least the following fields:
        //
        //      ID          NVARCHAR    Measurement key formatted as: ArchiveSource:PointID
        //      PointTag    NVARCHAR    Point tag of measurement
        //      Adder       FLOAT       Adder to apply to value, if any (default to 0.0)
        //      Multiplier  FLOAT       Multipler to apply to value, if any (default to 1.0)
        //
        // Could have used standard SQL syntax here but didn't want to give user the impression
        // that this a standard SQL expression when it isn't - so chose the word FILTER to make
        // consumer was aware that this was not SQL, but SQL "like". The WHERE clause expression
        // uses standard SQL syntax (it is simply the DataTable.Select filter expression).

        // Parse input measurement keys from connection string
        private MeasurementKey[] ParseInputMeasurementKeys(string value)
        {
            List<MeasurementKey> keys = new List<MeasurementKey>();
            Match filterMatch;

            value = value.Trim();
            filterMatch = m_filterExpression.Match(value);

            if (filterMatch.Success)
            {
                string tableName = filterMatch.Result("${TableName}").Trim();
                string expression = filterMatch.Result("${Expression}").Trim();
                string sortField = filterMatch.Result("${SortField}").Trim();

                foreach (DataRow row in DataSource.Tables[tableName].Select(expression, sortField))
                {
                    keys.Add(MeasurementKey.Parse(row["ID"].ToString()));
                }
            }
            else
            {
                // Add manually defined measurement keys
                foreach (string item in value.Split(';'))
                {
                    keys.Add(MeasurementKey.Parse(item));
                }
            }
            
            return keys.ToArray();
        }

        // Parse output measurements from connection string
        private IMeasurement[] ParseOutputMeasurements(string value)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            MeasurementKey key;
            Match filterMatch;

            value = value.Trim();
            filterMatch = m_filterExpression.Match(value);

            if (filterMatch.Success)
            {
                string tableName = filterMatch.Result("${TableName}").Trim();
                string expression = filterMatch.Result("${Expression}").Trim();
                string sortField = filterMatch.Result("${SortField}").Trim();

                foreach (DataRow row in DataSource.Tables[tableName].Select(expression, sortField))
                {
                    key = MeasurementKey.Parse(row["ID"].ToString());
                    measurements.Add(new Measurement(key.ID, key.Source, row["PointTag"].ToNonNullString(), double.Parse(row["Adder"].ToString()), double.Parse(row["Multiplier"].ToString())));
                }
            }
            else
            {
                string[] elem;
                double adder, multipler;

                foreach (string item in value.Split(';'))
                {
                    elem = item.Trim().Split(',');

                    key = MeasurementKey.Parse(elem[0]);

                    if (elem.Length > 1)
                        adder = double.Parse(elem[1].Trim());
                    else
                        adder = 0.0D;

                    if (elem.Length > 2)
                        multipler = double.Parse(elem[2].Trim());
                    else
                        multipler = 1.0D;

                    measurements.Add(new Measurement(key.ID, key.Source, string.Empty, adder, multipler));
                }
            }

            return measurements.ToArray();
        }

        #endregion		
	}
}