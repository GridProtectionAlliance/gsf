'***********************************************************************
'  DataCellBase.vb - Data cell base class
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

    ' This class represents the protocol independent common implementation of a set of phasor related data values that can be sent or received from a PMU.
    Public MustInherit Class DataCellBase

        Inherits ChannelCellBase
        Implements IDataCell

        Private m_configurationCell As IConfigurationCell
        Private m_statusFlags As Int16
        Private m_phasorValues As PhasorValueCollection
        Private m_frequencyValue As IFrequencyValue
        Private m_analogValues As AnalogValueCollection
        Private m_digitalValues As DigitalValueCollection

        Protected Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell)

            MyBase.New(parent)

            m_configurationCell = configurationCell
            m_phasorValues = New PhasorValueCollection
            m_analogValues = New AnalogValueCollection
            m_digitalValues = New DigitalValueCollection

        End Sub

        Protected Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal statusFlags As Int16, ByVal phasorValues As PhasorValueCollection, ByVal frequencyValue As IFrequencyValue, ByVal analogValues As AnalogValueCollection, ByVal digitalValues As DigitalValueCollection)

            MyBase.New(parent)

            m_configurationCell = configurationCell
            m_statusFlags = statusFlags
            m_phasorValues = phasorValues
            m_frequencyValue = frequencyValue
            m_analogValues = analogValues
            m_digitalValues = digitalValues

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        ' and automatically pass in type parameters
        Protected Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal phasorValueType As Type, ByVal frequencyValueType As Type, ByVal analogValueType As Type, ByVal digitalValueType As Type)

            Me.New(parent, configurationCell)

            Dim x As Integer

            m_configurationCell = configurationCell
            m_statusFlags = EndianOrder.ReverseToInt16(binaryImage, startIndex)
            startIndex += 2

            With m_configurationCell
                For x = 0 To .PhasorDefinitions.Count - 1
                    m_phasorValues.Add(Activator.CreateInstance(phasorValueType, New Object() {Me, .PhasorDefinitions(x), binaryImage, startIndex}))
                    startIndex += m_phasorValues(x).BinaryLength
                Next

                m_frequencyValue = Activator.CreateInstance(frequencyValueType, New Object() {Me, .FrequencyDefinition, binaryImage, startIndex})
                startIndex += m_frequencyValue.BinaryLength

                For x = 0 To .AnalogDefinitions.Count - 1
                    m_analogValues.Add(Activator.CreateInstance(analogValueType, New Object() {Me, .AnalogDefinitions(x), binaryImage, startIndex}))
                    startIndex += m_analogValues(x).BinaryLength
                Next

                For x = 0 To .DigitalDefinitions.Count - 1
                    m_digitalValues.Add(Activator.CreateInstance(digitalValueType, New Object() {Me, .DigitalDefinitions(x), binaryImage, startIndex}))
                    startIndex += m_digitalValues(x).BinaryLength
                Next
            End With

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal dataCell As IDataCell)
        Protected Sub New(ByVal dataCell As IDataCell)

            Me.New(dataCell.Parent, dataCell.ConfigurationCell, dataCell.StatusFlags, dataCell.PhasorValues, _
                dataCell.FrequencyValue, dataCell.AnalogValues, dataCell.DigitalValues)

        End Sub

        Public Overridable Shadows ReadOnly Property Parent() As IDataFrame Implements IDataCell.Parent
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overridable Property ConfigurationCell() As IConfigurationCell Implements IDataCell.ConfigurationCell
            Get
                Return m_configurationCell
            End Get
            Set(ByVal Value As IConfigurationCell)
                m_configurationCell = Value
            End Set
        End Property

        Public Overridable Property StatusFlags() As Int16 Implements IDataCell.StatusFlags
            Get
                Return m_statusFlags
            End Get
            Set(ByVal Value As Short)
                m_statusFlags = Value
            End Set
        End Property

        Public ReadOnly Property IsEmpty() As Boolean Implements IDataCell.IsEmpty
            Get
                Return (PhasorValues.IsEmpty And FrequencyValue.IsEmpty And AnalogValues.IsEmpty And DigitalValues.IsEmpty)
            End Get
        End Property

        Public Overridable ReadOnly Property PhasorValues() As PhasorValueCollection Implements IDataCell.PhasorValues
            Get
                Return m_phasorValues
            End Get
        End Property

        Public Overridable Property FrequencyValue() As IFrequencyValue Implements IDataCell.FrequencyValue
            Get
                Return m_frequencyValue
            End Get
            Set(ByVal Value As IFrequencyValue)
                m_frequencyValue = Value
            End Set
        End Property

        Public Overridable ReadOnly Property AnalogValues() As AnalogValueCollection Implements IDataCell.AnalogValues
            Get
                Return m_analogValues
            End Get
        End Property

        Public Overridable ReadOnly Property DigitalValues() As DigitalValueCollection Implements IDataCell.DigitalValues
            Get
                Return m_digitalValues
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Short
            Get
                Return 2 + m_frequencyValue.BinaryLength + m_phasorValues.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim x, index As Integer

                EndianOrder.SwapCopyBytes(m_statusFlags, buffer, 0)
                index = 2

                For x = 0 To m_phasorValues.Count - 1
                    CopyImage(m_phasorValues(x), buffer, index)
                Next

                CopyImage(m_frequencyValue, buffer, index)

                For x = 0 To m_analogValues.Count - 1
                    CopyImage(m_analogValues(x), buffer, index)
                Next

                For x = 0 To m_digitalValues.Count - 1
                    CopyImage(m_digitalValues(x), buffer, index)
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace