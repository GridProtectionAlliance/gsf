Imports System.Data
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports TVA.Text.Common
Imports TVA.TRO.Speech.Common

Namespace Speech

    Public Class SpeechApplication

        Private m_applicationID, m_applicationName, m_applicationOwner, m_phoneNumber, m_extension, m_message As String
        Private m_loopCount As Integer
        Private m_loopCalls, m_logResponses, m_isLongDistance As Boolean
        Private m_connection As SqlConnection

#Region " Properties"

        Public ReadOnly Property ApplicationID() As String
            Get
                Return m_applicationID
            End Get
        End Property

        Public ReadOnly Property ApplicationName() As String
            Get
                Return m_applicationName
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ApplicationOwner() As String
            Get
                Return m_applicationOwner
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property LoopCount() As Integer
            Get
                Return m_loopCount
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property LoopCalls() As Boolean
            Get
                Return m_loopCalls
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property LogResponses() As Boolean
            Get
                Return m_logResponses
            End Get
        End Property

        ''' <summary>
        ''' 10 digit phone number to call.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Description("Provide 10 digit phone number.")> _
        Public Property PhoneNumber() As String
            Get
                Return m_PhoneNumber
            End Get
            Set(ByVal value As String)
                m_phoneNumber = RemoveCharacters(value, AddressOf IsCharacterInvalid)
            End Set
        End Property

        <Description("Provide extension number if any for this phone call."), DefaultValue("")> _
        Public Property Extension() As String
            Get
                Return m_extension
            End Get
            Set(ByVal value As String)
                m_extension = value
            End Set
        End Property

        ''' <summary>
        ''' Message to be played.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Description("Message to be played to the recipient.")> _
        Public Property Message() As String
            Get
                Return m_message
            End Get
            Set(ByVal value As String)
                m_message = value
            End Set
        End Property

#End Region

#Region " Constructors"
        ''' <summary>
        ''' Initialize SpeechApplication Object
        ''' </summary>
        ''' <param name="ApplicationID">Four Characters Application ID</param>
        Public Sub New(ByVal ApplicationID As String)

            MyClass.New(ApplicationID, Environment.Development)

        End Sub

        Public Sub New(ByVal ApplicationID As String, ByVal EnvironmentType As Environment)
            m_connection = New SqlConnection(GetConnectionString(EnvironmentType))
            Try
                m_extension = String.Empty
                m_message = String.Empty
                m_phoneNumber = String.Empty
                If m_connection IsNot Nothing AndAlso m_connection.State <> ConnectionState.Open Then
                    m_connection.Open()
                End If
                Dim appInfoRow As DataRow = RetrieveRow("Select * From Applications Where ID = '" & ApplicationID & "'", m_connection)
                If appInfoRow IsNot Nothing Then
                    m_applicationID = ApplicationID
                    m_applicationName = appInfoRow("ApplicationName").ToString()
                    m_applicationOwner = appInfoRow("ApplicationOwner").ToString()
                    m_loopCalls = Convert.ToBoolean(appInfoRow("LoopCalls"))
                    m_loopCount = Convert.ToInt32(appInfoRow("LoopCount"))
                    m_logResponses = Convert.ToBoolean(appInfoRow("LogResponses"))
                Else
                    Throw New ArgumentException("Application does not exist in the database.")
                End If
            Catch
                Throw
            Finally
                If m_connection.State <> ConnectionState.Closed Then
                    m_connection.Close()
                End If
            End Try
        End Sub

#End Region

#Region " Private Methods"

        Private Function IsLongDistance(ByVal PhoneNumber As String) As Boolean
            
            If m_connection IsNot Nothing AndAlso m_connection.State <> ConnectionState.Open Then
                m_connection.Open()
            End If
            Dim areaCode As String = PhoneNumber.Substring(0, 3)
            Dim exchange As String = PhoneNumber.Substring(3, 3)

            Return Not Convert.ToBoolean(ExecuteScalar("Select Count(*) From ChattanoogaLocalPhones Where AreaCode = '" & areaCode & "' AND Exchange = '" & exchange & "'", m_connection))
            
        End Function

        ''' <summary>
        ''' Test the specified character to see if it is one of the invalid (non-digit) characters found 
        ''' in phone numbers.
        ''' </summary>
        Private Function IsCharacterInvalid(ByVal character As Char) As Boolean

            Return (character = " "c OrElse character = "("c OrElse character = ")"c OrElse character = "-"c)

        End Function

#End Region

#Region " Public Methods"

        Public Function MakeCall() As Integer
            Dim eventId As Integer = 0
            Try
                If m_connection IsNot Nothing AndAlso m_connection.State <> ConnectionState.Open Then
                    m_connection.Open()
                End If
                If String.IsNullOrEmpty(m_message) Then Throw New ArgumentException("Message property cannot be null or empty.")
                If String.IsNullOrEmpty(m_PhoneNumber) Then Throw New ArgumentException("PhoneNumber property cannot be null or empty.")
                If m_PhoneNumber.Length <> 10 Then Throw New ArgumentException("PhoneNumber must be 10 digits.")
                If IsLongDistance(m_PhoneNumber) Then m_PhoneNumber = "1" & m_PhoneNumber

                eventId = Convert.ToInt32(ExecuteScalar("CreateEvent", m_connection, m_applicationID, m_phoneNumber, m_extension, m_message))
            Catch
                Throw
            Finally
                If m_connection.State <> ConnectionState.Closed Then
                    m_connection.Close()
                End If
            End Try
            Return eventId
        End Function

#End Region

    End Class

End Namespace
