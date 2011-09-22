//*******************************************************************************************************
//  AdapterLoader.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/06/2009 - Pinal C. Patel
//       Modified Dispose(boolean) to iterate through the adapter collection correctly.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Modified ProcessAdapter() to instantiate types with a default public contructor only.
//  12/10/2009 - Pinal C. Patel
//       Added new AdapterCreated event.
//       Implemented IProvideStatus interface.
//       Enhanced the implementation of Enabled property.
//  09/23/2010 - Pinal C. Patel
//       Added adapter isolation capability to allow for loading qualified adapters in seperate 
//       application domain for isolated execution.
//  10/01/2010 - Pinal C. Patel
//       Added adapter monitoring capability to monitor the resource utilization of adapters.
//  03/08/2011 - Pinal C. Patel
//       Made IAdapter a requirement for using AdapterLoader for effectively managing serialized 
//       adapter instances.
//       Added AdapterFileExtension and AdapterFileFormat properties to allow loading of binary and XML 
//       serialized adapter instances.
//       Updated AdapterLoadException event to not provide the adapter's type since it might not be 
//       available when processing serialized adapter instances.
//  04/05/2011 - Pinal C. Patel
//       Modified Deserializer class to use TypeName property instead of Type property to get the type 
//       of the object being deserialized when deserializing from an XML file.
//  04/14/2011 - Pinal C. Patel
//       Updated to use new serialization methods in TVA.Serialization class.
//  05/11/2011 - Pinal C. Patel
//       Implemented IPersistSettings interface.
//       Changed the unit for AllowableProcessMemoryUsage and AllowableAdapterMemoryUsage properties 
//       from bytes to megabytes.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using TVA.Collections;
using TVA.Configuration;
using TVA.IO;
using TVA.Units;

namespace TVA.Adapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the file format of the adapters to be loaded by the <see cref="AdapterLoader{T}"/>.
    /// </summary>
    public enum AdapterFileFormat
    {
        /// <summary>
        /// Adapters are <see cref="Type"/>s inside <see cref="Assembly"/> files (DLLs).
        /// </summary>
        Assembly,
        /// <summary>
        /// Adapters are binary serialized instances persisted to files.
        /// </summary>
        SerializedBin,
        /// <summary>
        /// Adapters are XML serialized instances persisted to files.
        /// </summary>
        SerializedXml
    }

    #endregion

    /// <summary>
    /// Represents a generic loader of adapters.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of adapters to be loaded.</typeparam>
    /// <example>
    /// This example show how to use the <see cref="AdapterLoader{T}"/> to isolate adapters in seperate <see cref="AppDomain"/>s and monitor their resource usage:
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Text;
    /// using System.Threading;
    /// using TVA;
    /// using TVA.Adapters;
    /// using TVA.Security.Cryptography;
    /// 
    /// class Program
    /// {
    ///     static AdapterLoader&lt;PublishAdapterBase&gt; s_adapterLoader;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Enable app domain resource monitoring.
    ///         AppDomain.MonitoringIsEnabled = true;
    /// 
    ///         // Load adapters that mimic data publishing.
    ///         s_adapterLoader = new AdapterLoader&lt;PublishAdapterBase&gt;();
    ///         s_adapterLoader.IsolateAdapters = true;
    ///         s_adapterLoader.MonitorAdapters = true;
    ///         s_adapterLoader.AdapterFileExtension = "*.exe";
    ///         s_adapterLoader.AllowableProcessMemoryUsage = 200;
    ///         s_adapterLoader.AllowableProcessProcessorUsage = 50;
    ///         s_adapterLoader.AllowableAdapterMemoryUsage = 100;
    ///         s_adapterLoader.AllowableAdapterProcessorUsage = 25;
    ///         s_adapterLoader.AdapterLoaded += OnAdapterLoaded;
    ///         s_adapterLoader.AdapterUnloaded += OnAdapterUnloaded;
    ///         s_adapterLoader.AdapterResourceUsageExceeded += OnAdapterResourceUsageExceeded;
    ///         s_adapterLoader.Initialize();
    /// 
    ///         // Shutdown.
    ///         Console.ReadLine();
    ///         s_adapterLoader.Dispose();
    ///     }
    /// 
    ///     static void OnAdapterLoaded(object sender, EventArgs&lt;PublishAdapterBase&gt; e)
    ///     {
    ///         Console.WriteLine("{0} has been loaded\r\n", e.Argument.GetType().Name);
    ///     }
    /// 
    ///     static void OnAdapterUnloaded(object sender, EventArgs&lt;PublishAdapterBase&gt; e)
    ///     {
    ///         Console.WriteLine("{0} has been unloaded\r\n", e.Argument.GetType().Name);
    ///     }
    /// 
    ///     static void OnAdapterResourceUsageExceeded(object sender, TVA.EventArgs&lt;PublishAdapterBase&gt; e)
    ///     {
    ///         Console.WriteLine("{0} status:", e.Argument.Name);
    ///         Console.WriteLine(e.Argument.Status);
    /// 
    ///         // Remove the adapter in order to reclaim the resources used by it.
    ///         lock (s_adapterLoader.Adapters)
    ///         {
    ///             s_adapterLoader.Adapters.Remove(e.Argument);
    ///         }
    ///     }
    /// }
    /// 
    /// /// &lt;summary&gt;
    /// /// Base adapter class.
    /// /// &lt;/summary&gt;
    /// public abstract class PublishAdapterBase : Adapter
    /// {
    ///     public PublishAdapterBase()
    ///     {
    ///         Data = new List&lt;byte[]&gt;();
    ///     }
    /// 
    ///     public List&lt;byte[]&gt; Data { get; set; }
    /// 
    ///     public override void Initialize()
    ///     {
    ///         base.Initialize();
    ///         new Thread(Publish).Start();
    ///     }
    /// 
    ///     protected abstract void Publish();
    /// }
    /// 
    /// /// &lt;summary&gt;
    /// /// Adapter that does not manage memory well.
    /// /// &lt;/summary&gt;
    /// public class PublishAdapterA : PublishAdapterBase
    /// {
    ///     protected override void Publish()
    ///     {
    ///         while (true)
    ///         {
    ///             for (int i = 0; i &lt; 10000; i++)
    ///             {
    ///                 Data.Add(new byte[10]);
    ///             }
    ///             Thread.Sleep(100);
    ///         }
    ///     }
    /// }
    /// 
    /// /// &lt;summary&gt;
    /// /// Adapter that uses the processor in excess.
    /// /// &lt;/summary&gt;
    /// public class PublishAdapterB : PublishAdapterBase
    /// {
    ///     protected override void Publish()
    ///     {
    ///         string text = string.Empty;
    ///         System.Random random = new System.Random();
    ///         while (true)
    ///         {
    ///             for (int i = 0; i &lt; 10; i++)
    ///             {
    ///                 for (int j = 0; j &lt; 4; j++)
    ///                 {
    ///                     text += (char)random.Next(256);
    ///                 }
    ///                 Data.Add(Encoding.ASCII.GetBytes(text.Encrypt("C1pH3r", CipherStrength.Aes256)).BlockCopy(0, 1));
    ///             }
    ///             Thread.Sleep(10);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Adapter"/>
    /// <seealso cref="IAdapter"/>
    public class AdapterLoader<T> : ISupportLifecycle, IProvideStatus, IPersistSettings where T : IAdapter
    {
        #region [ Members ]

        // Nested Types

        private class Deserializer : MarshalByRefObject
        {
            public T Deserialize(string adapterFile, AdapterFileFormat adapterFormat)
            {
                if (adapterFormat == AdapterFileFormat.SerializedBin)
                {
                    // Attempt binary desrialization.
                    return Serialization.Deserialize<T>(File.ReadAllBytes(adapterFile), SerializationFormat.Binary);
                }
                else if (adapterFormat == AdapterFileFormat.SerializedXml)
                {
                    // Attempt XML deserialization.
                    XDocument xml = XDocument.Parse(File.ReadAllText(adapterFile));
                    XElement type = xml.Root.Element("TypeName");
                    if ((object)type != null)
                    {
                        // Type element required for looking up the adapter's type.
                        XmlSerializer serializer = new XmlSerializer(Type.GetType(type.Value));
                        return (T)serializer.Deserialize(new StringReader(xml.ToString()));
                    }
                    else
                    {
                        throw new InvalidOperationException("TypeName element is missing in the XML");
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AdapterDirectory"/> property.
        /// </summary>
        public const string DefaultAdapterDirectory = "";

        /// <summary>
        /// Specifies the default value for the <see cref="AdapterFileExtension"/> property.
        /// </summary>
        public const string DefaultAdapterFileExtension = "*.dll";

        /// <summary>
        /// Specifies the default value for the <see cref="AdapterFileFormat"/> property.
        /// </summary>
        public const AdapterFileFormat DefaultAdapterFileFormat = AdapterFileFormat.Assembly;

        /// <summary>
        /// Specifies the default value for the <see cref="WatchForAdapters"/> property.
        /// </summary>
        public const bool DefaultWatchForAdapters = true;

        /// <summary>
        /// Specifies the default value for the <see cref="IsolateAdapters"/> property.
        /// </summary>
        public const bool DefaultIsolateAdapters = false;

        /// <summary>
        /// Specifies the default value for the <see cref="MonitorAdapters"/> property.
        /// </summary>
        public const bool DefaultMonitorAdapters = false;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowableProcessMemoryUsage"/> property
        /// </summary>
        public const double DefaultAllowableProcessMemoryUsage = 500;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowableProcessProcessorUsage"/> property.
        /// </summary>
        public const double DefaultAllowableProcessProcessorUsage = 75;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowableAdapterMemoryUsage"/> property.
        /// </summary>
        public const double DefaultAllowableAdapterMemoryUsage = 100;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowableAdapterProcessorUsage"/> property.
        /// </summary>
        public const double DefaultAllowableAdapterProcessorUsage = 50;

        // Events

        /// <summary>
        /// Occurs when a new adapter is found and instantiated.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that was created.
        /// </remarks>
        public event EventHandler<EventArgs<T>> AdapterCreated;

        /// <summary>
        /// Occurs when a new adapter is loaded to the <see cref="Adapters"/> list.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that was loaded.
        /// </remarks>
        public event EventHandler<EventArgs<T>> AdapterLoaded;

        /// <summary>
        /// Occurs when an existing adapter is unloaded from the <see cref="Adapters"/> list.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that was unloaded.
        /// </remarks>
        public event EventHandler<EventArgs<T>> AdapterUnloaded;

        /// <summary>
        /// Occurs when an adapter has exceeded either the <see cref="AllowableAdapterMemoryUsage"/> or <see cref="AllowableAdapterProcessorUsage"/>.
        /// </summary>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that exceeded the allowable system resource utlization.
        public event EventHandler<EventArgs<T>> AdapterResourceUsageExceeded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when loading an adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when loading the adapter.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> AdapterLoadException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while executing a queued operation on one the <see cref="Adapters"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the adapter on which the operation was being executed.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when executing an operation on the adapter.
        /// </remarks>
        public event EventHandler<EventArgs<T, Exception>> OperationExecutionException;

        // Fields
        private string m_adapterDirectory;
        private string m_adapterFileExtension;
        private AdapterFileFormat m_adapterFileFormat;
        private bool m_watchForAdapters;
        private bool m_isolateAdapters;
        private bool m_monitorAdapters;
        private double m_allowableProcessMemoryUsage;
        private double m_allowableProcessProcessorUsage;
        private double m_allowableAdapterMemoryUsage;
        private double m_allowableAdapterProcessorUsage;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private ObservableCollection<T> m_adapters;
        private FileSystemWatcher m_adapterWatcher;
        private ProcessQueue<object> m_operationQueue;
        private Dictionary<Type, bool> m_enabledStates;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;
#if !MONO
        private Thread m_adapterMonitoringThread;
#endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterLoader{T}"/> class.
        /// </summary>
        public AdapterLoader()
        {
            m_adapterDirectory = DefaultAdapterDirectory;
            m_adapterFileExtension = DefaultAdapterFileExtension;
            m_adapterFileFormat = DefaultAdapterFileFormat;
            m_watchForAdapters = DefaultWatchForAdapters;
            m_isolateAdapters = DefaultIsolateAdapters;
            m_monitorAdapters = DefaultMonitorAdapters;
            m_allowableProcessMemoryUsage = DefaultAllowableProcessMemoryUsage;
            m_allowableProcessProcessorUsage = DefaultAllowableProcessProcessorUsage;
            m_allowableAdapterMemoryUsage = DefaultAllowableAdapterMemoryUsage;
            m_allowableAdapterProcessorUsage = DefaultAllowableAdapterProcessorUsage;
            m_settingsCategory = this.GetType().Name;
            m_adapters = new ObservableCollection<T>();
            m_adapters.CollectionChanged += Adapters_CollectionChanged;
            m_adapterWatcher = new FileSystemWatcher();
            m_adapterWatcher.Created += AdapterWatcher_Events;
            m_adapterWatcher.Changed += AdapterWatcher_Events;
            m_adapterWatcher.Deleted += AdapterWatcher_Events;
            m_operationQueue = ProcessQueue<object>.CreateRealTimeQueue(ExecuteOperation);
            m_enabledStates = new Dictionary<Type, bool>();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdapterLoader{T}"/> is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdapterLoader()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the directory where <see cref="Adapters"/> are located.
        /// </summary>
        /// <remarks>
        /// When an empty string is assigned to <see cref="AdapterDirectory"/>, <see cref="Adapters"/> are loaded from the directory where application is executing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string AdapterDirectory
        {
            get
            {
                return m_adapterDirectory;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException("value");

                m_adapterDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets the file extension of the <see cref="Adapters"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is a null or empty string.</exception>
        public string AdapterFileExtension
        {
            get
            {
                return m_adapterFileExtension;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_adapterFileExtension = value;
            }
        }

        /// <summary>
        /// Gets or sets the file format of the <see cref="Adapters"/>.
        /// </summary>
        public AdapterFileFormat AdapterFileFormat
        {
            get
            {
                return m_adapterFileFormat;
            }
            set
            {
                m_adapterFileFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="AdapterDirectory"/> is to be monitored for new <see cref="Adapters"/>.
        /// </summary>
        public bool WatchForAdapters
        {
            get
            {
                return m_watchForAdapters;
            }
            set
            {
                m_watchForAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Adapters"/> are loaded in seperate <see cref="AppDomain"/> for isolated execution.
        /// </summary>
        public bool IsolateAdapters
        {
            get
            {
                return m_isolateAdapters;
            }
            set
            {
                m_isolateAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether resource utilization of <see cref="Adapters"/> executing in <see cref="IsolateAdapters">isolation</see> is to be monitored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use <see cref="AllowableProcessMemoryUsage"/>, <see cref="AllowableProcessProcessorUsage"/>, <see cref="AllowableAdapterMemoryUsage"/> and 
        /// <see cref="AllowableAdapterProcessorUsage"/> properties to configure how adapter resource utilization is to be monitored.
        /// </para>
        /// <para>
        /// This option is ignored under Mono deployments.
        /// </para>
        /// </remarks>
        public bool MonitorAdapters
        {
            get
            {
                return m_monitorAdapters;
            }
            set
            {
                m_monitorAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets the memory in megabytes the current process is allowed to use before the internal monitoring process starts looking for offending <see cref="Adapters"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
        public double AllowableProcessMemoryUsage
        {
            get
            {
                return m_allowableProcessMemoryUsage;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                m_allowableProcessMemoryUsage = value;
            }
        }

        /// <summary>
        /// Gets or sets the processor time in % the current process is allowed to use before the internal monitoring process starts looking for offending <see cref="Adapters"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
        public double AllowableProcessProcessorUsage
        {
            get
            {
                return m_allowableProcessProcessorUsage;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                m_allowableProcessProcessorUsage = value;
            }
        }

        /// <summary>
        /// Gets or sets the memory in megabytes the <see cref="Adapters"/> are allowed to use before being flagged as offending by the internal monitoring process.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
        public double AllowableAdapterMemoryUsage
        {
            get
            {
                return m_allowableAdapterMemoryUsage;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                m_allowableAdapterMemoryUsage = value;
            }
        }

        /// <summary>
        /// Gets or sets the processor time in % the <see cref="Adapters"/> are allowed to use before being flagged as offending by the internal monitoring process.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
        public double AllowableAdapterProcessorUsage
        {
            get
            {
                return m_allowableAdapterProcessorUsage;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                m_allowableAdapterProcessorUsage = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="AdapterLoader{T}"/> settings are to be saved to the config file.
        /// </summary>
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
        /// Gets or sets the category under which <see cref="AdapterLoader{T}"/> settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string SettingsCategory
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
        /// Gets or sets a boolean value that indicates whether the <see cref="AdapterLoader{T}"/> is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (!m_initialized && value)
                {
                    // Initialize if uninitialized when enabled.
                    Initialize();
                }
                else
                {
                    // Change current state of various components.
                    if (value && !m_enabled)
                    {
                        // Enable - restore previously saved state.
                        m_adapterWatcher.EnableRaisingEvents = m_enabledStates[m_adapterWatcher.GetType()];
                        m_operationQueue.Enabled = m_enabledStates[m_operationQueue.GetType()];

                        bool savedState;
                        lock (m_adapters)
                        {
                            foreach (T adapter in m_adapters)
                            {
                                if (m_enabledStates.TryGetValue(adapter.GetType(), out savedState))
                                    adapter.Enabled = savedState;
                            }
                        }
                    }
                    else if (!value && m_enabled)
                    {
                        // Disable - save current state and disable.
                        SaveCurrentState();
                        m_adapterWatcher.EnableRaisingEvents = false;
                        m_operationQueue.Enabled = false;

                        lock (m_adapters)
                        {
                            foreach (T adapter in m_adapters)
                            {
                                adapter.Enabled = false;
                            }
                        }
                    }
                }
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("             Adapters type: ");
                status.Append(typeof(T).Name);
                status.AppendLine();
                status.Append("        Adapters directory: ");
                status.Append(FilePath.TrimFileName(m_adapterDirectory, 30));
                status.AppendLine();
                status.Append("         Adapter isolation: ");
                status.Append(m_isolateAdapters ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("        Adapter monitoring: ");
                status.Append(m_monitorAdapters ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("           Dynamic loading: ");
                status.Append(m_adapterWatcher.EnableRaisingEvents ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("          Operations queue: ");
                status.Append(m_operationQueue.Enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                lock (m_adapters)
                {
                    status.Append("     Total loaded adapters: ");
                    status.Append(m_adapters.Count);
                    status.AppendLine();
                    foreach (T adapter in m_adapters)
                    {
                        status.AppendLine();
                        status.Append("              Adapter name: ");
                        status.Append(adapter.Name);
                        status.AppendLine();
                        status.Append(adapter.Status);
                    }
                }

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a list of adapters loaded from the <see cref="AdapterDirectory"/>.
        /// </summary>
        public IList<T> Adapters
        {
            get
            {
                return m_adapters;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileSystemWatcher"/> object watching for new adapter assemblies added at runtime.
        /// </summary>
        protected FileSystemWatcher AdapterWatcher
        {
            get
            {
                return m_adapterWatcher;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueue{T}"/> object to be used for queuing operations to be executed on <see cref="Adapters"/>.
        /// </summary>
        protected ProcessQueue<object> OperationQueue
        {
            get
            {
                return m_operationQueue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public virtual void Initialize()
        {
            Initialize(null);
        }

        /// <summary>
        /// Initializes the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        /// <param name="adapterTypes">Collection of adapter <see cref="Type"/>s from which <see cref="Adapters"/> are to be created.</param>
        public virtual void Initialize(IEnumerable<Type> adapterTypes)
        {
            if (!m_initialized)
            {
                // Load settings from the config file.
                LoadSettings();

                // Process adapters.
                m_adapterDirectory = FilePath.GetAbsolutePath(m_adapterDirectory);
                if (m_adapterFileFormat == AdapterFileFormat.Assembly)
                {
                    if ((object)adapterTypes == null)
                        adapterTypes = typeof(T).LoadImplementations(Path.Combine(m_adapterDirectory, m_adapterFileExtension));

                    foreach (Type type in adapterTypes)
                    {
                        ProcessAdapter(type);
                    }
                }
                else
                {
                    foreach (string adapterFile in Directory.GetFiles(m_adapterDirectory, m_adapterFileExtension))
                    {
                        ProcessAdapter(adapterFile);
                    }
                }

                // Watch for adapters.
                if (m_watchForAdapters)
                {
                    m_adapterWatcher.Path = m_adapterDirectory;
                    m_adapterWatcher.EnableRaisingEvents = true;
                }

                // Save current state.
                SaveCurrentState();

                // Start adapter monitoring.
#if !MONO
                if (m_monitorAdapters && m_isolateAdapters)
                {
                    // Following must be true in order for us to monitor adapter resources:
                    // - Adapter monitoring is enabled.
                    // - Adapter isolation is enabled.
                    m_adapterMonitoringThread = new Thread(MonitorAdapterResources);
                    m_adapterMonitoringThread.Start();
                }
#endif

                m_enabled = true;       // Mark as enabled.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves <see cref="AdapterLoader{T}"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["AdapterDirectory", true].Update(m_adapterDirectory);
                settings["AdapterFileExtension", true].Update(m_adapterFileExtension);
                settings["AdapterFileFormat", true].Update(m_adapterFileFormat);
                settings["WatchForAdapters", true].Update(m_watchForAdapters);
                settings["IsolateAdapters", true].Update(m_isolateAdapters);
                settings["MonitorAdapters", true].Update(m_monitorAdapters);
                settings["AllowableProcessMemoryUsage", true].Update(m_allowableProcessMemoryUsage);
                settings["AllowableProcessProcessorUsage", true].Update(m_allowableProcessProcessorUsage);
                settings["AllowableAdapterMemoryUsage", true].Update(m_allowableAdapterMemoryUsage);
                settings["AllowableAdapterProcessorUsage", true].Update(m_allowableAdapterProcessorUsage);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="AdapterLoader{T}"/> settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("AdapterDirectory", m_adapterDirectory, "Directory where adapters are located.");
                settings.Add("AdapterFileExtension", m_adapterFileExtension, "Extension of the adapter files.");
                settings.Add("AdapterFileFormat", m_adapterFileFormat, "Format (Assembly; SerializedBin; SerializedXml) of the adapter files.");
                settings.Add("WatchForAdapters", m_watchForAdapters, "True to monitor adapter directory for new adapters, otherwise False.");
                settings.Add("IsolateAdapters", m_isolateAdapters, "True to isolate adapters in seperate application domains, otherwise False.");
                settings.Add("MonitorAdapters", m_monitorAdapters, "True to monitor adapter resource utilization when isolated in seperate application domains, otherwise False.");
                settings.Add("AllowableProcessMemoryUsage", m_allowableProcessMemoryUsage, "Memory in megabytes the current process is allowed to use before the internal monitoring process starts looking for offending adapters.");
                settings.Add("AllowableProcessProcessorUsage", m_allowableProcessProcessorUsage, "Processor time in % the current process is allowed to use before the internal monitoring process starts looking for offending adapters.");
                settings.Add("AllowableAdapterMemoryUsage", m_allowableAdapterMemoryUsage, "Memory in megabytes the adapters are allowed to use before being flagged as offending by the internal monitoring process.");
                settings.Add("AllowableAdapterProcessorUsage", m_allowableAdapterProcessorUsage, "Processor time in % the adapters are allowed to use before being flagged as offending by the internal monitoring process.");
                AdapterDirectory = settings["AdapterDirectory"].ValueAs(m_adapterDirectory);
                AdapterFileExtension = settings["AdapterFileExtension"].ValueAs(m_adapterFileExtension);
                AdapterFileFormat = settings["AdapterFileFormat"].ValueAs(m_adapterFileFormat);
                WatchForAdapters = settings["WatchForAdapters"].ValueAs(m_watchForAdapters);
                IsolateAdapters = settings["IsolateAdapters"].ValueAs(m_isolateAdapters);
                MonitorAdapters = settings["MonitorAdapters"].ValueAs(m_monitorAdapters);
                AllowableProcessMemoryUsage = settings["AllowableProcessMemoryUsage"].ValueAs(m_allowableProcessMemoryUsage);
                AllowableProcessProcessorUsage = settings["AllowableProcessProcessorUsage"].ValueAs(m_allowableProcessProcessorUsage);
                AllowableAdapterMemoryUsage = settings["AllowableAdapterMemoryUsage"].ValueAs(m_allowableAdapterMemoryUsage);
                AllowableAdapterProcessorUsage = settings["AllowableAdapterProcessorUsage"].ValueAs(m_allowableAdapterProcessorUsage);
            }
        }

        /// <summary>
        /// Processes the <paramref name="adapterFile"/> by deserializing it.
        /// </summary>
        /// <param name="adapterFile">Path to the adapter file to be deserialized.</param>
        protected virtual void ProcessAdapter(string adapterFile)
        {
            T adapter = default(T);
            try
            {
                Deserializer deserializer;
                if (m_isolateAdapters)
                {
                    // Adapter isolation is enabled.
                    AppDomain domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                    deserializer = (Deserializer)domain.CreateInstanceAndUnwrap(typeof(Deserializer).Assembly.FullName, typeof(Deserializer).FullName);

                }
                else
                {
                    // Adapter isolation is not enabled.
                    deserializer = new Deserializer();
                }

                // Deserialize adapter instance.
                adapter = deserializer.Deserialize(adapterFile, m_adapterFileFormat);
                SetAdapterFilePath(adapter, adapterFile);

                OnAdapterCreated(adapter);

                // Add adapter and notify via event.
                lock (m_adapters)
                {
                    int adapterIndex = m_adapters.IndexOf(currentAdapter => currentAdapter.HostFile == adapterFile);
                    if (adapterIndex < 0)
                        m_adapters.Add(adapter);    // Add adapter.
                    else
                        m_adapters[0] = adapter;    // Update adapter.
                }
            }
            catch (Exception ex)
            {
                OnAdapterLoadException(adapter, ex);
            }
        }

        /// <summary>
        /// Processes the <paramref name="adapterType"/> by instantiating it.
        /// </summary>
        /// <param name="adapterType"><see cref="Type"/> of the adapter to be instantiated.</param>
        protected virtual void ProcessAdapter(Type adapterType)
        {
            T adapter = default(T);
            try
            {
                if ((object)adapterType.GetConstructor(Type.EmptyTypes) != null)
                {
                    // Instantiate adapter instance.
                    if (m_isolateAdapters && (adapterType.IsMarshalByRef || adapterType.IsSerializable))
                    {
                        // Adapter isolation is enabled and possible.
                        AppDomain domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                        adapter = (T)domain.CreateInstanceAndUnwrap(adapterType.Assembly.FullName, adapterType.FullName);
                    }
                    else
                    {
                        // Load adapter in the current executing domain.
                        adapter = (T)(Activator.CreateInstance(adapterType));
                    }
                    OnAdapterCreated(adapter);

                    // Add adapter and notify via event.
                    lock (m_adapters)
                    {
                        m_adapters.Add(adapter);
                    }
                }
            }
            catch (Exception ex)
            {
                OnAdapterLoadException(adapter, ex);
            }
        }

        /// <summary>
        /// Monitors the resource utilization of <see cref="Adapters"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="AppDomain"/> monitoring is not enabled under Mono deployments.
        /// </remarks>
        protected virtual void MonitorAdapterResources()
        {
#if MONO
            return;
#else
            // Enable individual application domain resource tracking if it is enabled.
            if (!AppDomain.MonitoringIsEnabled)
                AppDomain.MonitoringIsEnabled = true;

            Process currentProcess;
            List<T> offendingAdapters = new List<T>();
            while (!m_disposed)
            {
                Thread.Sleep(5000);

                // Don't interfere if process memory and processor utlization is in check.
                currentProcess = Process.GetCurrentProcess();
                if (GetMemoryUsage(currentProcess) / SI2.Mega <= m_allowableProcessMemoryUsage &&
                    GetProcessorUsage(currentProcess) <= m_allowableProcessProcessorUsage)
                    continue;

                if (Monitor.TryEnter(m_adapters))
                {
                    try
                    {
                        // Force a full blocking GC so the memory usage of adapters is updated.
                        GC.Collect();

                        foreach (T adapter in m_adapters)
                        {
                            if (adapter.MemoryUsage / SI2.Mega > m_allowableAdapterMemoryUsage ||
                                adapter.ProcessorUsage > m_allowableAdapterProcessorUsage)
                                offendingAdapters.Add(adapter);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(m_adapters);
                    }

                    // Notify about the offending adapters.
                    foreach (T adapter in offendingAdapters)
                    {
                        OnAdapterResourceUsageExceeded(adapter);
                    }
                    offendingAdapters.Clear();
                }
            }
#endif
        }

        /// <summary>
        /// Gets the memory usage in bytes of the specified <paramref name="process"/>.
        /// </summary>
        /// <param name="process">The <see cref="Process"/> whose memory usage is to be determined.</param>
        /// <returns>Memory usage in bytes of the specified <paramref name="process"/>.</returns>
        protected virtual double GetMemoryUsage(Process process)
        {
            return process.WorkingSet64;
        }

        /// <summary>
        /// Gets the % processor usage of the specified <paramref name="process"/>.
        /// </summary>
        /// <param name="process">The <see cref="Process"/> whose processot usage is to be determined.</param>
        /// <returns>Processor usage in % of the specified <paramref name="process"/>.</returns>
        protected virtual double GetProcessorUsage(Process process)
        {
            return process.TotalProcessorTime.TotalSeconds / Ticks.ToSeconds(DateTime.Now - process.StartTime) / Environment.ProcessorCount * 100;
        }

        /// <summary>
        /// Executes an operation on the <paramref name="adapter"/> with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="adapter">Adapter on which an operation is to be executed.</param>
        /// <param name="data">Data to be used when executing an operation.</param>
        /// <exception cref="NotSupportedException">Always</exception>
        protected virtual void ExecuteAdapterOperation(T adapter, object data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdapterLoader{T}"/> and optionally releases the managed resources.
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

                        if ((object)m_enabledStates != null)
                            m_enabledStates.Clear();

                        if ((object)m_operationQueue != null)
                            m_operationQueue.Dispose();

                        if ((object)m_adapterWatcher != null)
                        {
                            m_adapterWatcher.Created -= AdapterWatcher_Events;
                            m_adapterWatcher.Changed -= AdapterWatcher_Events;
                            m_adapterWatcher.Deleted -= AdapterWatcher_Events;
                            m_adapterWatcher.Dispose();
                        }

                        if ((object)m_adapters != null)
                        {
                            lock (m_adapters)
                            {
                                for (int i = 0; i < m_adapters.Count; i += 0)
                                {
                                    m_adapters.RemoveAt(0);
                                }
                            }
                            m_adapters.CollectionChanged -= Adapters_CollectionChanged;
                        }
                    }
                }
                finally
                {
                    m_enabled = false;  // Mark as disabled.
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="AdapterCreated"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterCreated"/> event.</param>
        protected virtual void OnAdapterCreated(T adapter)
        {
            // Raise the event.
            if ((object)AdapterCreated != null)
                AdapterCreated(this, new EventArgs<T>(adapter));
        }

        /// <summary>
        /// Raises the <see cref="AdapterLoaded"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterLoaded"/> event.</param>
        protected virtual void OnAdapterLoaded(T adapter)
        {
            // Initialize the adapter.
            if ((object)adapter != null)
                adapter.Initialize();

            // Raise the event.
            if ((object)AdapterLoaded != null)
                AdapterLoaded(this, new EventArgs<T>(adapter));
        }

        /// <summary>
        /// Raises the <see cref="AdapterUnloaded"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterUnloaded"/> event.</param>
        protected virtual void OnAdapterUnloaded(T adapter)
        {
            // Dispose the adapter.
            try
            {
                if ((object)adapter != null)
                    adapter.Dispose();
            }
            catch
            {
            }

            // Unload the adapter domain.
            try
            {
                if ((object)adapter != null && !adapter.Domain.IsDefaultAppDomain())
                    AppDomain.Unload(adapter.Domain);
            }
            catch
            {
            }

            // Raise the event.
            if ((object)AdapterUnloaded != null)
                AdapterUnloaded(this, new EventArgs<T>(adapter));
        }

        /// <summary>
        /// Raises the <see cref="AdapterResourceUsageExceeded"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterResourceUsageExceeded"/> event.</param>
        protected virtual void OnAdapterResourceUsageExceeded(T adapter)
        {
            // Raise the event.
            if ((object)AdapterResourceUsageExceeded != null)
                AdapterResourceUsageExceeded(this, new EventArgs<T>(adapter));
        }


        /// <summary>
        /// Raises the <see cref="AdapterLoadException"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance that caused the <paramref name="exception"/>.</param>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="AdapterLoadException"/> event.</param>
        protected virtual void OnAdapterLoadException(T adapter, Exception exception)
        {
            // Remove the adapter if it exists.
            if ((object)adapter != null)
            {
                lock (m_adapters)
                {
                    if (m_adapters.Contains(adapter))
                        m_adapters.Remove(adapter);
                }
            }

            // Raise the event.
            if ((object)AdapterLoadException != null)
                AdapterLoadException(this, new EventArgs<Exception>(exception));
        }

        /// <summary>
        /// Raises the <see cref="OperationExecutionException"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="OperationExecutionException"/> event.</param>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="OperationExecutionException"/> event.</param>
        protected virtual void OnOperationExecutionException(T adapter, Exception exception)
        {
            // Raise the event.
            if ((object)OperationExecutionException != null)
                OperationExecutionException(this, new EventArgs<T, Exception>(adapter, exception));
        }

        private static string GetAdapterFilePath(T adapter)
        {
            if ((object)adapter != null)
                return adapter.HostFile;
            else
                return null;
        }

        private static bool SetAdapterFilePath(T adapter, string adapterFile)
        {
            if ((object)adapter != null)
            {
                adapter.HostFile = adapterFile;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SaveCurrentState()
        {
            m_enabledStates[m_adapterWatcher.GetType()] = m_adapterWatcher.EnableRaisingEvents;
            m_enabledStates[m_operationQueue.GetType()] = m_operationQueue.Enabled;

            lock (m_adapters)
            {
                foreach (T adapter in m_adapters)
                {
                    m_enabledStates[adapter.GetType()] = adapter.Enabled;
                }
            }
        }

        private void ExecuteOperation(object[] data)
        {
            foreach (object operationData in data)
            {
                lock (m_adapters)
                {
                    foreach (T adapter in m_adapters)
                    {
                        try
                        {
                            ExecuteAdapterOperation(adapter, operationData);
                        }
                        catch (Exception ex)
                        {
                            OnOperationExecutionException(adapter, ex);
                        }
                    }
                }
            }
        }

        private void AdapterWatcher_Events(object sender, FileSystemEventArgs e)
        {
            if (FilePath.IsFilePatternMatch(m_adapterFileExtension, FilePath.GetFileName(e.FullPath), true))
            {
                // Updated file has a matching extension.
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    // Remove loaded adapter.
                    lock (m_adapters)
                    {
                        int adapterIndex = m_adapters.IndexOf(currentAdapter => currentAdapter.HostFile == e.FullPath);
                        if (adapterIndex >= 0)
                            m_adapters.RemoveAt(adapterIndex);
                    }
                }
                else
                {
                    // Process new adapter.
                    if (m_adapterFileFormat == AdapterFileFormat.Assembly)
                    {
                        foreach (Type type in typeof(T).LoadImplementations(e.FullPath))
                        {
                            ProcessAdapter(type);
                        }
                    }
                    else
                    {
                        ProcessAdapter(e.FullPath);
                    }
                }
            }
        }

        private void Adapters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Notify additions.
                    foreach (T adapter in e.NewItems)
                    {
                        OnAdapterLoaded(adapter);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Notify deletions.
                    foreach (T adapter in e.OldItems)
                    {
                        OnAdapterUnloaded(adapter);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Notify deletions.
                    foreach (T adapter in e.OldItems)
                    {
                        OnAdapterUnloaded(adapter);
                    }
                    // Notify additions.
                    foreach (T adapter in e.NewItems)
                    {
                        OnAdapterLoaded(adapter);
                    }
                    break;
            }
        }

        #endregion
    }
}
