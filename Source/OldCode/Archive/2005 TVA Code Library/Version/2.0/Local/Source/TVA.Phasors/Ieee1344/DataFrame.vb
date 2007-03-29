'*******************************************************************************************************
'  DataFrame.vb - IEEE1344 Data Frame
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
Imports TVA.DateTime
Imports TVA.IO.Compression.Common

Namespace Phasors.Ieee1344

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False), Serializable()> _
    Public Class DataFrame

        Inherits DataFrameBase
        Implements ICommonFrameHeader

        Private m_sampleCount As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize data frame
            m_sampleCount = info.GetInt16("sampleCount")

        End Sub

        Public Sub New(ByVal ticks As Long, ByVal configurationFrame As ConfigurationFrame)

            MyBase.New(New DataCellCollection, ticks, configurationFrame)
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.DataFrame

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal configurationFrame As ConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, parsedFrameHeader.FrameLength, configurationFrame, _
                AddressOf Ieee1344.DataCell.CreateNewDataCell), binaryImage, startIndex)

            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.DataFrame
            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal dataFrame As IDataFrame)

            MyBase.New(dataFrame)
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.DataFrame

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Shadows Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return MyBase.ConfigurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                MyBase.ConfigurationFrame = value
            End Set
        End Property

        Public Shadows Property IDCode() As UInt64 Implements ICommonFrameHeader.IDCode
            Get
                Return ConfigurationFrame.IDCode
            End Get
            Friend Set(ByVal value As UInt64)
                ' ID code is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the ID code on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public ReadOnly Property FrameLength() As Int16 Implements ICommonFrameHeader.FrameLength
            Get
                Return CommonFrameHeader.FrameLength(Me)
            End Get
        End Property

        Public ReadOnly Property DataLength() As Int16 Implements ICommonFrameHeader.DataLength
            Get
                Return CommonFrameHeader.DataLength(Me)
            End Get
        End Property

        Public Property SampleCount() As Int16
            Get
                Return CommonFrameHeader.SampleCount(Me)
            End Get
            Set(ByVal value As Int16)
                CommonFrameHeader.SampleCount(Me) = value
            End Set
        End Property

        Private Property InternalSampleCount() As Int16 Implements ICommonFrameHeader.InternalSampleCount
            Get
                Return m_sampleCount
            End Get
            Set(ByVal value As Int16)
                m_sampleCount = value
            End Set
        End Property

        ' Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share status flags with our only child
        ' and expose the value at the parent level for convience in determing frame length at the frame level
        Private Property InternalStatusFlags() As Short Implements ICommonFrameHeader.InternalStatusFlags
            Get
                Return Cells(0).StatusFlags
            End Get
            Set(ByVal value As Int16)
                Cells(0).StatusFlags = value
            End Set
        End Property

        Public Shadows ReadOnly Property TimeTag() As NtpTimeTag Implements ICommonFrameHeader.TimeTag
            Get
                Return CommonFrameHeader.TimeTag(Me)
            End Get
        End Property

        Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return Ieee1344.FrameType.DataFrame
            End Get
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' IEEE 1344 uses CRC16 to calculate checksum for frames
            Return CRC16(UInt16.MaxValue, buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return CommonFrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return CommonFrameHeader.BinaryImage(Me)
            End Get
        End Property

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize data frame
            info.AddValue("sampleCount", m_sampleCount)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Frame Type", FrameType & ": " & [Enum].GetName(GetType(FrameType), FrameType))
                baseAttributes.Add("Frame Length", FrameLength)
                baseAttributes.Add("64-Bit ID Code", IDCode)
                baseAttributes.Add("Sample Count", SampleCount)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace