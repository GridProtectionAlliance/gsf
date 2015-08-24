'*******************************************************************************************************
'  IPhasorValue.vb - Phasor value interface
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

''' <summary>This class represents the protocol independent interface of a phasor value.</summary>
<CLSCompliant(False)> _
Public Interface IPhasorValue

    Inherits IChannelValue(Of IPhasorDefinition)

    ReadOnly Property CoordinateFormat() As CoordinateFormat

    ReadOnly Property [Type]() As PhasorType

    Property Angle() As Single

    Property Magnitude() As Single

    Property Real() As Single

    Property Imaginary() As Single

    Property UnscaledReal() As Int16

    Property UnscaledImaginary() As Int16

End Interface