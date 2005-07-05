'*******************************************************************************************************
'  ConfigurationCellCollection.vb - Configuration cell collection class
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports System.Text
Imports TVA.Interop
Imports TVA.EE.Phasor.Common

Namespace EE.Phasor

    ' This class represents the protocol independent collection of the common implementation of a set of configuration related data settings that can be sent or received from a PMU.
    Public Class ConfigurationCellCollection

        Inherits ChannelCellCollectionBase

        Public Shadows Sub Add(ByVal value As IConfigurationCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IConfigurationCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property InheritedType() As Type
            Get
                Return Me.GetType()
            End Get
        End Property

    End Class

End Namespace