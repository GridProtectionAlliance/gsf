'*******************************************************************************************************
'  ConfigurationFrameBase.vb - Configuration frame base class
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.
    Public MustInherit Class ConfigurationFrameBase

        Inherits ChannelFrameBase
        Implements IConfigurationFrame

        Private m_idCode As Int16
        Private m_sampleRate As Int16
        Private m_nominalFrequency As LineFrequency

        Protected Sub New(ByVal cells As ConfigurationCellCollection)

            MyBase.New(cells)

            m_nominalFrequency = LineFrequency.Hz60

        End Sub

        Protected Sub New(ByVal cells As ConfigurationCellCollection, ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal idCode As Int16, ByVal sampleRate As Int16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(cells, timeTag, milliseconds, synchronizationIsValid, dataIsValid)

            m_idCode = idCode
            m_sampleRate = sampleRate
            m_nominalFrequency = nominalFrequency

        End Sub

        ' Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame)
        Protected Sub New(ByVal configurationFrame As IConfigurationFrame)

            Me.New(configurationFrame.Cells, configurationFrame.TimeTag, configurationFrame.Milliseconds, configurationFrame.SynchronizationIsValid, _
                configurationFrame.DataIsValid, configurationFrame.IDCode, configurationFrame.SampleRate, configurationFrame.NominalFrequency)

        End Sub

        Public Overridable Property IDCode() As Int16 Implements IConfigurationFrame.IDCode
            Get
                Return m_idCode
            End Get
            Set(ByVal Value As Int16)
                m_idCode = Value
            End Set
        End Property

        Public Overridable Shadows ReadOnly Property Cells() As ConfigurationCellCollection Implements IConfigurationFrame.Cells
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Overridable Property SampleRate() As Int16 Implements IConfigurationFrame.SampleRate
            Get
                Return m_sampleRate
            End Get
            Set(ByVal Value As Int16)
                m_sampleRate = Value
            End Set
        End Property

        Public Overridable Property NominalFrequency() As LineFrequency Implements IConfigurationFrame.NominalFrequency
            Get
                Return m_nominalFrequency
            End Get
            Set(ByVal Value As LineFrequency)
                m_nominalFrequency = Value
            End Set
        End Property

    End Class

End Namespace