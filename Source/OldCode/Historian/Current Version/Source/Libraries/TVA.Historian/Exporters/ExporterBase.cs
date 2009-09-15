//*******************************************************************************************************
//  ExporterBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/13/2007 - Pinal C. Patel
//       Original version of source code generated.
//  02/13/2008 - Pinal C. Patel
//       Modified DataSetTemplate() method to handle null strings for input and output tables.
//       Changed the type for dataset columns ID to Integer, Value to Single and Quality to Integer.
//  02/25/2008 - Pinal C. Patel
//       Modified the DataSetTemplate() method to remove table 3 as it had little use.
//  06/05/2008 - Pinal C. Patel
//       Made use of new properties LastProcessResult and LastProcessError added to Export class.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//  9/15/2009 - Stephen C. Wills
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using TVA.Collections;
using TVA.Historian.Files;
using TVA.Historian.Packets;

namespace TVA.Historian.Exporters
{
    /// <summary>
    /// A base class for an exporter of real-time time series data.
    /// </summary>
    /// <seealso cref="Export"/>
    /// <seealso cref="DataListener"/>
    public abstract class ExporterBase : IExporter
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// A class that can be used to save real-time time series data for <see cref="Export"/>s of type <see cref="ExportType.RealTime"/>.
        /// </summary>
        /// <seealso cref="Export"/>
        protected class RealTimeData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RealTimeData"/> class.
            /// </summary>
            /// <param name="export">The <see cref="Export"/> to which the <paramref name="data"/> belongs.</param>
            /// <param name="listener">The <see cref="DataListener"/> that provided the <paramref name="data"/>.</param>
            /// <param name="data">The real-time data packets received by the <paramref name="listener"/>.</param>
            public RealTimeData(Export export, DataListener listener, IList<IPacket> data)
            {
                this.Export = export;
                this.Listener = listener;
                this.Data = data;
            }

            /// <summary>
            /// Gets or sets the <see cref="Export"/> to which the <see cref="Data"/> belongs.
            /// </summary>
            public Export Export;

            /// <summary>
            /// Gets or sets the <see cref="DataListener"/> that provided the <see cref="Data"/>.
            /// </summary>
            public DataListener Listener;

