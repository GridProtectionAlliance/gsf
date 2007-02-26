'*******************************************************************************************************
'  ConnectionParameters.vb - FNet specific connection parameters
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/26/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports System.Runtime.Serialization
Imports Tva.Phasors.FNet.Common

Namespace FNet

    <Serializable()> _
    Public Class ConnectionParameters

        Inherits ConnectionParametersBase

        Private m_frameRate As Int16
        Private m_nominalFrequency As LineFrequency
        Private m_stationName As String

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Deserialize connection parameters
            m_frameRate = info.GetInt16("frameRate")
            m_nominalFrequency = info.GetValue("nominalFrequency", GetType(LineFrequency))
            m_stationName = info.GetString("stationName")

        End Sub

        Public Sub New()

            m_frameRate = DefaultFrameRate
            m_nominalFrequency = DefaultNominalFrequency

        End Sub

        <Category("Optional Connection Parameters"), Description("Configured frame rate for FNET device."), DefaultValue(DefaultFrameRate)> _
        Public Property FrameRate() As Int16
            Get
                Return m_frameRate
            End Get
            Set(ByVal value As Int16)
                m_frameRate = value
            End Set
        End Property

        <Category("Optional Connection Parameters"), Description("Configured nominal frequency for FNET device."), DefaultValue(DefaultNominalFrequency)> _
        Public Property NominalFrequency() As LineFrequency
            Get
                Return m_nominalFrequency
            End Get
            Set(ByVal value As LineFrequency)
                m_nominalFrequency = value
            End Set
        End Property

        <Category("Optional Connection Parameters"), Description("Station name to use for FNET device.")> _
        Public Property StationName() As String
            Get
                Return m_stationName
            End Get
            Set(ByVal value As String)
                m_stationName = value
            End Set
        End Property

        Public Overrides Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Serialize connection parameters
            info.AddValue("frameRate", m_frameRate)
            info.AddValue("nominalFrequency", m_nominalFrequency, GetType(LineFrequency))
            info.AddValue("stationName", m_stationName)

        End Sub

    End Class

End Namespace
