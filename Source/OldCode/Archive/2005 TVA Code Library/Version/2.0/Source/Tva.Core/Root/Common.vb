'*******************************************************************************************************
'  Tva.Common.vb - Globally available common functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This is the location for handy miscellaneous functions that are difficult to categorize anywhere else
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/03/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

''' <summary>Defines common global functions</summary>
Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>Strongly typed immediate (a.k.a. inline) if.  Returns one of two objects, depending on the evaluation of an expression.</summary>
    ''' <param name="expression">The expression you want to evaluate.</param>
    ''' <param name="truePart">Returned if expression evaluates to True.</param>
    ''' <param name="falsePart">Returned if expression evaluates to False.</param>
    ''' <typeparam name="T">Return type used for immediate expression</typeparam>
    Public Shared Function IIf(Of T)(ByVal expression As Boolean, ByVal truePart As T, ByVal falsePart As T) As T

        If expression Then Return truePart Else Return falsePart

    End Function

End Class
