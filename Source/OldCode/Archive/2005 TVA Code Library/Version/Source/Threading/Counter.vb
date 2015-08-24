Namespace Threading

    Public Class Counter

        Private m_Lock As New Object
        Private m_Count As Integer

        Public Property Count() As Integer
            Get
                SyncLock m_Lock
                    Return m_Count
                End SyncLock
            End Get
            Set(ByVal Value As Integer)
                SyncLock m_Lock
                    m_Count = Value
                End SyncLock
            End Set
        End Property

        Public Sub Increment(Optional ByVal Amount As Integer = 1)

            SyncLock m_Lock
                m_Count += Amount
            End SyncLock

        End Sub

        Public Sub Decrement(Optional ByVal Amount As Integer = 1)

            Increment(-Amount)

        End Sub

    End Class

End Namespace