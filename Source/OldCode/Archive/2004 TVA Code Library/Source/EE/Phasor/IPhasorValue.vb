'***********************************************************************
'  IPhasorValue.vb - Phasor value interface
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    Public Interface IPhasorValue

        ReadOnly Property Definition() As IPhasorDefinition

        Property Format() As PhasorFormat

        Property Angle() As Double

        Property Magnitude() As Double

        ReadOnly Property Real() As Int16

        ReadOnly Property Imaginary() As Int16

        ReadOnly Property ScaledReal() As Double

        ReadOnly Property ScaledImaginary() As Double

        ReadOnly Property IsEmpty() As Boolean

        ReadOnly Property BinaryLength() As Integer

        ReadOnly Property BinaryImage() As Byte()

    End Interface

End Namespace