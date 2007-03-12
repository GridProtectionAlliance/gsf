'*******************************************************************************************************
'  ConfigurationFrame.vb - PDCstream Configuration Frame / File
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.IO
Imports System.Text
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports System.Threading
Imports System.Buffer
Imports Tva.Assembly
Imports Tva.Math.Common
Imports Tva.Phasors.Common
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    ' Note that it is expected that the end user will typically create only one instance of this class per INI file for use by any
    ' number of different threads and a request can be made at anytime to "reload" the config file, so we make sure all publically
    ' accessible methods in the class make proper use of the internal reader-writer lock.  This also allows end user to place a
    ' file-watcher on the INI file so class can "reload" config file when it's updated...
    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase
        Implements ICommonFrameHeader

        Private m_readWriteLock As ReaderWriterLock
        Private m_iniFile As IniFile
        Private m_defaultPhasorV As PhasorDefinition
        Private m_defaultPhasorI As PhasorDefinition
        Private m_defaultFrequency As FrequencyDefinition
        Private m_rowLength As UInt16
        Private m_packetsPerSample As Int16
        Private m_streamType As StreamType
        Private m_revisionNumber As RevisionNumber
        Private m_ticksPerFrame As Decimal

        Public Event ConfigFileReloaded()

        Public Const DefaultVoltagePhasorEntry As String = "V,4500.0,0.0060573,0,0,500,Default 500kV"
        Public Const DefaultCurrentPhasorEntry As String = "I,600.00,0.000040382,0,1,1.0,Default Current"
        Public Const DefaultFrequencyEntry As String = "F,1000,60,1000,0,0,Frequency"

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration frame
            m_packetsPerSample = info.GetInt16("packetsPerSample")
            m_streamType = info.GetValue("streamType", GetType(StreamType))
            m_revisionNumber = info.GetValue("revisionNumber", GetType(RevisionNumber))
            m_iniFile = New IniFile(info.GetString("configurationFileName"))

        End Sub

        Public Sub New(ByVal configurationFileName As String)

            MyBase.New(New ConfigurationCellCollection)

            m_iniFile = New IniFile(configurationFileName)
            m_readWriteLock = New ReaderWriterLock
            m_packetsPerSample = 1
            Refresh()

        End Sub

        ' If you are going to create multiple data packets, you can use this constructor
        ' Note that this only starts becoming necessary if you start hitting data size
        ' limits imposed by the nature of the transport protocol...
        Public Sub New(ByVal configurationFileName As String, ByVal packetsPerSample As Int16)

            MyClass.New(configurationFileName)
            m_packetsPerSample = packetsPerSample

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal configurationFileName As String, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, _
                AddressOf BpaPdcStream.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

            m_iniFile = New IniFile(configurationFileName)
            m_readWriteLock = New ReaderWriterLock
            m_packetsPerSample = 1

            Refresh()

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Property StreamType() As StreamType
            Get
                Return m_streamType
            End Get
            Set(ByVal Value As StreamType)
                m_streamType = Value
            End Set
        End Property

        Public Property RevisionNumber() As RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Private Property SampleNumber() As Short Implements ICommonFrameHeader.SampleNumber
            Get
                Return -1
            End Get
            Set(ByVal value As Short)
                ' Sample number is readonly for configuration frames - we don't throw an exception here if someone attempts to change
                ' the packet number on a configuration frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property PacketNumber() As Byte Implements ICommonFrameHeader.PacketNumber
            Get
                Return DescriptorPacketFlag
            End Get
            Private Set(ByVal value As Byte)
                ' Packet number is readonly for configuration frames - we don't throw an exception here if someone attempts to change
                ' the packet number on a configuration frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return BpaPdcStream.FrameType.DataFrame
            End Get
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Public Property WordCount() As Int16 Implements ICommonFrameHeader.WordCount
            Get
                Return MyBase.BinaryLength / 2
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value * 2
            End Set
        End Property

        Public ReadOnly Property FrameLength() As Short Implements ICommonFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
        End Property

        Public Sub Refresh()

            ' The only time we need a write lock is when we reload the config file...
            m_readWriteLock.AcquireWriterLock(-1)

            Try
                If File.Exists(m_iniFile.FileName) Then
                    Dim pmuCell As ConfigurationCell
                    Dim x, phasorCount As Int32

                    m_defaultPhasorV = New PhasorDefinition(Nothing, 0, m_iniFile.KeyValue("DEFAULT", "PhasorV", DefaultVoltagePhasorEntry))
                    m_defaultPhasorI = New PhasorDefinition(Nothing, 0, m_iniFile.KeyValue("DEFAULT", "PhasorI", DefaultCurrentPhasorEntry))
                    m_defaultFrequency = New FrequencyDefinition(Nothing, m_iniFile.KeyValue("DEFAULT", "Frequency", DefaultFrequencyEntry))
                    FrameRate = Convert.ToInt16(m_iniFile.KeyValue("CONFIG", "SampleRate", "30"))

                    Cells.Clear()

                    ' Load phasor data for each section in config file...
                    For Each section As String In m_iniFile.SectionNames()
                        If Len(section) > 0 Then
                            ' Make sure this is not a special section
                            If String.Compare(section, "DEFAULT", True) <> 0 And String.Compare(section, "CONFIG", True) <> 0 Then
                                ' Create new PMU entry structure from config file settings...
                                phasorCount = Convert.ToInt32(m_iniFile.KeyValue(section, "NumberPhasors", "0"))

                                pmuCell = New ConfigurationCell(Me, 0, LineFrequency.Hz60)

                                pmuCell.IDLabel = section
                                pmuCell.StationName = m_iniFile.KeyValue(section, "Name", section)
                                pmuCell.IDCode = Convert.ToUInt16(m_iniFile.KeyValue(section, "PMU", Cells.Count.ToString))

                                For x = 0 To phasorCount - 1
                                    pmuCell.PhasorDefinitions.Add(New PhasorDefinition(pmuCell, x + 1, m_iniFile.KeyValue(section, "Phasor" & (x + 1), DefaultVoltagePhasorEntry)))
                                Next

                                pmuCell.FrequencyDefinition = New FrequencyDefinition(pmuCell, m_iniFile.KeyValue(section, "Frequency", DefaultFrequencyEntry))

                                Cells.Add(pmuCell)
                            End If
                        End If
                    Next
                Else
                    Throw New InvalidOperationException("PDC config file """ & m_iniFile.FileName & """ does not exist.")
                End If
            Catch
                Throw
            Finally
                m_readWriteLock.ReleaseWriterLock()
            End Try

            ' In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            ' you do this after the write lock has been released to avoid possible dead-lock situations)
            RaiseEvent ConfigFileReloaded()

        End Sub

        Public Property PacketsPerSample() As Int16
            Get
                Return m_packetsPerSample
            End Get
            Set(ByVal Value As Int16)
                m_packetsPerSample = Value
            End Set
        End Property

        Public ReadOnly Property ConfigurationFileName() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_iniFile.FileName
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorV() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorV
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorI() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorI
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultFrequency() As FrequencyDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultFrequency
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property IniFileImage() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    With New StringBuilder
                        .Append("; File - " & m_iniFile.FileName & Environment.NewLine)
                        .Append("; Auto-generated on " & Now() & " by TVA DatAWare PDC" & Environment.NewLine)
                        .Append(";    Assembly: " & GetShortAssemblyName(GetExecutingAssembly) & Environment.NewLine)
                        .Append(";    Compiled: " & File.GetLastWriteTime(GetExecutingAssembly.Location) & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append("; Format:" & Environment.NewLine)
                        .Append(";   Each Column in data file is given a bracketed identifier, numbered in the order it" & Environment.NewLine)
                        .Append(";   appears in the data file, and identified by data type ( PMU, PDC, or other)" & Environment.NewLine)
                        .Append(";     PMU designates column data format from a single PMU" & Environment.NewLine)
                        .Append(";     PDC designates column data format from another PDC which is somewhat different from a single PMU" & Environment.NewLine)
                        .Append(";   Default gives default values for a processing algorithm in case quantities are omitted" & Environment.NewLine)
                        .Append(";   Name= gives the overall station name for print labels" & Environment.NewLine)
                        .Append(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column" & Environment.NewLine)
                        .Append(";                     for PDC data, gives the number of PMUs data included in the column" & Environment.NewLine)
                        .Append(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU" & Environment.NewLine)
                        .Append(";   Quantities within the column are listed by PhasorI=, Frequency=, etc" & Environment.NewLine)
                        .Append(";   Each quantity has 7 comma separated fields followed by an optional comment" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments" & Environment.NewLine)
                        .Append(";    Type:       Type of measurement, V=voltage, I=current, N=don't care, single ASCII character" & Environment.NewLine)
                        .Append(";    Ratio:      PT/CT ratio N:1 where N is a floating point number" & Environment.NewLine)
                        .Append(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point" & Environment.NewLine)
                        .Append(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point" & Environment.NewLine)
                        .Append(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point" & Environment.NewLine)
                        .Append(";                Voltage- empty, not used" & Environment.NewLine)
                        .Append(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer" & Environment.NewLine)
                        .Append(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer" & Environment.NewLine)
                        .Append(";    Label:      Phasor quantity label for print label, text" & Environment.NewLine)
                        .Append(";    Comments:   All text after the semicolon on a line are optional comments not for processing" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor * PTR (line-neutral)" & Environment.NewLine)
                        .Append(";   Current Magnitude = MAG(Real,Imaginary) * CalFactor * CTR / Shunt (phase current)" & Environment.NewLine)
                        .Append(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset (usually degrees)" & Environment.NewLine)
                        .Append(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements" & Environment.NewLine)
                        .Append(";           from different systems or through transformer banks" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments" & Environment.NewLine)
                        .Append(";   Frequency = Number / scale + offset" & Environment.NewLine)
                        .Append(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)
                        .Append(";" & Environment.NewLine)

                        .Append("[DEFAULT]" & Environment.NewLine)
                        .Append("PhasorV=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorV) & Environment.NewLine)
                        .Append("PhasorI=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorI) & Environment.NewLine)
                        .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(DefaultFrequency) & Environment.NewLine)
                        .Append(Environment.NewLine)

                        .Append("[CONFIG]" & Environment.NewLine)
                        .Append("SampleRate=" & FrameRate & Environment.NewLine)
                        .Append("NumberOfPMUs=" & Cells.Count & Environment.NewLine)
                        .Append(Environment.NewLine)

                        For x As Int32 = 0 To Cells.Count - 1
                            .Append("[" & Cells(x).IDLabel & "]" & Environment.NewLine)
                            .Append("Name=" & Cells(x).StationName & Environment.NewLine)
                            .Append("PMU=" & x & Environment.NewLine)
                            .Append("NumberPhasors=" & Cells(x).PhasorDefinitions.Count & Environment.NewLine)
                            For y As Int32 = 0 To Cells(x).PhasorDefinitions.Count - 1
                                .Append("Phasor" & (y + 1) & "=" & PhasorDefinition.ConfigFileFormat(Cells(x).PhasorDefinitions(y)) & Environment.NewLine)
                            Next
                            .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(Cells(x).FrequencyDefinition) & Environment.NewLine)
                            .Append(Environment.NewLine)
                        Next

                        Return .ToString()
                    End With
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property RowLength() As Int32
            Get
                Return RowLength(False)
            End Get
        End Property

        ' RowLength property calculates cell offsets - so it must be called before
        ' accessing cell offsets - this happens automatically since HeaderImage is
        ' called before base class BodyImage which just gets Cells.BinaryImage
        Public ReadOnly Property RowLength(ByVal recalculate As Boolean) As UInt16
            Get
                If m_rowLength = 0 OrElse recalculate Then
                    m_rowLength = 0
                    For x As Int32 = 0 To Cells.Count - 1
                        With Cells(x)
                            .Offset = m_rowLength
                            m_rowLength += 12 + FrequencyValue.CalculateBinaryLength(.FrequencyDefinition)
                            For y As Int32 = 0 To .PhasorDefinitions.Count - 1
                                m_rowLength += PhasorValue.CalculateBinaryLength(.PhasorDefinitions(y))
                            Next
                        End With
                    Next
                End If

                Return m_rowLength
            End Get
        End Property

        <CLSCompliant(False)> _
        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' PDCstream uses an XOR based check sum
            Return Xor16BitCheckSum(buffer, offset, length)

        End Function

        ' Oddly enough, check sum for frames in BPA PDC stream is little-endian
        Protected Overrides Sub AppendChecksum(ByVal buffer() As Byte, ByVal startIndex As Integer)

            EndianOrder.LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overrides Function ChecksumIsValid(ByVal buffer() As Byte, ByVal startIndex As Integer) As Boolean

            Dim sumLength As Int16 = BinaryLength - 2
            Return EndianOrder.LittleEndian.ToUInt16(buffer, startIndex + sumLength) = CalculateChecksum(buffer, startIndex, sumLength)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return 16
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)

                ' Common in common frame header portion of header image
                System.Buffer.BlockCopy(CommonFrameHeader.BinaryImage(Me), 0, buffer, 0, CommonFrameHeader.BinaryLength)

                buffer(4) = StreamType
                buffer(5) = RevisionNumber
                EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 6)
                EndianOrder.BigEndian.CopyBytes(RowLength(True), buffer, 8) ' <-- Important: This step calculates all PMU row offsets!
                EndianOrder.BigEndian.CopyBytes(PacketsPerSample, buffer, 12)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 14)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' We parse the PDC stream specific header image here...
            Dim parsingState As IConfigurationFrameParsingState = DirectCast(state, IConfigurationFrameParsingState)

            ' Only need to parse what wan't already parsed in common frame header
            StreamType = binaryImage(startIndex + 4)
            RevisionNumber = binaryImage(startIndex + 5)
            FrameRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)
            m_rowLength = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 8)
            PacketsPerSample = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 12)

            parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 14)

            ' The data that's in the data stream will take precedence over what's in the
            ' in the configuration file.  The configuration file may define more PMU's than
            ' are in the stream - in my opinion that's OK - it's when you have PMU's in the
            ' stream that aren't defined in the INI file that you'll have trouble..

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration frame
            info.AddValue("packetsPerSample", m_packetsPerSample)
            info.AddValue("streamType", m_streamType, GetType(StreamType))
            info.AddValue("revisionNumber", m_revisionNumber, GetType(RevisionNumber))
            info.AddValue("configurationFileName", m_iniFile.FileName)

        End Sub

        Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                If m_iniFile IsNot Nothing Then baseAttributes.Add("Configuration File Name", m_iniFile.FileName)
                baseAttributes.Add("Packet Number", DescriptorPacketFlag)
                baseAttributes.Add("Stream Type", m_streamType & ": " & [Enum].GetName(GetType(StreamType), m_streamType))
                baseAttributes.Add("Revision Number", m_revisionNumber & ": " & [Enum].GetName(GetType(RevisionNumber), m_revisionNumber))
                baseAttributes.Add("Packets Per Sample", m_packetsPerSample)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace