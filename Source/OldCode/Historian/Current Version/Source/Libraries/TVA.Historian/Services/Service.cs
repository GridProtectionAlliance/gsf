//*******************************************************************************************************
//  Service.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/27/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/02/2009 - Pinal C. Patel
//       Modified configuration of the default WebHttpBinding to enable receiving of large payloads.
//
//*******************************************************************************************************

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using TVA.Configuration;

namespace TVA.Historian.Services
{
    /// <summary>
    /// A base class for web service that can send and receive data over REST (Representational State Transfer) interface.
    /// </summary>
    public class Service : IService
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has been created for the specified <see cref="ServiceUri"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="ServiceHostCreated"/> event is fired, changes like adding new endpoints, can be made to the <see cref="ServiceHost"/>.
        /// </remarks>
        public event EventHandler ServiceHostCreated;

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has can process requests via all of its endpoints.
        /// </summary>
        public event EventHandler ServiceHostStarted;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when processing a request.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception encountered when processing a request.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ServiceProcessError;

        // Fields
        private IArchive m_archive;
        private string m_serviceUri;
        private string m_serviceContract;
        private DataFlowDirection m_serviceDataFlow;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;
        private WebServiceHost m_serviceHost;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the web service.
        /// </summary>
        protected Service()
        {
            m_serviceContract = this.GetType().Namespace + ".I" + this.GetType().Name;
            m_serviceDataFlow = DataFlowDirection.BothWays;
            m_persistSettings = true;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the web service is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~Service()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IArchive"/> used by the web service for its data.
        /// </summary>
        public IArchive Archive
        {
            get
            {
                return m_archive;
            }
            set
            {
                m_archive = value;
            }
        }

        /// <summary>
        /// Gets or sets the URI where the web service is to be hosted.
        /// </summary>
        /// <remarks>
        /// Set <see cref="ServiceUri"/> to a null or empty string to disable web service hosting.
        /// </remarks>
        public string ServiceUri
        {
            get
            {
                return m_serviceUri;
            }
            set
            {
                m_serviceUri = value;
            }
        }

        /// <summary>
        /// Gets or sets the contract interface implemented by the web service.
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Type.FullName"/> of the contract interface implemented by the web service.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string ServiceContract
        {
            get
            {
                return m_serviceContract;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_serviceContract = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFlowDirection"/> of the web service.
        /// </summary>
        public DataFlowDirection ServiceDataFlow
        {
            get
            {
                return m_serviceDataFlow;
            }
            set
            {
                m_serviceDataFlow = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the web service is currently enabled.
        /// </summary>
        public bool Enabled
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
        /// Gets or sets a boolean value that indicates whether the web service settings are to be saved to the config file.
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
        /// Gets or sets the category under which the web service settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
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
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="WebServiceHost"/> hosting the web service.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="ServiceHost"/> only has <see cref="WebHttpBinding"/> endpoint at the <see cref="ServiceUri"/>. 
        /// Additional endpoints can be added to the <see cref="ServiceHost"/> when <see cref="ServiceHostCreated"/> event is fired.
        /// </remarks>
        public WebServiceHost ServiceHost
        {
            get 
            {
                return m_serviceHost;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the web service can read <see cref="Archive"/> data and send it.
        /// </summary>
        protected bool CanRead
        {
            get
            {
                if (m_enabled && 
                    (m_serviceDataFlow == DataFlowDirection.Outgoing || m_serviceDataFlow == DataFlowDirection.BothWays))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the web service can receive data and write it to <see cref="Archive"/>.
        /// </summary>
        protected bool CanWrite
        {
            get
            {
                if (m_enabled && 
                    (m_serviceDataFlow == DataFlowDirection.Incoming || m_serviceDataFlow == DataFlowDirection.BothWays))
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the web service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the web service.
        /// </summary>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();
                if (m_enabled && !string.IsNullOrEmpty(m_serviceUri))
                {
                    // Create service host.
                    m_serviceHost = new WebServiceHost(this, new Uri(m_serviceUri));
                    // Add default endpoint.
                    WebHttpBinding webHttpBinding = new WebHttpBinding();
                    webHttpBinding.MaxReceivedMessageSize = int.MaxValue;
                    m_serviceHost.AddServiceEndpoint(Type.GetType(m_serviceContract), webHttpBinding, "");
                    OnServiceHostCreated();
                    // Change serialization behavior.
                    foreach (ServiceEndpoint endpoint in m_serviceHost.Description.Endpoints)
                    {
                        foreach (OperationDescription operation in endpoint.Contract.Operations)
                        {
                            // Following behavior property must be set for all operations of the web service to allow for the maximum number 
                            // of items of any object sent or received by the operation to be serialized/deserialized by the serializer.
                            DataContractSerializerOperationBehavior behavior = operation.Behaviors[typeof(DataContractSerializerOperationBehavior)] as DataContractSerializerOperationBehavior;
                            if (behavior != null)
                                behavior.MaxItemsInObjectGraph = int.MaxValue;
                        }
                    }
                    // Start service host.
                    m_serviceHost.Open();
                    OnServiceHostStarted();
                }

                // Initialize only once.
                m_initialized = true;
            }
        }

        /// <summary>
        /// Saves web service settings to the config file if the <see cref="PersistSettings"/> property is set to true.
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
                element = settings["Enabled", true];
                element.Update(m_enabled, element.Description, element.Encrypted);
                element = settings["ServiceUri", true];
                element.Update(m_serviceUri, element.Description, element.Encrypted);
                element = settings["ServiceContract", true];
                element.Update(m_serviceContract, element.Description, element.Encrypted);
                element = settings["ServiceDataFlow", true];
                element.Update(m_serviceDataFlow, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved web service settings from the config file if the <see cref="PersistSettings"/> property is set to true.
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
                settings.Add("Enabled", m_enabled, "True if this web service is enabled; otherwise False.");
                settings.Add("ServiceUri", m_serviceUri, "URI where this web service is to be hosted.");
                settings.Add("ServiceContract", m_serviceContract, "Contract interface implemented by this web service.");
                settings.Add("ServiceDataFlow", m_serviceDataFlow, "Flow of data (Incoming; Outgoing; BothWays) allowed for this web service.");
                Enabled = settings["Enabled"].ValueAs(m_enabled);
                ServiceUri = settings["ServiceUri"].ValueAs(m_serviceUri);
                ServiceContract = settings["ServiceContract"].ValueAs(m_serviceContract);
                ServiceDataFlow = settings["ServiceDataFlow"].ValueAs(m_serviceDataFlow);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the web service and optionally releases the managed resources.
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

                        if (m_serviceHost != null)
                            m_serviceHost.Close();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ServiceHostCreated"/> event.
        /// </summary>
        protected virtual void OnServiceHostCreated()
        {
            if (ServiceHostCreated != null)
                ServiceHostCreated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceHostStarted"/> event.
        /// </summary>
        protected virtual void OnServiceHostStarted()
        {
            if (ServiceHostStarted != null)
                ServiceHostStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceProcessError"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to sent to <see cref="ServiceProcessError"/> event.</param>
        protected virtual void OnServiceProcessError(Exception exception)
        {
            if (ServiceProcessError != null)
                ServiceProcessError(this, new EventArgs<Exception>(exception));
        }

        #endregion
    }
}
