'*******************************************************************************************************
'  DataCellBase.vb - Data cell base class
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.Buffer
Imports PhasorProtocols.Common
Imports TVA.Measurements

''' <summary>This class represents the protocol independent common implementation of a set of phasor related data values that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class DataCellBase

    Inherits ChannelCellBase
    Implements IDataCell

    Private m_configurationCell As IConfigurationCell
    Private m_statusFlags As Int16
    Private m_phasorValues As PhasorValueCollection
    Private m_frequencyValue As IFrequencyValue
    Private m_analogValues As AnalogValueCollection
    Private m_digitalValues As DigitalValueCollection

#Region " IMeasurement Implementation Members "

    Private m_id As Integer
    Private m_source As String
    Private m_key As MeasurementKey
    Private m_tagName As String
    Private m_ticks As Long
    Private m_adder As Double
    Private m_multiplier As Double

#End Region

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize data cell values
        m_configurationCell = info.GetValue("configurationCell", GetType(IConfigurationCell))
        m_statusFlags = info.GetInt16("statusFlags")
        m_phasorValues = info.GetValue("phasorValues", GetType(PhasorValueCollection))
        m_frequencyValue = info.GetValue("frequencyValue", GetType(IFrequencyValue))
        m_analogValues = info.GetValue("analogValues", GetType(AnalogValueCollection))
        m_digitalValues = info.GetValue("digitalValues", GetType(DigitalValueCollection))

    End Sub

    Protected Sub New(ByVal parent As IDataFrame, ByVal alignOnDWordBoundry As Boolean, ByVal configurationCell As IConfigurationCell, ByVal maximumPhasors As Int32, ByVal maximumAnalogs As Int32, ByVal maximumDigitals As Int32)

        MyBase.New(parent, alignOnDWordBoundry)

        m_configurationCell = configurationCell
        m_statusFlags = -1
        m_phasorValues = New PhasorValueCollection(maximumPhasors)
        m_analogValues = New AnalogValueCollection(maximumAnalogs)
        m_digitalValues = New DigitalValueCollection(maximumDigitals)

        ' Initialize IMeasurement members
        m_id = -1
        m_source = "__"
        m_key = UndefinedKey
        m_ticks = -1
        m_adder = 0.0R
        m_multiplier = 1.0R

    End Sub

    Protected Sub New(ByVal parent As IDataFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Int32, ByVal maximumAnalogs As Int32, ByVal maximumDigitals As Int32, ByVal state As IDataCellParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

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

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

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

    ' "This" method exists in two inherited interfaces, so we shadow method to avoid ambiguity
    Public Shadows ReadOnly Property This() As IDataCell Implements IDataCell.This
        Get
            Return Me
        End Get
    End Property

    Public Overridable Property ConfigurationCell() As IConfigurationCell Implements IDataCell.ConfigurationCell
        Get
            Return m_configurationCell
        End Get
        Set(ByVal value As IConfigurationCell)
            m_configurationCell = value
        End Set
    End Property

    Public Overridable ReadOnly Property StationName() As String Implements IDataCell.StationName
        Get
            Return m_configurationCell.StationName
        End Get
    End Property

    Public Overridable ReadOnly Property IDLabel() As String Implements IDataCell.IDLabel
        Get
            Return m_configurationCell.IDLabel
        End Get
    End Property

    Public Overrides Property IDCode() As UInt16
        Get
            Return m_configurationCell.IDCode
        End Get
        Set(ByVal value As UInt16)
            Throw New NotSupportedException("Cannot change IDCode of a data cell, change IDCode is associated configuration cell instead")
        End Set
    End Property

    Public Overridable Property StatusFlags() As Int16 Implements IDataCell.StatusFlags
        Get
            Return m_statusFlags
        End Get
        Set(ByVal value As Int16)
            m_statusFlags = value
        End Set
    End Property

    Public Property CommonStatusFlags() As Int32 Implements IDataCell.CommonStatusFlags
        Get
            ' Start with lo-word protocol specific flags
            Dim commonFlags As Int32 = StatusFlags

            ' Add hi-word protocol independent common flags
            If Not DataIsValid Then commonFlags = (commonFlags Or PhasorProtocols.CommonStatusFlags.DataIsValid)
            If Not SynchronizationIsValid Then commonFlags = (commonFlags Or PhasorProtocols.CommonStatusFlags.SynchronizationIsValid)
            If DataSortingType <> PhasorProtocols.DataSortingType.ByTimestamp Then commonFlags = (commonFlags Or PhasorProtocols.CommonStatusFlags.DataSortingType)
            If PmuError Then commonFlags = (commonFlags Or PhasorProtocols.CommonStatusFlags.PmuError)

            Return commonFlags
        End Get
        Set(ByVal value As Int32)
            ' Set lo-word protocol specific status flags
            m_statusFlags = Convert.ToInt16(value And &HFFFF)

            ' Derive common states via common status flags
            DataIsValid = ((value And PhasorProtocols.CommonStatusFlags.DataIsValid) = 0)
            SynchronizationIsValid = ((value And PhasorProtocols.CommonStatusFlags.SynchronizationIsValid) = 0)
            DataSortingType = IIf((value And PhasorProtocols.CommonStatusFlags.DataSortingType) = 0, PhasorProtocols.DataSortingType.ByTimestamp, PhasorProtocols.DataSortingType.ByArrival)
            PmuError = ((value And PhasorProtocols.CommonStatusFlags.PmuError) > 0)
        End Set
    End Property

    Public MustOverride Property DataIsValid() As Boolean Implements IDataCell.DataIsValid, IMeasurement.ValueQualityIsGood

    Public MustOverride Property SynchronizationIsValid() As Boolean Implements IDataCell.SynchronizationIsValid, IMeasurement.TimestampQualityIsGood

    Public MustOverride Property DataSortingType() As DataSortingType Implements IDataCell.DataSortingType

    Public MustOverride Property PmuError() As Boolean Implements IDataCell.PmuError

    Public Overridable ReadOnly Property AllValuesAssigned() As Boolean Implements IDataCell.AllValuesAssigned
        Get
            Return ( _
                PhasorValues.AllValuesAssigned AndAlso _
                (Not FrequencyValue.IsEmpty) AndAlso _
                AnalogValues.AllValuesAssigned AndAlso _
                DigitalValues.AllValuesAssigned)
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
            m_frequencyValue = value
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

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return 2 + m_phasorValues.BinaryLength + m_frequencyValue.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)
            Dim index As Int32

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

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        Dim parsingState As IDataCellParsingState = DirectCast(state, IDataCellParsingState)
        Dim x As Int32

        StatusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
        startIndex += 2

        ' By the very nature of the three protocols supporting the same order of phasors, frequency, dfreq, analog and digitals
        ' we are able to "automatically" parse this data out in the data cell base class - BEAUTIFUL!!!
        ' Parse out phasor values
        For x = 0 To parsingState.PhasorCount - 1
            m_phasorValues.Add(parsingState.CreateNewPhasorValueFunction.Invoke(Me, m_configurationCell.PhasorDefinitions(x), binaryImage, startIndex))
            startIndex += m_phasorValues(x).BinaryLength
        Next

        ' Parse out frequency and df/dt values
        m_frequencyValue = parsingState.CreateNewFrequencyValueFunction.Invoke(Me, m_configurationCell.FrequencyDefinition, binaryImage, startIndex)
        startIndex += m_frequencyValue.BinaryLength

        ' Parse out analog values
        For x = 0 To parsingState.AnalogCount - 1
            m_analogValues.Add(parsingState.CreateNewAnalogValueFunction.Invoke(Me, m_configurationCell.AnalogDefinitions(x), binaryImage, startIndex))
            startIndex += m_analogValues(x).BinaryLength
        Next

        ' Parse out digital values
        For x = 0 To parsingState.DigitalCount - 1
            m_digitalValues.Add(parsingState.CreateNewDigitalValueFunction.Invoke(Me, m_configurationCell.DigitalDefinitions(x), binaryImage, startIndex))
            startIndex += m_digitalValues(x).BinaryLength
        Next

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize data cell values
        info.AddValue("configurationCell", m_configurationCell, GetType(IConfigurationCell))
        info.AddValue("statusFlags", m_statusFlags)
        info.AddValue("phasorValues", m_phasorValues, GetType(PhasorValueCollection))
        info.AddValue("frequencyValue", m_frequencyValue, GetType(IFrequencyValue))
        info.AddValue("analogValues", m_analogValues, GetType(AnalogValueCollection))
        info.AddValue("digitalValues", m_digitalValues, GetType(DigitalValueCollection))

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Station Name", StationName)
            baseAttributes.Add("ID Label", IDLabel)
            baseAttributes.Add("Status Flags", StatusFlags)
            baseAttributes.Add("Data Is Valid", DataIsValid)
            baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid)
            baseAttributes.Add("Data Sorting Type", [Enum].GetName(GetType(DataSortingType), DataSortingType))
            baseAttributes.Add("PMU Error", PmuError)
            baseAttributes.Add("Total Phasor Values", PhasorValues.Count)
            baseAttributes.Add("Total Analog Values", AnalogValues.Count)
            baseAttributes.Add("Total Digital Values", DigitalValues.Count)
            baseAttributes.Add("All Values Assigned", AllValuesAssigned)

            Return baseAttributes
        End Get
    End Property

#Region " IMeasurement Implementation "

    ' We keep the IMeasurement implementation of the DataCell completely private.  Exposing
    ' these properties publically would only stand to add confusion as to where measurements
    ' typically come from (i.e., the IDataCell's values) - the only value the cell itself has
    ' to offer is the "CommonStatusFlags" property, which we expose below

    Private Property IMeasurementValue() As Double Implements IMeasurement.Value
        Get
            Return Convert.ToDouble(CommonStatusFlags)
        End Get
        Set(ByVal value As Double)
            CommonStatusFlags = Convert.ToInt32(value)
        End Set
    End Property

    ' The only "measured value" a data cell exposes is its "StatusFlags"
    Private ReadOnly Property IMeasurementAdjustedValue() As Double Implements IMeasurement.AdjustedValue
        Get
            Return CommonStatusFlags * m_multiplier + m_adder
        End Get
    End Property

    ' I don't imagine you would want offsets for status flags - but this may yet be handy for
    ' "forcing" a particular set of quality flags to come through the system (M=0, A=New Flags)
    Private Property IMeasurementAdder() As Double Implements IMeasurement.Adder
        Get
            Return m_adder
        End Get
        Set(ByVal value As Double)
            m_adder = value
        End Set
    End Property

    Private Property IMeasurementMultiplier() As Double Implements IMeasurement.Multiplier
        Get
            Return m_multiplier
        End Get
        Set(ByVal value As Double)
            m_multiplier = value
        End Set
    End Property

    Private Property IMeasurementTicks() As Long Implements IMeasurement.Ticks
        Get
            If m_ticks = -1 Then m_ticks = Parent.Ticks
            Return m_ticks
        End Get
        Set(ByVal value As Long)
            m_ticks = value
        End Set
    End Property

    Private ReadOnly Property IMeasurementTimestamp() As Date Implements IMeasurement.Timestamp
        Get
            Dim ticks As Long = IMeasurementTicks

            If ticks = -1 Then
                Return Date.MinValue
            Else
                Return New Date(ticks)
            End If
        End Get
    End Property

    Private Property IMeasurementID() As Integer Implements IMeasurement.ID
        Get
            Return m_id
        End Get
        Set(ByVal value As Integer)
            m_id = value
        End Set
    End Property

    Private Property IMeasurementSource() As String Implements IMeasurement.Source
        Get
            Return m_source
        End Get
        Set(ByVal value As String)
            m_source = value
        End Set
    End Property

    Private ReadOnly Property IMeasurementKey() As MeasurementKey Implements IMeasurement.Key
        Get
            If m_key.Equals(UndefinedKey) Then m_key = New MeasurementKey(m_id, m_source)
            Return m_key
        End Get
    End Property

    Private Property IMeasurementTagName() As String Implements IMeasurement.TagName
        Get
            Return m_tagName
        End Get
        Set(ByVal value As String)
            m_tagName = value
        End Set
    End Property

    Private ReadOnly Property IMeasurementThis() As IMeasurement Implements IMeasurement.This
        Get
            Return Me
        End Get
    End Property

    Private Function IComparableCompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is IMeasurement Then Return IComparableCompareTo(DirectCast(obj, IMeasurement))
        Throw New ArgumentException(DerivedType.Name & " measurement can only be compared with other IMeasurements...")

    End Function

    Private Function IComparableCompareTo(ByVal other As Measurements.IMeasurement) As Integer Implements System.IComparable(Of Measurements.IMeasurement).CompareTo

        Return IMeasurementValue.CompareTo(other.Value)

    End Function

    Private Function IEquatableEquals(ByVal other As Measurements.IMeasurement) As Boolean Implements System.IEquatable(Of Measurements.IMeasurement).Equals

        Return (IComparableCompareTo(other) = 0)

    End Function

#End Region

End Class