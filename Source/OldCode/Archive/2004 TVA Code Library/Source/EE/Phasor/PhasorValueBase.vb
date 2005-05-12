'***********************************************************************
'  PhasorValueBase.vb - Phasor value base class
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Math

Namespace EE.Phasor

    ' This class represents the protocol independent definition of a phasor value.
    Public MustInherit Class PhasorValueBase

        Inherits ChannelValueBase
        Implements IPhasorValue

        Private m_phasorFormat As PhasorFormat
        Private m_phasorDefinition As IPhasorDefinition
        Private m_real As Double
        Private m_imaginary As Double
        Private m_compositeValues As CompositeValues

        Private Enum CompositeValue
            Angle
            Magnitude
        End Enum

        ' Create phasor value from other phasor value
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorValueType
        ' Dervied class must expose a Public Sub New(ByVal phasorValue As IPhasorValue)
        Protected Shared Shadows Function CreateFrom(ByVal phasorValueType As Type, ByVal phasorValue As IPhasorValue) As IPhasorValue

            Return CType(Activator.CreateInstance(phasorValueType, New Object() {phasorValue}), IPhasorValue)

        End Function

        ' Create phasor from polar coordinates (angle expected in Degrees)
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorValueType
        Protected Shared Function CreateFromPolarValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal dataFormat As DataFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal angle As Double, ByVal magnitude As Double) As IPhasorValue

            Return CreateFromRectangularValues(phasorValueType, phasorFormat, dataFormat, phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude))

        End Function

        ' Create phasor from rectangular coordinates
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorValueType
        ' Dervied class must expose a Public Sub New(ByVal phasorFormat As PhasorFormat, ByVal dataFormat As DataFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Double, ByVal imaginary As Double)
        Protected Shared Function CreateFromRectangularValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal dataFormat As DataFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Double, ByVal imaginary As Double) As IPhasorValue

            If phasorDefinition Is Nothing Then Throw New ArgumentNullException("No phasor definition specified")
            Return CType(Activator.CreateInstance(phasorValueType, New Object() {phasorFormat, dataFormat, phasorDefinition, real, imaginary}), IPhasorValue)

        End Function

        ' Create phasor from unscaled rectangular coordinates
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorValueType
        Protected Shared Function CreateFromUnscaledRectangularValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal dataFormat As DataFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16) As IPhasorValue

            Dim scale As Integer = phasorDefinition.ScalingFactor
            Return CreateFromRectangularValues(phasorValueType, phasorFormat, dataFormat, phasorDefinition, real / scale, imaginary / scale)

        End Function

        ' Gets real component from angle (in Degrees) and magnitude
        Public Shared Function CalculateRealComponent(ByVal angle As Double, ByVal magnitude As Double) As Double

            Return magnitude * Math.Cos(angle * Math.PI / 180)

        End Function

        ' Gets imaginary component from angle (in Degrees) and magnitude
        Public Shared Function CalculateImaginaryComponent(ByVal angle As Double, ByVal magnitude As Double) As Double

            Return magnitude * Math.Sin(angle * Math.PI / 180)

        End Function

        ' Calculate watts from imaginary and real components of two phasors
        Public Shared Function CalculatePower(ByVal voltage As IPhasorValue, ByVal current As IPhasorValue) As Double

            If voltage Is Nothing Then Throw New ArgumentNullException("No voltage specified")
            If current Is Nothing Then Throw New ArgumentNullException("No current specified")
            Return 3 * (voltage.Real * current.Real + voltage.Imaginary * current.Imaginary)
            'Return 3 * voltage.Magnitude * current.Magnitude * Math.Cos((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        ' Calculate vars from imaginary and real components of two phasors
        Public Shared Function CalculateVars(ByVal voltage As IPhasorValue, ByVal current As IPhasorValue) As Double

            If voltage Is Nothing Then Throw New ArgumentNullException("No voltage specified")
            If current Is Nothing Then Throw New ArgumentNullException("No current specified")
            Return 3 * (voltage.Imaginary * current.Real - voltage.Real * current.Imaginary)
            'Return 3 * voltage.Magnitude * current.Magnitude * Math.Sin((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Double, ByVal imaginary As Double)

            MyBase.New(dataFormat)

            m_phasorFormat = phasorFormat
            m_phasorDefinition = phasorDefinition
            m_real = real
            m_imaginary = imaginary
            m_compositeValues = New CompositeValues(2)

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal unscaledReal As Int16, ByVal unscaledImaginary As Int16)

            Me.New(dataFormat, phasorFormat, phasorDefinition, unscaledReal / phasorDefinition.ScalingFactor, unscaledImaginary / phasorDefinition.ScalingFactor)

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(dataFormat)

            m_phasorFormat = phasorFormat
            m_phasorDefinition = phasorDefinition

            If phasorFormat = EE.Phasor.PhasorFormat.Rectangular Then
                If dataFormat = EE.Phasor.DataFormat.FixedInteger Then
                    UnscaledReal = EndianOrder.ReverseToInt16(binaryImage, startIndex)
                    UnscaledImaginary = EndianOrder.ReverseToInt16(binaryImage, startIndex + 2)
                Else
                    m_real = EndianOrder.ReverseToSingle(binaryImage, startIndex)
                    m_imaginary = EndianOrder.ReverseToSingle(binaryImage, startIndex + 4)
                End If
            Else
                Dim magnitude As Double
                Dim angle As Double

                If dataFormat = EE.Phasor.DataFormat.FixedInteger Then
                    magnitude = Convert.ToDouble(EndianOrder.ReverseToUInt16(binaryImage, startIndex))
                    angle = EndianOrder.ReverseToInt16(binaryImage, startIndex + 2) * 180 / Math.PI / 10000
                Else
                    magnitude = EndianOrder.ReverseToSingle(binaryImage, startIndex)
                    angle = EndianOrder.ReverseToSingle(binaryImage, startIndex + 4) * 180 / Math.PI
                End If

                m_real = CalculateRealComponent(angle, magnitude)
                m_imaginary = CalculateImaginaryComponent(angle, magnitude)
            End If

            m_compositeValues = New CompositeValues(2)

        End Sub

        Protected Sub New(ByVal phasorValue As IPhasorValue)

            Me.New(phasorValue.DataFormat, phasorValue.PhasorFormat, phasorValue.Definition, phasorValue.Real, phasorValue.Imaginary)

        End Sub

        Public Overridable Property PhasorFormat() As PhasorFormat Implements IPhasorValue.PhasorFormat
            Get
                Return m_phasorFormat
            End Get
            Set(ByVal Value As PhasorFormat)
                m_phasorFormat = Value
            End Set
        End Property

        Public Overridable ReadOnly Property Definition() As IPhasorDefinition Implements IPhasorValue.Definition
            Get
                Return m_phasorDefinition
            End Get
        End Property

        Public Overridable Property Angle() As Double Implements IPhasorValue.Angle
            Get
                Return Math.Atan2(m_imaginary, m_real) * 180 / Math.PI
            End Get
            Set(ByVal Value As Double)
                ' We store angle as one of our required composite values
                m_compositeValues(CompositeValue.Angle) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValueFromComposites()
            End Set
        End Property

        Public Overridable Property Magnitude() As Double Implements IPhasorValue.Magnitude
            Get
                Return Math.Sqrt(m_real * m_real + m_imaginary * m_imaginary)
            End Get
            Set(ByVal Value As Double)
                ' We store magnitude as one of our required composite values
                m_compositeValues(CompositeValue.Magnitude) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValueFromComposites()
            End Set
        End Property

        Private Sub CalculatePhasorValueFromComposites()

            If m_compositeValues.AllReceived Then
                Dim angle, magnitude As Double

                ' All values received, create a new phasor value from composite values
                angle = m_compositeValues(CompositeValue.Angle)
                magnitude = m_compositeValues(CompositeValue.Magnitude)

                m_real = CalculateRealComponent(angle, magnitude)
                m_imaginary = CalculateImaginaryComponent(angle, magnitude)
            End If

        End Sub

        Public Overridable Property Real() As Double Implements IPhasorValue.Real
            Get
                Return m_real
            End Get
            Set(ByVal Value As Double)
                m_real = Value
            End Set
        End Property

        Public Overridable Property Imaginary() As Double Implements IPhasorValue.Imaginary
            Get
                Return m_imaginary
            End Get
            Set(ByVal Value As Double)
                m_imaginary = Value
            End Set
        End Property

        Public Overridable Property UnscaledReal() As Int16 Implements IPhasorValue.UnscaledReal
            Get
                Return Convert.ToInt16(m_real * m_phasorDefinition.ScalingFactor)
            End Get
            Set(ByVal Value As Int16)
                m_real = Value / m_phasorDefinition.ScalingFactor
            End Set
        End Property

        Public Overridable Property UnscaledImaginary() As Int16 Implements IPhasorValue.UnscaledImaginary
            Get
                Return Convert.ToInt16(m_imaginary * m_phasorDefinition.ScalingFactor)
            End Get
            Set(ByVal Value As Int16)
                m_imaginary = Value / m_phasorDefinition.ScalingFactor
            End Set
        End Property

        Public Overrides ReadOnly Property Values() As Double()
            Get
                Return New Double() {m_real, m_imaginary}
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty() As Boolean
            Get
                Return (m_real = 0 And m_imaginary = 0)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                If DataFormat = EE.Phasor.DataFormat.FixedInteger Then
                    Return 4
                Else
                    Return 8
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                If m_phasorFormat = EE.Phasor.PhasorFormat.Rectangular Then
                    If DataFormat = EE.Phasor.DataFormat.FixedInteger Then
                        EndianOrder.SwapCopyBytes(UnscaledReal, buffer, 0)
                        EndianOrder.SwapCopyBytes(UnscaledImaginary, buffer, 2)
                    Else
                        EndianOrder.SwapCopyBytes(Convert.ToSingle(m_real), buffer, 0)
                        EndianOrder.SwapCopyBytes(Convert.ToSingle(m_imaginary), buffer, 4)
                    End If
                Else
                    If DataFormat = EE.Phasor.DataFormat.FixedInteger Then
                        EndianOrder.SwapCopyBytes(Convert.ToUInt16(Magnitude), buffer, 0)
                        EndianOrder.SwapCopyBytes(Convert.ToInt16(Angle * Math.PI / 180 * 10000), buffer, 2)
                    Else
                        EndianOrder.SwapCopyBytes(Convert.ToSingle(Magnitude), buffer, 0)
                        EndianOrder.SwapCopyBytes(Convert.ToSingle(Angle * Math.PI / 180), buffer, 4)
                    End If
                End If

                Return buffer
            End Get
        End Property

    End Class

End Namespace