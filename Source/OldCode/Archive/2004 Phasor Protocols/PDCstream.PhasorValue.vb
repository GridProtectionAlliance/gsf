'***********************************************************************
'  PDCstream.PhasorValue.vb - Phasor value
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

Namespace PDCstream

    Public Enum PhasorType
        Voltage
        Current
        DontCare
    End Enum

    Public Class PhasorValue

        Private m_phasorDefinition As PhasorDefinition
        Private m_real As UInt16
        Private m_imaginary As UInt16

        Public Enum PolarCompositeValue
            Angle
            Magnitude
        End Enum

        Public Enum RectangularCompositeValue
            Real
            Imaginary
        End Enum

        Public CompositeValues As New CompositeValues(2)

        Public Const BinaryLength As Integer = 4

        Public Shared ReadOnly Property Empty() As PhasorValue
            Get
                Return New PhasorValue(Nothing, Convert.ToUInt16(0), Convert.ToUInt16(0))
            End Get
        End Property

        ' Create phasor from polar coordinates (angle expected in Degrees)
        Public Shared Function CreateFromPolarValues(ByVal phasorDefinition As PhasorDefinition, ByVal angle As Double, ByVal magnitude As Double) As PhasorValue

            Return CreateFromScaledRectangularValues(phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude))

        End Function

        ' Create phasor from scaled rectangular coordinates
        Public Shared Function CreateFromScaledRectangularValues(ByVal phasorDefinition As PhasorDefinition, ByVal real As Double, ByVal imaginary As Double) As PhasorValue

            Dim scale As Double = phasorDefinition.ScalingFactor(phasorDefinition)
            Return CreateFromUnscaledRectangularValues(phasorDefinition, Convert.ToUInt16(real / scale), Convert.ToUInt16(imaginary / scale))

        End Function

        ' Create phasor from unscaled rectangular coordinates
        Public Shared Function CreateFromUnscaledRectangularValues(ByVal phasorDefinition As PhasorDefinition, ByVal real As UInt16, ByVal imaginary As UInt16) As PhasorValue

            Return New PhasorValue(phasorDefinition, real, imaginary)

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
        Public Shared Function CalculatePower(ByVal voltage As PhasorValue, ByVal current As PhasorValue) As Double

            Return 3 * (((voltage.UnscaledReal * current.UnscaledReal) + (voltage.UnscaledImaginary * current.UnscaledImaginary)) * voltage.ScalingFactor * current.ScalingFactor)

        End Function

        ' Calculate vars from imaginary and real components of two phasors
        Public Shared Function CalculateVars(ByVal voltage As PhasorValue, ByVal current As PhasorValue) As Double

            Return 3 * (((voltage.UnscaledImaginary * current.UnscaledReal) - (voltage.UnscaledReal * current.UnscaledImaginary)) * voltage.ScalingFactor * current.ScalingFactor)

        End Function

        Private Sub New(ByVal phasorDefinition As PhasorDefinition, ByVal real As UInt16, ByVal imaginary As UInt16)

            m_phasorDefinition = phasorDefinition
            m_real = real
            m_imaginary = imaginary

        End Sub

        Public ReadOnly Property PhasorDefinition() As PhasorDefinition
            Get
                Return m_phasorDefinition
            End Get
        End Property

        Public ReadOnly Property ScalingFactor() As Double
            Get
                Return PhasorDefinition.ScalingFactor(m_phasorDefinition)
            End Get
        End Property

        Public ReadOnly Property Magnitude() As Double
            Get
                Return Math.Sqrt(UnscaledReal * UnscaledReal + UnscaledImaginary * UnscaledImaginary) * ScalingFactor()
            End Get
        End Property

        Public ReadOnly Property Angle() As Double
            Get
                Return (Math.Atan2(UnscaledImaginary, UnscaledReal) + (m_phasorDefinition.Offset * Math.PI / 180)) * 180 / Math.PI
            End Get
        End Property

        Public ReadOnly Property Real() As UInt16
            Get
                Return m_real
            End Get
        End Property

        Public ReadOnly Property Imaginary() As UInt16
            Get
                Return m_imaginary
            End Get
        End Property

        ' In .NET, unsigned ints aren't typically usable directly in equations, so we provide these functions to provide usable values
        Public ReadOnly Property UnscaledReal() As Double
            Get
                Return Convert.ToDouble(m_real)
            End Get
        End Property

        Public ReadOnly Property UnscaledImaginary() As Double
            Get
                Return Convert.ToDouble(m_imaginary)
            End Get
        End Property

        Public ReadOnly Property ScaledReal() As Double
            Get
                Return UnscaledReal * ScalingFactor
            End Get
        End Property

        Public ReadOnly Property ScaledImaginary() As Double
            Get
                Return UnscaledImaginary * ScalingFactor
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return (UnscaledReal = 0 And UnscaledImaginary = 0)
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(BitConverter.GetBytes(m_real), 0, buffer, 0, 2)
                Array.Copy(BitConverter.GetBytes(m_imaginary), 0, buffer, 2, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace