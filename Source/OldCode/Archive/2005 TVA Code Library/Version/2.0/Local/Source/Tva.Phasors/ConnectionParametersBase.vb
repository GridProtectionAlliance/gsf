'*******************************************************************************************************
'  ConnectionParametersBase.vb - Connection parameters base class
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  2/26/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

''' <summary>This class represents the common implementation of the protocol independent connection parameters base class.</summary>
''' <remarks>
''' <para>This class is inherited by subsequent classes to provide protocol specific connection parameters that may be needed to make a connection.</para>
''' <para>Derived implementations of this class are designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connectin parameters.</para>
''' </remarks>
<Serializable()> _
Public MustInherit Class ConnectionParametersBase

    Implements IConnectionParameters

    Public Overridable ReadOnly Property ValuesAreValid() As Boolean Implements IConnectionParameters.ValuesAreValid
        Get
            Return True
        End Get
    End Property

    Public MustOverride Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext) Implements ISerializable.GetObjectData

End Class
