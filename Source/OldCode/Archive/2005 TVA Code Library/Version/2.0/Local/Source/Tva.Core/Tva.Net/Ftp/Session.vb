' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing

' This FTP library is based on a similar C# library found on "The Code Project" web site written by
' Uwe Keim of Germany.  It was translated into VB with most of classes being renamed (removed Ftp prefix)
' and the namespace was changed to Tva.Ftp. Many bug fixes, additions and modifications have been made to
' this code as well as extensive testing.  Note worthy changes:  converted the C# delegates to standard
' .NET events for ease of use, made the library work with IIS based FTP servers that were in Unix mode,
' added detailed file system information for FTP files and directories (size, timestamp, etc), coverted
' FTP session into a component that could be dragged onto a design surface, created an FTP FileWatcher
' component and an FTP file system crawler based on this library - JRC
Namespace Net.Ftp

    Friend Interface ISessionState

        Property Server() As String
        Property Port() As Integer
        Property CurrentDirectory() As Directory
        ReadOnly Property RootDirectory() As Directory
        ReadOnly Property ControlChannel() As ControlChannel
        ReadOnly Property IsBusy() As Boolean
        Sub AbortTransfer()
        Sub Connect(ByVal UserName As String, ByVal Password As String)
        Sub Close()

    End Interface

    <ToolboxBitmap(GetType(Session)), DefaultProperty("Server"), DefaultEvent("FileTransferProgress")> _
    Public Class Session

        Inherits Component

        Public Event BeginFileTransfer(ByVal LocalFileName As String, ByVal RemoteFileName As String, ByVal TransferDirection As TransferDirection)
        Public Event EndFileTransfer(ByVal LocalFileName As String, ByVal RemoteFileName As String, ByVal TransferDirection As TransferDirection, ByVal TransferResult As AsyncResult)
        Public Event FileTransferProgress(ByVal TotalBytes As Long, ByVal TotalBytesTransfered As Long, ByVal TransferDirection As TransferDirection)
        Public Event FileTransferNotification(ByVal TransferResult As AsyncResult)
        Public Event ResponseReceived(ByVal Response As String)
        Public Event CommandSent(ByVal Command As String)

        Private m_caseInsensitive As Boolean
        Private m_currentState As ISessionState
        Private m_waitLockTimeOut As Integer

        Public Sub New()

            MyClass.New(False)

        End Sub

        Public Sub New(ByVal CaseInsensitive As Boolean)

            m_caseInsensitive = CaseInsensitive
            m_waitLockTimeOut = 10
            m_currentState = New SessionDisconnected(Me, m_caseInsensitive)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Specify FTP server name (do not prefix with FTP://).")> _
        Public Property Server() As String
            Get
                Return m_currentState.Server
            End Get
            Set(ByVal Value As String)
                m_currentState.Server = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(False)> _
        Public Property CaseInsensitive() As Boolean
            Get
                Return m_caseInsensitive
            End Get
            Set(ByVal Value As Boolean)
                m_caseInsensitive = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify FTP server post if needed."), DefaultValue(21)> _
        Public Property Port() As Integer
            Get
                Return m_currentState.Port
            End Get
            Set(ByVal Value As Integer)
                m_currentState.Port = Value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property CurrentDirectory() As Directory
            Get
                Return m_currentState.CurrentDirectory
            End Get
            Set(ByVal Value As Directory)
                m_currentState.CurrentDirectory = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property RootDirectory() As Directory
            Get
                Return m_currentState.RootDirectory
            End Get
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify the maximum number of seconds to wait for read lock for files to be uploaded."), DefaultValue(10)> _
        Public Property WaitLockTimeout() As Integer
            Get
                Return m_waitLockTimeOut
            End Get
            Set(ByVal Value As Integer)
                m_waitLockTimeOut = Value
            End Set
        End Property

        Public Sub SetCurrentDirectory(ByVal DirectoryPath As String)

            If Not IsConnected() Then Throw New InvalidOperationException("You must be connected to the FTP server before you can set the current directory.")

            If Len(DirectoryPath) > 0 Then
                m_currentState.CurrentDirectory = New Directory(m_currentState, CaseInsensitive, DirectoryPath)
                m_currentState.CurrentDirectory.Refresh()
            End If

        End Sub

        <Browsable(False)> _
        Public ReadOnly Property ControlChannel() As ControlChannel
            Get
                Return m_currentState.ControlChannel
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsConnected() As Boolean
            Get
                Return (TypeOf m_currentState Is SessionConnected)
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsBusy() As Boolean
            Get
                Return m_currentState.IsBusy
            End Get
        End Property

        Public Sub AbortTransfer()

            m_currentState.AbortTransfer()

        End Sub

        Public Sub Connect(ByVal UserName As String, ByVal Password As String)

            m_currentState.Connect(UserName, Password)

        End Sub

        Public Sub Close()

            m_currentState.Close()

        End Sub

        Friend Property State() As ISessionState
            Get
                Return m_currentState
            End Get
            Set(ByVal Value As ISessionState)
                m_currentState = Value
            End Set
        End Property

        Friend Sub RaiseResponse(ByVal response As String)

            RaiseEvent ResponseReceived(response)

        End Sub

        Friend Sub RaiseCommand(ByVal command As String)

            RaiseEvent CommandSent(command)

        End Sub

        Friend Sub RaiseBeginFileTransfer(ByVal LocalFileName As String, ByVal RemoteFileName As String, ByVal TransferDirection As TransferDirection)

            RaiseEvent BeginFileTransfer(LocalFileName, RemoteFileName, TransferDirection)

        End Sub

        Friend Sub RaiseEndFileTransfer(ByVal LocalFileName As String, ByVal RemoteFileName As String, ByVal TransferDirection As TransferDirection, ByVal TransferResult As AsyncResult)

            RaiseEvent EndFileTransfer(LocalFileName, RemoteFileName, TransferDirection, TransferResult)

        End Sub

        Friend Sub RaiseFileTransferProgress(ByVal TotalBytes As Long, ByVal TotalBytesTransfered As Long, ByVal TransferDirection As TransferDirection)

            RaiseEvent FileTransferProgress(TotalBytes, TotalBytesTransfered, TransferDirection)

        End Sub

        Friend Sub RaiseFileTranferNotification(ByVal TransferResult As AsyncResult)

            RaiseEvent FileTransferNotification(TransferResult)

        End Sub

    End Class

End Namespace