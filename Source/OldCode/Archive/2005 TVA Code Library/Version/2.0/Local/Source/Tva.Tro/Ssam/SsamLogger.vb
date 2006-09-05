'*******************************************************************************************************
'  Tva.Tro.Ssam.SsamLogger.vb - SSAM Logger
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/24/2006 - Pinal C. Patel
'       Original version of source code generated
'  08/25/2006 - Pinal C. Patel
'       Changed property ApiInstance to SsamApi and made its content serializable in the designer.
'       Removed properties Server and KeepConnectionOpen that can now be modified through the SsamApi 
'       property.
'*******************************************************************************************************

Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Services
Imports Tva.Collections

Namespace Ssam

    ''' <summary>
    ''' Defines a component for logging events to the SSAM server.
    ''' </summary>
    ''' <remarks></remarks>
    <ToolboxBitmap(GetType(SsamLogger))> _
    Public Class SsamLogger
        Implements ISupportInitialize, IServiceComponent

        Private m_enabled As Boolean
        Private m_ssamApi As SsamApi
        Private WithEvents m_eventQueue As ProcessQueue(Of SsamEvent)

        ''' <summary>
        ''' Occurs when an exception is encountered when logging an event to the SSAM server.
        ''' </summary>
        ''' <param name="ex">The exception that was encountered when logging an event to the SSAM server.</param>
        ''' <remarks></remarks>
        <Description("Occurs when an exception is encountered when logging an event to the SSAM server.")> _
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
        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean)
            MyClass.New(server, keepConnectionOpen, True)
        End Sub

        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean, _
                ByVal persistConnectionStrings As Boolean)
            MyClass.New(New SsamApi(server, keepConnectionOpen, persistConnectionStrings))
        End Sub

        Public Sub New(ByVal ssamApi As SsamApi)
            MyBase.New()
            m_ssamApi = ssamApi
            m_enabled = True
            m_eventQueue = ProcessQueue(Of SsamEvent).CreateSynchronousQueue(AddressOf ProcessEvent)
            m_eventQueue.RequeueOnException = True
            m_eventQueue.Start()
        End Sub

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the logging of SSAM events is enabled.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if logging of SSAM events is enabled; otherwise False.</returns>
        ''' <remarks></remarks>
        <Description("Determines whether the logging of SSAM events is enabled."), Category("Configuration"), DefaultValue(GetType(Boolean), "True")> _
        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    m_eventQueue.Start()    ' Start processing any queued events when the logger is enabled.
                Else
                    m_eventQueue.Stop()     ' Stop processing any queued events when the logger is disabled.
                End If
                m_enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the Tva.Tro.Ssam.SsamApi inistance used for logging events to the SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Tva.Tro.Ssam.SsamApi inistance used for logging events to the SSAM server.</returns>
        ''' <remarks></remarks>
        <Description("The Tva.Tro.Ssam.SsamApi inistance used for logging events to the SSAM server."), Category("Configuration"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
        Public ReadOnly Property SsamApi() As SsamApi
            Get
                Return m_ssamApi
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
        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEntityType, _
                ByVal eventType As SsamEventType)

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
        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEntityType, _
                ByVal eventType As SsamEventType, ByVal errorNumber As String, ByVal message As String, _
                ByVal description As String)

            LogEvent(New SsamEvent(entityID, entityType, eventType, errorNumber, message, description))

        End Sub

        ''' <summary>
        ''' Queues the specified event for logging it to the SSAM server.
        ''' </summary>
        ''' <param name="newEvent">The event that is to be logged to the SSAM server.</param>
        ''' <remarks></remarks>
        Public Sub LogEvent(ByVal newEvent As SsamEvent)

            ' Discard the event if SSAM Logger has been disabled.
            If m_enabled Then
                ' SSAM Logger is enabled so queue the event for logging.
                m_eventQueue.Add(newEvent)
            End If

        End Sub

        ''' <summary>
        ''' This is delagate that will be invoked by the queue for processing an event in the queue.
        ''' </summary>
        ''' <param name="item">The event that is to be processed (logged to the SSAM server).</param>
        ''' <remarks></remarks>
        Private Sub ProcessEvent(ByVal item As SsamEvent)

            If m_enabled Then
                ' Process the queued events only when the SSAM Logger is enabled.
                m_ssamApi.LogEvent(item)
            End If

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
            ' Note: The DesignMode property is available only after the component has initialized.
            If Not DesignMode Then m_ssamApi.Initialize()

        End Sub

#End Region

#Region " IServiceComponent Implementation "

        Private m_previouslyEnabled As Boolean = False

        <Browsable(False)> _
        Public ReadOnly Property Name() As String Implements Services.IServiceComponent.Name
            Get
                Return Me.GetType.Name()
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal processName As String, ByVal newState As Services.ProcessState) Implements Services.IServiceComponent.ProcessStateChanged

            ' Ssam logger, when used as a service component, doesn't need to respond to changes in process state.

        End Sub

        Public Sub ServiceStateChanged(ByVal newState As Services.ServiceState) Implements Services.IServiceComponent.ServiceStateChanged

            Select Case newState
                Case ServiceState.Started
                    ' No action required when the service is started.
                Case ServiceState.Stopped
                    ' No action required when the service is stopped.
                Case ServiceState.Paused
                    m_previouslyEnabled = Me.Enabled
                    Me.Enabled = False
                Case ServiceState.Resumed
                    Me.Enabled = m_previouslyEnabled
                Case ServiceState.Shutdown
                    Me.Dispose()
            End Select

        End Sub

        <Browsable(False)> _
        Public ReadOnly Property Status() As String Implements Services.IServiceComponent.Status
            Get
                With New StringBuilder()
                    .Append("                    Logger: ")
                    Select Case Me.Enabled()
                        Case True
                            .Append("Enabled")
                        Case False
                            .Append("Disabled")
                    End Select
                    .Append(Environment.NewLine())
                    .Append(m_eventQueue.Status())
                    Return .ToString()
                End With
            End Get
        End Property

#End Region

    End Class

End Namespace
