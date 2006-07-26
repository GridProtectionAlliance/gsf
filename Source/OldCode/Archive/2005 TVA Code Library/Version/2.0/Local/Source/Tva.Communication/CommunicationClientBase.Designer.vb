'*******************************************************************************************************
'  Tva.Communication.ClientBase.Designer.vb - Base functionality of a client for transporting data
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/01/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Partial Class CommunicationClientBase
    Inherits System.ComponentModel.Component

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New(ByVal Container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        Container.Add(Me)

    End Sub

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()
        ' Setup the default values.
        m_connectionString = ""
        m_receiveBufferSize = 8192
        m_receiveTimeout = -1
        m_maximumConnectionAttempts = -1
        m_textEncoding = System.Text.Encoding.ASCII()
        m_secureSession = False
        m_handshake = True
        m_encryption = Tva.Security.Cryptography.EncryptLevel.None
        m_compression = Tva.IO.Compression.CompressLevel.NoCompression
        m_crcCheck = CRCCheckType.None
        m_enabled = True
        m_clientID = Guid.NewGuid()
        m_isConnected = False
        m_connectTime = 0
        m_disconnectTime = 0
        m_totalBytesSent = 0
        m_totalBytesReceived = 0

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            Disconnect()    ' Disconnect client from the server.
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
    End Sub

End Class