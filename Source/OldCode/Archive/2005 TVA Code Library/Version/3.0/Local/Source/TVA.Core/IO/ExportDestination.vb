'*******************************************************************************************************
'  ExportDestination.vb - Defines an Export Destination - used by MultipleDestinationExporter
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/13/2008 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO

Namespace IO

    ''' <summary>
    ''' Export destination.
    ''' </summary>
    Public Structure ExportDestination

        ''' <summary>
        ''' Path and file name of export destination.
        ''' </summary>
        Public DestinationFile As String

        ''' <summary>
        ''' Determines whether or not to attempt network connection to share specified in DestinationFile.
        ''' </summary>
        Public ConnectToShare As Boolean

        ''' <summary>
        ''' Domain used to authenticate network connection if ConnectToShare is True.
        ''' </summary>
        Public Domain As String

        ''' <summary>
        ''' User name used to authenticate network connection if ConnectToShare is True.
        ''' </summary>
        Public UserName As String

        ''' <summary>
        ''' Password used to authenticate network connection if ConnectToShare is True.
        ''' </summary>
        Public Password As String

        Public Sub New(ByVal destinationFile As String, ByVal connectToShare As Boolean, ByVal domain As String, ByVal userName As String, ByVal password As String)

            Me.DestinationFile = destinationFile
            Me.ConnectToShare = connectToShare
            Me.Domain = domain
            Me.UserName = userName
            Me.Password = password

        End Sub

        ''' <summary>
        ''' Path root of DestinationFile (e.g., E:\ or \\server\share)
        ''' </summary>
        Public ReadOnly Property Share() As String
            Get
                Return Path.GetPathRoot(DestinationFile)
            End Get
        End Property

        ''' <summary>
        ''' Path and filename of DestinationFile
        ''' </summary>
        Public ReadOnly Property FileName() As String
            Get
                Return DestinationFile.Substring(Share.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String

            Return DestinationFile

        End Function

    End Structure

End Namespace
