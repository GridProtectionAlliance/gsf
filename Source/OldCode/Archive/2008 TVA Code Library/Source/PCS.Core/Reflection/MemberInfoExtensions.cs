//*******************************************************************************************************
//  MemberInfoExtensions.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Reflection;

namespace PCS.Reflection
{
    /// <summary>
    /// Defines extensions methods related to <see cref="MemberInfo"/> objects and derived types (e.g., <see cref="FieldInfo"/>,
    /// <see cref="PropertyInfo"/>, <see cref="MethodInfo"/>, etc.)
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Attempts to get the specified <paramref name="Attribute"/> from a <see cref="MemberInfo"/> object, returning <c>true</c> if it does.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> object over which to search attributes.</param>
        /// <param name="attribute">The <see cref="Attribute"/> that was found, if any.</param>
        /// <returns><c>true</c> if <paramref name="Attribute"/> was found; otherwise <c>false</c>.</returns>
        /// <typeparam name="TMemberInfo"><see cref="MemberInfo"/> or derived type to get <see cref="Attribute"/> from.</typeparam>
        /// <typeparam name="TAttribute"><see cref="Type"/> of <see cref="Attribute"/> to attempt to retrieve.</typeparam>
        public static bool TryGetAttribute<TMemberInfo, TAttribute>(this TMemberInfo member, out TAttribute attribute)
            where TMemberInfo : MemberInfo
            where TAttribute : Attribute
        {
            object[] attributes = member.GetCustomAttributes(typeof(TAttribute), true);

            if (attributes.Length > 0)
            {
                attribute = attributes[0] as TAttribute;
                return true;
            }

            attribute = null;
            return false;
        }
    }
}
