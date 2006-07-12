'*******************************************************************************************************
'  CommandFrame.vb - IEEE1344 Command Frame
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.ComponentModel
Imports Tva.DateTime
Imports Tva.IO.Compression.Common

Namespace Ieee1344

    <CLSCompliant(False)> _
    Public Class CommandFrame

        Inherits CommandFrameBase

        Public Const FrameLength As Int16 = 16

        Private m_idCode As UInt64

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize command frame
            m_idCode = info.GetUInt64("idCode64Bit")

        End Sub

        Public Sub New(ByVal idCode As UInt64, ByVal command As Command)

            MyBase.New(New CommandCellCollection(0), command)
            m_idCode = idCode

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New CommandFrameParsingState(New CommandCellCollection(0), FrameLength, 0), binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal commandFrame As ICommandFrame)

            MyBase.New(commandFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        ' IEEE 1344 command frame doesn't support extended data - so we hide cell collection and extended data property...
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides ReadOnly Property Cells() As CommandCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Property ExtendedData() As Byte()
            Get
                Return MyBase.ExtendedData
            End Get
            Set(ByVal value As Byte())
                MyBase.ExtendedData = value
            End Set
        End Property

        Public Shadows Property IDCode() As UInt64
            Get
                Return m_idCode
            End Get
            Set(ByVal value As UInt64)
                m_idCode = value
            End Set
        End Property

        Public Shadows ReadOnly Property TimeTag() As NtpTimeTag
            Get
                Return New NtpTimeTag(New Date(Ticks))
            End Get
        End Property

        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' IEEE 1344 uses CRC16 to calculate checksum for frames
            Return CRC16(UInt16.MaxValue, buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return 12
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)

                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 4)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            Ticks = (New NtpTimeTag(EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex))).ToDateTime.Ticks
            m_idCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex + 4)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize command frame
            info.AddValue("idCode64Bit", m_idCode)

        End Sub

    End Class

End Namespace