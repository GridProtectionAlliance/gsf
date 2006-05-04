'*******************************************************************************************************
'  ConfigurationCellCollection.vb - IEEE C37.118 specific configuration cell collection
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class ConfigurationCellCollection

        Inherits Phasors.ConfigurationCellCollection

        Public Sub New()

            MyBase.New(Int16.MaxValue, False)

        End Sub

        Public Shadows Sub Add(ByVal value As ConfigurationCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Int32) As ConfigurationCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

End Namespace
