//******************************************************************************************************
//  Adapter.cs - Gbtc
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
//  09/23/2010 - Pinal C. Patel
//       Generated original version of source code.
//  11/19/2010 - Pinal C. Patel
//       Removed the persistence of Enabled property to the config file.
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
//       DataContractSerializer
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using GSF.IO;

namespace GSF.Adapters
{
    /// <summary>
    /// Represents an adapter that could execute in isolation in a separate <see cref="AppDomain"/>.
    /// </summary>
    /// <seealso cref="IAdapter"/>
    /// <seealso cref="AdapterLoader{T}"/>
    [Serializable, DataContract(Namespace = "")]
    public class Adapter : MarshalByRefObject, IAdapter
    {
        #region [ Members ]

        // Fields
        [NonSerialized]
        private DateTime m_created;
        [NonSerialized]
        private string m_hostFile;
        [NonSerialized]
        private bool m_persistSettings;
        [NonSerialized]
        private string m_settingsCategory;
        [NonSerialized]
        private bool m_enabled;
        [NonSerialized]
        private bool m_disposed;
        [NonSerialized]
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
            m_created = DateTime.UtcNow;
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
        [XmlIgnore, Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [XmlIgnore]
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
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [XmlIgnore]
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="Adapter"/> settings are to be saved to the config file.
        /// </summary>
        [XmlIgnore]
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
        [XmlIgnore]
        public virtual string SettingsCategory
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
        /// Gets the memory utilization of the <see cref="Adapter"/> in bytes if executing in a separate <see cref="AppDomain"/>, otherwise <see cref="Double.NaN"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="MemoryUsage"/> gets updated only after a full blocking collection by <see cref="GC"/> (e.g. <see cref="GC.Collect()"/>).
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
#endif
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the % processor utilization of the <see cref="Adapter"/> if executing in a separate <see cref="AppDomain"/> otherwise <see cref="Double.NaN"/>.
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
                    return Domain.MonitoringTotalProcessorTime.TotalSeconds / Ticks.ToSeconds(DateTime.UtcNow.Ticks - m_created.Ticks) / Environment.ProcessorCount * 100;
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
                status.Append(double.IsNaN(MemoryUsage) ? "Not tracked" : MemoryUsage.ToString("0 bytes"));
                status.AppendLine();
                status.Append("           Processor usage: ");
                status.Append(double.IsNaN(ProcessorUsage) ? "Not tracked" : ProcessorUsage.ToString("0'%'"));
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
