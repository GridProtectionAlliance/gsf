'*******************************************************************************************************
'  ConfigurationCellCollection.vb - PDCstream specific configuration cell collection
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

Imports System.Runtime.Serialization

Namespace BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationCellCollection

        Inherits Phasors.ConfigurationCellCollection

        Public Sub New()

            ' Although the number of configuration cells are not restricted in the
            ' INI file, the data stream limits the maximum number of associated
            ' data cells to 32767, so we limit the configurations cells to the same.
            ' Also, in PDCstream configuration cells are constant length
            MyBase.New(Int16.MaxValue, True)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

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
