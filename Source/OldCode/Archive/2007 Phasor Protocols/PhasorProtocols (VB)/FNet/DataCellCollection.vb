'*******************************************************************************************************
'  DataCellCollection.vb - FNet specific data cell collection
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
'  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class DataCellCollection

        Inherits PhasorProtocols.DataCellCollection

        Public Sub New()

            ' IEEE 1344 only supports a single PMU - so there should only be one cell
            MyBase.New(1, False)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Shadows Sub Add(ByVal value As DataCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Int32) As DataCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

End Namespace
