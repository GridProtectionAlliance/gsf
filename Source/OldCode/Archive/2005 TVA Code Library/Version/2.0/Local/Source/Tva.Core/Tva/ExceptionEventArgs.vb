' 11/17/2006

Public Class ExceptionEventArgs
    Inherits EventArgs

    Private m_retries As Integer
    Private m_exception As Exception

    ''' <summary>
    ''' Initializes a new instance of the ExceptionEventArgs class.
    ''' </summary>
    ''' <param name="exception">The exception that was encountered.</param>
    Public Sub New(ByVal exception As Exception)

        MyClass.New(exception, 0)

    End Sub

    ''' <summary>
    ''' Initializes a new instance of the ExceptionEventArgs class.
    ''' </summary>
    ''' <param name="exception">The exception that was encountered.</param>
    ''' <param name="retries">The number of retries that were performed as a result of the encountered exception.</param>
    Public Sub New(ByVal exception As Exception, ByVal retries As Integer)

        MyBase.New()
        Me.Retries = retries
        Me.Exception = exception

    End Sub

    ''' <summary>
    ''' Gets or sets the number of retries if any that were performed as a result of the encountered exception.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The number of retries that were performed as a result of the encountered exception.</returns>
    Public Property Retries() As Integer
        Get
            Return m_retries
        End Get
        Set(ByVal value As Integer)
            m_retries = value
        End Set
    End Property

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

End Class
