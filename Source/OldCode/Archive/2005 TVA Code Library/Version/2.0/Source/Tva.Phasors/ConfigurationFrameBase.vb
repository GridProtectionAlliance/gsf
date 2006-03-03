'*******************************************************************************************************
'  ConfigurationFrameBase.vb - Configuration frame base class
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

' This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.
Public MustInherit Class ConfigurationFrameBase

    Inherits ChannelFrameBase(Of IConfigurationCell)
    Implements IConfigurationFrame

    Private m_sampleRate As Int16

    Protected Sub New(ByVal cells As ConfigurationCellCollection)

        MyBase.New(cells)

    End Sub

    Protected Sub New(ByVal cells As ConfigurationCellCollection, ByVal ticks As Long, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal idCode As Int16, ByVal sampleRate As Int16)

        MyBase.New(cells, ticks, synchronizationIsValid, dataIsValid, idCode)

        m_sampleRate = sampleRate

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    ' and automatically pass in state parameter
    Protected Sub New(ByVal state As IConfigurationFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyBase.New(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame)
    Protected Sub New(ByVal configurationFrame As IConfigurationFrame)

        MyClass.New(configurationFrame.Cells, configurationFrame.Ticks, configurationFrame.SynchronizationIsValid, _
            configurationFrame.DataIsValid, configurationFrame.IDCode, configurationFrame.SampleRate)

    End Sub

    Public Overridable Shadows ReadOnly Property Cells() As ConfigurationCellCollection Implements IConfigurationFrame.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

    Public Overridable Property SampleRate() As Int16 Implements IConfigurationFrame.SampleRate
        Get
            Return m_sampleRate
        End Get
        Set(ByVal value As Int16)
            m_sampleRate = value
        End Set
    End Property

    Public Sub SetNominalFrequency(ByVal value As LineFrequency) Implements IConfigurationFrame.SetNominalFrequency

    End Sub

    'Public Overridable Property NominalFrequency() As LineFrequency Implements IConfigurationFrame.SetNominalFrequency
    '    Get
    '        Return m_nominalFrequency
    '    End Get
    '    Set(ByVal value As LineFrequency)
    '        m_nominalFrequency = value
    '    End Set
    'End Property


End Class
