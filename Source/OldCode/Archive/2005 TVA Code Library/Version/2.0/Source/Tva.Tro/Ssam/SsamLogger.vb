' 04-04-06

Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Collections

Namespace Ssam

    <ToolboxBitmap(GetType(SsamLogger))> _
    Public Class SsamLogger

        Private m_apiInstance As SsamApi
        Private WithEvents m_eventQueue As ProcessQueue(Of SsamEvent)

        Public Event LogException(ByVal ex As Exception)

        <Category("Settings")> _
        Public ReadOnly Property ApiInstance() As SsamApi
            Get
                Return m_apiInstance
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property EventQueue() As ProcessQueue(Of SsamEvent)
            Get
                Return m_eventQueue
            End Get
        End Property

        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEvent.SsamEntityType, _
                ByVal eventType As SsamEvent.SsamEventType)

            LogEvent(entityID, entityType, eventType, "", "", "")

        End Sub

        Public Sub LogEvent(ByVal entityID As String, ByVal entityType As SsamEvent.SsamEntityType, _
                ByVal eventType As SsamEvent.SsamEventType, ByVal errorNumber As String, ByVal message As String, _
                ByVal description As String)

            LogEvent(New SsamEvent(entityID, entityType, eventType, errorNumber, message, description))

        End Sub

        Public Sub LogEvent(ByVal newEvent As SsamEvent)

            m_eventQueue.Add(newEvent)

        End Sub

        Public Sub ProcessEvent(ByVal item As SsamEvent)

            Try
                m_apiInstance.LogEvent(item)
            Catch ex As Exception
                RaiseEvent LogException(ex)
            End Try

        End Sub

    End Class

End Namespace
