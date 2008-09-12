//*******************************************************************************************************
//  TVA.Reflection.Extensions.vb - Defines extension functions related to Assemblies
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2008 - J. Ritchie Carroll
//      Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Reflection;

namespace TVA.Reflection
{
    public static class Extensions
    {
        /// <summary>Returns only assembly name and version from full assembly name.</summary>
        public static string ShortName(this Assembly assemblyInstance)
        {
            return assemblyInstance.FullName.Split(',')[0];
        }

        /// <summary>Gets the specified embedded resource from the assembly.</summary>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public static Stream GetEmbeddedResource(this Assembly assemblyInstance, string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return assemblyInstance.GetManifestResourceStream(resourceName);
        }
    }
}
