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

        Public Sub ProcessEvent(ByVal item As SsamEvent)

        End Sub

    End Class

End Namespace
