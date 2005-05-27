'***********************************************************************
'  ConfigurationFrameBase.vb - Configuration frame base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.
    Public MustInherit Class ConfigurationFrameBase

        Inherits ChannelFrameBase
        Implements IConfigurationFrame

        Private m_idCode As Int16

        Protected Sub New()

            MyBase.New()

        End Sub

        Protected Sub New(ByVal cells As ChannelCellCollection, ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal idCode As Int16)

            MyBase.New(cells, timeTag, milliseconds, synchronizationIsValid, dataIsValid)

            m_idCode = idCode

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal configurationFrame As IDataFrame)
        Protected Sub New(ByVal configurationFrame As IConfigurationFrame)

            Me.New(configurationFrame.Cells, configurationFrame.TimeTag, configurationFrame.Milliseconds, _
                configurationFrame.SynchronizationIsValid, configurationFrame.DataIsValid, configurationFrame.IDCode)

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

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TVA.EE.Phasor.ConfigurationFrameBase"
            End Get
        End Property

    End Class

End Namespace