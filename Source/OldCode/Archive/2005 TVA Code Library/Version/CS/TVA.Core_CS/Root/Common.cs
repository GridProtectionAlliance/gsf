using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;

//*******************************************************************************************************
//  TVA.Common.vb - Globally available common functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************



/// <summary>Defines common global functions.</summary>
namespace TVA
{
	public sealed class Common
	{
		
		
		private Common()
		{
			
			// This class contains only global functions and is not meant to be instantiated.
			
		}
		
		/// <summary>Returns one of two strongly-typed objects.</summary>
		/// <returns>One of two objects, depending on the evaluation of given expression.</returns>
		/// <param name="expression">The expression you want to evaluate.</param>
		/// <param name="truePart">Returned if expression evaluates to True.</param>
		/// <param name="falsePart">Returned if expression evaluates to False.</param>
		/// <typeparam name="T">Return type used for immediate expression</typeparam>
		/// <remarks>This function acts as a strongly-typed immediate if (a.k.a. inline if).</remarks>
		public static T IIf<T>(bool expression, T truePart, T falsePart)
		{
			
			if (expression)
			{
				return truePart;
			}
			else
			{
				return falsePart;
			}
			
		}
		
		/// <summary>Creates a strongly-typed Array.</summary>
		/// <returns>New array of specified type.</returns>
		/// <param name="length">Desired length of new array.</param>
		/// <typeparam name="T">Return type for new array.</typeparam>
		/// <remarks>
		/// <para>
		/// The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to
		/// mention less confusion on the matter of array lengths), however the returned System.Array is not typed properly.
		/// This function properly casts the return array based on the the type specification helping when Option Strict is
		/// enabled.
		/// </para>
		/// <para>
		/// Examples:
		/// <code>
		///     Dim buffer As Byte() = CreateArray(Of Byte)(12)
		///     Dim matrix As Integer()() = CreateArray(Of Integer())(10)
		/// </code>
		/// </para>
		/// </remarks>
		public static T[] CreateArray<T>(int length)
		{
			
			// The following provides better performance than "Return New T(length) {}".
			return ((T[]) (Array.CreateInstance(typeof(T), length)));
			
		}
		
		/// <summary>Creates a strongly-typed Array with an initial value parameter.</summary>
		/// <returns>New array of specified type.</returns>
		/// <param name="length">Desired length of new array.</param>
		/// <param name="initialValue">Value used to initialize all array elements.</param>
		/// <typeparam name="T">Return type for new array.</typeparam>
		/// <remarks>
		/// <para>
		/// Examples:
		/// <code>
		///     Dim elements As Integer() = CreateArray(12, -1)
		///     Dim names As String() = CreateArray(100, "undefined")
		/// </code>
		/// </para>
		/// </remarks>
		public static T[] CreateArray<T>(int length, T initialValue)
		{
			
			T[] typedArray = CreateArray<T>(length);
			
			// Initializes all elements with the default value.
			for (int x = 0; x <= typedArray.Length - 1; x++)
			{
				typedArray[x] = initialValue;
			}
			
			return typedArray;
			
		}
		
		/// <summary>
		/// Gets the root type in the inheritace hierarchy from which the specified type inherits.
		/// </summary>
		/// <param name="type">The System.Type whose root type is to be found.</param>
		/// <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
		/// <remarks>The type returned will never be System.Object, even though all types ultimately inherit from it.</remarks>
		public static Type GetRootType(Type type)
		{
			
			if (!(type.BaseType is typeof)(System.Object))
			{
				return GetRootType(type.BaseType);
			}
			else
			{
				return type;
			}
			
		}
		
		/// <summary>
		/// Gets the type of the currently executing application.
		/// </summary>
		/// <returns>One of the TVA.ApplicationType values.</returns>
		public static ApplicationType GetApplicationType()
		{
			
			if (System.Web.HttpContext.Current == null)
			{
				try
				{
					// References:
					// - http://support.microsoft.com/kb/65122
					// - http://support.microsoft.com/kb/90493/en-us
					// - http://www.codeguru.com/cpp/w-p/system/misc/article.php/c2897/
					// We will always have an entry assembly for windows application.
					FileStream exe = new FileStream(TVA.Assembly.EntryAssembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					byte[] dosHeader = CreateArray<byte>(64);
					byte[] exeHeader = CreateArray<byte>(248);
					byte[] subSystem = CreateArray<byte>(2);
					exe.Read(dosHeader, 0, dosHeader.Length);
					exe.Seek(BitConverter.ToInt16(dosHeader, 60), SeekOrigin.Begin);
					exe.Read(exeHeader, 0, exeHeader.Length);
					exe.Close();
					
					Array.Copy(exeHeader, 92, subSystem, 0, 2);
					
					return ((ApplicationType) (BitConverter.ToInt16(subSystem, 0)));
				}
				catch (Exception)
				{
					// We are unable to determine the application type. This is possible in case of a web app/web site
					// when this method is being called from a thread other than the main thread, in which case the
					// System.Web.HttpContext.Current property, used to determine if it is a web app, will not be set.
					return ApplicationType.Unknown;
				}
			}
			else
			{
				return ApplicationType.Web;
			}
			
		}
		
		/// <summary>Determines if given item is an object (i.e., a reference type) but not a string.</summary>
		public static bool IsNonStringReference(object item)
		{
			
			return (Information.IsReference(item) && ! item is string);
			
		}
		
	}
	
}
