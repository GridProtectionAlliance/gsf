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

Imports DatAWarePDC.Interop
Imports DatAWarePDC.PDCstream.Common

Namespace PDCstream

    ' This is essentially a "row" of PMU data at a given timestamp
    Public Class DataPacket

        Implements IComparable

        Private m_configFile As ConfigFile
        Private m_timeTag As Unix.TimeTag
        Private m_index As Integer

        Public Cells As PMUDataCell()
        Public Published As Boolean

        Public Const SyncByte As Byte = &HAA

        Public Sub New(ByVal configFile As ConfigFile, ByVal timeStamp As DateTime, ByVal index As Integer)

            m_configFile = configFile
            m_timeTag = New Unix.TimeTag(timeStamp)
            m_index = index

            With m_configFile
                Cells = Array.CreateInstance(GetType(PMUDataCell), .PMUCount)

                For x As Integer = 0 To Cells.Length - 1
                    Cells(x) = New PMUDataCell(.PMU(x), index)
                Next
            End With

        End Sub

        Public ReadOnly Property TimeTag() As Unix.TimeTag
            Get
                Return m_timeTag
            End Get
        End Property

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

        ' This provides a regular .NET timestamp with milliseconds sitting in the middle of the sample index
        Public ReadOnly Property Timestamp() As DateTime
            Get
                Return m_timeTag.ToDateTime.AddMilliseconds((m_index + 0.5) * (1000 / m_configFile.SampleRate))
            End Get
        End Property

        Public ReadOnly Property ReadyToPublish() As Boolean
            Get
                Dim isReady As Boolean = True

                ' If we have data for each cell in the row, we can go ahead and publish it...
                For x As Integer = 0 To Cells.Length - 1
                    If Cells(x).IsEmpty Then
                        isReady = False
                        Exit For
                    End If
                Next

                Return isReady
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer = 14

                For x As Integer = 0 To Cells.Length - 1
                    length += Cells(x).BinaryLength
                Next

                Return length
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim pmuID As Byte()
                Dim index As Integer

                buffer(0) = SyncByte
                buffer(1) = m_index + 1
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(buffer.Length \ 2)), 0, buffer, 2, 2)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt32(m_timeTag.Value)), 0, buffer, 4, 4)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(m_index)), 0, buffer, 8, 2)
                Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(Cells.Length)), 0, buffer, 10, 2)
                index = 12

                For x As Integer = 0 To Cells.Length - 1
                    Array.Copy(Cells(x).BinaryImage, 0, buffer, index, Cells(x).BinaryLength)
                    index += Cells(x).BinaryLength
                Next

                ' Add check sum
                Array.Copy(BitConverter.GetBytes(XorCheckSum(buffer, 0, index)), 0, buffer, index, 2)

                Return buffer
            End Get
        End Property

        ' We sort data packets by timetag and index
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is DataPacket Then
                Dim comparison As Integer = m_timeTag.CompareTo(DirectCast(obj, DataPacket).TimeTag)

                If comparison = 0 Then
                    Return m_index.CompareTo(DirectCast(obj, DataPacket).Index)
                Else
                    Return comparison
                End If
            Else
                Throw New ArgumentException("DataPacket can only be compared with other DataPackets...")
            End If

        End Function

    End Class

End Namespace