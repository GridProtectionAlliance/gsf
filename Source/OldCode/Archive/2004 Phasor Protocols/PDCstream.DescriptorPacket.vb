'***********************************************************************
'  PDCstream.DescriptorPacket.vb - PDC stream descriptor packet
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports DatAWarePDC.PDCstream.Common

Namespace PDCstream

    ' According to the PDCstream specification, the descriptor packet is sent once per minute
    Public Class DescriptorPacket

        Private m_configFile As ConfigFile
        Private m_dataPacketCount As Integer

        Public Const SyncByte As Byte = &HAA
        Public Const PacketFlag As Byte = &H0
        Public Const VersionFlag As Byte = 1    ' Using full data stream with PMU ID's and offsets removed from data packet
        Public Const RevisionNumber As Byte = 1 ' July 2002 revision for std. 37.118 using UNIX timetag (start count 1970)

        Public Sub New(ByVal configFile As ConfigFile, ByVal dataPacketCount As Integer)

            m_configFile = configFile
            m_dataPacketCount = dataPacketCount

        End Sub

        Public ReadOnly Property DataPacketCount() As Integer
            Get
                Return m_dataPacketCount
            End Get
        End Property

        Public ReadOnly Property RowLength() As Integer
            Get
                Dim length As Integer

                For x As Integer = 0 To m_configFile.PMUCount - 1
                    With m_configFile.PMU(x)
                        .Offset = length
                        length += 12 + FrequencyValue.BinaryLength + PhasorValue.BinaryLength * .Phasors.Length
                    End With
                Next

                Return length
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Integer
            Get
                Return 18 + 8 * m_configFile.PMUCount
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim pmuID As Byte()
                Dim index As Integer

                buffer(0) = SyncByte
                buffer(1) = PacketFlag
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(buffer.Length \ 2)), 0, buffer, 2, 2)
                buffer(4) = VersionFlag
                buffer(5) = RevisionNumber
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(m_configFile.SampleRate)), 0, buffer, 6, 2)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt32(RowLength)), 0, buffer, 8, 4)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(m_dataPacketCount)), 0, buffer, 12, 2)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(m_configFile.PMUCount)), 0, buffer, 14, 2)
                index = 16

                For x As Integer = 0 To m_configFile.PMUCount - 1
                    With m_configFile.PMU(x)
                        Array.Copy(Text.Encoding.ASCII.GetBytes(Left(Trim(.ID).PadRight(4), 4)), 0, buffer, index, 4)
                        Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(0)), 0, buffer, index + 4, 2)
                        Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(.Offset)), 0, buffer, index + 6, 2)
                    End With
                    index += 8
                Next

                ' Add check sum
                Array.Copy(BitConverter.GetBytes(XorCheckSum(buffer, 0, index)), 0, buffer, index, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace