'*******************************************************************************************************
'  TVA.Common.vb - Globally available common functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/03/2006 - J. Ritchie Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

''' <summary>Defines common global functions</summary>
Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>Strongly typed immediate (a.k.a. inline) if.</summary>
    ''' <returns>One of two objects, depending on the evaluation of given expression.</returns>
    ''' <param name="expression">The expression you want to evaluate.</param>
    ''' <param name="truePart">Returned if expression evaluates to True.</param>
    ''' <param name="falsePart">Returned if expression evaluates to False.</param>
    ''' <typeparam name="T">Return type used for immediate expression</typeparam>
    Public Shared Function IIf(Of T)(ByVal expression As Boolean, ByVal truePart As T, ByVal falsePart As T) As T

        If expression Then Return truePart Else Return falsePart

    End Function

    ''' <summary>Strongly typed Array creation function.</summary>
    ''' <returns>New array of specified type.</returns>
    ''' <param name="length">Desired length of new array.</param>
    ''' <typeparam name="T">Return type for new array.</typeparam>
    ''' <remarks>
    ''' <para>
    ''' The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to mention less
    ''' confusion on the matter of array lengths), however the returned System.Array is not typed properly.  This function
    ''' properly casts the return array based on the the type specification helping when Option Strict is enabled.
    ''' </para>
    ''' <para>
    ''' Examples:
    ''' <code>
    '''     Dim buffer As Byte() = CreateArray(Of Byte)(12)
    '''     Dim matrix As Integer()() = CreateArray(Of Integer())(10)
    ''' </code>
    ''' </para>
    ''' </remarks>
    Public Shared Function CreateArray(Of T)(ByVal length As Integer) As T()

        Return DirectCast(Array.CreateInstance(GetType(T), length), T())

    End Function

    ''' <summary>Strongly typed Array creation function with initial value parameter.</summary>
    ''' <returns>New array of specified type.</returns>
    ''' <param name="length">Desired length of new array.</param>
    ''' <param name="initialValue">Value used to initialize all array elements</param>
    ''' <typeparam name="T">Return type for new array.</typeparam>
    ''' <remarks>
    ''' <para>
    ''' Examples:
    ''' <code>
    '''     Dim elements As Integer() = CreateArray(12, -1)
    '''     Dim names As String() = CreateArray(100, "undefined")
    ''' </code>
    ''' </para>
    ''' </remarks>
    Public Shared Function CreateArray(Of T)(ByVal length As Integer, ByVal initialValue As T) As T()

        Dim typedArray As T() = CreateArray(Of T)(length)

        ' Initialize all elements with the default value
        For x As Integer = 0 To typedArray.Length - 1
            typedArray(x) = initialValue
        Next

        Return typedArray

    End Function

End Class
