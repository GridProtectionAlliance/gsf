'*******************************************************************************************************
'  Tva.Tro.Ssam.SsamLogger.vb - SSAM Logger
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/24/2006 - Pinal C Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Collections

Namespace Ssam

    ''' <summary>
    ''' Defines a component for logging events to the SSAM server.
    ''' </summary>
    ''' <remarks></remarks>
    <ToolboxBitmap(GetType(SsamLogger))> _
    Public Class SsamLogger
        Implements ISupportInitialize

        Private m_enabled As Boolean
        Private m_apiInstance As SsamApi
        Private WithEvents m_eventQueue As ProcessQueue(Of SsamEvent)

        ''' <summary>
        ''' Occurs when an exception is encountered when logging an event to the SSAM server.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when logging an event to the SSAM server.</param>
        ''' <remarks></remarks>
        Public Event LogException(ByVal ex As Exception)

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamLogger with the specified information.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is loggged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        ''' <remarks></remarks>
        Public Sub New(ByVal server As SsamApi.SsamServer, ByVal keepConnectionOpen As Boolean)
            MyClass.New(server, keepConnectionOpen, True)
        End Sub

        ''' <summary>
        ''' This constructor is for internal use only.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is logged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        ''' <param name="initializeApi">
        ''' True to update the configuration file of the client application with the connection strings required 
        ''' for connecting with any of the SSAM servers.
        ''' </param>
        ''' <remarks></remarks>
        Friend Sub New(ByVal server As SsamApi.SsamServer, ByVal keepConnectionOpen As Boolean, ByVal initializeApi As Boolean)
            MyBase.New()
            m_enabled = True
            m_apiInstance = New SsamApi(server, keepConnectionOpen, initializeApi)
            m_eventQueue = ProcessQueue(Of SsamEvent).CreateSynchronousQueue(AddressOf ProcessEvent, _
                ProcessQueue(Of SsamEvent).DefaultProcessInterval, ProcessQueue(Of SsamEvent).DefaultProcessTimeout, _
                ProcessQueue(Of SsamEvent).DefaultRequeueOnTimeout, True)
        End Sub

        ''' <summary>
        ''' Gets or sets the SSAM server to which event are to be logged.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The SSAM server to which event are to be logged.</returns>
        ''' <remarks></remarks>
        <Category("Configuration"), Description("The SSAM server to which event are to be logged."), DefaultValue(GetType(SsamApi.SsamServer), "Development")> _
        Public Property Server() As SsamApi.SsamServer
            Get
                Return m_apiInstance.Server()
            End Get
            Set(ByVal value As SsamApi.SsamServer)
                m_apiInstance.Server = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the connection with SSAM server is to be kept open after 
        ''' logging an event.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the connection with SSAM server is to be kept open after logging an event; otherwise False.</returns>
        ''' <remarks></remarks>
        <Category("Configuration"), Description("Determines whether the connection with SSAM server is to be kept open after logging an event."), DefaultValue(GetType(Boolean), "True")> _
        Public Property KeepConnectionOpen() As Boolean
            Get
                Return m_apiInstance.KeepConnectionOpen()
            End Get
            Set(ByVal value As Boolean)
                m_apiInstance.KeepConnectionOpen = value
            End Set
        End Property

        <Category("Configuration"), Description("Determines whether the logging of SSAM events is enabled."), DefaultValue(GetType(Boolean), "True")> _
        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                m_enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the Tva.Tro.Ssam.SsamApi inistance used for logging events to the SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Tva.Tro.Ssam.SsamApi inistance used for logging events to the SSAM server.</returns>
        ''' <remarks></remarks>
        <Browsable(False)> _
        Public ReadOnly Property ApiInstance() As SsamApi
            Get
                Return m_apiInstance
            End Get
        End Property

        ''' <summary>
        ''' Gets the Tva.Collections.ProcessQueue(Of SsamEvent) in which events are queued for logging to the 
        ''' SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The Tva.Collections.ProcessQueue(Of SsamEvent) in which events are queued for logging to the 
        ''' SSAM server.
        ''' </returns>
        ''' <remarks></remarks>
        <Browsable(False)> _
        Public ReadOnly Property EventQueue() As ProcessQueue(Of SsamEvent)
            Get
                Return m_eventQueue
            End Get
        End Property

        ''' <summary>
        ''' Creates an event with the specified information and queues it for logging to the SSAM server.
        ''' </summary>
        ''' <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
        ''' <param name="entityType">One of the Tva.Tro.Ssam.SsamEntityType values.</param>
        ''' <param name="eventType">One of the Tva.Tro.Ssam.SsamEvent.SsamEventType values.</param>
        ''' <remarks></remarks>
        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEvent.SsamEntityType, _
                ByVal eventType As SsamEvent.SsamEventType)

            LogEvent(entityID, entityType, eventType, "", "", "")

        End Sub

        ''' <summary>
        ''' Creates an event with the specified information and queues it for logging to the SSAM server.
        ''' </summary>
        ''' <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
        ''' <param name="entityType">One of the Tva.Tro.Ssam.SsamEntityType values.</param>
        ''' <param name="eventType">One of the Tva.Tro.Ssam.SsamEvent.SsamEventType values.</param>
        ''' <param name="errorNumber">The error number encountered, if any, for which the event is being logged.</param>
        ''' <param name="message">A brief description of the event (max 120 characters).</param>
        ''' <param name="description">A detailed description of the event (max 2GB).</param>
        ''' <remarks></remarks>
        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEvent.SsamEntityType, _
                ByVal eventType As SsamEvent.SsamEventType, ByVal errorNumber As String, ByVal message As String, _
                ByVal description As String)

            LogEvent(New SsamEvent(entityID, entityType, eventType, errorNumber, message, description))

        End Sub

        ''' <summary>
        ''' Queues the specified event for logging it to the SSAM server.
        ''' </summary>
        ''' <param name="newEvent">The event that is to be logged to the SSAM server.</param>
        ''' <remarks></remarks>
        Public Sub LogEvent(ByVal newEvent As SsamEvent)

            m_eventQueue.Add(newEvent)
            If Not m_eventQueue.Enabled() Then m_eventQueue.Start()

        End Sub

        ''' <summary>
        ''' This is delagate that will be invoked by the queue for processing an event in the queue.
        ''' </summary>
        ''' <param name="item">The event that is to be processed (logged to the SSAM server).</param>
        ''' <remarks></remarks>
        Private Sub ProcessEvent(ByVal item As SsamEvent)

            m_apiInstance.LogEvent(item)

        End Sub

        Private Sub m_eventQueue_ProcessException(ByVal ex As Exception) Handles m_eventQueue.ProcessException

            RaiseEvent LogException(ex)

        End Sub

#Region " ISupportInitialize Implementation "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            ' There is nothing we need to do before the component is initialized.

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            ' We will initialize the API only when we are not in design mode. This is done to avoid 
            ' exception raised by the configuration API because initialization of the Ssam API
            ' involves saving connection strings for connecting to the SSAM servers in the configuration
            ' file of the client application.
            ' Note: The DesignMode() property is available only after the component has initialized.
            If Not DesignMode() Then m_apiInstance.Initialize()

        End Sub

#End Region

    End Class

End Namespace
