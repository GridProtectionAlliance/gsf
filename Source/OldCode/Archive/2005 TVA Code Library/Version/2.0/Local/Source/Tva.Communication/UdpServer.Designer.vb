'*******************************************************************************************************
'  Tva.Communication.UdpServer.Designer.vb - UDP-based communication server
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
'  07/06/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports Tva.Communication.Common

Partial Class UdpServer

    Inherits Tva.Communication.CommunicationServerBase

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

        m_payloadAware = False
        m_destinationReachabilityCheck = True
        m_udpClients = New Dictionary(Of Guid, StateKeeper(Of System.Net.IPEndPoint))
        MyBase.ConfigurationString = "Port=8888; Clients=255.255.255.255:8888"
        MyBase.Protocol = TransportProtocol.Udp
        MyBase.ReceiveBufferSize = MaximumUdpDatagramSize

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
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
