'***********************************************************************
'  DataPacket.vb - PDC stream data packet
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports System.Buffer
Imports Tva.Interop
Imports Tva.DateTime
Imports Tva.Math.Common

Namespace BpaPdcStream

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False)> _
    Public Class DataPacket

        Implements IComparable

        Private m_configFile As ConfigurationFrame
        Private m_timeTag As UnixTimeTag
        Private m_timeStamp As Date
        Private m_index As Integer

        Public Cells As DataCell()
        Public Published As Boolean

        Public Const SyncByte As Byte = &HAA

        Public Sub New(ByVal configFile As ConfigurationFrame, ByVal timeStamp As Date, ByVal index As Integer)

            m_configFile = configFile
            m_timeTag = New UnixTimeTag(timeStamp)
            m_index = index

            ' We precalculate a regular .NET timestamp with milliseconds sitting in the middle of the sample index
            m_timeStamp = timeStamp.AddMilliseconds((m_index + 0.5@) * (1000@ / m_configFile.FrameRate))

            With m_configFile
                Cells = Array.CreateInstance(GetType(DataCell), .Cells.Count)

                For x As Integer = 0 To Cells.Length - 1
                    'Cells(x) = New DataCell(.PMU(x), index)
                Next
            End With

        End Sub

        Public ReadOnly Property TimeTag() As UnixTimeTag
            Get
                Return m_timeTag
            End Get
        End Property

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

        Public ReadOnly Property Timestamp() As Date
            Get
                Return m_timeStamp
            End Get
        End Property

        Public ReadOnly Property ReadyToPublish() As Boolean
            Get
                Static packetReady As Boolean

                If Not packetReady Then
                    Dim isReady As Boolean = True

                    ' If we have data for each cell in the row, we can go ahead and publish it...
                    For x As Integer = 0 To Cells.Length - 1
                        If Cells(x).AllValuesAreEmpty Then
                            isReady = False
                            Exit For
                        End If
                    Next

                    If isReady Then packetReady = True
                    Return isReady
                Else
                    Return True
                End If
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
                'Dim pmuID As Byte()
                Dim index As Integer

                buffer(0) = SyncByte
                buffer(1) = Convert.ToByte(1)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(m_timeTag.Value), buffer, 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_index), buffer, 8)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Length), buffer, 10)
                index = 12

                For x As Integer = 0 To Cells.Length - 1
                    BlockCopy(Cells(x).BinaryImage, 0, buffer, index, Cells(x).BinaryLength)
                    index += Cells(x).BinaryLength
                Next

                ' Add check sum
                BlockCopy(BitConverter.GetBytes(Xor16BitCheckSum(buffer, 0, index)), 0, buffer, index, 2)

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