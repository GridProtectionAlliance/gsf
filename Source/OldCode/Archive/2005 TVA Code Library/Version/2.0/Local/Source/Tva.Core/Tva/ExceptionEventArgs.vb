' 11/17/2006

Public Class ExceptionEventArgs
    Inherits EventArgs

    Private m_exception As Exception
    Private m_occurrenceCount As Integer

    ''' <summary>
    ''' Initializes a new instance of the Tva.ExceptionEventArgs class.
    ''' </summary>
    ''' <param name="exception">The exception that was encountered.</param>
    Public Sub New(ByVal exception As Exception)

        MyClass.New(exception, 1)

    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Tva.ExceptionEventArgs class.
    ''' </summary>
    ''' <param name="exception">The exception that was encountered.</param>
    ''' <param name="occurrenceCount">The number of time the exception has been encountered.</param>
    Public Sub New(ByVal exception As Exception, ByVal occurrenceCount As Integer)

        MyBase.New()
        m_exception = exception
        m_occurrenceCount = occurrenceCount

    End Sub

    ''' <summary>
    ''' Gets or sets the exception that was encountered.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The exception that was encountered.</returns>
    Public Property Exception() As Exception
        Get
            Return m_exception
        End Get
        Set(ByVal value As Exception)
            m_exception = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the number of time the exception has been encountered.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The number of time the exception has been encountered.</returns>
    Public Property OccurrenceCount() As Integer
        Get
            Return m_occurrenceCount
        End Get
        Set(ByVal value As Integer)
            m_occurrenceCount = value
        End Set
    End Property

End Class
