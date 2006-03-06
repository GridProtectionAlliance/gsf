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

        Dim m_frameHeader As FrameHeader

        Public Sub New()

            MyBase.New(New DataCellCollection)

        End Sub

        Public Sub New(ByVal frameHeader As FrameHeader, ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, frameHeader.FrameLength, configurationFrame, AddressOf IeeeC37_118.DataCell.CreateNewDataCell), binaryImage, startIndex)
            m_frameHeader = frameHeader

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

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return FrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return m_frameHeader.BinaryImage()
            End Get
        End Property

        Public Overrides ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, Measurements.IMeasurement)
            Get
                ' TODO: Oh my - how to handle this...
            End Get
        End Property

    End Class

End Namespace