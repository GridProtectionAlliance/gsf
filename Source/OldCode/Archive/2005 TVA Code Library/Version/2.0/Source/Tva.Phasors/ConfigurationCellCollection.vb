'*******************************************************************************************************
'  ConfigurationCellCollection.vb - Configuration cell collection class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
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

' This class represents the protocol independent collection of the common implementation of a set of configuration related data settings that can be sent or received from a PMU.
Public Class ConfigurationCellCollection

    Inherits ChannelCellCollectionBase(Of IConfigurationCell)

    Public Sub New(ByVal maximumCount As Integer, ByVal constantCellLength As Boolean)

        MyBase.New(maximumCount, constantCellLength)

    End Sub

    Public Overrides ReadOnly Property InheritedType() As Type
        Get
            Return Me.GetType()
        End Get
    End Property

End Class
