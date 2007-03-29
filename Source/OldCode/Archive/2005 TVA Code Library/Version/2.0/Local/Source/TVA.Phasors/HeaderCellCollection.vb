'*******************************************************************************************************
'  HeaderCellCollection.vb - Header cell collection class
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

    ''' <summary>This class represents the protocol independent collection of the common implementation of a set of header data that can be received from a PMU.</summary>
    <CLSCompliant(False), Serializable()> _
    Public Class HeaderCellCollection

        Inherits ChannelCellCollectionBase(Of IHeaderCell)

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal maximumCount As Int32)

            MyBase.New(maximumCount, True)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As Type
            Get
                Return Me.GetType()
            End Get
        End Property

    End Class

End Namespace