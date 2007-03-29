'*******************************************************************************************************
'  IConfigurationFrame.vb - Configuration frame interface
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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

Namespace Phasors

    ''' <summary>This interface represents the protocol independent representation of any specific needed connection parameters.</summary>
    Public Interface IConnectionParameters

        Inherits ISerializable

        ''' <summary>Returns True if all connection parmaters are valid</summary>
        ReadOnly Property ValuesAreValid() As Boolean

    End Interface

End Namespace