'*******************************************************************************************************
'  DataFrameBase.vb - Data frame base class
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

Imports Tva.DateTime

' This class represents the protocol independent common implementation of a data frame that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class DataFrameBase

    Inherits ChannelFrameBase(Of IDataCell)
    Implements IDataFrame

    Private m_configurationFrame As IConfigurationFrame

    Protected Sub New(ByVal cells As DataCellCollection)

        MyBase.New(cells)

    End Sub

    Protected Sub New(ByVal cells As DataCellCollection, ByVal ticks As Long, ByVal configurationFrame As IConfigurationFrame)

        MyBase.New(0, cells, ticks)

        m_configurationFrame = configurationFrame

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    ' and automatically pass in parsing state
    Protected Sub New(ByVal state As IDataFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(state, binaryImage, startIndex)

        m_configurationFrame = state.ConfigurationFrame

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal dataFrame As IDataFrame)
    Protected Sub New(ByVal dataFrame As IDataFrame)

        MyClass.New(dataFrame.Cells, dataFrame.Ticks, dataFrame.ConfigurationFrame)

    End Sub

    Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType
        Get
            Return Phasors.FundamentalFrameType.DataFrame
        End Get
    End Property

    Public Overridable Property ConfigurationFrame() As IConfigurationFrame Implements IDataFrame.ConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
        Set(ByVal value As IConfigurationFrame)
            m_configurationFrame = value
        End Set
    End Property

    Public Overrides Property IDCode() As UInt16
        Get
            Return m_configurationFrame.IDCode
        End Get
        Set(ByVal value As UInt16)
            Throw New NotSupportedException("IDCode of a data frame is read-only, change IDCode of associated configuration frame instead")
        End Set
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As DataCellCollection Implements IDataFrame.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class
