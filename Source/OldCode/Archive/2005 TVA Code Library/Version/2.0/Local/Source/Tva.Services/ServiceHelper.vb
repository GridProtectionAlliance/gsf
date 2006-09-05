' 08-29-06

Imports System.ComponentModel
Imports Tva.Tro.Ssam
Imports Tva.Communication
Imports Tva.Serialization

Public Class ServiceHelper

    Private m_serviceComponents As List(Of IServiceComponent)
    Private m_startedEventHandlerList As New List(Of EventHandler)

    Public Custom Event Started As EventHandler
        AddHandler(ByVal value As EventHandler)
            m_startedEventHandlerList.Add(value)
        End AddHandler

        RemoveHandler(ByVal value As EventHandler)
            m_startedEventHandlerList.Remove(value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
            For Each handler As EventHandler In m_startedEventHandlerList
                handler.BeginInvoke(sender, e, Nothing, Nothing)
            Next
        End RaiseEvent
    End Event

    Public Event Stoped As EventHandler

    Public Event Paused As EventHandler

    Public Event Resumed As EventHandler

    Public Event Shutdown As EventHandler

    Public Event ReceivedClientRequest(ByVal request As ClientRequest)

    <TypeConverter(GetType(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public ReadOnly Property TcpServer() As TcpServer
        Get
            Return SHTcpServer
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public ReadOnly Property ScheduleManager() As ScheduleManager
        Get
            Return SHScheduleManager
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public ReadOnly Property SsamLogger() As SsamLogger
        Get
            Return SHSsamLogger
        End Get
    End Property

    Public ReadOnly Property ServiceComponents() As List(Of IServiceComponent)
        Get
            Return m_serviceComponents
        End Get
    End Property

    Public Sub Start()

        For Each component As IServiceComponent In m_serviceComponents
            component.ServiceStateChanged(ServiceState.Started)
        Next

        RaiseEvent Started(Me, EventArgs.Empty)

    End Sub

    Public Sub OnStop()

        For Each component As IServiceComponent In m_serviceComponents
            component.ServiceStateChanged(ServiceState.Stopped)
        Next

        RaiseEvent Stoped(Me, EventArgs.Empty)

    End Sub

    Public Sub OnPause()

        For Each component As IServiceComponent In m_serviceComponents
            component.ServiceStateChanged(ServiceState.Paused)
        Next

        RaiseEvent Paused(Me, EventArgs.Empty)

    End Sub

    Public Sub OnResume()

        For Each component As IServiceComponent In m_serviceComponents
            component.ServiceStateChanged(ServiceState.Resumed)
        Next

        RaiseEvent Resumed(Me, EventArgs.Empty)

    End Sub

    Public Sub OnShutdown()

        For Each component As IServiceComponent In m_serviceComponents
            component.ServiceStateChanged(ServiceState.Shutdown)
        Next

        RaiseEvent Shutdown(Me, EventArgs.Empty)

    End Sub


#Region " TcpServer Events "

    Private Sub SHTcpServer_ReceivedClientData(ByVal clientID As System.Guid, ByVal data() As System.Byte) Handles SHTcpServer.ReceivedClientData

        Dim request As ClientRequest = GetObject(Of ClientRequest)(data)
        If request IsNot Nothing Then
            RaiseEvent ReceivedClientRequest(request)

            Select Case request.Type.ToUpper()
                Case "PAUSESERVICE"
                Case "RESUMESERVICE"
                Case "STOPSERVICE"
                Case "RESTARTSERVICE"
                Case "LISTPROCESSES"
                Case "STARTPROCESS"
                Case "ABORTPROCESS"
                Case "UNSCHEDULEPROCESS"
                Case "RESCHEDULEPROCESS"
                Case "PINGSERVICE"
                Case "PINGALLCLIENTS"
                Case "LISTALLCLIENTS"
                Case "GETSERVICESTATUS"
                Case "GETPROCESSSTATUS"
                Case "GETCOMMANDHISTORY"
                Case "GETDIRECTORYLISTING"
                Case "LISTSETTINGS"
                Case "UPDATESETTINGS"
                Case "SAVESETTINGS"
            End Select
        End If

    End Sub

#End Region

End Class