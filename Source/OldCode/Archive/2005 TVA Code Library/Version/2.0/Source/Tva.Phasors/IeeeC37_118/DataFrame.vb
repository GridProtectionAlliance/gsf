'*******************************************************************************************************
'  DataFrame.vb - IEEE C37.118 data frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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
        Implements IFrameHeader

        Private m_timeQualityFlags As Int32

        Public Sub New(ByVal ticks As Long, ByVal configurationFrame As ConfigurationFrame)

            MyBase.New(New DataCellCollection, ticks, configurationFrame)

        End Sub

        Public Sub New(ByVal parsedFrameHeader As IFrameHeader, ByVal configurationFrame As ConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, parsedFrameHeader.FrameLength, configurationFrame, _
                AddressOf IeeeC37_118.DataCell.CreateNewDataCell), binaryImage, startIndex)

            FrameHeader.Clone(parsedFrameHeader, Me)

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

        Public Property RevisionNumber() As RevisionNumber Implements IFrameHeader.RevisionNumber
            Get
                Return ConfigurationFrame.RevisionNumber
            End Get
            Friend Set(ByVal value As RevisionNumber)
                ' Revision number is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the revision number on a data frame (e.g., the FrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property FrameType() As FrameType Implements IFrameHeader.FrameType
            Get
                Return IeeeC37_118.FrameType.DataFrame
            End Get
            Friend Set(ByVal value As FrameType)
                ' Frame type is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the frame type on a data frame (e.g., the FrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property Version() As Byte Implements IFrameHeader.Version
            Get
                Return ConfigurationFrame.Version
            End Get
            Friend Set(ByVal value As Byte)
                ' Version number is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the version number on a data frame (e.g., the FrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property FrameLength() As Int16 Implements IFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value
            End Set
        End Property

        Public Overrides Property IDCode() As UInt16 Implements IFrameHeader.IDCode
            Get
                Return MyBase.IDCode
            End Get
            Set(ByVal value As UShort)
                ' ID code is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the ID code on a data frame (e.g., the FrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Overrides Property Ticks() As Long Implements IFrameHeader.Ticks
            Get
                Return MyBase.Ticks
            End Get
            Set(ByVal value As Long)
                MyBase.Ticks = value
            End Set
        End Property

        Public Property TimeBase() As Int32 Implements IFrameHeader.TimeBase
            Get
                Return ConfigurationFrame.TimeBase
            End Get
            Friend Set(ByVal value As Int32)
                ' Time base is readonly for data frames - we don't throw an exception here if someone attempts to change
                ' the time base on a data frame (e.g., the FrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Private Property InternalTimeQualityFlags() As Int32 Implements IFrameHeader.InternalTimeQualityFlags
            Get
                Return m_timeQualityFlags
            End Get
            Set(ByVal value As Int32)
                m_timeQualityFlags = value
            End Set
        End Property

        Public ReadOnly Property SecondOfCentury() As UInt32 Implements IFrameHeader.SecondOfCentury
            Get
                Return FrameHeader.SecondOfCentury(Me)
            End Get
        End Property

        Public ReadOnly Property FractionOfSecond() As Int32 Implements IFrameHeader.FractionOfSecond
            Get
                Return FrameHeader.FractionOfSecond(Me)
            End Get
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags Implements IFrameHeader.TimeQualityFlags
            Get
                Return FrameHeader.TimeQualityFlags(Me)
            End Get
            Set(ByVal value As TimeQualityFlags)
                FrameHeader.TimeQualityFlags(Me) = value
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements IFrameHeader.TimeQualityIndicatorCode
            Get
                Return FrameHeader.TimeQualityIndicatorCode(Me)
            End Get
            Set(ByVal value As TimeQualityIndicatorCode)
                FrameHeader.TimeQualityIndicatorCode(Me) = value
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

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return FrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return FrameHeader.BinaryImage(Me)
            End Get
        End Property

        Public Overrides ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, Measurements.IMeasurement)
            Get
                ' TODO: Oh my - how to handle this...
            End Get
        End Property

    End Class

End Namespace