Public Class DataPoints

    Private m_list As New ArrayList

    Public Sub Add(ByVal value As Double)

        m_list.Add(value)
        m_list.Sort()

    End Sub

    Public Sub Remove(ByVal value As Double)

        m_list.Remove(value)

    End Sub

    Public Sub Clear()

        m_list.Clear()

    End Sub

    Public ReadOnly Property Minimum(Optional ByVal offset As Double = 0) As Double
        Get
            If m_list.Count > 0 Then
                Return m_list(0) + offset
            End If
        End Get
    End Property

    Public ReadOnly Property Maximum(Optional ByVal offset As Double = 0) As Double
        Get
            If m_list.Count > 0 Then
                Return m_list(m_list.Count - 1) + offset
            End If
        End Get
    End Property

End Class
