'*******************************************************************************************************
'  DataFrame.vb - IEEE C37.118 data frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace IeeeC37_118

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False)> _
    Public Class DataFrame

        Inherits DataFrameBase
        Implements ICommonFrameHeader

        Private m_timeQualityFlags As Int32

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

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Property RevisionNumber() As RevisionNumber Implements ICommonFrameHeader.RevisionNumber
            Get
                Return ConfigurationFrame.RevisionNumber
            End Get
            Private Set(ByVal value As RevisionNumber)
                ' Revision number is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the revision number on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
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

        Public Shadows Property ConfigurationFrame() As IeeeC37_118.ConfigurationFrame
            Get
                Return MyBase.ConfigurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                MyBase.ConfigurationFrame = value
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

    End Class

End Namespace