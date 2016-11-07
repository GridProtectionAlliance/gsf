//******************************************************************************************************
//  AdapterLoader.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  07/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/06/2009 - Pinal C. Patel
//       Modified Dispose(boolean) to iterate through the adapter collection correctly.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Modified ProcessAdapter() to instantiate types with a default public constructor only.
//  12/10/2009 - Pinal C. Patel
//       Added new AdapterCreated event.
//       Implemented IProvideStatus interface.
//       Enhanced the implementation of Enabled property.
//  09/23/2010 - Pinal C. Patel
//       Added adapter isolation capability to allow for loading qualified adapters in separate 
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
//       Modified deserializer class to use TypeName property instead of Type property to get the type 
//       of the object being deserialized when deserializing from an XML file.
//  04/14/2011 - Pinal C. Patel
//       Updated to use new serialization methods in GSF.Serialization class.
//  05/11/2011 - Pinal C. Patel
//       Implemented IPersistSettings interface.
//       Changed the unit for AllowableProcessMemoryUsage and AllowableAdapterMemoryUsage properties 
//       from bytes to megabytes.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/06/2011 - Pinal C. Patel
//       Updated to instantiate a FileSystemWatcher object that watches for adapters only if needed 
//       to avoid a issue introduced in .NET 4.0 that causes memory leak.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/29/2016 - Pinal C. Patel
//       Fixed a bug in ProcessAdapter(string) method that was always replacing the first adapter
//       in the adapter list instead of the adapter that was modified.
//       Updated Deserializer.Deserialize() method to get an exclusive lock on the adapter file prior to
//       processing it.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
#if !MONO
using GSF.Units;
using System.Threading;
#endif

