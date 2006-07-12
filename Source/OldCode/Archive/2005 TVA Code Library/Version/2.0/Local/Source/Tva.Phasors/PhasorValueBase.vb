'*******************************************************************************************************
'  PhasorValueBase.vb - Phasor value base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Note: Phasors are stored in rectangular format internally
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.Math

' This class represents the protocol independent representation of a phasor value.
<CLSCompliant(False)> _
Public MustInherit Class PhasorValueBase

    Inherits ChannelValueBase(Of IPhasorDefinition)
    Implements IPhasorValue

    Protected Delegate Function CreateNewPhasorValueFunctionSignature(ByVal parent As IDataCell, ByVal phasorDefintion As IPhasorDefinition, ByVal real As Single, ByVal imaginary As Single) As IPhasorValue

    Private m_real As Single
    Private m_imaginary As Single
    Private m_compositeValues As CompositeValues

    ' Create phasor from polar coordinates (angle expected in Degrees)
    ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction
    Protected Shared Function CreateFromPolarValues(ByVal createNewPhasorValueFunction As CreateNewPhasorValueFunctionSignature, ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal angle As Single, ByVal magnitude As Single) As IPhasorValue

        Return CreateFromRectangularValues(createNewPhasorValueFunction, parent, phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude))

    End Function

    ' Create phasor from rectangular coordinates
    ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction
    Protected Shared Function CreateFromRectangularValues(ByVal createNewPhasorValueFunction As CreateNewPhasorValueFunctionSignature, ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Single, ByVal imaginary As Single) As IPhasorValue

        If phasorDefinition Is Nothing Then Throw New ArgumentNullException("No phasor definition specified")
        Return createNewPhasorValueFunction.Invoke(parent, phasorDefinition, real, imaginary)

    End Function

    ' Create phasor from unscaled rectangular coordinates
    ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction
    Protected Shared Function CreateFromUnscaledRectangularValues(ByVal createNewPhasorValueFunction As CreateNewPhasorValueFunctionSignature, ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16) As IPhasorValue

        Dim factor As Single = phasorDefinition.ConversionFactor
        Return CreateFromRectangularValues(createNewPhasorValueFunction, parent, phasorDefinition, real * factor, imaginary * factor)

    End Function

    ' Gets real component from angle (in Degrees) and magnitude
    Public Shared Function CalculateRealComponent(ByVal angle As Single, ByVal magnitude As Single) As Single

        Return magnitude * System.Math.Cos(angle * System.Math.PI / 180)

    End Function

    ' Gets imaginary component from angle (in Degrees) and magnitude
    Public Shared Function CalculateImaginaryComponent(ByVal angle As Single, ByVal magnitude As Single) As Single

        Return magnitude * System.Math.Sin(angle * System.Math.PI / 180)

    End Function

    ' Calculate watts from imaginary and real components of two phasors
    Public Shared Function CalculatePower(ByVal voltage As IPhasorValue, ByVal current As IPhasorValue) As Single

        If voltage Is Nothing Then Throw New ArgumentNullException("No voltage specified")
        If current Is Nothing Then Throw New ArgumentNullException("No current specified")

        Return 3 * (voltage.Real * current.Real + voltage.Imaginary * current.Imaginary)
        'Return 3 * voltage.Magnitude * current.Magnitude * System.Math.Cos((voltage.Angle - current.Angle) * System.Math.PI / 180)

    End Function

    ' Calculate vars from imaginary and real components of two phasors
    Public Shared Function CalculateVars(ByVal voltage As IPhasorValue, ByVal current As IPhasorValue) As Single

        If voltage Is Nothing Then Throw New ArgumentNullException("No voltage specified")
        If current Is Nothing Then Throw New ArgumentNullException("No current specified")

        Return 3 * (voltage.Imaginary * current.Real - voltage.Real * current.Imaginary)
        'Return 3 * voltage.Magnitude * current.Magnitude * System.Math.Sin((voltage.Angle - current.Angle) * System.Math.PI / 180)

    End Function

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize phasor value
        m_real = info.GetSingle("real")
        m_imaginary = info.GetSingle("imaginary")

    End Sub

    Protected Sub New(ByVal parent As IDataCell)

        MyBase.New(parent)

        m_compositeValues = New CompositeValues(2)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Single, ByVal imaginary As Single)
    Protected Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Single, ByVal imaginary As Single)

        MyBase.New(parent, phasorDefinition)

        m_real = real
        m_imaginary = imaginary
        m_compositeValues = New CompositeValues(2)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal unscaledReal As Int16, ByVal unscaledImaginary As Int16)
    Protected Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal unscaledReal As Int16, ByVal unscaledImaginary As Int16)

        MyClass.New(parent, phasorDefinition, unscaledReal * phasorDefinition.ConversionFactor, unscaledImaginary * phasorDefinition.ConversionFactor)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, phasorDefinition)
        ParseBinaryImage(Nothing, binaryImage, startIndex)
        m_compositeValues = New CompositeValues(2)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal phasorValue As IPhasorValue)
    Protected Sub New(ByVal phasorValue As IPhasorValue)

        MyClass.New(phasorValue.Parent, phasorValue.Definition, phasorValue.Real, phasorValue.Imaginary)

    End Sub

    Public Overridable ReadOnly Property CoordinateFormat() As CoordinateFormat Implements IPhasorValue.CoordinateFormat
        Get
            Return Definition.CoordinateFormat
        End Get
    End Property

    Public Overridable ReadOnly Property Type() As PhasorType Implements IPhasorValue.Type
        Get
            Return Definition.Type
        End Get
    End Property

    Public Overridable Property Angle() As Single Implements IPhasorValue.Angle
        Get
            Return System.Math.Atan2(m_imaginary, m_real) * 180 / System.Math.PI
        End Get
        Set(ByVal value As Single)
            ' We store angle as one of our required composite values
            m_compositeValues(CompositePhasorValue.Angle) = value

            ' If all composite values have been received, we can calculate phasor's real and imaginary values
            CalculatePhasorValueFromComposites()
        End Set
    End Property

    Public Overridable ReadOnly Property AngleReceived() As Boolean
        Get
            Return m_compositeValues.Received(CompositePhasorValue.Angle)
        End Get
    End Property

    Public Overridable Property Magnitude() As Single Implements IPhasorValue.Magnitude
        Get
            Return System.Math.Sqrt(m_real * m_real + m_imaginary * m_imaginary)
        End Get
        Set(ByVal value As Single)
            m_compositeValues(CompositePhasorValue.Magnitude) = value

            ' If all composite values have been received, we can calculate phasor's real and imaginary values
            CalculatePhasorValueFromComposites()
        End Set
    End Property

    Public Overridable ReadOnly Property MagnitudeReceived() As Boolean
        Get
            Return m_compositeValues.Received(CompositePhasorValue.Magnitude)
        End Get
    End Property

    Private Sub CalculatePhasorValueFromComposites()

        If m_compositeValues.AllReceived Then
            Dim angle, magnitude As Single

            ' All values received, create a new phasor value from composite values
            angle = m_compositeValues(CompositePhasorValue.Angle)
            magnitude = m_compositeValues(CompositePhasorValue.Magnitude)

            m_real = CalculateRealComponent(angle, magnitude)
            m_imaginary = CalculateImaginaryComponent(angle, magnitude)
        End If

    End Sub

    Public Overridable Property Real() As Single Implements IPhasorValue.Real
        Get
            Return m_real
        End Get
        Set(ByVal value As Single)
            m_real = value
        End Set
    End Property

    Public Overridable Property Imaginary() As Single Implements IPhasorValue.Imaginary
        Get
            Return m_imaginary
        End Get
        Set(ByVal value As Single)
            m_imaginary = value
        End Set
    End Property

    Public Overridable Property UnscaledReal() As Int16 Implements IPhasorValue.UnscaledReal
        Get
            Return Convert.ToInt16(m_real / Definition.ConversionFactor)
        End Get
        Set(ByVal value As Int16)
            m_real = value * Definition.ConversionFactor
        End Set
    End Property

    Public Overridable Property UnscaledImaginary() As Int16 Implements IPhasorValue.UnscaledImaginary
        Get
            Return Convert.ToInt16(m_imaginary / Definition.ConversionFactor)
        End Get
        Set(ByVal value As Int16)
            m_imaginary = value * Definition.ConversionFactor
        End Set
    End Property

    Default Public Overrides Property CompositeValue(ByVal index As Integer) As Single
        Get
            Select Case index
                Case CompositePhasorValue.Angle
                    Return Angle
                Case CompositePhasorValue.Magnitude
                    Return Magnitude
                Case Else
                    Throw New IndexOutOfRangeException("Specified phasor value composite index, " & index & ", is out of range - there are only two composite values for a phasor value: angle (0) and magnitude (1)")
            End Select
        End Get
        Set(ByVal value As Single)
            Select Case index
                Case CompositePhasorValue.Angle
                    Angle = value
                Case CompositePhasorValue.Magnitude
                    Magnitude = value
                Case Else
                    Throw New IndexOutOfRangeException("Specified phasor value composite index, " & index & ", is out of range - there are only two composite values for a phasor value: angle (0) and magnitude (1)")
            End Select
        End Set
    End Property

    Public Overrides ReadOnly Property CompositeValueCount() As Integer
        Get
            Return 2
        End Get
    End Property

    Public Overrides ReadOnly Property IsEmpty() As Boolean
        Get
            Return (m_real = 0 And m_imaginary = 0)
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            If DataFormat = Phasors.DataFormat.FixedInteger Then
                Return 4
            Else
                Return 8
            End If
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)

            If CoordinateFormat = Phasors.CoordinateFormat.Rectangular Then
                If DataFormat = Phasors.DataFormat.FixedInteger Then
                    EndianOrder.BigEndian.CopyBytes(UnscaledReal, buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(UnscaledImaginary, buffer, 2)
                Else
                    EndianOrder.BigEndian.CopyBytes(m_real, buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(m_imaginary, buffer, 4)
                End If
            Else
                If DataFormat = Phasors.DataFormat.FixedInteger Then
                    EndianOrder.BigEndian.CopyBytes(Convert.ToUInt16(Magnitude), buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Angle * System.Math.PI / 180 * 10000), buffer, 2)
                Else
                    EndianOrder.BigEndian.CopyBytes(Magnitude, buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(Convert.ToSingle(Angle * System.Math.PI / 180), buffer, 4)
                End If
            End If

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        If CoordinateFormat = Phasors.CoordinateFormat.Rectangular Then
            If DataFormat = Phasors.DataFormat.FixedInteger Then
                UnscaledReal = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
                UnscaledImaginary = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            Else
                m_real = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
                m_imaginary = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4)
            End If
        Else
            Dim magnitude As Single
            Dim angle As Single

            If DataFormat = Phasors.DataFormat.FixedInteger Then
                magnitude = Convert.ToSingle(EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex))
                angle = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2) * 180 / System.Math.PI / 10000
            Else
                magnitude = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
                angle = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4) * 180 / System.Math.PI
            End If

            m_real = CalculateRealComponent(angle, magnitude)
            m_imaginary = CalculateImaginaryComponent(angle, magnitude)
        End If

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize phasor value
        info.AddValue("real", m_real)
        info.AddValue("imaginary", m_imaginary)

    End Sub

End Class
