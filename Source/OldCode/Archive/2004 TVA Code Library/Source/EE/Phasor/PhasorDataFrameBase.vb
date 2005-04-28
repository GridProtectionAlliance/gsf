'***********************************************************************
'  PhasorDataFrameBase.vb - Phasor data frame base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
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
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a phasor data frame that can be sent or received from a PMU.
    Public MustInherit Class PhasorDataFrameBase

        Inherits ChannelFrameBase
        Implements IPhasorDataFrame

        Private m_phasorFormat As PhasorFormat
        Private m_statusFlags As Int16
        Private m_phasorValues As PhasorValues
        Private m_frequencyValue As IFrequencyValue
        Private m_digitalValues As DigitalValues

        ' Create phasor data frame from other phasor data frame
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorDataFrameType
        ' Dervied class must expose a Public Sub New(ByVal phasorDataFrame As IPhasorDataFrame)
        Protected Shared Shadows Function CreateFrom(ByVal phasorDataFrameType As Type, ByVal phasorDataFrame As IPhasorDataFrame) As IPhasorDataFrame

            Return CType(Activator.CreateInstance(phasorDataFrameType, New Object() {phasorDataFrame}), IPhasorDataFrame)

        End Function

        Protected Sub New(ByVal frequencyValue As IFrequencyValue)

            MyBase.New()

            m_phasorFormat = PhasorFormat.Rectangular
            m_phasorValues = New PhasorValues
            m_frequencyValue = frequencyValue
            m_digitalValues = New DigitalValues

        End Sub

        Protected Sub New(ByVal timeTag As NtpTimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal dataImage As Byte(), ByVal phasorFormat As PhasorFormat, ByVal statusFlags As Int16, ByVal phasorValues As PhasorValues, ByVal frequencyValue As IFrequencyValue, ByVal digitalValues As DigitalValues)

            MyBase.New(timeTag, milliseconds, synchronizationIsValid, dataIsValid, dataImage)

            m_phasorFormat = phasorFormat
            m_statusFlags = statusFlags
            m_phasorValues = phasorValues
            m_frequencyValue = frequencyValue
            m_digitalValues = digitalValues

        End Sub

        Protected Sub New(ByVal phasorDataFrame As IPhasorDataFrame)

            Me.New(phasorDataFrame.TimeTag, phasorDataFrame.Milliseconds, phasorDataFrame.SynchronizationIsValid, phasorDataFrame.DataIsValid, _
                    phasorDataFrame.DataImage, phasorDataFrame.PhasorFormat, phasorDataFrame.StatusFlags, phasorDataFrame.PhasorValues, _
                    phasorDataFrame.FrequencyValue, phasorDataFrame.DigitalValues)

        End Sub

        Public Overridable Property PhasorFormat() As PhasorFormat Implements IPhasorDataFrame.PhasorFormat
            Get
                Return m_phasorFormat
            End Get
            Set(ByVal Value As PhasorFormat)
                m_phasorFormat = Value
            End Set
        End Property

        Public Overridable Property StatusFlags() As Int16 Implements IPhasorDataFrame.StatusFlags
            Get
                Return m_statusFlags
            End Get
            Set(ByVal Value As Short)
                m_statusFlags = Value
            End Set
        End Property

        Public Overridable ReadOnly Property PhasorValues() As PhasorValues Implements IPhasorDataFrame.PhasorValues
            Get
                Return m_phasorValues
            End Get
        End Property

        Public ReadOnly Property FrequencyValue() As IFrequencyValue Implements IPhasorDataFrame.FrequencyValue
            Get
                Return m_frequencyValue
            End Get
        End Property

        Public Overridable ReadOnly Property DigitalValues() As DigitalValues Implements IPhasorDataFrame.DigitalValues
            Get
                Return m_digitalValues
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TVA.EE.Phasor.DataFrameBase"
            End Get
        End Property

    End Class

End Namespace