namespace GSF.Adapters
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
    /// This example show how to use the <see cref="AdapterLoader{T}"/> to isolate adapters in separate <see cref="AppDomain"/>s and monitor their resource usage:
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Text;
    /// using System.Threading;
    /// using GSF;
    /// using GSF.Adapters;
    /// using GSF.Security.Cryptography;
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
    ///     static void OnAdapterResourceUsageExceeded(object sender, GSF.EventArgs&lt;PublishAdapterBase&gt; e)
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
    public class AdapterLoader<T> : ISupportLifecycle, IProvideStatus, IPersistSettings where T : class, IAdapter
    {
        #region [ Members ]

        // Nested Types

        private class Deserializer : MarshalByRefObject
        {
            public T Deserialize(string adapterFile, AdapterFileFormat adapterFormat)
            {
                // Attempt binary deserialization.
                if (adapterFormat == AdapterFileFormat.SerializedBin)
                {
                    FilePath.WaitForReadLockExclusive(adapterFile);
                    return Serialization.Deserialize<T>(File.ReadAllBytes(adapterFile), SerializationFormat.Binary);
                }

                if (adapterFormat != AdapterFileFormat.SerializedXml)
                    throw new NotSupportedException();

                // Attempt XML deserialization.
                FilePath.WaitForReadLockExclusive(adapterFile);
                XDocument xml = XDocument.Parse(File.ReadAllText(adapterFile));

                if ((object)xml.Root != null)
                {
                    XElement type = xml.Root.Element("TypeName");

                    if ((object)type != null)
                    {
                        // Type element required for looking up the adapter's type.
                        XmlSerializer serializer = new XmlSerializer(Type.GetType(type.Value));

                        using (StringReader reader = new StringReader(xml.ToString()))
                        {
                            return (T)serializer.Deserialize(reader);
                        }
                    }
                }

                throw new InvalidOperationException("TypeName element is missing in the XML");
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
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that exceeded the allowable system resource utilization.
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

        /// <summary>
        /// Occurs when the class has been disposed.
        /// </summary>
        public event EventHandler Disposed;

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
        private readonly ObservableCollection<T> m_adapters;
        private SafeFileWatcher m_adapterWatcher;
        private readonly AsyncQueue<object> m_operationQueue;
        private readonly Dictionary<Type, bool> m_enabledStates;
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
            m_settingsCategory = GetType().Name;
            m_adapters = new ObservableCollection<T>();
            m_adapters.CollectionChanged += Adapters_CollectionChanged;
            m_operationQueue = new AsyncQueue<object>();
            m_enabledStates = new Dictionary<Type, bool>();

            m_operationQueue.ProcessItemFunction = ExecuteOperation;
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
                    throw new ArgumentNullException(nameof(value));

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
                    throw new ArgumentNullException(nameof(value));

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
        /// Gets or sets a boolean value that indicates whether <see cref="Adapters"/> are loaded in separate <see cref="AppDomain"/> for isolated execution.
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
                    throw new ArgumentOutOfRangeException(nameof(value));

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
                    throw new ArgumentOutOfRangeException(nameof(value));

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
                    throw new ArgumentOutOfRangeException(nameof(value));

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
                    throw new ArgumentOutOfRangeException(nameof(value));

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
                    throw new ArgumentNullException(nameof(value));

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
                        if ((object)m_adapterWatcher != null)
                            m_adapterWatcher.EnableRaisingEvents = m_enabledStates[m_adapterWatcher.GetType()];

                        if (m_operationQueue != null)
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

                        if ((object)m_adapterWatcher != null)
                            m_adapterWatcher.EnableRaisingEvents = false;

                        if (m_operationQueue != null)
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
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return GetType().Name;
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
                status.Append((object)m_adapterWatcher != null && m_adapterWatcher.EnableRaisingEvents ? "Enabled" : "Disabled");
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
        protected SafeFileWatcher AdapterWatcher
        {
            get
            {
                return m_adapterWatcher;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueue{T}"/> object to be used for queuing operations to be executed on <see cref="Adapters"/>.
        /// </summary>
        protected AsyncQueue<object> OperationQueue
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
                    m_adapterWatcher = new SafeFileWatcher();
                    m_adapterWatcher.Path = m_adapterDirectory;
                    m_adapterWatcher.EnableRaisingEvents = true;
                    m_adapterWatcher.Created += AdapterWatcher_Events;
                    m_adapterWatcher.Changed += AdapterWatcher_Events;
                    m_adapterWatcher.Deleted += AdapterWatcher_Events;
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
                settings.Add("IsolateAdapters", m_isolateAdapters, "True to isolate adapters in separate application domains, otherwise False.");
                settings.Add("MonitorAdapters", m_monitorAdapters, "True to monitor adapter resource utilization when isolated in separate application domains, otherwise False.");
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
                        m_adapters[adapterIndex] = adapter;    // Update adapter.
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
#if !MONO
            // Enable individual application domain resource tracking if it is enabled.
            if (!AppDomain.MonitoringIsEnabled)
                AppDomain.MonitoringIsEnabled = true;

            Process currentProcess;
            List<T> offendingAdapters = new List<T>();
            while (!m_disposed)
            {
                Thread.Sleep(5000);

                // Don't interfere if process memory and processor utilization is in check.
                currentProcess = Process.GetCurrentProcess();
                if (GetMemoryUsage(currentProcess) / SI2.Mega <= m_allowableProcessMemoryUsage &&
                    GetProcessorUsage(currentProcess) <= m_allowableProcessProcessorUsage)
                    continue;

                if (Monitor.TryEnter(m_adapters))
                {
                    try
                    {
                        // JRC - This is not a great idea...
                        //// Force a full blocking GC so the memory usage of adapters is updated.
                        //GC.Collect();

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
        /// <param name="process">The <see cref="Process"/> whose processor usage is to be determined.</param>
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

                        if ((object)m_adapterWatcher != null)
                        {
                            m_adapterWatcher.EnableRaisingEvents = false;
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

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
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

            return null;
        }

        private static bool SetAdapterFilePath(T adapter, string adapterFile)
        {
            if ((object)adapter != null)
            {
                adapter.HostFile = adapterFile;
                return true;
            }

            return false;
        }

        private void SaveCurrentState()
        {
            if ((object)m_adapterWatcher != null)
                m_enabledStates[m_adapterWatcher.GetType()] = m_adapterWatcher.EnableRaisingEvents;

            if (m_operationQueue != null)
                m_enabledStates[m_operationQueue.GetType()] = m_operationQueue.Enabled;

            lock (m_adapters)
            {
                foreach (T adapter in m_adapters)
                {
                    m_enabledStates[adapter.GetType()] = adapter.Enabled;
                }
            }
        }

        private void ExecuteOperation(object data)
        {
            lock (m_adapters)
            {
                foreach (T adapter in m_adapters)
                {
                    try
                    {
                        ExecuteAdapterOperation(adapter, data);
                    }
                    catch (Exception ex)
                    {
                        OnOperationExecutionException(adapter, ex);
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
