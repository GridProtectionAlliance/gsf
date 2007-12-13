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
'       Generated original version of source code.
'  12/13/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Option Strict On

Imports System.IO

''' <summary>Defines common global functions.</summary>
Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated.

    End Sub

    ''' <summary>Returns one of two strongly-typed objects.</summary>
    ''' <returns>One of two objects, depending on the evaluation of given expression.</returns>
    ''' <param name="expression">The expression you want to evaluate.</param>
    ''' <param name="truePart">Returned if expression evaluates to True.</param>
    ''' <param name="falsePart">Returned if expression evaluates to False.</param>
    ''' <typeparam name="T">Return type used for immediate expression</typeparam>
    ''' <remarks>This function acts as a strongly-typed immediate if (a.k.a. inline if).</remarks>
    Public Shared Function IIf(Of T)(ByVal expression As Boolean, ByVal truePart As T, ByVal falsePart As T) As T

        If expression Then Return truePart Else Return falsePart

    End Function

    ''' <summary>Creates a strongly-typed Array.</summary>
    ''' <returns>New array of specified type.</returns>
    ''' <param name="length">Desired length of new array.</param>
    ''' <typeparam name="T">Return type for new array.</typeparam>
    ''' <remarks>
    ''' <para>
    ''' The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to
    ''' mention less confusion on the matter of array lengths), however the returned System.Array is not typed properly.
    ''' This function properly casts the return array based on the the type specification helping when Option Strict is
    ''' enabled.
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

        ' The following provides better performance than "Return New T(length) {}".
        Return DirectCast(Array.CreateInstance(GetType(T), length), T())

    End Function

    ''' <summary>Creates a strongly-typed Array with an initial value parameter.</summary>
    ''' <returns>New array of specified type.</returns>
    ''' <param name="length">Desired length of new array.</param>
    ''' <param name="initialValue">Value used to initialize all array elements.</param>
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

        ' Initializes all elements with the default value.
        For x As Integer = 0 To typedArray.Length - 1
            typedArray(x) = initialValue
        Next

        Return typedArray

    End Function

    ''' <summary>
    ''' Gets the root type in the inheritace hierarchy from which the specified type inherits. 
    ''' </summary>
    ''' <param name="type">The System.Type whose root type is to be found.</param>
    ''' <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
    ''' <remarks>The type returned will never be System.Object, even though all types ultimately inherit from it.</remarks>
    Public Shared Function GetRootType(ByVal type As Type) As Type

        If type.BaseType IsNot GetType(System.Object) Then
            Return GetRootType(type.BaseType)
        Else
            Return type
        End If

    End Function

    ''' <summary>
    ''' Gets the type of the currently executing application.
    ''' </summary>
    ''' <returns>One of the TVA.ApplicationType values.</returns>
    Public Shared Function GetApplicationType() As ApplicationType

        If System.Web.HttpContext.Current Is Nothing Then
            ' References:
            ' - http://support.microsoft.com/kb/65122
            ' - http://support.microsoft.com/kb/90493/en-us
            ' - http://www.codeguru.com/cpp/w-p/system/misc/article.php/c2897/
            ' We will always have an entry assembly for windows application.
            Dim exe As New FileStream(TVA.Assembly.EntryAssembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim dosHeader As Byte() = CreateArray(Of Byte)(64)
            Dim exeHeader As Byte() = CreateArray(Of Byte)(248)
            Dim subSystem As Byte() = CreateArray(Of Byte)(2)
            exe.Read(dosHeader, 0, dosHeader.Length)
            exe.Seek(BitConverter.ToInt16(dosHeader, 60), SeekOrigin.Begin)
            exe.Read(exeHeader, 0, exeHeader.Length)
            exe.Close()

            Array.Copy(exeHeader, 92, subSystem, 0, 2)

            Return CType(BitConverter.ToInt16(subSystem, 0), ApplicationType)
        Else
            Return ApplicationType.Web
        End If

    End Function

    ''' <summary>Determines if given item is an object (i.e., a reference type) but not a string.</summary>
    Public Shared Function IsNonStringReference(ByVal item As Object) As Boolean

        Return (IsReference(item) And Not TypeOf item Is String)

    End Function

End Class
