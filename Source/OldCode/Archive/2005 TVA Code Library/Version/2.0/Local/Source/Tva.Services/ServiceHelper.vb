' 08-29-06

Public Class ServiceHelper

    Private m_startEventHandlerList As New List(Of EventHandler)

    Public Custom Event Start As EventHandler
        AddHandler(ByVal value As EventHandler)
            m_startEventHandlerList.Add(value)
        End AddHandler

        RemoveHandler(ByVal value As EventHandler)
            m_startEventHandlerList.Remove(value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
            For Each handler As EventHandler In m_startEventHandlerList
                handler.BeginInvoke(sender, e, Nothing, Nothing)
            Next
        End RaiseEvent
    End Event

End Class