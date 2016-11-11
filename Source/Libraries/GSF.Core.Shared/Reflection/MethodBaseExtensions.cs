//******************************************************************************************************
//  MethodBaseExtensions.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//******************************************************************************************************

using System;
using System.Reflection;
using System.Text;

namespace GSF.Reflection
{
    /// <summary>
    /// Defines extensions methods related to extension functions for <see cref="MethodBase"/> instances.
    /// </summary>
    public static class MethodBaseExtensions
    {
        /// <summary>
        /// Gets the friendly method name of the provided type, trimming generic parameters.
        /// </summary>
        /// <param name="method">Type to get friendly method name for.</param>
        /// <returns>Friendly method name of the provided type, or <see cref="string.Empty"/> if <paramref name="method"/> is <c>null</c>.</returns>
        public static string GetFriendlyMethodName(this MethodBase method)
        {
            if ((object)method == null)
                return string.Empty;

            bool appendComma;
            StringBuilder name = new StringBuilder();

            name.Append(method.Name);

            if (method is MethodInfo && method.IsGenericMethod)
            {
                appendComma = false;
                name.Append("<");

                foreach (Type arg in method.GetGenericArguments())
                {
                    if (appendComma)
                        name.Append(",");
                    else
                        appendComma = true;

                    name.Append(arg.Name);
                }

                name.Append(">");
            }

            appendComma = false;
            name.Append("(");

            foreach (ParameterInfo param in method.GetParameters())
            {
                if (appendComma)
                    name.Append(", ");
                else
                    appendComma = true;

                name.Append(param.ParameterType.Name);
                name.Append(" ");
                name.Append(param.Name);
            }

            name.Append(")");

            return name.ToString();
        }

        /// <summary>
        /// Gets the friendly method name with class of the provided type, trimming generic parameters.
        /// </summary>
        /// <param name="method">Type to get friendly method name with class for.</param>
        /// <returns>Friendly method name with class of the provided type, or <see cref="string.Empty"/> if <paramref name="method"/> is <c>null</c>.</returns>
        public static string GetFriendlyMethodNameWithClass(this MethodBase method)
        {
            string className = method.GetFriendlyClassName();
            string methodName = method.GetFriendlyMethodName();

            if (className.Length == 0)
                return methodName;

            return className + "." + methodName;
        }
    }
}
