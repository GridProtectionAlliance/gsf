'*******************************************************************************************************
'  PmuInfoCollection.vb - PMU Information Collection
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Public Class PmuInfoCollection

    Inherits List(Of PmuInfo)

    Public Shadows Sub Add(ByVal item As PmuInfo)

        MyBase.Add(item)
        Sort()

    End Sub

    Public Function TryGetValue(ByVal id As UShort, ByRef item As PmuInfo) As Boolean

        Dim pmuID As New PmuInfo(id, Nothing)
        Dim index As Integer

        index = BinarySearch(pmuID)

        If index < 0 Then
            Return False
        Else
            item = Me(index)
            Return True
        End If

    End Function

End Class
