'*******************************************************************************************************
'  PMUCell.vb - PMU cell - PDCstream PDC block unit
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'   Note: The PMU cell within a PDC block only supports rectangular values - Ken felt
'   that this feature was not used much and need not be changed
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class PMUCell

        Inherits ChannelCellBase

        Private m_flags As ChannelFlags
        Private m_frameRate As Byte
        Private m_statusFlags As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize pmu cell values
            m_flags = info.GetValue("flags", GetType(ChannelFlags))
            m_frameRate = info.GetByte("frameRate")
            m_statusFlags = info.GetInt16("statusFlags")

        End Sub

        Public Sub New(ByVal parent As PMUCellCollection)

            MyBase.New(parent, True, parent.IDCode)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
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

        Public Property FrameRate() As Byte
            Get
                Return m_frameRate
            End Get
            Set(ByVal Value As Byte)
                m_frameRate = Value
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

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize pmu cell values
            info.AddValue("flags", m_flags, GetType(ChannelFlags))
            info.AddValue("frameRate", m_frameRate)
            info.AddValue("statusFlags", m_statusFlags)

        End Sub

    End Class

End Namespace