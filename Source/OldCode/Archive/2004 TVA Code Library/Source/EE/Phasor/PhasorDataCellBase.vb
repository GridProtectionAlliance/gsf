'***********************************************************************
'  PhasorDataCellBase.vb - Phasor data cell base class
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

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a set of phasor values that can be sent or received from a PMU.
    Public MustInherit Class PhasorDataCellBase

        Implements IPhasorDataCell

        Private m_statusFlags As Int16
        Private m_phasorValues As PhasorValueCollection
        Private m_frequencyValue As IFrequencyValue
        Private m_analogValues As AnalogValueCollection
        Private m_digitalValues As DigitalValueCollection

        ' Create phasor data cell from other phasor data cell
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorDataCellType
        ' Dervied class must expose a Public Sub New(ByVal phasorDataCell As IPhasorDataCell)
        Protected Shared Shadows Function CreateFrom(ByVal phasorDataCellType As Type, ByVal phasorDataCell As IPhasorDataCell) As IPhasorDataCell

            Return CType(Activator.CreateInstance(phasorDataCellType, New Object() {phasorDataCell}), IPhasorDataCell)

        End Function

        Protected Sub New(ByVal frequencyValue As IFrequencyValue)

            m_phasorValues = New PhasorValueCollection
            m_frequencyValue = frequencyValue
            m_analogValues = New AnalogValueCollection
            m_digitalValues = New DigitalValueCollection

        End Sub

        Protected Sub New(ByVal statusFlags As Int16, ByVal phasorValues As PhasorValueCollection, ByVal frequencyValue As IFrequencyValue, ByVal analogValues As AnalogValueCollection, ByVal digitalValues As DigitalValueCollection)

            m_statusFlags = statusFlags
            m_phasorValues = phasorValues
            m_frequencyValue = frequencyValue
            m_analogValues = analogValues
            m_digitalValues = digitalValues

        End Sub

        Protected Sub New(ByVal phasorDataCell As IPhasorDataCell)

            Me.New(phasorDataCell.StatusFlags, phasorDataCell.PhasorValues, phasorDataCell.FrequencyValue, phasorDataCell.AnalogValues, phasorDataCell.DigitalValues)

        End Sub

        Public MustOverride ReadOnly Property InheritedType() As System.Type Implements IChannel.InheritedType

        Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
            Get
                Return Me
            End Get
        End Property

        Public Overridable Property StatusFlags() As Int16 Implements IPhasorDataCell.StatusFlags
            Get
                Return m_statusFlags
            End Get
            Set(ByVal Value As Short)
                m_statusFlags = Value
            End Set
        End Property

        Public Overridable ReadOnly Property PhasorValues() As PhasorValueCollection Implements IPhasorDataCell.PhasorValues
            Get
                Return m_phasorValues
            End Get
        End Property

        Public ReadOnly Property FrequencyValue() As IFrequencyValue Implements IPhasorDataCell.FrequencyValue
            Get
                Return m_frequencyValue
            End Get
        End Property

        Public Overridable ReadOnly Property AnalogValues() As AnalogValueCollection Implements IPhasorDataCell.AnalogValues
            Get
                Return m_analogValues
            End Get
        End Property

        Public Overridable ReadOnly Property DigitalValues() As DigitalValueCollection Implements IPhasorDataCell.DigitalValues
            Get
                Return m_digitalValues
            End Get
        End Property

        Public Overridable ReadOnly Property BinaryLength() As Short Implements IChannel.BinaryLength
            Get
                Return 2 + m_frequencyValue.BinaryLength + m_phasorValues.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength
            End Get
        End Property

        Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IChannel.BinaryImage
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                EndianOrder.SwapCopyBytes(m_statusFlags, buffer, 0)
                index = 2

                For x As Integer = 0 To m_phasorValues.Count - 1
                    Array.Copy(m_phasorValues(x).BinaryImage, 0, buffer, index, m_phasorValues(x).BinaryLength)
                    index += m_phasorValues(x).BinaryLength
                Next

                Array.Copy(m_frequencyValue.BinaryImage, 0, buffer, index, m_frequencyValue.BinaryLength)
                index += m_frequencyValue.BinaryLength

                For x As Integer = 0 To m_analogValues.Count - 1
                    Array.Copy(m_analogValues(x).BinaryImage, 0, buffer, index, m_analogValues(x).BinaryLength)
                    index += m_analogValues(x).BinaryLength
                Next

                For x As Integer = 0 To m_digitalValues.Count - 1
                    Array.Copy(m_digitalValues(x).BinaryImage, 0, buffer, index, m_digitalValues(x).BinaryLength)
                    index += m_digitalValues(x).BinaryLength
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace