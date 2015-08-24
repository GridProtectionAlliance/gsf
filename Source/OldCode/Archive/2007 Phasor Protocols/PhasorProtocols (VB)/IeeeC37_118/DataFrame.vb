'*******************************************************************************************************
'  DataFrame.vb - IEEE C37.118 data frame
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace IeeeC37_118

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False), Serializable()> _
    Public Class DataFrame

        Inherits DataFrameBase
        Implements ICommonFrameHeader

        Private m_timeQualityFlags As Int32

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize data frame
            m_timeQualityFlags = info.GetInt32("timeQualityFlags")

        End Sub

        Public Sub New(ByVal ticks As Long, ByVal configurationFrame As ConfigurationFrame)

            MyBase.New(New DataCellCollection, ticks, configurationFrame)

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal configurationFrame As ConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, parsedFrameHeader.FrameLength, configurationFrame, _
                AddressOf IeeeC37_118.DataCell.CreateNewDataCell), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal dataFrame As IDataFrame)

            MyBase.New(dataFrame)

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

        Public Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return IeeeC37_118.FrameType.DataFrame
            End Get
            Private Set(ByVal value As FrameType)
                ' Frame type is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the frame type on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Public Property Version() As Byte Implements ICommonFrameHeader.Version
            Get
                Return ConfigurationFrame.Version
            End Get
            Private Set(ByVal value As Byte)
                ' Version number is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the version number on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property FrameLength() As Int16 Implements ICommonFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value
            End Set
        End Property

        Public Overrides Property IDCode() As UInt16
            Get
                Return MyBase.IDCode
            End Get
            Set(ByVal value As UInt16)
                ' ID code is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the ID code on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public ReadOnly Property TimeBase() As Int32 Implements ICommonFrameHeader.TimeBase
            Get
                Return ConfigurationFrame.TimeBase
            End Get
        End Property

        Private Property InternalTimeQualityFlags() As Int32 Implements ICommonFrameHeader.InternalTimeQualityFlags
            Get
                Return m_timeQualityFlags
            End Get
            Set(ByVal value As Int32)
                m_timeQualityFlags = value
            End Set
        End Property

        Public ReadOnly Property SecondOfCentury() As UInt32 Implements ICommonFrameHeader.SecondOfCentury
            Get
                Return CommonFrameHeader.SecondOfCentury(Me)
            End Get
        End Property

        Public ReadOnly Property FractionOfSecond() As Int32 Implements ICommonFrameHeader.FractionOfSecond
            Get
                Return CommonFrameHeader.FractionOfSecond(Me)
            End Get
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags Implements ICommonFrameHeader.TimeQualityFlags
            Get
                Return CommonFrameHeader.TimeQualityFlags(Me)
            End Get
            Set(ByVal value As TimeQualityFlags)
                CommonFrameHeader.TimeQualityFlags(Me) = value
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements ICommonFrameHeader.TimeQualityIndicatorCode
            Get
                Return CommonFrameHeader.TimeQualityIndicatorCode(Me)
            End Get
            Set(ByVal value As TimeQualityIndicatorCode)
                CommonFrameHeader.TimeQualityIndicatorCode(Me) = value
            End Set
        End Property

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
            info.AddValue("timeQualityFlags", m_timeQualityFlags)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Frame Type", FrameType & ": " & [Enum].GetName(GetType(FrameType), FrameType))
                baseAttributes.Add("Frame Length", FrameLength)
                baseAttributes.Add("Version", Version)
                baseAttributes.Add("Second of Century", SecondOfCentury)
                baseAttributes.Add("Fraction of Second", FractionOfSecond)
                baseAttributes.Add("Time Quality Flags", TimeQualityFlags & ": " & [Enum].GetName(GetType(TimeQualityFlags), TimeQualityFlags))
                baseAttributes.Add("Time Quality Indicator Code", TimeQualityIndicatorCode & ": " & [Enum].GetName(GetType(TimeQualityIndicatorCode), TimeQualityIndicatorCode))
                baseAttributes.Add("Time Base", TimeBase)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace