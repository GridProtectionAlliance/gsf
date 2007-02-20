'*******************************************************************************************************
'  ConfigurationFrameBase.vb - Configuration frame base class
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
Imports Tva.DateTime

''' <summary>This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ConfigurationFrameBase

    Inherits ChannelFrameBase(Of IConfigurationCell)
    Implements IConfigurationFrame

    Private m_frameRate As Int16

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize configuration frame
        m_frameRate = info.GetInt16("frameRate")

    End Sub

    Protected Sub New(ByVal cells As ConfigurationCellCollection)

        MyBase.New(cells)

    End Sub

    Protected Sub New(ByVal idCode As UInt16, ByVal cells As ConfigurationCellCollection, ByVal ticks As Long, ByVal frameRate As Int16)

        MyBase.New(idCode, cells, ticks)

        m_frameRate = frameRate

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    ' and automatically pass in state parameter
    Protected Sub New(ByVal state As IConfigurationFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame)
    Protected Sub New(ByVal configurationFrame As IConfigurationFrame)

        MyClass.New(configurationFrame.IDCode, configurationFrame.Cells, configurationFrame.Ticks, configurationFrame.FrameRate)

    End Sub

    Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType
        Get
            Return Phasors.FundamentalFrameType.ConfigurationFrame
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As ConfigurationCellCollection Implements IConfigurationFrame.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

    Public Overridable Property FrameRate() As Int16 Implements IConfigurationFrame.FrameRate
        Get
            Return m_frameRate
        End Get
        Set(ByVal value As Int16)
            m_frameRate = value
        End Set
    End Property

    Public Overridable Sub SetNominalFrequency(ByVal value As LineFrequency) Implements IConfigurationFrame.SetNominalFrequency

        For Each cell As IConfigurationCell In Cells
            cell.NominalFrequency = value
        Next

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize configuration frame
        info.AddValue("frameRate", m_frameRate)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Frame Rate", FrameRate)

            Return baseAttributes
        End Get
    End Property

End Class
