//*******************************************************************************************************
//  AdapterCollectionBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/06/2009 - J. Ritchie Carroll
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
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TVA.IO;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="IAdapter"/> implementations.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IAdapter"/> this collection contains.</typeparam>
    [CLSCompliant(false)]
    public abstract class AdapterCollectionBase<T> : Collection<T>, IAdapterCollection where T : IAdapter
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
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private string m_name;
        private uint m_id;
        private bool m_initialized;
        private string m_connectionString;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private string m_dataMember;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        protected AdapterCollectionBase()
        {
            m_name = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdapterCollectionBase{T}"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdapterCollectionBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of this <see cref="AdapterCollectionBase{T}"/>.
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
        /// Gets or sets numeric ID associated with this <see cref="AdapterCollectionBase{T}"/>.
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
        /// Gets or sets flag indicating if the adapter collection has been initialized successfully.
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
            }
        }

        /// <summary>
        /// Gets or sets key/value pair connection information specific to this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
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
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>.
        /// Updates to this property will cascade to all items in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
                
                // Update data source for items in this collection
                foreach (T item in this)
                {
                    item.DataSource = m_dataSource;
                }
            }
        }

        /// <summary>
        /// Gets or sets specific data member (e.g., table name) in <see cref="DataSource"/> used to <see cref="Initialize"/> this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        public virtual string DataMember
        {
            get
            {
                return m_dataMember;
            }
            set
            {
                m_dataMember = value;
            }
        }

        /// <summary>
        /// Gets or sets enabled state of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (m_enabled && !value)
                    Stop();
                else if (!m_enabled && value)
                    Start();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="AdapterCollectionBase{T}"/> is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that detemines if <see cref="IAdapter"/> implementations are automatically initialized
        /// when they are added to the collection.
        /// </summary>
        protected virtual bool AutoInitialize
        {
            get
            {
                return true;
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
        /// Gets the descriptive status of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                // Show collection status
                status.AppendFormat("  Total adapter components: {0}", Count);
                status.AppendLine();
                status.AppendFormat("    Collection initialized: {0}", m_initialized);
                status.AppendLine();
                status.AppendFormat(" Current operational state: {0}", (m_enabled ? "Enabled" : "Disabled"));
                status.AppendLine();
                status.AppendFormat("     Configuration defined: {0}", (m_dataSource != null));
                status.AppendLine();
                status.AppendFormat("    Referenced data source: {0}, {1} tables", DataSource.DataSetName, DataSource.Tables.Count);
                status.AppendLine();
                status.AppendFormat("    Data source table name: {0}", m_dataMember);
                status.AppendLine();

                if (Count > 0)
                {
                    int index = 0;

                    status.AppendLine();
                    status.AppendFormat("Status of each {0} component:", Name);
                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();

                    // Show the status of registered components.
                    foreach (T item in this)
                    {
                        IProvideStatus statusProvider = item as IProvideStatus;

                        if (statusProvider != null)
                        {
                            // This component provides status information.                       
                            status.AppendLine();
                            status.AppendFormat("Status of {0} component {1}, {2}:", typeof(T).Name, ++index, statusProvider.Name);
                            status.AppendLine();
                            status.Append(statusProvider.Status);
                        }
                    }

                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterCollectionBase{T}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdapterCollectionBase{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        Clear();        // This disposes all items in collection...
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Loads all <see cref="IAdapter"/> implementations defined in <see cref="DataSource"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </para>
        /// <para>
        /// Note that when calling this method any existing items will be cleared allowing a "re-initialize".
        /// </para>
        /// </remarks>
        /// <exception cref="NullReferenceException">DataSource is null.</exception>
        /// <exception cref="InvalidOperationException">DataMember is null or empty.</exception>
        public virtual void Initialize()
        {
            if (m_dataSource == null)
                throw new NullReferenceException(string.Format("DataSource is null, cannot load {0}.", Name));

            if (string.IsNullOrEmpty(m_dataMember))
                throw new InvalidOperationException(string.Format("DataMember is null or empty, cannot load {0}.", Name));

            T item;

            Clear();

            if (m_dataSource.Tables.Contains(m_dataMember))
            {
                foreach (DataRow adapterRow in m_dataSource.Tables[m_dataMember].Rows)
                {
                    if (TryCreateAdapter(adapterRow, out item))
                        Add(item);
                }
            }

            m_initialized = true;
        }

        /// <summary>
        /// Attempts to create an <see cref="IAdapter"/> from the specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="adapterRow"><see cref="DataRow"/> containing item information to initialize.</param>
        /// <param name="adapter">Initialized adapter if successful; otherwise null.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// See <see cref="DataSource"/> property for expected <see cref="DataRow"/> column names.
        /// </remarks>
        /// <exception cref="NullReferenceException"><paramref name="adapterRow"/> is null.</exception>
        public virtual bool TryCreateAdapter(DataRow adapterRow, out T adapter)
        {
            if (adapterRow == null)
                throw new NullReferenceException(string.Format("Cannot initialize from null adpater DataRow"));

            Assembly assembly;
            string name = "", assemblyName = "", typeName = "", connectionString;
            uint id;

            try
            {
                name = adapterRow["AdapterName"].ToNonNullString("[IAdapter]");
                assemblyName = FilePath.GetAbsolutePath(adapterRow["AssemblyName"].ToNonNullString());
                typeName = adapterRow["TypeName"].ToNonNullString();
                connectionString = adapterRow["ConnectionString"].ToNonNullString();
                id = uint.Parse(adapterRow["ID"].ToNonNullString("0"));

                if (string.IsNullOrEmpty(typeName))
                    throw new InvalidOperationException("Type was undefined");

                if (!File.Exists(assemblyName))
                    throw new InvalidOperationException("Assembly does not exist.");

                assembly = Assembly.LoadFrom(assemblyName);
                adapter = (T)Activator.CreateInstance(assembly.GetType(typeName));

                adapter.Name = name;
                adapter.ID = id;
                adapter.ConnectionString = connectionString;
                adapter.DataSource = m_dataSource;

                return true;
            }
            catch (Exception ex)
            {
                // We report any errors encountered during type creation...
                OnProcessException(new InvalidOperationException(string.Format("Failed to load adapter \"{0}\" [{1}] from \"{2}\": {3}", name, typeName, assemblyName, ex.Message), ex));
            }

            adapter = default(T);
            return false;
        }

        // Explicit IAdapter implementation of TryCreateAdapter
        bool IAdapterCollection.TryCreateAdapter(DataRow adapterRow, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryCreateAdapter(adapterRow, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="ID"/>.
        /// </summary>
        /// <param name="ID">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="ID"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAdapterByID(uint ID, out T adapter)
        {
            return TryGetAdapter<uint>(ID, (item, value) => item.ID == value, out adapter);
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAdapterByName(string name, out T adapter)
        {
            return TryGetAdapter<string>(name, (item, value) => string.Compare(item.Name, value, true) == 0, out adapter);
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="value"/> given <paramref name="testItem"/> function.
        /// </summary>
        /// <param name="value">Value of adapter to get.</param>
        /// <param name="testItem">Function delegate used to test item <paramref name="value"/>.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="value"/> was found; otherwise <c>false</c>.</returns>
        protected virtual bool TryGetAdapter<TValue>(TValue value, Func<T, TValue, bool> testItem, out T adapter)
        {
            foreach (T item in this)
            {
                if (testItem(item, value))
                {
                    adapter = item;
                    return true;
                }
            }

            adapter = default(T);
            return false;
        }

        // Explicit IAdapter implementation of TryGetAdapterByID
        bool IAdapterCollection.TryGetAdapterByID(uint ID, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryGetAdapterByID(ID, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        // Explicit IAdapter implementation of TryGetAdapterByName
        bool IAdapterCollection.TryGetAdapterByName(string name, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryGetAdapterByName(name, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        /// <summary>
        /// Attempts to initialize (or reinitialize) an individual <see cref="IAdapter"/> based on its ID.
        /// </summary>
        /// <param name="id">The numeric ID associated with the <see cref="IAdapter"/> to be initialized.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        public virtual bool TryInitializeAdapterByID(uint id)
        {
            T newAdapter, oldAdapter;
            uint rowID;

            foreach (DataRow adapterRow in m_dataSource.Tables[m_dataMember].Rows)
            {
                rowID = uint.Parse(adapterRow["ID"].ToNonNullString("0"));

                if (rowID == id)
                {
                    if (TryCreateAdapter(adapterRow, out newAdapter))
                    {
                        // Found and created new item - update collection reference
                        bool foundItem = false;

                        for (int i = 0; i < Count; i++)
                        {
                            oldAdapter = this[i];

                            if (oldAdapter.ID == id)
                            {
                                // Cache original running state
                                bool enabled = oldAdapter.Enabled;

                                // Stop old item
                                oldAdapter.Stop();

                                // Dispose old item, initialize new item
                                this[i] = newAdapter;

                                // If old item was running, start new item
                                if (enabled)
                                    newAdapter.Start();

                                foundItem = true;
                                break;
                            }
                        }

                        // Add item to collection if it didn't exist
                        if (!foundItem)
                            Add(newAdapter);

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts each <see cref="IAdapter"/> implementation in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual void Start()
        {
            m_enabled = true;

            foreach (T item in this)
            {
                item.Start();
            }
        }
        /// <summary>
        /// Stops each <see cref="IAdapter"/> implementation in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            foreach (T item in this)
            {
                item.Stop();
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            return string.Format("Total components: {0}", Count.ToString().PadLeft(5)).PadLeft(maxLength);
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

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            // Dispose each item before clearing the collection
            foreach (T item in this)
            {
                DisposeItem(item);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            // Wire up item events and handle item initialization
            InitializeItem(item);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Assigns a new element to the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index for which item should be assigned.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to assign.</param>
        protected override void SetItem(int index, T item)
        {
            // Dispose of existing item
            DisposeItem(this[index]);

            // Wire up item events and handle initialization of new item
            InitializeItem(item);

            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            // Dispose of item before removing it from the collection
            DisposeItem(this[index]);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IAdapter"/> implementation.</param>
        /// <remarks>
        /// Derived classes should override if more events are defined.
        /// </remarks>
        protected virtual void InitializeItem(T item)
        {
            if (item != null)
            {
                // Wire up events
                item.StatusMessage += StatusMessage;
                item.ProcessException += ProcessException;

                // If automatically initializing new elements, handle object initialization from
                // thread pool so it can take needed amount of time
                if (AutoInitialize)
                    ThreadPool.QueueUserWorkItem(InitializeItem, item);
            }
        }

        // Thread pool delegate to handle item initialization
        private void InitializeItem(object state)
        {
            T item = (T)state;

            try
            {
                item.Initialize();
                item.Initialized = true;
            }
            catch (Exception ex)
            {
                // We report any errors encountered during initialization...
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IAdapter"/> to dispose.</param>
        /// <remarks>
        /// Derived classes should override if more events are defined.
        /// </remarks>
        protected virtual void DisposeItem(T item)
        {
            if (item != null)
            {
                // Un-wire events
                item.StatusMessage -= StatusMessage;
                item.ProcessException -= ProcessException;
                item.Dispose();
            }
        }

        #region [ Explicit ICollection<IAdapter> Implementation ]

        void ICollection<IAdapter>.Add(IAdapter item)
        {
            Add((T)item);
        }

        bool ICollection<IAdapter>.Contains(IAdapter item)
        {
            return Contains((T)item);
        }

        void ICollection<IAdapter>.CopyTo(IAdapter[] array, int arrayIndex)
        {
            CopyTo(array.Cast<T>().ToArray(), arrayIndex);
        }

        bool ICollection<IAdapter>.Remove(IAdapter item)
        {
            return Remove((T)item);
        }

        IEnumerator<IAdapter> IEnumerable<IAdapter>.GetEnumerator()
        {
            foreach (IAdapter item in this)
            {
                yield return item;
            }
        }

        #endregion

        #endregion
    }
}