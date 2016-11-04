//******************************************************************************************************
//  ServiceBusService.cs - Gbtc
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
//  10/06/2010 - Pinal C. Patel
//       Generated original version of source code.
//  10/26/2010 - Pinal C. Patel
//       Added management operations GetClients(), GetQueues() and GetTopics().
//  11/23/2010 - Pinal C. Patel
//       Added new BufferThreshold and ProcessingMode properties.
//       Enhanced thread synchronization using ReaderWriterLockSlim for better performance.
//  11/24/2010 - Pinal C. Patel
//       Updated the text returned by Status property.
//  01/07/2011 - Pinal C. Patel
//       Fixed initialization issue that prevented the service from functioning when hosted inside ASP.NET.
//  02/03/2011 - Pinal C. Patel
//       Added GetLatestMessage() operation that can be used to retrieve the latest message published 
//       to the subscribers of a topic.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/2/2014 - Pinal C. Patel
//       Updated to support publishing locally from inheriting class without establishing a communications 
//       channel over the web service interface.
//       Added bubbling up of exception encountered when publishing messages.
//       Updated Status to include information about clients, queues and topics.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using GSF.Collections;
using GSF.Configuration;
using GSF.ServiceModel;

namespace GSF.ServiceBus
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates how the distribution of <see cref="Message"/>s is processed by the <see cref="ServiceBusService"/>.
    /// </summary>
    public enum MessageProcessingMode
    {
        /// <summary>
        /// <see cref="Message"/> distribution is processed in parallel for increased distribution performance.
        /// </summary>
        Parallel,
        /// <summary>
        /// <see cref="Message"/> distribution is processed sequentially to preserve <see cref="Message"/> ordering.
        /// </summary>
        Sequential
    }

    #endregion

    /// <summary>
    /// A service bus for event-based messaging between disjoint systems.
    /// </summary>
    /// <example>
    /// This example shows how to host <see cref="ServiceBusService"/> inside a console application:
    /// <code>
    /// using System;
    /// using System.ServiceModel;
    /// using System.ServiceModel.Description;
    /// using System.Threading;
    /// using GSF;
    /// using GSF.ServiceBus;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Prompt for security option.
    ///         Console.Write("Enable security (Y/N): ");
    ///         bool enableSecurity = Console.ReadLine().ParseBoolean();
    /// 
    ///         // Initialize service bus.
    ///         ServiceBusService service = new ServiceBusService();
    ///         service.Singleton = true;
    ///         service.PublishMetadata = true;
    ///         service.PersistSettings = false;
    ///         service.Endpoints = "http.duplex://localhost:4501; net.tcp://locahost:4502";
    ///         if (enableSecurity)
    ///             service.SecurityPolicy = typeof(ServiceBusSecurityPolicy).FullName;
    ///         service.Initialize();
    /// 
    ///         // Show service bus status.
    ///         if (service.ServiceHost.State == CommunicationState.Opened)
    ///         {
    ///             Console.WriteLine("\r\n{0} is running:", service.GetType().Name);
    ///             foreach (ServiceEndpoint endpoint in service.ServiceHost.Description.Endpoints)
    ///             {
    ///                 Console.WriteLine("- {0} ({1})", endpoint.Address, endpoint.Binding.GetType().Name);
    ///             }
    /// 
    ///             new Thread(delegate() 
    ///                 {
    ///                     while (service.ServiceHost.State == CommunicationState.Opened)
    ///                     {
    ///                         Console.WriteLine();
    ///                         Console.WriteLine();
    ///                         Console.WriteLine("Status of {0}:", service.Name);
    ///                         Console.WriteLine(service.Status);
    ///                         Console.Write("Press Enter key to stop...");
    /// 
    ///                         Thread.Sleep(5000);
    ///                     }
    ///                 }).Start();
    ///         }
    ///         else
    ///         {
    ///             Console.WriteLine("\r\n{0} could not be started", service.GetType().Name);
    ///         }
    /// 
    ///         // Shutdown.
    ///         Console.ReadLine();
    ///         service.Dispose();
    ///     }
    /// }
    /// </code>
    /// This example shows how to host <see cref="ServiceBusService"/> inside a web application:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <serviceBusService>
    ///       <add name="Endpoints" value="" description="Semicolon delimited list of URIs where the web service can be accessed."
    ///         encrypted="false" />
    ///       <add name="Contract" value="GSF.ServiceBus.IServiceBusService, GSF.ServiceBus"
    ///         description="Assembly qualified name of the contract interface implemented by the web service."
    ///         encrypted="false" />
    ///       <add name="Singleton" value="True" description="True if the web service is singleton; otherwise False."
    ///         encrypted="false" />
    ///       <add name="SecurityPolicy" value="" description="Assembly qualified name of the authorization policy to be used for securing the web service."
    ///         encrypted="false" />
    ///       <add name="PublishMetadata" value="True" description="True if the web service metadata is to be published at all the endpoints; otherwise False."
    ///         encrypted="false" />
    ///       <add name="BufferThreshold" value="-1" description="Maximum number of messages that can be queued for distribution before the oldest ones are discarded."
    ///         encrypted="false" />
    ///       <add name="ProcessingMode" value="Sequential" description="Processing mode (Parallel; Sequential) to be used for the distribution of messages."
    ///         encrypted="false" />
    ///     </serviceBusService>
    ///   </categorizedSettings>
    ///   <system.serviceModel>
    ///     <services>
    ///       <service name="GSF.ServiceBus.ServiceBusService">
    ///         <endpoint address="" contract="GSF.ServiceBus.IServiceBusService" binding="wsDualHttpBinding" />
    ///       </service>
    ///     </services>
    ///     <behaviors>
    ///       <serviceBehaviors>
    ///         <behavior>
    ///           <serviceMetadata httpGetEnabled="true" />
    ///           <serviceDebug includeExceptionDetailInFaults="false" />
    ///         </behavior>
    ///       </serviceBehaviors>
    ///     </behaviors>
    ///     <serviceHostingEnvironment multipleSiteBindingsEnabled="true">
    ///       <serviceActivations>
    ///         <add relativeAddress="ServiceBusService.svc" service="GSF.ServiceBus.ServiceBusService, GSF.ServiceBus" />
    ///       </serviceActivations>
    ///     </serviceHostingEnvironment>
    ///   </system.serviceModel>
    /// </configuration>
    /// ]]>
    /// </code>
    /// This example shows how to publish <see cref="Message"/>s to <see cref="ServiceBusService"/>:
    /// <code>
    /// using System;
    /// using System.ServiceModel;
    /// using System.Threading;
    /// 
    /// class Program : IServiceBusServiceCallback
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // NOTE: Service reference to the service bus service must be added to generate the service proxy.
    /// 
    ///         // Initialize auto-generated service bus service proxy.
    ///         InstanceContext callbackContext = new InstanceContext(new Program());
    ///         ServiceBusServiceClient serviceBusService = new ServiceBusServiceClient(callbackContext, "NetTcpBinding_IServiceBusService");
    /// 
    ///         // Create registration request for publishing messages.
    ///         RegistrationRequest registration = new RegistrationRequest();
    ///         registration.MessageType = MessageType.Topic;
    ///         registration.MessageName = "Topic.Frequency";
    ///         registration.RegistrationType = RegistrationType.Produce;
    ///         serviceBusService.Register(registration);
    /// 
    ///         // Start publishing messages to the bus asynchronously.
    ///         new Thread(delegate() 
    ///             {
    ///                 Message message = new Message();
    ///                 message.Type = registration.MessageType;
    ///                 message.Name = registration.MessageName;
    ///                 message.Format = "application/octet-stream";
    /// 
    ///                 Random random = new Random(59);
    ///                 while (serviceBusService.State == CommunicationState.Opened)
    ///                 {
    ///                     message.Time = DateTime.UtcNow;
    ///                     message.Content = BitConverter.GetBytes(random.Next(61));
    ///                     serviceBusService.Publish(message);
    /// 
    ///                     Thread.Sleep(5000);
    ///                 }
    ///             }).Start();
    /// 
    ///         // Shutdown.
    ///         Console.Write("Press Enter key to stop...");
    ///         Console.ReadLine();
    ///         serviceBusService.Close();
    ///     }
    /// 
    ///     public void ProcessMessage(Message message)
    ///     {
    ///         // This method will not be invoked since we are not consuming messages.
    ///         throw new NotSupportedException();
    ///     }
    /// }
    /// </code>
    /// This example shows how to subscribe to <see cref="ServiceBusService"/> for receiving <see cref="Message"/>s:
    /// <code>
    /// using System;
    /// using System.ServiceModel;
    /// 
    /// class Program : IServiceBusServiceCallback
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // NOTE: Service reference to the service bus service must be added to generate the service proxy.
    /// 
    ///         // Initialize auto-generated service bus service proxy.
    ///         InstanceContext callbackContext = new InstanceContext(new Program());
    ///         ServiceBusServiceClient serviceBusService = new ServiceBusServiceClient(callbackContext, "NetTcpBinding_IServiceBusService");
    /// 
    ///         // Subscribe with service bus service to receive messages.
    ///         RegistrationRequest registration = new RegistrationRequest();
    ///         registration.MessageType = MessageType.Topic;
    ///         registration.MessageName = "Topic.Frequency";
    ///         registration.RegistrationType = RegistrationType.Consume;
    ///         serviceBusService.Register(registration);
    /// 
    ///         // Shutdown.
    ///         Console.WriteLine("Press Enter key to stop...");
    ///         Console.WriteLine();
    ///         Console.ReadLine();
    ///         serviceBusService.Close();
    ///     }
    /// 
    ///     public void ProcessMessage(Message message)
    ///     {
    ///         if (message.Format != "application/octet-stream")
    ///             Console.WriteLine("Message format '{0}' is not supported", message.Format);
    ///         else
    ///             Console.WriteLine("Message received: {0} Hz", BitConverter.ToInt32(message.Content, 0));
    ///     }
    /// }
    /// </code>
    /// This example shows how to monitor <see cref="ServiceBusService"/> remotely:
    /// <code>
    /// using System;
    /// using System.ServiceModel;
    /// using System.Threading;
    /// using GSF;
    /// 
    /// class Program : IServiceBusServiceCallback
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // NOTE: Service reference to the service bus service must be added to generate the service proxy.
    /// 
    ///         // Initialize auto-generated service bus service proxy.
    ///         InstanceContext callbackContext = new InstanceContext(new Program());
    ///         ServiceBusServiceClient serviceBusService = new ServiceBusServiceClient(callbackContext, "NetTcpBinding_IServiceBusService");
    ///         serviceBusService.ChannelFactory.Open();
    /// 
    ///         // Start querying service bus service status asynchronously.
    ///         new Thread(delegate()
    ///         {
    ///             while (serviceBusService.State == CommunicationState.Opened)
    ///             {
    ///                 Console.Clear();
    ///                 Console.WriteLine(new string('-', 79));
    ///                 Console.WriteLine("|" + "Service Bus Status".CenterText(77) + "|");
    ///                 Console.WriteLine(new string('-', 79));
    ///                 Console.WriteLine();
    /// 
    ///                 // Show clients.
    ///                 Console.Write("Client ID".PadRight(25));
    ///                 Console.Write(" ");
    ///                 Console.Write("Connected".PadRight(21));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Produced".PadRight(15));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Consumed".PadRight(15));
    ///                 Console.WriteLine();
    ///                 Console.Write(new string('-', 25));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 21));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.WriteLine();
    ///                 foreach (ClientInfo client in serviceBusService.GetClients())
    ///                 {
    ///                     Console.Write(client.SessionId.TruncateRight(25).PadRight(25));
    ///                     Console.Write(" ");
    ///                     Console.Write(client.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(21));
    ///                     Console.Write(" ");
    ///                     Console.Write(client.MessagesProduced.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(client.MessagesConsumed.ToString().PadRight(15));
    ///                     Console.WriteLine();
    ///                 }
    ///                 Console.WriteLine();
    /// 
    ///                 // Show queues.
    ///                 Console.Write("Queue Name".PadRight(25));
    ///                 Console.Write(" ");
    ///                 Console.Write("Producers".PadRight(10));
    ///                 Console.Write(" ");
    ///                 Console.Write("Consumers".PadRight(10));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Received".PadRight(15));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Processed".PadRight(15));
    ///                 Console.WriteLine();
    ///                 Console.Write(new string('-', 25));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 10));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 10));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.WriteLine();
    ///                 foreach (RegistrationInfo queue in serviceBusService.GetQueues())
    ///                 {
    ///                     Console.Write(queue.MessageName.PadRight(25));
    ///                     Console.Write(" ");
    ///                     Console.Write(queue.Producers.Length.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(queue.Consumers.Length.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(queue.MessagesReceived.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(queue.MessagesProcessed.ToString().PadRight(15));
    ///                     Console.WriteLine();
    ///                 }
    ///                 Console.WriteLine();
    /// 
    ///                 // Show topics.
    ///                 Console.Write("Topic Name".PadRight(25));
    ///                 Console.Write(" ");
    ///                 Console.Write("Producers".PadRight(10));
    ///                 Console.Write(" ");
    ///                 Console.Write("Consumers".PadRight(10));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Received".PadRight(15));
    ///                 Console.Write(" ");
    ///                 Console.Write("Msg. Processed".PadRight(15));
    ///                 Console.WriteLine();
    ///                 Console.Write(new string('-', 25));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 10));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 10));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.Write(" ");
    ///                 Console.Write(new string('-', 15));
    ///                 Console.WriteLine();
    ///                 foreach (RegistrationInfo topic in serviceBusService.GetTopics())
    ///                 {
    ///                     Console.Write(topic.MessageName.PadRight(25));
    ///                     Console.Write(" ");
    ///                     Console.Write(topic.Producers.Length.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(topic.Consumers.Length.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(topic.MessagesReceived.ToString().PadRight(15));
    ///                     Console.Write(" ");
    ///                     Console.Write(topic.MessagesProcessed.ToString().PadRight(15));
    ///                     Console.WriteLine();
    ///                 }
    ///                 Console.WriteLine();
    ///                 Console.Write("Press Enter key to stop...");
    /// 
    ///                 Thread.Sleep(5000);
    ///             }
    ///         }).Start();
    /// 
    ///         // Shutdown.
    ///         Console.ReadLine();
    ///         serviceBusService.Close();
    ///     }
    /// 
    ///     public void ProcessMessage(Message message)
    ///     {
    ///         // This method will not be invoked since we are not consuming messages.
    ///         throw new NotSupportedException();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Message"/>
    /// <seealso cref="ClientInfo"/>
    /// <seealso cref="RegistrationInfo"/>
    /// <seealso cref="RegistrationRequest"/>
    /// <seealso cref="ServiceBusSecurityPolicy"/>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ServiceBusService : SelfHostingService, IServiceBusService
    {
        #region [ Members ]

        // Nested Types

        private class PublishContext
        {
            public PublishContext(Message message, RegistrationInfo registration)
            {
                Message = message;
                Registration = registration;
            }

            public readonly Message Message;

            public readonly RegistrationInfo Registration;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="BufferThreshold"/> property.
        /// </summary>
        public const int DefaultBufferThreshold = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ProcessingMode"/> property.
        /// </summary>
        public const MessageProcessingMode DefaultProcessingMode = MessageProcessingMode.Sequential;

        // Fields
        private int m_bufferThreshold;
        private MessageProcessingMode m_processingMode;
        private readonly Dictionary<string, ClientInfo> m_clients;
        private readonly Dictionary<string, RegistrationInfo> m_queues;
        private readonly Dictionary<string, RegistrationInfo> m_topics;
        private readonly ReaderWriterLockSlim m_clientsLock;
        private readonly ReaderWriterLockSlim m_queuesLock;
        private readonly ReaderWriterLockSlim m_topicsLock;
        private ProcessQueue<PublishContext> m_publishQueue;
        private long m_discardedMessages;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusService"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServiceBusService()
        {
            // Override base class settings.
            Singleton = true;
            PublishMetadata = true;
            PersistSettings = true;

            // Initialize member variables.
            m_bufferThreshold = DefaultBufferThreshold;
            m_processingMode = DefaultProcessingMode;
            m_clients = new Dictionary<string, ClientInfo>(StringComparer.CurrentCultureIgnoreCase);
            m_queues = new Dictionary<string, RegistrationInfo>(StringComparer.CurrentCultureIgnoreCase);
            m_topics = new Dictionary<string, RegistrationInfo>(StringComparer.CurrentCultureIgnoreCase);
            m_clientsLock = new ReaderWriterLockSlim();
            m_queuesLock = new ReaderWriterLockSlim();
            m_topicsLock = new ReaderWriterLockSlim();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceBusService"/> is currently enabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return (m_publishQueue != null && m_publishQueue.Enabled);
            }
            set
            {
                if (value && m_publishQueue == null)
                    Initialize();

                m_publishQueue.Enabled = value;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="ServiceBusService"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("          Buffer threshold: ");
                status.Append(m_bufferThreshold == -1 ? "Disabled" : m_bufferThreshold.ToString());
                status.AppendLine();
                status.Append("           Processing mode: ");
                status.Append(m_processingMode);
                status.AppendLine();

                // Show publish queue statistics.
                if (m_publishQueue != null)
                {
                    ProcessQueueStatistics statistics = m_publishQueue.CurrentStatistics;
                    status.Append("         Messages received: ");
                    status.Append(statistics.QueueCount + statistics.TotalProcessedItems + statistics.ItemsBeingProcessed + m_discardedMessages);
                    status.AppendLine();
                    status.Append("        Messages discarded: ");
                    status.Append(m_discardedMessages);
                    status.AppendLine();
                    status.Append("        Messages processed: ");
                    status.Append(statistics.TotalProcessedItems);
                    status.AppendLine();
                    status.Append("  Messages being processed: ");
                    status.Append(statistics.ItemsBeingProcessed);
                    status.AppendLine();
                }

                // Show connected clients.
                m_clientsLock.EnterReadLock();
                try
                {
                    status.AppendFormat("         Number of clients: {0}", m_clients.Count);
                    status.AppendLine();
                    foreach (ClientInfo client in m_clients.Values)
                    {
                        status.AppendLine();
                        status.AppendFormat("                 Client Id: {0}", client.SessionId);
                        status.AppendLine();
                        status.AppendFormat("              Connected at: {0}", client.ConnectedAt);
                        status.AppendLine();
                        status.AppendFormat("         Messages produced: {0}", client.MessagesProduced);
                        status.AppendLine();
                        status.AppendFormat("         Messages consumed: {0}", client.MessagesConsumed);
                        status.AppendLine();
                    }

                    if (m_clients.Count > 0)
                        status.AppendLine();
                }
                finally
                {
                    m_clientsLock.ExitReadLock();
                }

                // Show registered queues.
                m_queuesLock.EnterReadLock();
                try
                {
                    status.AppendFormat("          Number of queues: {0}", m_queues.Count);
                    status.AppendLine();
                    foreach (RegistrationInfo queue in m_queues.Values)
                    {
                        status.AppendLine();
                        status.AppendFormat("                Queue name: {0}", queue.MessageName);
                        status.AppendLine();
                        status.AppendFormat("       Number of producers: {0}", queue.Producers.Count);
                        status.AppendLine();
                        status.AppendFormat("       Number of consumers: {0}", queue.Consumers.Count);
                        status.AppendLine();
                        status.AppendFormat("         Messages received: {0}", queue.MessagesReceived);
                        status.AppendLine();
                        status.AppendFormat("        Messages processed: {0}", queue.MessagesProcessed);
                        status.AppendLine();
                        if (queue.LatestMessage != null)
                        {
                            status.AppendFormat("       Latest message time: {0}", queue.LatestMessage.Time);
                            status.AppendLine();
                        }
                    }

                    if (m_queues.Count > 0)
                        status.AppendLine();
                }
                finally
                {
                    m_queuesLock.ExitReadLock();
                }

                // Show registered topics.
                m_topicsLock.EnterReadLock();
                try
                {
                    status.AppendFormat("          Number of topics: {0}", m_topics.Count);
                    status.AppendLine();
                    foreach (RegistrationInfo topic in m_topics.Values)
                    {
                        status.AppendLine();
                        status.AppendFormat("                Topic name: {0}", topic.MessageName);
                        status.AppendLine();
                        status.AppendFormat("       Number of producers: {0}", topic.Producers.Count);
                        status.AppendLine();
                        status.AppendFormat("       Number of consumers: {0}", topic.Consumers.Count);
                        status.AppendLine();
                        status.AppendFormat("         Messages received: {0}", topic.MessagesReceived);
                        status.AppendLine();
                        status.AppendFormat("        Messages processed: {0}", topic.MessagesProcessed);
                        status.AppendLine();
                        if (topic.LatestMessage != null)
                        {
                            status.AppendFormat("       Latest message time: {0}", topic.LatestMessage.Time);
                            status.AppendLine();
                        }
                    }

                    if (m_topics.Count > 0)
                        status.AppendLine();
                }
                finally
                {
                    m_topicsLock.ExitReadLock();
                }

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of <see cref="Message"/>s that can be buffered for distribution by the <see cref="ServiceBusService"/> before the 
        /// the oldest buffered <see cref="Message"/>s are discarded to keep memory consumption in check by avoiding <see cref="Message"/> flooding.
        /// </summary>
        /// <remarks>Set <see cref="BufferThreshold"/> to -1 to disable discarding of <see cref="Message"/>s.</remarks>
        public int BufferThreshold
        {
            get
            {
                return m_bufferThreshold;
            }
            set
            {
                if (value < 0)
                    value = -1;

                m_bufferThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MessageProcessingMode"/> used by the <see cref="ServiceBusService"/> for processing <see cref="Message"/> distribution.
        /// </summary>
        public MessageProcessingMode ProcessingMode
        {
            get
            {
                return m_processingMode;
            }
            set
            {
                m_processingMode = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The specified <see cref="ProcessingMode"/> is not supported.</exception>
        public override void Initialize()
        {
            base.Initialize();
            if (m_publishQueue == null)
            {
                // Instantiate the process queue.
                if (m_processingMode == MessageProcessingMode.Parallel)
                    m_publishQueue = ProcessQueue<PublishContext>.CreateAsynchronousQueue(PublishMessages);
                else if (m_processingMode == MessageProcessingMode.Sequential)
                    m_publishQueue = ProcessQueue<PublishContext>.CreateRealTimeQueue(PublishMessages);
                else
                    throw new NotSupportedException(string.Format("Processing mode '{0}' is not supported", m_processingMode));

                // Start the process queue.
                m_publishQueue.ProcessException += OnProcessException;
                m_publishQueue.Start();
            }
        }

        /// <summary>
        /// Saves <see cref="ServiceBusService"/> settings to the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["BufferThreshold", true].Update(m_bufferThreshold);
                settings["ProcessingMode", true].Update(m_processingMode);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="ServiceBusService"/> settings from the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("BufferThreshold", m_bufferThreshold, "Maximum number of messages that can be queued for distribution before the oldest ones are discarded.");
                settings.Add("ProcessingMode", m_processingMode, "Processing mode (Parallel; Sequential) to be used for the distribution of messages.");
                BufferThreshold = settings["BufferThreshold"].ValueAs(m_bufferThreshold);
                ProcessingMode = settings["ProcessingMode"].ValueAs(m_processingMode);
            }
        }

        /// <summary>
        /// Registers with the <see cref="ServiceBusService"/> to produce or consume <see cref="Message"/>s.
        /// </summary>
        /// <param name="request">An <see cref="RegistrationRequest"/> containing registration data.</param>
        public virtual void Register(RegistrationRequest request)
        {
            // Initialize if uninitialized.
            Initialize();

            // Save client information if not already present.
            ClientInfo client = null;
            if (OperationContext.Current != null)
            {
                m_clientsLock.EnterUpgradeableReadLock();
                try
                {
                    if (!m_clients.TryGetValue(OperationContext.Current.SessionId, out client))
                    {
                        m_clientsLock.EnterWriteLock();
                        try
                        {
                            client = new ClientInfo(OperationContext.Current);
                            m_clients.Add(client.SessionId, client);
                            client.OperationContext.Channel.Faulted += OnChannelFaulted;
                            client.OperationContext.Channel.Closing += OnChannelClosing;
                        }
                        finally
                        {
                            m_clientsLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    m_clientsLock.ExitUpgradeableReadLock();
                }
            }

            // Retrieve registration information.
            RegistrationInfo registration = null;
            if (request.MessageType == MessageType.Queue)
            {
                // Queue
                m_queuesLock.EnterUpgradeableReadLock();
                try
                {
                    if (!m_queues.TryGetValue(request.MessageName, out registration))
                    {
                        m_queuesLock.EnterWriteLock();
                        try
                        {
                            registration = new RegistrationInfo(request);
                            m_queues.Add(request.MessageName, registration);
                        }
                        finally
                        {
                            m_queuesLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    m_queuesLock.ExitUpgradeableReadLock();
                }
            }
            else if (request.MessageType == MessageType.Topic)
            {
                // Topic
                m_topicsLock.EnterUpgradeableReadLock();
                try
                {
                    if (!m_topics.TryGetValue(request.MessageName, out registration))
                    {
                        m_topicsLock.EnterWriteLock();
                        try
                        {
                            registration = new RegistrationInfo(request);
                            m_topics.Add(request.MessageName, registration);
                        }
                        finally
                        {
                            m_topicsLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    m_topicsLock.ExitUpgradeableReadLock();
                }
            }
            else
            {
                // Unsupported
                throw new NotSupportedException(string.Format("Message type '{0}' is not supported by this operation", request.MessageType));
            }

            // Update registration information.
            if (registration != null && client != null)
            {
                List<ClientInfo> clients = (request.RegistrationType == RegistrationType.Produce ? registration.Producers : registration.Consumers);
                lock (clients)
                {
                    if (!clients.Contains(client))
                        clients.Add(client);
                }
            }
        }

        /// <summary>
        /// Unregisters a previous registration with the <see cref="ServiceBusService"/> to produce or consume <see cref="Message"/>s
        /// </summary>
        /// <param name="request">The <see cref="RegistrationRequest"/> used when registering.</param>
        public virtual void Unregister(RegistrationRequest request)
        {
            // Retrieve registration information.
            RegistrationInfo registration = null;
            if (request.MessageType == MessageType.Queue)
            {
                // Queue
                m_queuesLock.EnterReadLock();
                try
                {
                    m_queues.TryGetValue(request.MessageName, out registration);
                }
                finally
                {
                    m_queuesLock.ExitReadLock();
                }
            }
            else if (request.MessageType == MessageType.Topic)
            {
                // Topic
                m_topicsLock.EnterReadLock();
                try
                {
                    m_topics.TryGetValue(request.MessageName, out registration);
                }
                finally
                {
                    m_topicsLock.ExitReadLock();
                }
            }
            else
            {
                // Unsupported
                throw new NotSupportedException(string.Format("Message type '{0}' is not supported by this operation", request.MessageType));
            }

            // Update registration information.
            if (registration != null && OperationContext.Current != null)
            {
                List<ClientInfo> clients = (request.RegistrationType == RegistrationType.Produce ? registration.Producers : registration.Consumers);
                lock (clients)
                {
                    clients.RemoveAt(clients.FindIndex(client => client.SessionId == OperationContext.Current.SessionId));
                }
            }
        }

        /// <summary>
        /// Sends the <paramref name="message"/> to the <see cref="ServiceBusService"/> for distribution amongst its registered consumers.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> that is to be distributed.</param>
        public virtual void Publish(Message message)
        {
            // Retrieve publisher information.
            ClientInfo client = null;
            if (OperationContext.Current != null)
            {
                m_clientsLock.EnterReadLock();
                try
                {
                    // Update statistics data.
                    if (m_clients.TryGetValue(OperationContext.Current.SessionId, out client))
                        Interlocked.Increment(ref client.MessagesProduced);
                }
                finally
                {
                    m_clientsLock.ExitReadLock();
                }
            }

            // Retrieve registration information.
            RegistrationInfo registration = null;
            if (message.Type == MessageType.Queue)
            {
                // Queue
                m_queuesLock.EnterReadLock();
                try
                {
                    m_queues.TryGetValue(message.Name, out registration);
                }
                finally
                {
                    m_queuesLock.ExitReadLock();
                }
            }
            else if (message.Type == MessageType.Topic)
            {
                // Topic
                m_topicsLock.EnterReadLock();
                try
                {
                    m_topics.TryGetValue(message.Name, out registration);
                }
                finally
                {
                    m_topicsLock.ExitReadLock();
                }
            }
            else
            {
                // Unsupported
                throw new NotSupportedException(string.Format("Message type '{0}' is not supported by this operation", message.Type));
            }

            // Queue message for distribution.
            if (registration != null && m_publishQueue != null)
            {
                Interlocked.Increment(ref registration.MessagesReceived);
                m_publishQueue.Add(new PublishContext(message, registration));
            }
        }

        /// <summary>
        /// Gets the latest <see cref="Message"/> distributed to the subscribers of the specified <paramref name="topic"/>.
        /// </summary>
        /// <param name="topic">The topic <see cref="RegistrationRequest"/> used when registering.</param>
        /// <returns>The latest <see cref="Message"/> distributed to the <paramref name="topic"/> subscribers.</returns>
        public virtual Message GetLatestMessage(RegistrationRequest topic)
        {
            // Retrieve registration information.
            RegistrationInfo registration;
            m_topicsLock.EnterReadLock();
            try
            {
                m_topics.TryGetValue(topic.MessageName, out registration);
            }
            finally
            {
                m_topicsLock.ExitReadLock();
            }

            // Retrieve the latest message.
            if (registration != null)
            {
                lock (registration.Consumers)
                {
                    if (registration.Consumers.Exists(consumer => consumer.SessionId == OperationContext.Current.SessionId))
                        // Requestor has subscribed to the topic.
                        return registration.LatestMessage;
                    else
                        // Requestor didn't subscribe to the topic.
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a list of all clients connected to the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="ClientInfo"/> objects.</returns>
        public virtual ICollection<ClientInfo> GetClients()
        {
            return m_clients.Values;
        }

        /// <summary>
        /// Gets a list of all <see cref="MessageType.Queue"/>s registered on the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="RegistrationInfo"/> objects.</returns>
        public virtual ICollection<RegistrationInfo> GetQueues()
        {
            return m_queues.Values;
        }

        /// <summary>
        /// Gets a list of all <see cref="MessageType.Topic"/>s registered on the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="RegistrationInfo"/> objects.</returns>
        public virtual ICollection<RegistrationInfo> GetTopics()
        {
            return m_topics.Values;
        }

        /// <summary>
        /// Raises the <see cref="SelfHostingService.ServiceHostCreated"/> event.
        /// </summary>
        protected override void OnServiceHostCreated()
        {
            base.OnServiceHostCreated();

            foreach (ServiceEndpoint endpoint in ServiceHost.Description.Endpoints)
            {
                endpoint.Binding.ReceiveTimeout = TimeSpan.MaxValue;

                // Enable reliable messaging for TCP endpoint.
                if (endpoint.Binding is NetTcpBinding)
                {
                    NetTcpBinding binding = endpoint.Binding as NetTcpBinding;
                    binding.ReliableSession.Enabled = true;
                    binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
                }

                // Enable reliable messaging for HTTP duplex endpoint.
                if (endpoint.Binding is WSDualHttpBinding)
                {
                    WSDualHttpBinding binding = endpoint.Binding as WSDualHttpBinding;
                    binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
                }
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceBusService"/> object and optionally releases the managed resources.
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

                        // Disconnect all clients.
                        if (m_clients != null)
                        {
                            List<string> clientIds;
                            m_clientsLock.EnterReadLock();
                            try
                            {
                                clientIds = new List<string>(m_clients.Keys);
                            }
                            finally
                            {
                                m_clientsLock.ExitReadLock();
                            }

                            foreach (string clientId in clientIds)
                            {
                                DisconnectClient(clientId);
                            }
                        }

                        // Remove queue registrations.
                        if (m_queues != null)
                        {
                            m_queuesLock.EnterWriteLock();
                            try
                            {
                                foreach (RegistrationInfo registration in m_queues.Values)
                                {
                                    registration.Dispose();
                                }
                                m_queues.Clear();
                            }
                            finally
                            {
                                m_queuesLock.ExitWriteLock();
                            }
                        }

                        // Remove topic registrations.
                        if (m_topics != null)
                        {
                            m_topicsLock.EnterWriteLock();
                            try
                            {
                                foreach (RegistrationInfo registration in m_topics.Values)
                                {
                                    registration.Dispose();
                                }
                                m_topics.Clear();
                            }
                            finally
                            {
                                m_topicsLock.ExitWriteLock();
                            }
                        }

                        if (m_publishQueue != null)
                        {
                            m_publishQueue.ProcessException -= OnProcessException;
                            m_publishQueue.Dispose();
                        }

                        if (m_clientsLock != null)
                            m_clientsLock.Dispose();

                        if (m_queuesLock != null)
                            m_queuesLock.Dispose();

                        if (m_topicsLock != null)
                            m_topicsLock.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private void PublishMessages(PublishContext[] contexts)
        {
            // Process distribution of all the messages.
            int requeCursor = 0;
            foreach (PublishContext context in contexts)
            {
                lock (context.Registration.Consumers)
                {
                    if (context.Registration.Consumers.Count > 0)
                    {
                        // Distribute message to all subscribed clients.
                        foreach (ClientInfo client in context.Registration.Consumers)
                        {
                            try
                            {
                                client.OperationContext.GetCallbackChannel<IServiceBusServiceCallback>().ProcessMessage(context.Message);
                                Interlocked.Increment(ref client.MessagesConsumed);

                                if (context.Message.Type == MessageType.Queue)
                                    // Queue messages gets delivered to the first client only.
                                    break;
                            }
                            catch
                            {
                                // Disconnect the subscriber if an error is encountered during transmission.
                                try
                                {
                                    if (client.OperationContext.Channel.State == CommunicationState.Opened)
                                        client.OperationContext.Channel.Close();
                                }
                                catch
                                {
                                }
                            }
                        }
                        PublishComplete(context);
                    }
                    else
                    {
                        // No clients are subscribed to the queue or topic.
                        if (context.Message.Type == MessageType.Queue)
                            // Preserve queue messages since their delivery is guaranteed.
                            m_publishQueue.Insert(requeCursor++, context);
                        else
                            // Discard topic messages since their delivery is not guaranteed.
                            PublishComplete(context);
                    }
                }

            }

            // Keep message buffer in check if specified.
            if (m_bufferThreshold > 0 && m_publishQueue.Count > m_bufferThreshold)
            {
                int discardCount = m_publishQueue.Count - m_bufferThreshold;

                m_publishQueue.RemoveRange(0, discardCount);
                Interlocked.Add(ref m_discardedMessages, discardCount);
            }
        }

        private void PublishComplete(PublishContext context)
        {
            // Save the message for on-demand request of subscribers.
            context.Registration.LatestMessage = context.Message;

            // Update the count for the number of messages processed.
            Interlocked.Increment(ref context.Registration.MessagesProcessed);
        }

        private void DisconnectClient(string clientId)
        {
            // Retrieve client information.
            ClientInfo client;
            m_clientsLock.EnterUpgradeableReadLock();
            try
            {
                if (m_clients.TryGetValue(clientId, out client))
                {
                    // Remove client.
                    m_clientsLock.EnterWriteLock();
                    try
                    {
                        m_clients.Remove(clientId);
                        client.OperationContext.Channel.Faulted -= OnChannelFaulted;
                        client.OperationContext.Channel.Closing -= OnChannelClosing;
                    }
                    finally
                    {
                        m_clientsLock.ExitWriteLock();
                    }

                    // Close channel.
                    try
                    {
                        if (client.OperationContext.Channel.State == CommunicationState.Opened)
                            client.OperationContext.Channel.Close();
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                m_clientsLock.ExitUpgradeableReadLock();
            }

            // Remove client registrations.
            if (client != null)
            {
                // Remove any queue registrations.
                m_queuesLock.EnterReadLock();
                try
                {
                    foreach (RegistrationInfo registration in m_queues.Values)
                    {
                        lock (registration.Producers)
                        {
                            registration.Producers.Remove(client);
                        }
                        lock (registration.Consumers)
                        {
                            registration.Consumers.Remove(client);
                        }
                    }
                }
                finally
                {
                    m_queuesLock.ExitReadLock();
                }

                // Remove any topic registrations.
                m_topicsLock.EnterReadLock();
                try
                {
                    foreach (RegistrationInfo registration in m_topics.Values)
                    {
                        lock (registration.Producers)
                        {
                            registration.Producers.Remove(client);
                        }
                        lock (registration.Consumers)
                        {
                            registration.Consumers.Remove(client);
                        }
                    }
                }
                finally
                {
                    m_topicsLock.ExitReadLock();
                }
            }
        }

        private void SaveMessages()
        {
            // For use later when enabling message persistence.
        }

        private void LoadMessages()
        {
            // For use later when enabling message persistence.
        }

        private void OnChannelClosing(object sender, EventArgs e)
        {
            DisconnectClient(((IContextChannel)sender).SessionId);
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            DisconnectClient(((IContextChannel)sender).SessionId);
        }

        private void OnProcessException(object sender, EventArgs<Exception> e)
        {
            OnExecutionException("Error publishing messages", e.Argument);
        }

        #endregion
    }
}
