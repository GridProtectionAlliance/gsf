'*******************************************************************************************************
'  HeaderFrameBase.vb - Header frame base class
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
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.DateTime

' This class represents the protocol independent common implementation of a header frame that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class HeaderFrameBase

    Inherits ChannelFrameBase(Of IHeaderCell)
    Implements IHeaderFrame

    Protected Sub New(ByVal cells As HeaderCellCollection)

        MyBase.New(cells)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    ' and automatically pass in parsing state
    Protected Sub New(ByVal state As IHeaderFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal headerFrame As IHeaderFrame)
    Protected Sub New(ByVal headerFrame As IHeaderFrame)

        MyClass.New(headerFrame.Cells)

    End Sub

    Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType
        Get
            Return Phasors.FundamentalFrameType.HeaderFrame
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As HeaderCellCollection Implements IHeaderFrame.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

    Public Property HeaderData() As String Implements IHeaderFrame.HeaderData
        Get
            Return Encoding.ASCII.GetString(Cells.BinaryImage)
        End Get
        Set(ByVal value As String)
            Cells.Clear()
            ParseBodyImage(New HeaderFrameParsingState(Cells, 0, value.Length), Encoding.ASCII.GetBytes(value), 0)
        End Set
    End Property

End Class
