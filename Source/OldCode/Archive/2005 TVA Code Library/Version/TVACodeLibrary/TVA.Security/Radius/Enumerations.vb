'*******************************************************************************************************
'  TVA.Security.Radius.Enumerations.vb - Enumerations related to RADIUS
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
'  04/11/2008 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Namespace Radius

    ''' <summary>
    ''' Specifies the type of RADIUS packet.
    ''' </summary>
    Public Enum PacketType
        ''' <summary>
        ''' Packet sent to a RADIUS server for verification of credentials.
        ''' </summary>
        AccessRequest = 1
        ''' <summary>
        ''' Packet sent by a RADIUS server when credential verification is successful.
        ''' </summary>
        AccessAccept = 2
        ''' <summary>
        ''' Packet sent by a RADIUS server when credential verification is unsuccessful.
        ''' </summary>
        AccessReject = 3
        ''' <summary>
        ''' Not used. No description available.
        ''' </summary>
        AccountingRequest = 4
        ''' <summary>
        ''' Not used. No description available.
        ''' </summary>
        AccountingResponse = 5
        ''' <summary>
        ''' Not used. No description available. [RFC 2882]
        ''' </summary>
        AccountingStatus = 6
        ''' <summary>
        ''' Not used. No description available. [RFC 2882]
        ''' </summary>
        PasswordRequest = 7
        ''' <summary>
        ''' Not used. No description available. [RFC 2882]
        ''' </summary>
        PasswordAccept = 8
        ''' <summary>
        ''' Not used. No description available. [RFC 2882]
        ''' </summary>
        PasswordReject = 9
        ''' <summary>
        ''' Not used. No description available. [RFC 2882]
        ''' </summary>
        AccountingMessage = 10
        ''' <summary>
        ''' Packet sent by a RADIUS server when further information is needed for credential verification.
        ''' </summary>
        AccessChallenge = 11
        ''' <summary>
        ''' Not used. No description available.
        ''' </summary>
        StatuServer = 12
        ''' <summary>
        ''' Not used. No description available.
        ''' </summary>
        StatusClient = 13
    End Enum

    ''' <summary>
    ''' Specifies the type of RADIUS packet attribute.
    ''' </summary>
    Public Enum AttributeType
        ''' <summary>
        ''' Attribute indicates the name of the user to be authenticated.
        ''' </summary>
        UserName = 1
        ''' <summary>
        ''' Attribute indicates the password of the user to be authenticated.
        ''' </summary>
        UserPassword = 2
        ''' <summary>
        ''' Attribute indicates the response provided by a PPP CHAP user in reponse to the challenge.
        ''' </summary>
        ChapPassword = 3
        ''' <summary>
        ''' Attribute indicates the identifying IP address of the NAS requesting user authentication.
        ''' </summary>
        NasIpAddress = 4
        ''' <summary>
        ''' Attribute indicates the physical port number of the NAS which is authenticating the user.
        ''' </summary>
        NasPort = 5
        ''' <summary>
        ''' Attribute indicates the type of service the user has requested, or the type of service to be provided.
        ''' </summary>
        ServiceType = 6
        ''' <summary>
        ''' Attribute indicates the framing to be used for framed access.
        ''' </summary>
        FramedProtocol = 7
        ''' <summary>
        ''' Attribute indicates the address to be configured for the user.
        ''' </summary>
        FramedIpAddress = 8
        ''' <summary>
        ''' Attribute indicates the IP netmask to be configured for the user when user is a router to a network.
        ''' </summary>
        FramedIpNetmask = 9
        ''' <summary>
        ''' Attribute indicates the routing method for the user when user is a router to a network.
        ''' </summary>
        FramedRouting = 10
        ''' <summary>
        ''' Attribute indicates the name of the filter list for this user.
        ''' </summary>
        FilterId = 11
        ''' <summary>
        ''' Attribute indicates the MTU to be configured for the user when it is not negotiated by some other means.
        ''' </summary>
        FramedMtu = 12
        ''' <summary>
        ''' Attribute indicates a compression protocol to be used for the link.
        ''' </summary>
        FramedCompression = 13
        ''' <summary>
        ''' Attribute indicates the system with which to connect the user when LoginService attribute is included.
        ''' </summary>
        LoginIpHost = 14
        ''' <summary>
        ''' Attribute indicates the service to use to connect the user to the login host.
        ''' </summary>
        LoginService = 15
        ''' <summary>
        ''' Attribute indicates the TCP port with which the user is to be connected when LoginService attribute
        ''' is included.
        ''' </summary>
        LoginTcpPort = 16
        ''' <summary>
        ''' Attribute indicates the text which may be displayed to the user.
        ''' </summary>
        ReplyMessage = 18
        ''' <summary>
        ''' Attribute indicates a dialing string to be used for callback.
        ''' </summary>
        CallbackNumber = 19
        ''' <summary>
        ''' Attribute indicates the name of a place to be called.
        ''' </summary>
        CallbackId = 20
        ''' <summary>
        ''' Attribute provides routing information to be configured for the user on the NAS.
        ''' </summary>
        FramedRoute = 22
        ''' <summary>
        ''' Attribute indicates the IPX Network number to be configured for the user.
        ''' </summary>
        FramedIpxNetwork = 23
        ''' <summary>
        ''' Attribute available to be sent by the server to the client in an AccessChallenge and must be sent
        ''' unmodified from the client to the server in the new AccessRequest reply to the challenge.
        ''' </summary>
        State = 24
        ''' <summary>
        ''' Attribute available to be sent by the server to the client in an AccessAccept and should be sent 
        ''' unmodified by the client to the accounting server as part of the AccountingRequest.
        ''' </summary>
        [Class] = 25
        ''' <summary>
        ''' Attribute available to allow vendors to support their own extended attributes.
        ''' </summary>
        VendorSpecific = 26
        ''' <summary>
        ''' Attribute sets the maximum number of seconds of service to be provided to the user before termination
        ''' of the session or prompt.
        ''' </summary>
        SessionTimeout = 27
        ''' <summary>
        ''' Attribute sets the maximum number of consecutive seconds of idle connection allowed to the user before 
        ''' termination of the session or prompt.
        ''' </summary>
        IdleTimeout = 28
        ''' <summary>
        ''' Attribute indicates the action the NAS should take when the specified service is complete.
        ''' </summary>
        TerminationAction = 29
        ''' <summary>
        ''' Attribute indicates the phone number that the user called using DNIS or similar technology.
        ''' </summary>
        CallerStationId = 30
        ''' <summary>
        ''' Attribute indicates the phone number the call came from using ANI or similar technology.
        ''' </summary>
        CallingStationId = 31
        ''' <summary>
        ''' Attribute indicates a string identifier for the NAS originating the AccessRequest.
        ''' </summary>
        NasIdentifier = 32
        ''' <summary>
        ''' Attribute indicates the state a proxy server forwarding requests to the server.
        ''' </summary>
        ProxyState = 33
        ''' <summary>
        ''' Attribute indicates the system with which the user is to be connected by LAT.
        ''' </summary>
        LoginLatService = 34
        ''' <summary>
        ''' Attribute indicates the Node with which the user is to be automatically connected by LAT.
        ''' </summary>
        LoginLatNode = 35
        ''' <summary>
        ''' Attribute indicates the string identifier for the LAT group codes which the user is authorized to use.
        ''' </summary>
        LoginLatGroup = 36
        ''' <summary>
        ''' Attribute indicates the AppleTalk network number which should be used for the serial link to the user.
        ''' </summary>
        FramedAppleTalkLink = 37
        ''' <summary>
        ''' Attribute indicates the AppleTalk Network number which the NAS should probe to allocate an AppleTalk
        ''' node for the user.
        ''' </summary>
        FramedAppleTalkNetwork = 38
        ''' <summary>
        ''' Attribute indicates the AppleTalk Default Zone to be used for this user.
        ''' </summary>
        FramedAppleTalkZone = 39
        ''' <summary>
        ''' Attribute contains the CHAP Challenge sent by the NAS to a PPP CHAP user.
        ''' </summary>
        ChapChallenge = 60
        ''' <summary>
        ''' Attribute indicates the type of physical port of the NAS which is authenticating the user.
        ''' </summary>
        NasPortType = 61
        ''' <summary>
        ''' Attribute sets the maximum number of ports to be provided to the user by the NAS.
        ''' </summary>
        PortLimit = 62
        ''' <summary>
        ''' Attribute indicates the Port with which the user is to be connected by the LAT.
        ''' </summary>
        LoginLatPort = 63
    End Enum

End Namespace