            /// <summary>
            /// Gets or sets the real-time data packets received by the <see cref="Listener"/>.
            /// </summary>
            public IList<IPacket> Data;
        }

        // Constants

        /// <summary>
        /// Number of seconds to wait to obtain a write lock on a file.
        /// </summary>
        public const int FileLockWaitTime = 3;

        /// <summary>
        /// Maximum number of request that could be queued in the real-time and non real-time queues.
        /// </summary>
        private const int MaximumQueuedRequest = 1000;

        // Events

        /// <summary>
        /// Occurs when the exporter want to provide a status update.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the status update message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusUpdate;

        /// <summary>
        /// Occurs when the exporter finishes processing an <see cref="Export"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Export"/> that the exporter finished processing.
        /// </remarks>
        public event EventHandler<EventArgs<Export>> ExportProcessed;

        /// <summary>
        /// Occurs when the exporter fails to process an <see cref="Export"/> due to an <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Export"/> that the exporter failed to process.
        /// </remarks>
        public event EventHandler<EventArgs<Export>> ExportProcessException;

        // Fields
        private string m_name;
        private ObservableCollection<Export> m_exports;
        private ObservableCollection<DataListener> m_listeners;
        private Action<Export> m_exportAddedHandler;
        private Action<Export> m_exportRemovedHandler;
        private Action<Export> m_exportUpdatedHandler;
        private Timer m_exportTimer;
        private ProcessQueue<RealTimeData> m_realTimeQueue;
        private ProcessQueue<Export> m_nonRealTimeQueue;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the exporter.
        /// </summary>
        /// <param name="name">Name of the exporter.</param>
        protected ExporterBase(string name)
        {
            m_name = name;
            m_exports = new ObservableCollection<Export>();
            m_exports.CollectionChanged += Exports_CollectionChanged;
            m_listeners = new ObservableCollection<DataListener>();
            m_listeners.CollectionChanged += Listeners_CollectionChanged;
            m_exportTimer = new Timer(1000);
            m_exportTimer.Elapsed += ExportTimer_Elapsed;
            m_realTimeQueue = ProcessQueue<RealTimeData>.CreateRealTimeQueue(ProcessRealTimeExports);
            m_nonRealTimeQueue = ProcessQueue<Export>.CreateRealTimeQueue(ProcessExport);

            m_exportTimer.Start();
            m_realTimeQueue.Start();
            m_nonRealTimeQueue.Start();
        }

        /// <summary>
        /// Releases the unmanaged resources before the exporter is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ExporterBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the exporter.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_name = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Export"/>s associated with the exporter.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="Exports"/> is therad unsafe. Synchronized access is required.
        /// </remarks>
        public IList<Export> Exports
        {
            get
            {
                return m_exports;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataListener"/>s providing real-time time series data to the exporter.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="Listeners"/> is therad unsafe. Synchronized access is required.
        /// </remarks>
        public IList<DataListener> Listeners
        {
            get
            {
                return m_listeners;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> to be invoked when a new <see cref="Export"/> is added to the <see cref="Exports"/>.
        /// </summary>
        protected Action<Export> ExportAddedHandler
        {
            get
            {
                return m_exportAddedHandler;
            }
            set
            {
                m_exportAddedHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> to be invoked when an existing <see cref="Export"/> is removed from the <see cref="Exports"/>.
        /// </summary>
        protected Action<Export> ExportRemovedHandler
        {
            get
            {
                return m_exportRemovedHandler;
            }
            set
            {
                m_exportRemovedHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> to be invoked when an existing <see cref="Export"/> from the <see cref="Exports"/> is updated.
        /// </summary>
        protected Action<Export> ExportUpdatedHandler
        {
            get
            {
                return m_exportUpdatedHandler;
            }
            set
            {
                m_exportUpdatedHandler = value;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, processes the <paramref name="export"/> using the current <see cref="DataListener.Data"/>.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to be processed.</param>
        protected abstract void ProcessExport(Export export);

        /// <summary>
        /// When overridden in a derived class, processes the <paramref name="export"/> using the real-time <paramref name="data"/>.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to be processed.</param>
        /// <param name="listener"><see cref="DataListener"/> that provided the <paramref name="data"/>.</param>
        /// <param name="data">Real-time time series data received by the <paramref name="listener"/>.</param>
        protected abstract void ProcessRealTimeExport(Export export, DataListener listener, IList<IDataPoint> data);

        #endregion

        /// <summary>
        /// Releases all the resources used by the exporter.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Processes <see cref="Export"/> with the specified <paramref name="exportName"/>.
        /// </summary>
        /// <param name="exportName"><see cref="Export.Name"/> of the <see cref="Export"/> to be processed.</param>
        /// <exception cref="InvalidOperationException"><see cref="Export"/> does not exist for the specified <paramref name="exportName"/>.</exception>
        public void ProcessExport(string exportName)
        {
            Export export = FindExport(exportName);
            if (export != null)
                // Queue the export for processing regardless of its type.
                m_nonRealTimeQueue.Add(export);
            else
                throw new InvalidOperationException(string.Format("Export \"{0}\" does not exist.", exportName));
        }

        /// <summary>
        /// Returns the <see cref="Export"/> for the specified <paramref name="exportName"/> from the <see cref="Exports"/>.
        /// </summary>
        /// <param name="exportName"><see cref="Export.Name"/> of the <see cref="Export"/> to be retrieved.</param>
        /// <returns>An <see cref="Export"/> object if a match is found; otherwise null.</returns>
        public Export FindExport(string exportName)
        {
            lock (m_exports)
            {
                return m_exports.FirstOrDefault(export => (string.Compare(export.Name, exportName, true) == 0));
            }
        }

        /// <summary>
        /// Returns the <see cref="DataListener"/> for the specified <paramref name="listenerName"/> from the <see cref="Listeners"/>.
        /// </summary>
        /// <param name="listenerName"><see cref="DataListener.Name"/> of the <see cref="DataListener"/> to be retrieved.</param>
        /// <returns>A <see cref="DataListener"/> object if a match is found; otherwise null.</returns>
        public DataListener FindListener(string listenerName)
        {
            lock (m_listeners)
            {
                return m_listeners.FirstOrDefault(listener => (string.Compare(listener.Name, listenerName, true) == 0));
            }
        }

        /// <summary>
        /// Determines whether the current exporter object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current exporter object is to be compared for equality.</param>
        /// <returns>true if the current exporter object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            ExporterBase other = obj as ExporterBase;
            if (other == null)
                return false;
            else
                return string.Compare(m_name, other.Name, true) == 0;
        }

        /// <summary>
        /// Returns the hash code for the current exporter object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_name.GetHashCode();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the exporter and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_realTimeQueue != null)
                            m_realTimeQueue.Dispose();

                        if (m_nonRealTimeQueue != null)
                            m_nonRealTimeQueue.Dispose();

                        if (m_exportTimer != null)
                        {
                            m_exportTimer.Elapsed -= ExportTimer_Elapsed;
                            m_exportTimer.Dispose();
                        }

                        // Remove all associated exports.
                        if (m_exports != null)
                        {
                            lock (m_exports)
                            {
                                while (m_exports.GetEnumerator().MoveNext())
                                {
                                    m_exports.RemoveAt(0);
                                }
                            }
                            m_exports.CollectionChanged -= Exports_CollectionChanged;
                        }

                        // Remove all associated listeners.
                        if (m_listeners != null)
                        {
                            lock (m_listeners)
                            {
                                while (m_listeners.GetEnumerator().MoveNext())
                                {
                                    m_listeners.RemoveAt(0);
                                }
                            }
                            m_listeners.CollectionChanged -= Listeners_CollectionChanged;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusUpdate"/> event.
        /// </summary>
        /// <param name="status">Status update message to send to <see cref="StatusUpdate"/> event.</param>
        protected virtual void OnStatusUpdate(string status)
        {
            if (StatusUpdate != null)
                StatusUpdate(this, new EventArgs<string>(status));
        }

        /// <summary>
        /// Raises the <see cref="ExportProcessed"/> event.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to send to <see cref="ExportProcessed"/> event.</param>
        protected virtual void OnExportProcessed(Export export)
        {
            export.LastProcessError = null;
            export.LastProcessResult = ExportProcessResult.Success;
            if (ExportProcessed != null)
                ExportProcessed(this, new EventArgs<Export>(export));
        }

        /// <summary>
        /// Raises the <see cref="ExportProcessException"/> event.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to send to <see cref="ExportProcessException"/> event.</param>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="ExportProcessException"/> event.</param>
        protected virtual void OnExportProcessException(Export export, Exception exception)
        {
            export.LastProcessError = exception;
            export.LastProcessResult = ExportProcessResult.Failure;
            if (ExportProcessException != null)
                ExportProcessException(this, new EventArgs<Export>(export));
        }

        /// <summary>
        /// Handles the event that get raised when the <see cref="DataListener.Parser"/> of one of the <see cref="Listeners"/> finishes parsing real-time packets.
        /// </summary>
        /// <param name="sender"><see cref="DataListener"/> object whose <see cref="DataListener.Parser"/> finished parsing real-time packets.</param>
        /// <param name="e"><see cref="EventArgs{T1,T2}"/> object where <see cref="EventArgs{T1,T2}.Argument2"/> is the collection of parsed real-time packets.</param>
        protected virtual void HandleParserDataParsed(object sender, EventArgs<Guid, IList<IPacket>> e)
        {
            DataListener listener = (DataListener)sender;
            List<Export> exportsList = new List<Export>();
            // Get a local copy of all the exports associated with this exporter.
            lock (m_exports)
            {
                exportsList.AddRange(m_exports);
            }

            // Process all the exports.
            foreach (Export export in exportsList)
            {
                if (export.Type == ExportType.RealTime && export.FindRecords(listener.ID).Count > 0)
                {
                    // This export is configured to be processed in real-time and has one or more records 
                    // from this listener to be exported, so we'll queue the real-time data for processing.
                    m_realTimeQueue.Add(new RealTimeData(export, listener, e.Argument2));
                }
            }

            // We prevent flooding the queue by allowing a fixed number of request to queued at a given time.
            // Doing so also prevents running out-of-memory that could be caused by exporters taking longer than
            // normal to process its exports. Longer than normal processing time will back-log processing to a
            // point that it might become impossible to catch-up because of the rate at which data may be parsed.
            while (m_realTimeQueue.Count > MaximumQueuedRequest)
            {
                m_realTimeQueue.RemoveAt(0);
                OnStatusUpdate("Dropped queued real-time export data to prevent flooding.");
            }
        }

        /// <summary>
        /// Returns the current time series data for the specified <paramref name="export"/> organized by listener.
        /// </summary>
        /// <param name="export"><see cref="Export"/> whose current time series data is to be returned.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> object where the <b>key</b> is the <see cref="DataListener.Name"/> and <b>value</b> is the time series data.</returns>
        protected Dictionary<string, IList<IDataPoint>> GetExportData(Export export)
        {
            // Arrange all export records by listeners.
            Dictionary<string, IList<ExportRecord>> exportRecords = new Dictionary<string, IList<ExportRecord>>(StringComparer.CurrentCultureIgnoreCase);
            foreach (ExportRecord record in export.Records)
            {
                if (!exportRecords.ContainsKey(record.Instance))
                    exportRecords.Add(record.Instance, new List<ExportRecord>());

                exportRecords[record.Instance].Add(record);
            }

            // Gather time series data for the export records.
            DataListener listener;
            List<IDataPoint> listenerData = new List<IDataPoint>();
            Dictionary<string, IList<IDataPoint>> exportData = new Dictionary<string, IList<IDataPoint>>();
            foreach (string listenerName in exportRecords.Keys)
            {
                // Don't proceed if the listener doesn't exist.
                if ((listener = FindListener(listenerName)) == null)
                    continue;

                // Get a local copy of the listener's current data.
                listenerData.Clear();
                lock (listener.Data)
                {
                    listenerData.AddRange(listener.Data);
                }

                exportData.Add(listenerName, new List<IDataPoint>());
                if (exportRecords[listenerName].Count == 1 && exportRecords[listenerName][0].Identifier == -1)
                {
                    // Include all current data from the listener.
                    exportData[listenerName].AddRange(listenerData);
                }
                else
                {
                    // Specific records have been defined for this listener, so we'll gather data for those records.
                    foreach (ExportRecord record in exportRecords[listenerName])
                    {
                        if (record.Identifier <= listenerData.Count)
                        {
                            // Data does exist for the defined point.
                            exportData[listenerName].Add(listenerData[record.Identifier - 1]);
                        }
                        else
                        {
                            // Data does not exist for the point, so provide empty data.
                            exportData[listenerName].Add(new ArchiveData(record.Identifier));
                        }
                    }
                }
            }
            return exportData;
        }

        /// <summary>
        /// Returns the current time series data for the specified <paramref name="export"/> in a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="export"><see cref="Export"/> whose current time series data is to be returned.</param>
        /// <param name="dataTableName">Name of the <see cref="DataTable"/> containing the time series data.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        protected DataSet GetExportDataAsDataset(Export export, string dataTableName)
        {
            DataSet result = ExporterBase.DatasetTemplate(dataTableName);
            Dictionary<string, IList<IDataPoint>> exportData = GetExportData(export);

            // Populate the dataset with time series data.
            foreach (string listenerName in exportData.Keys)
            {
                foreach (IDataPoint dataPoint in exportData[listenerName])
                {
                    result.Tables[0].Rows.Add(listenerName, dataPoint.HistorianID, dataPoint.Time.ToString(), dataPoint.Value, (int)dataPoint.Quality);
                }
            }

            // Set the export timestamp, row count and interval.
            result.Tables[1].Rows.Add(DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), result.Tables[0].Rows.Count, string.Format("{0} seconds", export.Interval));

            return result;
        }

        /// <summary>
        /// Processes non-real-time exports.
        /// </summary>
        private void ProcessExports(Export[] items)
        {
            Stopwatch watch = new Stopwatch();
            foreach (Export item in items)
            {
                try
                {
                    watch.Reset();
                    watch.Start();
                    ProcessExport(item);
                    watch.Stop();
                    item.LastProcessTime = watch.Elapsed.TotalSeconds;
                    OnExportProcessed(item);
                }
                catch (Exception ex)
                {
                    OnExportProcessException(item, ex);
                }
            }
        }

        /// <summary>
        /// Processes real-time exports.
        /// </summary>
        private void ProcessRealTimeExports(RealTimeData[] items)
        {
            IList<ExportRecord> exportRecords;
            IEnumerable<IDataPoint> dataPoints;
            foreach (RealTimeData item in items)
            {
                try
                {
                    List<IDataPoint> filteredData = new List<IDataPoint>();

                    exportRecords = item.Export.FindRecords(item.Listener.ID);
                    if (exportRecords.Count == 1 && exportRecords[0].Identifier == -1)
                    {
                        // Include all data from the listener.
                        foreach (IPacket packet in item.Data)
                        {
                            dataPoints = packet.ExtractTimeSeriesData();
                            if (dataPoints != null)
                                filteredData.AddRange(dataPoints);
                        }
                    }
                    else
                    {
                        // Export data for selected records only (filtered).
                        foreach (IPacket packet in item.Data)
                        {
                            dataPoints = packet.ExtractTimeSeriesData();
                            if (dataPoints != null)
                            {
                                foreach (IDataPoint dataPoint in dataPoints)
                                {
                                    if (exportRecords.FirstOrDefault(record => record.Identifier == dataPoint.HistorianID) != null)
                                        filteredData.Add(dataPoint);
                                }
                            }
                        }
                    }

                    ProcessRealTimeExport(item.Export, item.Listener, filteredData);
                    item.Export.LastProcessResult = ExportProcessResult.Success;
                }
                catch (Exception ex)
                {
                    OnExportProcessException(item.Export, ex);
                }
            }
        }

        private void ExportTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<Export> exports = new List<Export>();
            lock (m_exports)
            {
                exports.AddRange(m_exports);
            }

            // Process all exports and queue them for processing if it's time.
            foreach (Export export in exports)
            {
                if (export.Type == ExportType.Intervaled && export.ShouldProcess())
                {
                    m_nonRealTimeQueue.Add(export);
                }
            }
        }

        private void Exports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Notify that a new export is added.
                    if (m_exportAddedHandler != null)
                    {
                        foreach (Export export in e.NewItems)
                        {
                            try
                            {
                                m_exportAddedHandler(export);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Notify that an existing export is removed.
                    if (m_exportRemovedHandler != null)
                    {
                        foreach (Export export in e.OldItems)
                        {
                            try
                            {
                                m_exportRemovedHandler(export);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Notify that an existing export is updated.
                    if (m_exportUpdatedHandler != null)
                    {
                        foreach (Export export in e.NewItems)
                        {
                            try
                            {
                                m_exportUpdatedHandler(export);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Listeners_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Register event handler.
                    foreach (DataListener listener in e.NewItems)
                    {
                        listener.Parser.DataParsed += HandleParserDataParsed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Unregister event handler.
                    foreach (DataListener listener in e.NewItems)
                    {
                        listener.Parser.DataParsed -= HandleParserDataParsed;
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Returns a template <see cref="DataSet"/> that can be used for storing time series data in a tabular format.
        /// </summary>
        /// <param name="dataTableName">Name of the <see cref="DataTable"/> that will be used for storing the time series data.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        /// <remarks>
        /// <para>
        /// The returned <see cref="DataSet"/> consists of two <see cref="DataTable"/>s with the following structure:<br/>
        /// </para>
        /// <para>
        /// Table 1 is to be used for storing time series data.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Column Name</term>
        ///         <description>Column Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Instance</term>
        ///         <description>Historian instance providing the time series data.</description>
        ///     </item>
        ///     <item>
        ///         <term>ID</term>
        ///         <description><see cref="IDataPoint.HistorianID"/> of the time series data.</description>
        ///     </item>
        ///     <item>
        ///         <term>Time</term>
        ///         <description><see cref="IDataPoint.Time"/> of the time series data.</description>
        ///     </item>
        ///     <item>
        ///         <term>Value</term>
        ///         <description><see cref="IDataPoint.Value"/> of the time series data.</description>
        ///     </item>
        ///     <item>
        ///         <term>Quality</term>
        ///         <description><see cref="IDataPoint.Quality"/> of the time series data.</description>
        ///     </item>
        /// </list>
        /// Table 2 is to be used for providing information about Table 1.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Column Name</term>
        ///         <description>Column Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>RunTime</term>
        ///         <description><see cref="DateTime"/> (in UTC) when the data in Table 1 was populated.</description>
        ///     </item>
        ///     <item>
        ///         <term>RecordCount</term>
        ///         <description>Number of time series data points in Table 1.</description>
        ///     </item>
        ///     <item>
        ///         <term>RefreshSchedule</term>
        ///         <description>Interval (in seconds) at which the data in Table 1 is to be refreshed.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public static DataSet DatasetTemplate(string dataTableName)
        {
            // Provide output table name if none provided.
            if (string.IsNullOrEmpty(dataTableName))
                dataTableName = "Measurements";

            DataSet data = new DataSet(dataTableName);
            // -- Table 1 --
            data.Tables.Add(dataTableName);
            DataTable dataTable = data.Tables[dataTableName];
            dataTable.Columns.Add("Instance", typeof(string));
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Time", typeof(string));
            dataTable.Columns.Add("Value", typeof(float));
            dataTable.Columns.Add("Quality", typeof(int));
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns[0], dataTable.Columns[1], dataTable.Columns[2] };
            // -- Table 2 --
            data.Tables.Add("ExportInformation");
            DataTable infoTable = data.Tables["ExportInformation"];
            infoTable.Columns.Add("RunTime", typeof(string));
            infoTable.Columns.Add("RecordCount", typeof(string));
            infoTable.Columns.Add("RefreshSchedule", typeof(string));

            // We'll output the xml data as attributes to save space.
            foreach (DataTable table in data.Tables)
            {
                foreach (DataColumn column in table.Columns)
                {
                    column.ColumnMapping = MappingType.Attribute;
                }
            }

            return data;
        }

        #endregion
    }
}
