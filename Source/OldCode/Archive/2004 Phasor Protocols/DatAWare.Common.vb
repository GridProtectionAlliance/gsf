'***********************************************************************
'  DatAWare.Common.vb - Common DatAWare functions for .NET
'  Copyright © 2004 - TVA, all rights reserved
'  
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source created
'
'  TODO: Flesh out code for more general DatAWare IO and add to the
'        TVA .NET code library as an independent assembly
'
'***********************************************************************

Imports TVA.Shared.DateTime
Imports System.Text

Namespace DatAWare

#Region " DatAWare Enumerations "

    Public Enum AccessMode
        [ReadOnly] = 1
        [WriteOnly] = 2
        [ReadWrite] = 3
    End Enum

    Public Enum Quality
        Unknown
        DeletedFromProcessing
        CouldNotCalcPoint
        DASFrontEndHardwareError
        SensorReadError
        OpenTransducerDetection
        InputCountsOutOfSensorRange
        UnreasonableHigh
        UnreasonableLow
        Old
        SuspectValueAboveHIHILimit
        SuspectValueBelowLOLOLimit
        SuspectValueAboveHILimit
        SuspectValueBelowLOLimit
        SuspectData
        DigitalSuspectAlarm
        InsertedValueAboveHIHILimit
        InsertedValueBelowLOLOLimit
        InsertedValueAboveHILimit
        InsertedValueBelowLOLimit
        InsertedValue
        DigitalInsertedStatusInAlarm
        LogicalAlarm
        ValueAboveHIHIAlarm
        ValueBelowLOLOAlarm
        ValueAboveHIAlarm
        ValueBelowLOAlarm
        DeletedFromAlarmChecks
        InhibitedByCutoutPoint
        Good
    End Enum

    Public Enum ReturnStatus
        Normal
        Abnormal
        Timeout
        NotConnected
    End Enum

    Public Enum RequestType
        Raw = 1
        Interpolated = 3
        Averaged = 4
    End Enum

#End Region

#Region " DatAWare TimeTag Class "

    Public Class TimeTag

        Implements IComparable

        ' DatAWare time tags are measured as the number of seconds since January 1, 1995,
        ' so we calculate this date to get offset in ticks for later conversion...
        Private Shared timeTagOffsetTicks As Long = (New DateTime(1995, 1, 1, 0, 0, 0)).Ticks

        Private ttag As Double

        Public Sub New(ByVal ttag As Double)

            Value = ttag

        End Sub

        Public Sub New(ByVal dtm As DateTime)

            ' Zero base 100-nanosecond ticks from 1/1/1995 and convert to seconds
            Value = (dtm.Ticks - timeTagOffsetTicks) / 10000000L

        End Sub

        Public Property Value() As Double
            Get
                Return ttag
            End Get
            Set(ByVal val As Double)
                ttag = val
                If ttag < 0 Then ttag = 0
            End Set
        End Property

        Public Function ToDateTime() As DateTime

            ' Convert time tag seconds to 100-nanosecond ticks and add the 1/1/1995 offset
            Return New DateTime(ttag * 10000000L + timeTagOffsetTicks)

        End Function

        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

        ' TimeTags are sorted in value order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is TimeTag Then
                Return ttag.CompareTo(DirectCast(obj, TimeTag).Value)
            ElseIf TypeOf obj Is Double Then
                Return ttag.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("TimeTag can only be compared with other TimeTags...")
            End If

        End Function

    End Class

#End Region

#Region " DatAWare Connection Class "

    Public Class Connection

        Implements IDisposable

        Private WithEvents m_dwAPI As DWAPI.DWAPIdllClass
        Private m_points As Points
        Private m_server As String
        Private m_plantCode As String
        Private m_access As AccessMode
        Private m_timeZone As Win32TimeZone
        Private m_connected As Boolean

        Public Event ServerMessage(ByVal message As String)
        Public Event ServerDebugMessage(ByVal message As String)

        Public Sub New(ByVal server As String, ByVal plantCode As String, Optional ByVal timeZone As String = "Central Standard Time", Optional ByVal access As AccessMode = AccessMode.ReadWrite)

            m_dwAPI = New DWApi.DWAPIdllClass
            m_points = New Points(Me)
            m_server = server
            m_plantCode = plantCode
            m_access = access
            m_timeZone = GetWin32TimeZone(timeZone)

        End Sub

        Protected Overrides Sub Finalize()

            Close()

        End Sub

        Public ReadOnly Property DWAPI() As DWApi.DWAPIdllClass
            Get
                Return m_dwAPI
            End Get
        End Property

        Public ReadOnly Property Points() As Points
            Get
                Return m_points
            End Get
        End Property

        Public ReadOnly Property Server() As String
            Get
                Return m_server
            End Get
        End Property

        Public ReadOnly Property PlantCode() As String
            Get
                Return m_plantCode
            End Get
        End Property

        Public ReadOnly Property Access() As AccessMode
            Get
                Return m_access
            End Get
        End Property

        Public ReadOnly Property TimeZone() As Win32TimeZone
            Get
                Return m_timeZone
            End Get
        End Property

        Public Sub Open()

            Dim errorMessage As String

            ' Open using integrated NT authentication
            m_dwAPI.ConnectTo(m_server, m_plantCode, m_access, errorMessage)

            If Len(errorMessage) > 0 Then
                Throw New InvalidOperationException("Failed to connect to DatAWare server """ & m_server & """ due to exception: " & errorMessage)
            Else
                m_connected = True
            End If

        End Sub

        Public Sub Open(ByVal userName As String, ByVal password As String)

            Dim errorMessage As String

            ' Open using specific username and password
            m_dwAPI.ConnectTo(m_server, m_plantCode, m_access, errorMessage, userName & "/" & password)

            If Len(errorMessage) > 0 Then
                Throw New InvalidOperationException("Failed to connect to DatAWare server """ & m_server & """ due to exception: " & errorMessage)
            Else
                m_connected = True
            End If

        End Sub

        Public Sub Close() Implements IDisposable.Dispose

            GC.SuppressFinalize(Me)
            If Not m_dwAPI Is Nothing Then m_dwAPI.Disconnect(m_plantCode)
            m_connected = False

        End Sub

        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_connected
            End Get
        End Property

        Private Sub m_dwAPI_NormalMessage(ByRef mMsg As String) Handles m_dwAPI.GotMessage

            RaiseEvent ServerMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & m_server & """ Message: " & mMsg)
#End If

        End Sub

        Private Sub m_dwAPI_DebugMessage(ByRef mMsg As String) Handles m_dwAPI.DEBUGMESSAGE

            RaiseEvent ServerDebugMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & m_server & """ Debug Message: " & mMsg)
#End If

        End Sub

    End Class

    Public Class Points

        Private m_connection As Connection

        Friend Sub New(ByVal connection As Connection)

            m_connection = connection

        End Sub

        Public ReadOnly Property Count() As Integer
            Get
                VerifyOpenConnection()

                Dim pointCount As Integer
                Dim errorMessage As String

                With m_connection
                    .DWAPI.GetDBCount(.PlantCode, pointCount, errorMessage)

                    If Len(errorMessage) > 0 Then
                        Throw New InvalidOperationException("Failed to retrieve point count from DatAWare server """ & .Server & """ due to exception: " & errorMessage)
                    End If
                End With

                Return pointCount
            End Get
        End Property

        Public ReadOnly Property Definition(ByVal databaseIndex As Integer) As DatabaseStructure
            Get
                VerifyOpenConnection()

                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), DatabaseStructure.BinaryLength)
                Dim eof As Boolean
                Dim errorMessage As String

                With m_connection
                    .DWAPI.GetDBData(.PlantCode, databaseIndex, buffer, eof, errorMessage)

                    If eof Then
                        Throw New InvalidOperationException("Attempt to access database index passed end of file on DatAWare server """ & .Server & """")
                    ElseIf Len(errorMessage) > 0 Then
                        Throw New InvalidOperationException("Failed to retrieve point definition from DatAWare server """ & .Server & """ due to exception: " & errorMessage)
                    End If
                End With

                Return New DatabaseStructure(databaseIndex, buffer, 0)
            End Get
        End Property

        Default Public ReadOnly Property Value(ByVal databaseIndex As Integer, Optional ByVal timeRequest As String = "*", Optional ByVal timeInterval As Single = 0) As StandardEvent
            Get
                VerifyOpenConnection()

                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), ProcessEvent.BinaryLength)
                Dim errorMessage As String

                With m_connection
                    .DWAPI.ReadEvent(.PlantCode, databaseIndex, timeRequest, timeInterval, buffer, errorMessage)

                    If Len(errorMessage) > 0 Then
                        Throw New InvalidOperationException("Failed to retrieve point value from DatAWare server """ & .Server & """ due to exception: " & errorMessage)
                    End If
                End With

                Return New StandardEvent(databaseIndex, New ProcessEvent(buffer, 0))
            End Get
        End Property

        Public ReadOnly Property ValueRange(ByVal databaseIndex As Integer, ByVal startTimeRequest As String, Optional ByVal endTimeRequest As String = "*", Optional ByVal requestCommand As RequestType = RequestType.Raw, Optional ByVal timeInterval As Single = 0) As StandardEvent()
            Get
                VerifyOpenConnection()

                Dim events As StandardEvent()
                Dim buffer As Byte()
                Dim eventCount As Integer
                Dim errorMessage As String
                Dim index As Integer

                With m_connection
                    .DWAPI.ReadRange(.PlantCode, databaseIndex, startTimeRequest, endTimeRequest, buffer, requestCommand, timeInterval, eventCount, errorMessage)

                    If eventCount > 0 Then
                        events = Array.CreateInstance(GetType(StandardEvent), eventCount)

                        For x As Integer = 0 To eventCount - 1
                            events(x) = New StandardEvent(databaseIndex, New ProcessEvent(buffer, index))
                            index += ProcessEvent.BinaryLength
                        Next
                    End If
                End With

                Return events
            End Get
        End Property

        Private Sub VerifyOpenConnection()

            If Not m_connection.IsOpen Then Throw New InvalidOperationException("Operation unavailable when connection is closed.")

        End Sub

    End Class

#End Region

#Region " DatAWare Data Structures "

    ' Standard database structure element
    Public Class DatabaseStructure

        Implements IComparable

        Public SourceID As Short                    ' 2
        Public Unit As Short                        ' 2
        Public SecurityLevel As Short               ' 2
        Public CompressionMinimumTime As Integer    ' 4
        Public CompressionMaximumTime As Integer    ' 4
        Public Spares As Byte()                     ' 64
        Public FlagWord As Integer                  ' 4
        Public TransitionFlag As Integer            ' 4
        Public ScanRate As Single                   ' 4
        Public BinaryInfo As Byte()                 ' 256
        Public TextEncoding As Encoding

        Private m_index As Integer
        Private m_description As String = ""        ' 40
        Private m_hardWareInfo As String = ""       ' 64
        Private m_pointID As String = ""            ' 20
        Private m_synonym1 As String = ""           ' 20
        Private m_synonym2 As String = ""           ' 20
        Private m_siteName As String = ""           ' 2
        Private m_system As String = ""             ' 4
        Private m_email As String = ""              ' 50
        Private m_pager As String = ""              ' 30
        Private m_phone As String = ""              ' 30
        Private m_remarks As String = ""            ' 128

        Public Const BinaryLength As Integer = 754

        Public Sub New(ByVal index As Integer)

            m_index = index
            TextEncoding = Encoding.Default ' By default we decode strings using encoding for the system's current ANSI code page
            Spares = Array.CreateInstance(GetType(Byte), 64)
            BinaryInfo = Array.CreateInstance(GetType(Byte), 256)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer, Optional ByVal encoding As Encoding = Nothing)

            Me.New(index)

            If Not encoding Is Nothing Then TextEncoding = encoding

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.DatabaseStructure")
            ElseIf binaryImage.Length - startIndex < BinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.DatabaseStructure")
            Else
                m_description = Trim(TextEncoding.GetString(binaryImage, startIndex, 40))
                Unit = BitConverter.ToInt16(binaryImage, startIndex + 40)
                SecurityLevel = BitConverter.ToInt16(binaryImage, startIndex + 42)
                m_hardWareInfo = Trim(TextEncoding.GetString(binaryImage, startIndex + 44, 64))
                Array.Copy(binaryImage, startIndex + 108, Spares, 0, 64)
                FlagWord = BitConverter.ToInt32(binaryImage, startIndex + 172)
                TransitionFlag = BitConverter.ToInt32(binaryImage, startIndex + 176)
                ScanRate = BitConverter.ToSingle(binaryImage, startIndex + 180)
                m_pointID = Trim(TextEncoding.GetString(binaryImage, startIndex + 184, 20))
                m_synonym1 = Trim(TextEncoding.GetString(binaryImage, startIndex + 204, 20))
                m_synonym2 = Trim(TextEncoding.GetString(binaryImage, startIndex + 224, 20))
                m_siteName = Trim(TextEncoding.GetString(binaryImage, startIndex + 244, 2))
                SourceID = BitConverter.ToInt16(binaryImage, startIndex + 246)
                CompressionMinimumTime = BitConverter.ToInt32(binaryImage, startIndex + 248)
                CompressionMaximumTime = BitConverter.ToInt32(binaryImage, startIndex + 252)
                m_system = Trim(TextEncoding.GetString(binaryImage, startIndex + 256, 4))
                m_email = Trim(TextEncoding.GetString(binaryImage, startIndex + 260, 50))
                m_pager = Trim(TextEncoding.GetString(binaryImage, startIndex + 310, 30))
                m_phone = Trim(TextEncoding.GetString(binaryImage, startIndex + 340, 30))
                m_remarks = Trim(TextEncoding.GetString(binaryImage, startIndex + 370, 128))
                Array.Copy(binaryImage, startIndex + 498, BinaryInfo, 0, 256)
            End If

        End Sub

        Public Shared Function Clone(ByVal pointDefinition As DatabaseStructure, ByVal newIndex As Integer) As DatabaseStructure

            Dim newPointDefinition As New DatabaseStructure(newIndex)

            With newPointDefinition
                .Description = pointDefinition.Description
                .Unit = pointDefinition.Unit
                .SecurityLevel = pointDefinition.SecurityLevel
                .HardwareInfo = pointDefinition.HardwareInfo
                Array.Copy(pointDefinition.Spares, .Spares, 64)
                .FlagWord = pointDefinition.FlagWord
                .TransitionFlag = pointDefinition.TransitionFlag
                .ScanRate = pointDefinition.ScanRate
                .PointID = pointDefinition.PointID
                .Synonym1 = pointDefinition.Synonym1
                .Synonym2 = pointDefinition.Synonym2
                .SiteName = pointDefinition.SiteName
                .SourceID = pointDefinition.SourceID
                .CompressionMinimumTime = pointDefinition.CompressionMinimumTime
                .CompressionMaximumTime = pointDefinition.CompressionMaximumTime
                .System = pointDefinition.System
                .Email = pointDefinition.Email
                .Pager = pointDefinition.Pager
                .Phone = pointDefinition.Phone
                .Remarks = pointDefinition.Remarks
                Array.Copy(pointDefinition.BinaryInfo, .BinaryInfo, 256)
                .TextEncoding = pointDefinition.TextEncoding
            End With

        End Function

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

        Public Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal Value As String)
                m_description = Value.Substring(0, 40)
            End Set
        End Property

        Public Property HardwareInfo() As String
            Get
                Return m_hardWareInfo
            End Get
            Set(ByVal Value As String)
                m_hardWareInfo = Value.Substring(0, 64)
            End Set
        End Property

        Public Property PointID() As String
            Get
                Return m_pointID
            End Get
            Set(ByVal Value As String)
                m_pointID = Value.Substring(0, 20)
            End Set
        End Property

        Public Property Synonym1() As String
            Get
                Return m_synonym1
            End Get
            Set(ByVal Value As String)
                m_synonym1 = Value.Substring(0, 20)
            End Set
        End Property

        Public Property Synonym2() As String
            Get
                Return m_synonym2
            End Get
            Set(ByVal Value As String)
                m_synonym2 = Value.Substring(0, 20)
            End Set
        End Property

        Public Property SiteName() As String
            Get
                Return m_siteName
            End Get
            Set(ByVal Value As String)
                m_siteName = Value.Substring(0, 2)
            End Set
        End Property

        Public Property System() As String
            Get
                Return m_system
            End Get
            Set(ByVal Value As String)
                m_system = Value.Substring(0, 4)
            End Set
        End Property

        Public Property Email() As String
            Get
                Return m_email
            End Get
            Set(ByVal Value As String)
                m_email = Value.Substring(0, 50)
            End Set
        End Property

        Public Property Pager() As String
            Get
                Return m_pager
            End Get
            Set(ByVal Value As String)
                m_pager = Value.Substring(0, 30)
            End Set
        End Property

        Public Property Phone() As String
            Get
                Return m_phone
            End Get
            Set(ByVal Value As String)
                m_phone = Value.Substring(0, 30)
            End Set
        End Property

        Public Property Remarks() As String
            Get
                Return m_remarks
            End Get
            Set(ByVal Value As String)
                m_remarks = Value.Substring(0, 128)
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                ' Construct the binary IP buffer for this event
                Array.Copy(TextEncoding.GetBytes(m_description.PadRight(40)), 0, buffer, 0, 40)
                Array.Copy(BitConverter.GetBytes(Unit), 0, buffer, 40, 2)
                Array.Copy(BitConverter.GetBytes(SecurityLevel), 0, buffer, 42, 2)
                Array.Copy(TextEncoding.GetBytes(m_hardWareInfo.PadRight(64)), 0, buffer, 44, 64)
                Array.Copy(Spares, 0, buffer, 108, 64)
                Array.Copy(BitConverter.GetBytes(FlagWord), 0, buffer, 172, 4)
                Array.Copy(BitConverter.GetBytes(TransitionFlag), 0, buffer, 176, 4)
                Array.Copy(BitConverter.GetBytes(ScanRate), 0, buffer, 180, 4)
                Array.Copy(TextEncoding.GetBytes(m_pointID.PadRight(20)), 0, buffer, 184, 20)
                Array.Copy(TextEncoding.GetBytes(m_synonym1.PadRight(20)), 0, buffer, 204, 20)
                Array.Copy(TextEncoding.GetBytes(m_synonym2.PadRight(20)), 0, buffer, 224, 20)
                Array.Copy(TextEncoding.GetBytes(m_siteName.PadRight(2)), 0, buffer, 244, 2)
                Array.Copy(BitConverter.GetBytes(SourceID), 0, buffer, 246, 2)
                Array.Copy(BitConverter.GetBytes(CompressionMinimumTime), 0, buffer, 248, 4)
                Array.Copy(BitConverter.GetBytes(CompressionMaximumTime), 0, buffer, 252, 4)
                Array.Copy(TextEncoding.GetBytes(m_system.PadRight(4)), 0, buffer, 256, 4)
                Array.Copy(TextEncoding.GetBytes(m_email.PadRight(50)), 0, buffer, 260, 50)
                Array.Copy(TextEncoding.GetBytes(m_pager.PadRight(30)), 0, buffer, 310, 30)
                Array.Copy(TextEncoding.GetBytes(m_phone.PadRight(30)), 0, buffer, 340, 30)
                Array.Copy(TextEncoding.GetBytes(m_remarks.PadRight(128)), 0, buffer, 370, 128)
                Array.Copy(BinaryInfo, 0, buffer, 498, 256)

                Return buffer
            End Get
        End Property

        ' We sort database structures in index order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is DatabaseStructure Then
                Return m_index.CompareTo(DirectCast(obj, DatabaseStructure).Index)
            Else
                Throw New ArgumentException("DatabaseStructure can only be compared with other DatabaseStructures")
            End If

        End Function
    End Class

    ' This is the most basic form of a point of data in DatAWare - used by ReadEvent and ReadRange
    Public Class ProcessEvent

        Implements IComparable

        Public TTag As TimeTag
        Public QualityBits As Integer
        Public Value As Single

        ' ***************************************
        ' *  Bit usage of IntEvent.Quality word
        ' *
        ' *    Bits 0-4 (data quality indicator, a number between 0 and 31, 5 bits.
        ' *              Maps to the same qualities as used by PMS process computer.)
        ' *
        ' *             mask = &H1F
        ' *
        ' *    Bits 5-10 (index of time-zone used, number between 0 and 51, 6 bits)
        ' *
        ' *             mask = &H7E0
        ' *
        ' *    Bit 11 (Flag for Daylight Savings Time, one bit.  When set, indicates
        ' *            DST is in effect.  When clear, Standard Time.
        ' *
        ' *             mask = &H800
        ' *
        ' ***************************************

        Public Const BinaryLength As Integer = 16
        Private Const QualityMask As Integer = &H1F

        Public Sub New(ByVal ttag As TimeTag, ByVal value As Single, Optional ByVal qual As DatAWare.Quality = DatAWare.Quality.Good)

            Me.TTag = ttag
            Me.Value = value
            QualityBits = -1    ' A quality set to -1 tells Archiver to perform limit checking
            Quality = qual

        End Sub

        Public Sub New(ByVal timestamp As DateTime, ByVal value As Single, Optional ByVal valueQuality As DatAWare.Quality = DatAWare.Quality.Good)

            Me.New(New TimeTag(timestamp), value, valueQuality)

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.ProcessEvent")
            ElseIf binaryImage.Length - startIndex < BinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.ProcessEvent")
            Else
                Me.TTag = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex))
                Me.QualityBits = BitConverter.ToInt32(binaryImage, startIndex + 8)
                Me.Value = BitConverter.ToSingle(binaryImage, startIndex + 12)
            End If

        End Sub

        Public Property Quality() As DatAWare.Quality
            Get
                Return (QualityBits And QualityMask)
            End Get
            Set(ByVal Value As DatAWare.Quality)
                QualityBits = (QualityBits Or Value)
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                ' Construct the binary IP buffer for this event
                Array.Copy(BitConverter.GetBytes(TTag.Value), 0, buffer, 0, 8)
                Array.Copy(BitConverter.GetBytes(QualityBits), 0, buffer, 8, 4)
                Array.Copy(BitConverter.GetBytes(Value), 0, buffer, 12, 4)

                Return buffer
            End Get
        End Property

        ' Process events are sorted in TimeTag order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is ProcessEvent Then
                Return TTag.CompareTo(DirectCast(obj, ProcessEvent).TTag)
            Else
                Throw New ArgumentException("ProcessEvent can only be compared with other ProcessEvents")
            End If

        End Function

    End Class

    ' Standard-format data event - used in DatAWare IP packets and DAQ plug-in's
    Public Class StandardEvent

        Implements IComparable

        Public DatabaseIndex As Integer
        Public [Event] As ProcessEvent

        Public Const BinaryLength As Integer = ProcessEvent.BinaryLength + 6

        Private Const PacketTypeID As Short = 1 ' We use the standard binary IP buffer format for efficiency
        Private Shared PacketType As Byte()

        Shared Sub New()

            ' We pre-load the byte array for the packet format type - we do this at a shared instance level
            ' since this will be the same for all events
            PacketType = BitConverter.GetBytes(PacketTypeID)

        End Sub

        Public Sub New(ByVal databaseIndex As Integer, ByVal [event] As ProcessEvent)

            Me.DatabaseIndex = databaseIndex
            Me.Event = [event]

        End Sub

        Public Sub New(ByVal databaseIndex As Integer, ByVal ttag As TimeTag, ByVal value As Single, Optional ByVal qual As DatAWare.Quality = DatAWare.Quality.Good)

            Me.New(databaseIndex, New ProcessEvent(ttag, value, qual))

        End Sub

        Public Sub New(ByVal databaseIndex As Integer, ByVal timestamp As DateTime, ByVal value As Single, Optional ByVal valueQuality As DatAWare.Quality = DatAWare.Quality.Good)

            Me.New(databaseIndex, New TimeTag(timestamp), value, valueQuality)

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.StandardEvent")
            ElseIf binaryImage.Length - startIndex < BinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.StandardEvent")
            Else
                Dim packetType As Integer = BitConverter.ToInt16(binaryImage, startIndex)

                If packetType <> PacketTypeID Then
                    Throw New ArgumentException("Unexcepted binaryImage packet ID type " & packetType & ", expected " & PacketTypeID & " - could not create DatAWare.StandardEvent")
                Else
                    Me.DatabaseIndex = BitConverter.ToInt32(binaryImage, startIndex + 2)
                    Me.Event = New ProcessEvent(binaryImage, startIndex + 6)
                End If
            End If

        End Sub

        ' For convience, we directly expose the relevant process event properties
        Public Property TTag() As TimeTag
            Get
                Return [Event].TTag
            End Get
            Set(ByVal Value As TimeTag)
                [Event].TTag = Value
            End Set
        End Property

        Public Property Quality() As DatAWare.Quality
            Get
                Return [Event].Quality
            End Get
            Set(ByVal Value As DatAWare.Quality)
                [Event].Quality = Value
            End Set
        End Property

        Public Property Value() As Single
            Get
                Return [Event].Value
            End Get
            Set(ByVal Value As Single)
                [Event].Value = Value
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                ' Construct the binary IP buffer for this event
                Array.Copy(PacketType, 0, buffer, 0, 2)
                Array.Copy(BitConverter.GetBytes(DatabaseIndex), 0, buffer, 2, 4)
                Array.Copy([Event].BinaryImage, 0, buffer, 6, ProcessEvent.BinaryLength)

                Return buffer
            End Get
        End Property

        ' StandardEvents are sorted in TimeTag order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is StandardEvent Then
                Return TTag.CompareTo(DirectCast(obj, StandardEvent).TTag)
            Else
                Throw New ArgumentException("StandardEvent can only be compared with other StandardEvents")
            End If

        End Function

    End Class

#End Region

#Region " DatAWare Archive Unit "

    ' All points coming in for a given timeframe can be captured as a single data unit
    ' for easy push into DatAWare with this class
    ' Note: This class expects a timestamp in UTC
    Public Class ArchiveUnit

        ' For the sake of efficiency in posting to DatAWare, we break down the points for this data unit into blocks...
        Public Class UnitBlock

            Public Channels As Integer()
            Public Values As Single()
            Public TTags As Double()
            Public Quals As Quality()
            Public Processed As Boolean

        End Class

        Public Connection As Connection
        Public TTag As TimeTag
        Public PlantCode As String
        Public PointCount As Integer
        Public Blocks As UnitBlock()
        Public PostAttempts As Integer

        Public Const BlockSize As Integer = 50

        Public Sub New(ByVal connection As Connection, ByVal pointCount As Integer, ByVal unitTime As DateTime, Optional ByVal unitQuality As Quality = Quality.Good)

            Dim blocks As Integer
            Dim remainder As Integer
            Dim x, y As Integer

            Me.Connection = connection
            Me.PlantCode = connection.PlantCode
            Me.PointCount = pointCount
            Me.PostAttempts = 2

            ' Create a new time tag in the proper time zone...
            Me.TTag = New TimeTag(connection.TimeZone.ToLocalTime(unitTime))

            ' Calculate the total number of needed blocks
            blocks = Math.DivRem(pointCount, BlockSize, remainder)
            If remainder > 0 Then blocks += 1

            Me.Blocks = Array.CreateInstance(GetType(UnitBlock), blocks)

            For x = 0 To blocks - 1
                Me.Blocks(x) = New UnitBlock
                If x = blocks - 1 And remainder > 0 Then
                    Me.Blocks(x).Channels = Array.CreateInstance(GetType(Integer), remainder)
                    Me.Blocks(x).Values = Array.CreateInstance(GetType(Single), remainder)
                    Me.Blocks(x).TTags = Array.CreateInstance(GetType(Double), remainder)
                    Me.Blocks(x).Quals = Array.CreateInstance(GetType(Quality), remainder)
                Else
                    Me.Blocks(x).Channels = Array.CreateInstance(GetType(Integer), BlockSize)
                    Me.Blocks(x).Values = Array.CreateInstance(GetType(Single), BlockSize)
                    Me.Blocks(x).TTags = Array.CreateInstance(GetType(Double), BlockSize)
                    Me.Blocks(x).Quals = Array.CreateInstance(GetType(Quality), BlockSize)
                End If
            Next

            ' Prepopulate time tag and qualilty values with given defaults
            For x = 0 To blocks - 1
                For y = 0 To Me.Blocks(x).TTags.Length - 1
                    Me.Blocks(x).TTags(y) = TTag.Value
                    Me.Blocks(x).Quals(y) = unitQuality
                Next
            Next

        End Sub

        Public Sub SetRow(ByVal index As Integer, ByVal channel As Integer, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            Dim block As Integer
            Dim offset As Integer

            block = index \ BlockSize
            offset = index - (block * BlockSize)

            With Blocks(block)
                .Channels(offset) = channel
                .Values(offset) = value
                If qual <> .Quals(offset) Then .Quals(offset) = qual
            End With

        End Sub

        Public Sub Post()

            Dim status As ReturnStatus

            ' We only archive data units with a valid timestamp...
            If TTag.Value > 0 Then
                For x As Integer = 0 To Blocks.Length - 1
                    With Blocks(x)
                        If Not .Processed Then
                            Dim attempts As Integer
                            Do
                                attempts += 1
                                'Connection.DWAPI.Archive_PutBlock(PlantCode, .Channels, .Values, TimeTag, .Quals, .Channels.Length, status)
                                Connection.DWAPI.Archive_Put(PlantCode, .Channels, .Values, .TTags, .Quals, .Channels.Length, status)
                                .Processed = (status = ReturnStatus.Normal)
                                If status = ReturnStatus.NotConnected Then Connection.Open()
                            Loop While Not .Processed And attempts < PostAttempts
                        End If

                        If Not .Processed Then Throw New InvalidOperationException("Failed to archive DatAWare unit block due to exception: Archive_Put " & [Enum].GetName(GetType(ReturnStatus), status) & " error")
                    End With
                Next
            End If

        End Sub

    End Class

#End Region

End Namespace