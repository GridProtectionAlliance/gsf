'*******************************************************************************************************
'  PMUCell.vb - PMU cell - PDCstream PDC block unit
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'   Note: I am assuming that the PMU cell within a PDC block only supports
'   rectangular phasor values - this was either an oversight by Ken or 
'   he felt this feature was not used much and need not be changed
'
'*******************************************************************************************************

Imports TVA.Interop

Namespace EE.Phasor.PDCstream

    Public Class PMUCell

        Inherits ChannelCellBase

        Private m_flags As ChannelFlags
        Private m_sampleRate As Byte
        Private m_statusFlags As Int16

        Public Sub New(ByVal parent As PMUCellCollection)

            MyBase.New(parent)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return GetType(PMUCell)
            End Get
        End Property

        Public Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
            Set(ByVal Value As ChannelFlags)
                m_flags = Value
            End Set
        End Property

        Public Property SampleRate() As Byte
            Get
                Return m_sampleRate
            End Get
            Set(ByVal Value As Byte)
                m_sampleRate = Value
            End Set
        End Property

        Public Overridable Property StatusFlags() As Int16
            Get
                Return m_statusFlags
            End Get
            Set(ByVal Value As Int16)
                m_statusFlags = Value
            End Set
        End Property

    End Class

End Namespace