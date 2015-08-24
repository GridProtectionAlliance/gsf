'*******************************************************************************************************
'  IPhasorValue.vb - Phasor value interface
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor

    ' This class represents the protocol independent interface of a phasor value.
    Public Interface IPhasorValue

        Inherits IChannelValue

        Shadows Property Definition() As IPhasorDefinition

        ReadOnly Property CoordinateFormat() As CoordinateFormat

        ReadOnly Property [Type]() As PhasorType

        Property Angle() As Double

        Property Magnitude() As Double

        Property Real() As Double

        Property Imaginary() As Double

        Property UnscaledReal() As Int16

        Property UnscaledImaginary() As Int16

    End Interface

End Namespace