'*******************************************************************************************************
'  DataCellBase.vb - Data cell base class
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

Imports System.Buffer
Imports Tva.Interop
Imports Tva.Phasors.Common

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

    Protected Sub New(ByVal parent As IDataFrame, ByVal alignOnDWordBoundry As Boolean, ByVal configurationCell As IConfigurationCell, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer)

        MyBase.New(parent, alignOnDWordBoundry)

        m_configurationCell = configurationCell
        m_phasorValues = New PhasorValueCollection(maximumPhasors)
        m_analogValues = New AnalogValueCollection(maximumAnalogs)
        m_digitalValues = New DigitalValueCollection(maximumDigitals)

    End Sub

    Protected Sub New(ByVal parent As IDataFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer, ByVal state As IDataCellParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(parent, alignOnDWordBoundry, state.ConfigurationCell, maximumPhasors, maximumAnalogs, maximumDigitals)
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    Protected Sub New(ByVal parent As IDataFrame, ByVal alignOnDWordBoundry As Boolean, ByVal configurationCell As IConfigurationCell, ByVal statusFlags As Int16, ByVal phasorValues As PhasorValueCollection, ByVal frequencyValue As IFrequencyValue, ByVal analogValues As AnalogValueCollection, ByVal digitalValues As DigitalValueCollection)

        MyBase.New(parent, alignOnDWordBoundry)

        m_configurationCell = configurationCell
        m_statusFlags = statusFlags
        m_phasorValues = phasorValues
        m_frequencyValue = frequencyValue
        m_analogValues = analogValues
        m_digitalValues = digitalValues

    End Sub

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

    ' Derived classes are expected to expose a Public Sub New(ByVal dataCell As IDataCell)
    Protected Sub New(ByVal dataCell As IDataCell)

        MyClass.New(dataCell.Parent, dataCell.AlignOnDWordBoundry, dataCell.ConfigurationCell, dataCell.StatusFlags, dataCell.PhasorValues, _
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
        Set(ByVal value As IConfigurationCell)
            m_configurationCell = Value
        End Set
    End Property

    Public Overridable Property StatusFlags() As Int16 Implements IDataCell.StatusFlags
        Get
            Return m_statusFlags
        End Get
        Set(ByVal value As Int16)
            m_statusFlags = Value
        End Set
    End Property

    Public ReadOnly Property AllValuesAreEmpty() As Boolean Implements IDataCell.AllValuesAreEmpty
        Get
            Return (PhasorValues.AllValuesAreEmpty And FrequencyValue.IsEmpty And AnalogValues.AllValuesAreEmpty And DigitalValues.AllValuesAreEmpty)
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
        Set(ByVal value As IFrequencyValue)
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

    Protected Overrides ReadOnly Property BodyLength() As Int16
        Get
            Return 2 + m_phasorValues.BinaryLength + m_frequencyValue.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
            Dim index As Integer

            ' Copy in common cell image
            EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index)
            index += 2

            CopyImage(m_phasorValues, buffer, index)
            CopyImage(m_frequencyValue, buffer, index)
            CopyImage(m_analogValues, buffer, index)
            CopyImage(m_digitalValues, buffer, index)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Dim parsingState As IDataCellParsingState = state
        Dim x As Integer

        m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
        startIndex += 2

        ' By the very nature of the three protocols supporting the same order of phasors, frequency, dfreq, analog and digitals
        ' we are able to "automatically" parse this data out in the data cell base class - BEAUTIFUL!!!
        With m_configurationCell
            For x = 0 To .PhasorDefinitions.Count - 1
                m_phasorValues.Add(Activator.CreateInstance(parsingState.PhasorValueType, New Object() {Me, .PhasorDefinitions(x), binaryImage, startIndex}))
                startIndex += m_phasorValues(x).BinaryLength
            Next

            m_frequencyValue = Activator.CreateInstance(parsingState.FrequencyValueType, New Object() {Me, .FrequencyDefinition, binaryImage, startIndex})
            startIndex += m_frequencyValue.BinaryLength

            For x = 0 To .AnalogDefinitions.Count - 1
                m_analogValues.Add(Activator.CreateInstance(parsingState.AnalogValueType, New Object() {Me, .AnalogDefinitions(x), binaryImage, startIndex}))
                startIndex += m_analogValues(x).BinaryLength
            Next

            For x = 0 To .DigitalDefinitions.Count - 1
                m_digitalValues.Add(Activator.CreateInstance(parsingState.DigitalValueType, New Object() {Me, .DigitalDefinitions(x), binaryImage, startIndex}))
                startIndex += m_digitalValues(x).BinaryLength
            Next
        End With

    End Sub

End Class
