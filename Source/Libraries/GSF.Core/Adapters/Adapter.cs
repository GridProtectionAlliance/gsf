//*******************************************************************************************************
//  Adapter.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/23/2010 - Pinal C. Patel
//       Generated original version of source code.
//  11/19/2010 - Pinal C. Patel
//       Removed the persistance of Enabled property to the config file.
//  11/24/2010 - Pinal C. Patel
//       Modified Name property to use SettingsCategory instead of Type name.
//  12/07/2010 - Pinal C. Patel
//       Updated PersistSettings property to default to false instead of true.
//  03/08/2011 - Pinal C. Patel
//       Added StatusUpdate and Disposed events.
//       Added Type and File properties to support serialized adapter instances.
//       Added attributes to fields and properties to enable serialization of derived type instances.
//  04/05/2011 - Pinal C. Patel
//       Changed properties Type to TypeName and File to HostFile to avoid naming conflict.
//       Modified Name property to use the file name (no extension) from HostFile property if available.
//  04/08/2011 - Pinal C. Patel
//       Added ExecutionException event.
//       Renamed StatusUpdate event to StatusMessage.
//  05/11/2011 - Pinal C. Patel
//       Modified OnStatusUpdate() method to allow for parameterized arguments.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  01/18/2012 - Pinal C. Patel
//       Added DataContract attribute to prevent serialization of events when serializing using 
//       DataContractSerializer.
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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

using GSF.IO;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace GSF.Adapters
{
    /// <summary>
    /// Represents an adapter that could execute in isolation in a seperate <see cref="AppDomain"/>.
    /// </summary>
    /// <seealso cref="IAdapter"/>
    /// <seealso cref="AdapterLoader{T}"/>
    [Serializable(), DataContract(Namespace = "")]
    public class Adapter : MarshalByRefObject, IAdapter
    {
        #region [ Members ]

        // Fields
        [NonSerialized()]
        private DateTime m_created;
        [NonSerialized()]
        private string m_hostFile;
        [NonSerialized()]
        private bool m_persistSettings;
        [NonSerialized()]
        private string m_settingsCategory;
        [NonSerialized()]
        private bool m_enabled;
        [NonSerialized()]
        private bool m_disposed;
        [NonSerialized()]
        private bool m_initialized;

        // Events

        /// <summary>
        /// Occurs when the <see cref="Adapter"/> wants to provide a status update.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="UpdateType"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the update message.
        /// </remarks>
        public event EventHandler<EventArgs<UpdateType, string>> StatusUpdate;

        /// <summary>
        /// Occurs when the <see cref="IAdapter"/> encounters an <see cref="Exception"/> during execution.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the text that describes the activity that was being performed by the <see cref="IAdapter"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the encountered <see cref="Exception"/>.
        /// </remarks>
        public event EventHandler<EventArgs<string, Exception>> ExecutionException;

        /// <summary>
        /// Occurs when <see cref="Adapter"/> is disposed.
        /// </summary>
        public event EventHandler Disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="Adapter"/>.
        /// </summary>
        public Adapter()
        {
            m_created = DateTime.Now;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="Adapter"/> is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~Adapter()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the text representation of the <see cref="Adapter"/>'s <see cref="TypeName"/>.
        /// </summary>
        /// <remarks>
        /// This can be used for looking up the <see cref="TypeName"/> of the <see cref="Adapter"/> when deserializing it using <see cref="XmlSerializer"/>.
        /// </remarks>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string TypeName
        {
            get
            {
                Type type = this.GetType();
                return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
            }
            set
            {
                // Ignore
            }
        }

        /// <summary>
        /// Gets or sets the path to the file where the <see cref="Adapter"/> is housed.
        /// </summary>
        /// <remarks>
        /// This can be used to update the <see cref="Adapter"/> when changes are made to the file where it is housed.
        /// </remarks>
        [XmlIgnore(), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string HostFile
        {
            get
            {
                return m_hostFile;
            }
            set
            {
                m_hostFile = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="Adapter"/> is currently enabled.
        /// </summary>
        [XmlIgnore()]
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Adapter"/> settings are to be saved to the config file.
        /// </summary>
        [XmlIgnore()]
        public virtual bool PersistSettings
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
        /// Gets or sets the category under which <see cref="Adapter"/> settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [XmlIgnore()]
        public virtual string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets the memory utilzation of the <see cref="Adapter"/> in bytes if executing in a seperate <see cref="AppDomain"/>, otherwise <see cref="Double.NaN"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="MemoryUsage"/> gets updated only after a full blocking collection by <see cref="GC"/> (eg. <see cref="GC.Collect()"/>).
        /// </para>
        /// <para>
        /// This method always returns <c><see cref="Double.NaN"/></c> under Mono deployments.
        /// </para>
        /// </remarks>
        public double MemoryUsage
        {
            get
            {
#if !MONO
                if (!Domain.IsDefaultAppDomain() && AppDomain.MonitoringIsEnabled)
                    // Both app domain isolation and app domain resource monitoring is enabled.
                    return Domain.MonitoringSurvivedMemorySize;
                else
#endif
                    return double.NaN;
            }
        }

        /// <summary>
        /// Gets the % processor utilization of the <see cref="Adapter"/> if executing in a seperate <see cref="AppDomain"/> otherwise <see cref="Double.NaN"/>.
        /// </summary>
        /// <remarks>
        /// This method always returns <c><see cref="Double.NaN"/></c> under Mono deployments.
        /// </remarks>
        public double ProcessorUsage
        {
            get
            {
#if !MONO
                if (!Domain.IsDefaultAppDomain() && AppDomain.MonitoringIsEnabled)
                    // Both app domain isolation and app domain resource monitoring is enabled.
                    return Domain.MonitoringTotalProcessorTime.TotalSeconds / Ticks.ToSeconds(DateTime.Now.Ticks - m_created.Ticks) / Environment.ProcessorCount * 100;
                else
#endif
                    return double.NaN;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="Adapter"/>.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(m_hostFile))
                    return m_settingsCategory;
                else
                    return FilePath.GetFileNameWithoutExtension(m_hostFile);
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="Adapter"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("            Adapter domain: ");
                status.Append(Domain.FriendlyName);
                status.AppendLine();
                status.Append("             Adapter state: ");
                status.Append(Enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("              Memory usage: ");
                status.Append(MemoryUsage.Equals(double.NaN) ? "Not tracked" : MemoryUsage.ToString("0 bytes"));
                status.AppendLine();
                status.Append("           Processor usage: ");
                status.Append(ProcessorUsage.Equals(double.NaN) ? "Not tracked" : ProcessorUsage.ToString("0'%'"));
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the <see cref="AppDomain"/> in which the <see cref="Adapter"/> is executing.
        /// </summary>
        public virtual AppDomain Domain
        {
            get
            {
                return AppDomain.CurrentDomain;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="Adapter"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the <see cref="Adapter"/>.
        /// </summary>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves <see cref="Adapter"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set.");
            }
        }

        /// <summary>
        /// Loads saved <see cref="Adapter"/> settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set.");
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Adapter"/> and optionally releases the managed resources.
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
                        SaveSettings();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                    OnDisposed();       // Raise dispose event.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusUpdate"/> event.
        /// </summary>
        /// <param name="updateType"><see cref="UpdateType"/> to send to <see cref="StatusUpdate"/> event.</param>
        /// <param name="updateMessage">Update message to send to <see cref="StatusUpdate"/> event.</param>
        /// <param name="args">Arguments to be used when formatting the <paramref name="updateMessage"/>.</param>
        protected virtual void OnStatusUpdate(UpdateType updateType, string updateMessage, params object[] args)
        {
            if ((object)StatusUpdate != null)
                StatusUpdate(this, new EventArgs<UpdateType, string>(updateType, string.Format(updateMessage, args)));
        }

        /// <summary>
        /// Raises the <see cref="ExecutionException"/> event.
        /// </summary>
        /// <param name="activityDescription">Activity description to send to <see cref="ExecutionException"/> event.</param>
        /// <param name="encounteredException">Encountered <see cref="Exception"/> to send to <see cref="ExecutionException"/> event.</param>
        protected virtual void OnExecutionException(string activityDescription, Exception encounteredException)
        {
            if ((object)ExecutionException != null)
                ExecutionException(this, new EventArgs<string, Exception>(activityDescription, encounteredException));
        }

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        protected virtual void OnDisposed()
        {
            if ((object)Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        #endregion
    }
}
