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

    Public MustInherit Class PhasorValueBase

        Implements IPhasorValue

        Private m_phasorFormat As PhasorFormat
        Private m_phasorDefinition As IPhasorDefinition
        Private m_real As Int16
        Private m_imaginary As Int16
        Private m_compositeValues As CompositeValues

        Private Enum CompositeValue
            Angle
            Magnitude
        End Enum

        ' Create phasor from polar coordinates (angle expected in Degrees)
        Public Shared Function CreateFromPolarValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal angle As Double, ByVal magnitude As Double) As PhasorValueBase

            Return CreateFromScaledRectangularValues(phasorValueType, phasorFormat, phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude))

        End Function

        ' Create phasor from scaled rectangular coordinates
        Public Shared Function CreateFromScaledRectangularValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Double, ByVal imaginary As Double) As PhasorValueBase

            Dim scale As Double = phasorDefinition.ScalingFactor
            Return CreateFromUnscaledRectangularValues(phasorValueType, phasorFormat, phasorDefinition, real / scale, imaginary / scale)

        End Function

        ' Create phasor from unscaled rectangular coordinates
        Public Shared Function CreateFromUnscaledRectangularValues(ByVal phasorValueType As Type, ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16) As PhasorValueBase

            Return Reflection.Assembly.GetExecutingAssembly.CreateInstance(phasorValueType.FullName, True, Reflection.BindingFlags.CreateInstance, Nothing, New Object() {phasorFormat, phasorDefinition, real, imaginary}, Nothing, Nothing)

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

            Return 3 * (voltage.ScaledReal * current.ScaledReal + voltage.ScaledImaginary * current.ScaledImaginary)
            'Return 3 * voltage.Magnitude * current.Magnitude * Math.Cos((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        ' Calculate vars from imaginary and real components of two phasors
        Public Shared Function CalculateVars(ByVal voltage As IPhasorValue, ByVal current As IPhasorValue) As Double

            Return 3 * (voltage.ScaledImaginary * current.ScaledReal - voltage.ScaledReal * current.ScaledImaginary)
            'Return 3 * voltage.Magnitude * current.Magnitude * Math.Sin((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        Public Sub New(ByVal phasorFormat As PhasorFormat, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16)

            m_phasorFormat = phasorFormat
            m_phasorDefinition = phasorDefinition
            m_real = real
            m_imaginary = imaginary
            m_compositeValues = New CompositeValues(2)

        End Sub

        Public Overridable Property Format() As PhasorFormat Implements IPhasorValue.Format
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
                Return Math.Atan2(ScaledImaginary, ScaledReal) * 180 / Math.PI
            End Get
            Set(ByVal Value As Double)
                ' We store angle as one of our required composite values
                m_compositeValues(CompositeValue.Angle) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValue()
            End Set
        End Property

        Public Overridable Property Magnitude() As Double Implements IPhasorValue.Magnitude
            Get
                Return Math.Sqrt(ScaledReal * ScaledReal + ScaledImaginary * ScaledImaginary)
            End Get
            Set(ByVal Value As Double)
                ' We store magnitude as one of our required composite values
                m_compositeValues(CompositeValue.Magnitude) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValue()
            End Set
        End Property

        Public Overridable ReadOnly Property Real() As Int16 Implements IPhasorValue.Real
            Get
                Return m_real
            End Get
        End Property

        Public Overridable ReadOnly Property Imaginary() As Int16 Implements IPhasorValue.Imaginary
            Get
                Return m_imaginary
            End Get
        End Property

        Public Overridable ReadOnly Property ScaledReal() As Double Implements IPhasorValue.ScaledReal
            Get
                Return Convert.ToDouble(m_real) * m_phasorDefinition.ScalingFactor
            End Get
        End Property

        Public Overridable ReadOnly Property ScaledImaginary() As Double Implements IPhasorValue.ScaledImaginary
            Get
                Return Convert.ToDouble(m_imaginary) * m_phasorDefinition.ScalingFactor
            End Get
        End Property

        Public Overridable ReadOnly Property IsEmpty() As Boolean Implements IPhasorValue.IsEmpty
            Get
                Return (m_real = 0 And m_imaginary = 0)
            End Get
        End Property

        Protected Overridable Sub CalculatePhasorValue()

            If m_compositeValues.AllReceived Then
                Dim angle, magnitude, scale As Double

                ' All values received, create a new phasor value from composite values
                angle = m_compositeValues(CompositeValue.Angle)
                magnitude = m_compositeValues(CompositeValue.Magnitude)
                scale = m_phasorDefinition.ScalingFactor

                m_real = CalculateRealComponent(angle, magnitude) / scale
                m_imaginary = CalculateImaginaryComponent(angle, magnitude) / scale
            End If

        End Sub

        Public MustOverride ReadOnly Property BinaryLength() As Integer Implements IPhasorValue.BinaryLength

        Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IPhasorValue.BinaryImage
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                If m_phasorFormat = PhasorFormat.Rectangular Then
                    EndianOrder.SwapCopyBytes(m_real, buffer, 0)
                    EndianOrder.SwapCopyBytes(m_imaginary, buffer, 2)
                Else
                    EndianOrder.SwapCopyBytes(Convert.ToInt16(Magnitude), buffer, 0)
                    EndianOrder.SwapCopyBytes(Convert.ToInt16(Angle * Math.PI / 180 * 10000), buffer, 2)
                End If

                Return buffer
            End Get
        End Property

    End Class

End Namespace