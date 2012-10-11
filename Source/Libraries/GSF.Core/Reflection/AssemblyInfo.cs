//*******************************************************************************************************
//  AssemblyInfo.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/29/2005 - Pinal C. Patel
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (GSF.Shared.Assembly).
//  12/12/2007 - Darrell Zuercher
//       Edited Code Comments.
//  09/08/2008 - J. Ritchie Carroll
//       Converted to C# as AssemblyInformation.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/21/2009 - Pinal C. Patel
//       Added error checking to assembly attribute properties.
//  09/28/2010 - Pinal C. Patel
//       Modified EntryAssembly to perform a reflection only load of the currently executing process 
//       to deal with entry assembly not being available in non-default application domains.
//       Changed GetCustomAttribute() to return CustomAttributeData instead of Object to deal with
//       possible reflection only load being performed in EntryAssembly.
//       Removed Debuggable property since it was not very useful and added complexity when extracting.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The GSF Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using GSF.IO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace GSF.Reflection
{
    /// <summary>Assembly Information Class.</summary>
    public class AssemblyInfo
    {
        #region [ Members ]

        // Fields
        private Assembly m_assemblyInstance;

        #endregion

        #region [ Constructors ]

        /// <summary>Initializes a new instance of the <see cref="AssemblyInfo"/> class.</summary>
        /// <param name="assemblyInstance">An <see cref="Assembly"/> object.</param>
        public AssemblyInfo(Assembly assemblyInstance)
        {
            m_assemblyInstance = assemblyInstance;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the underlying <see cref="Assembly"/> being represented by this <see cref="AssemblyInfo"/> object.
        /// </summary>
        public Assembly Assembly
        {
            get
            {
                return m_assemblyInstance;
            }
        }

        /// <summary>
        /// Gets the title information of the <see cref="Assembly"/>.
        /// </summary>
        public string Title
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyTitleAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the description information of the <see cref="Assembly"/>.
        /// </summary>
        public string Description
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyDescriptionAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the company name information of the <see cref="Assembly"/>.
        /// </summary>
        public string Company
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyCompanyAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the product name information of the <see cref="Assembly"/>.
        /// </summary>
        public string Product
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyProductAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the copyright information of the <see cref="Assembly"/>.
        /// </summary>
        public string Copyright
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyCopyrightAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the trademark information of the <see cref="Assembly"/>.
        /// </summary>
        public string Trademark
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyTrademarkAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the configuration information of the <see cref="Assembly"/>.
        /// </summary>
        public string Configuration
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyConfigurationAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if the <see cref="Assembly"/> has been built as delay-signed.
        /// </summary>
        public bool DelaySign
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyDelaySignAttribute));
                if ((object)attribute == null)
                    return false;
                else
                    return (bool)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the version information of the <see cref="Assembly"/>.
        /// </summary>
        public string InformationalVersion
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the name of the file containing the key pair used to generate a strong name for the attributed <see cref="Assembly"/>.
        /// </summary>
        public string KeyFile
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(AssemblyKeyFileAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the culture name of the <see cref="Assembly"/>.
        /// </summary>
        public string CultureName
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(NeutralResourcesLanguageAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the assembly version used to instruct the System.Resources.ResourceManager to ask for a particular
        /// version of a satellite assembly to simplify updates of the main assembly of an application.
        /// </summary>
        public string SatelliteContractVersion
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(SatelliteContractVersionAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the string representing the assembly version used to indicate to a COM client that all classes
        /// in the current version of the assembly are compatible with classes in an earlier version of the assembly.
        /// </summary>
        public string ComCompatibleVersion
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(ComCompatibleVersionAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return attribute.ConstructorArguments[0].Value.ToString() + "." +
                           attribute.ConstructorArguments[1].Value.ToString() + "." +
                           attribute.ConstructorArguments[2].Value.ToString() + "." +
                           attribute.ConstructorArguments[3].Value.ToString();
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if the <see cref="Assembly"/> is exposed to COM.
        /// </summary>
        public bool ComVisible
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(ComVisibleAttribute));
                if ((object)attribute == null)
                    return false;
                else
                    return (bool)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the GUID that is used as an ID if the <see cref="Assembly"/> is exposed to COM.
        /// </summary>
        public string Guid
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(GuidAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return (string)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the string representing the <see cref="Assembly"/> version number in MajorVersion.MinorVersion format.
        /// </summary>
        public string TypeLibVersion
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(TypeLibVersionAttribute));
                if ((object)attribute == null)
                    return string.Empty;
                else
                    return attribute.ConstructorArguments[0].Value.ToString() + "." +
                           attribute.ConstructorArguments[1].Value.ToString();
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the <see cref="Assembly"/> is CLS-compliant.
        /// </summary>
        public bool CLSCompliant
        {
            get
            {
                CustomAttributeData attribute = GetCustomAttribute(typeof(CLSCompliantAttribute));
                if ((object)attribute == null)
                    return false;
                else
                    return (bool)attribute.ConstructorArguments[0].Value;
            }
        }

        /// <summary>
        /// Gets the path or UNC location of the loaded file that contains the manifest.
        /// </summary>
        public string Location
        {
            get
            {
                return m_assemblyInstance.Location.ToLower();
            }
        }

        /// <summary>
        /// Gets the location of the <see cref="Assembly"/> as specified originally.
        /// </summary>
        public string CodeBase
        {
            get
            {
                return m_assemblyInstance.CodeBase.Replace("file:///", "").ToLower();
            }
        }

        /// <summary>
        /// Gets the display name of the <see cref="Assembly"/>.
        /// </summary>
        public string FullName
        {
            get
            {
                return m_assemblyInstance.FullName;
            }
        }

        /// <summary>
        /// Gets the simple, unencrypted name of the <see cref="Assembly"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return m_assemblyInstance.GetName().Name;
            }
        }

        /// <summary>
        /// Gets the major, minor, revision, and build numbers of the <see cref="Assembly"/>.
        /// </summary>
        public Version Version
        {
            get
            {
                return m_assemblyInstance.GetName().Version;
            }
        }

        /// <summary>
        /// Gets the string representing the version of the common language runtime (CLR) saved in the file
        /// containing the manifest.
        /// </summary>
        public string ImageRuntimeVersion
        {
            get
            {
                return m_assemblyInstance.ImageRuntimeVersion;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the <see cref="Assembly"/> was loaded from the global assembly cache.
        /// </summary>
        public bool GACLoaded
        {
            get
            {
                return m_assemblyInstance.GlobalAssemblyCache;
            }
        }

        /// <summary>
        /// Gets the date and time when the <see cref="Assembly"/> was built.
        /// </summary>
        public DateTime BuildDate
        {
            get
            {
                return File.GetLastWriteTime(m_assemblyInstance.Location);
            }
        }

        /// <summary>
        /// Gets the root namespace of the <see cref="Assembly"/>.
        /// </summary>
        public string RootNamespace
        {
            get
            {
                return m_assemblyInstance.GetExportedTypes()[0].Namespace;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Gets a collection of assembly attributes exposed by the assembly.</summary>
        /// <returns>A System.Specialized.KeyValueCollection of assembly attributes.</returns>
        public NameValueCollection GetAttributes()
        {
            NameValueCollection assemblyAttributes = new NameValueCollection();

            //Add some values that are not in AssemblyInfo.
            assemblyAttributes.Add("Full Name", FullName);
            assemblyAttributes.Add("Name", Name);
            assemblyAttributes.Add("Version", Version.ToString());
            assemblyAttributes.Add("Image Runtime Version", ImageRuntimeVersion);
            assemblyAttributes.Add("Build Date", BuildDate.ToString());
            assemblyAttributes.Add("Location", Location);
            assemblyAttributes.Add("Code Base", CodeBase);
            assemblyAttributes.Add("GAC Loaded", GACLoaded.ToString());

            //Add all attributes available from AssemblyInfo.
            assemblyAttributes.Add("Title", Title);
            assemblyAttributes.Add("Description", Description);
            assemblyAttributes.Add("Company", Company);
            assemblyAttributes.Add("Product", Product);
            assemblyAttributes.Add("Copyright", Copyright);
            assemblyAttributes.Add("Trademark", Trademark);
            assemblyAttributes.Add("Configuration", Configuration);
            assemblyAttributes.Add("Delay Sign", DelaySign.ToString());
            assemblyAttributes.Add("Informational Version", InformationalVersion);
            assemblyAttributes.Add("Key File", KeyFile);
            assemblyAttributes.Add("Culture Name", CultureName);
            assemblyAttributes.Add("Satellite Contract Version", SatelliteContractVersion);
            assemblyAttributes.Add("Com Compatible Version", ComCompatibleVersion);
            assemblyAttributes.Add("Com Visible", ComVisible.ToString());
            assemblyAttributes.Add("Guid", Guid);
            assemblyAttributes.Add("Type Lib Version", TypeLibVersion);
            assemblyAttributes.Add("CLS Compliant", CLSCompliant.ToString());

            return assemblyAttributes;
        }

        /// <summary>Gets the specified assembly attribute if it is exposed by the assembly.</summary>
        /// <param name="attributeType">Type of the attribute to get.</param>
        /// <returns>The requested assembly attribute if it exists; otherwise null.</returns>
        /// <remarks>
        /// This method always returns <c>null</c> under Mono deployments.
        /// </remarks>
        public CustomAttributeData GetCustomAttribute(Type attributeType)
        {
#if MONO
            return null;
#else
            //Returns the requested assembly attribute.
            return m_assemblyInstance.GetCustomAttributesData().FirstOrDefault(assemblyAttribute => assemblyAttribute.Constructor.DeclaringType == attributeType);
#endif
        }

        /// <summary>Gets the specified embedded resource from the assembly.</summary>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public Stream GetEmbeddedResource(string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return m_assemblyInstance.GetEmbeddedResource(resourceName);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static AssemblyInfo s_callingAssembly;
        private static AssemblyInfo s_entryAssembly;
        private static AssemblyInfo s_executingAssembly;
        private static Dictionary<string, Assembly> s_assemblyCache;
        private static bool s_addedResolver;

        // Static Properties

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the assembly that invoked the currently executing method.</summary>
        public static AssemblyInfo CallingAssembly
        {
            get
            {
                if ((object)s_callingAssembly == null)
                {
                    // We have to find the calling assembly of the caller.
                    StackTrace trace = new StackTrace();
                    Assembly caller = Assembly.GetCallingAssembly();
                    Assembly current = Assembly.GetExecutingAssembly();
                    foreach (StackFrame frame in trace.GetFrames())
                    {
                        Assembly assembly = Assembly.GetAssembly(frame.GetMethod().DeclaringType);
                        if (assembly != caller && assembly != current)
                        {
                            // Assembly is neither the current assembly or the calling assembly.
                            s_callingAssembly = new AssemblyInfo(assembly);
                            break;
                        }
                    }
                }

                return s_callingAssembly;
            }
        }

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the process executable in the default application domain.</summary>
        public static AssemblyInfo EntryAssembly
        {
            get
            {
                if ((object)s_entryAssembly == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if ((object)entryAssembly == null)
                        entryAssembly = Assembly.ReflectionOnlyLoadFrom(Process.GetCurrentProcess().MainModule.FileName);

                    s_entryAssembly = new AssemblyInfo(entryAssembly);
                }

                return s_entryAssembly;
            }
        }

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the assembly that contains the code that is currently executing.</summary>
        public static AssemblyInfo ExecutingAssembly
        {
            get
            {
                if ((object)s_executingAssembly == null)
                    // Caller's assembly will be the executing assembly for the caller.
                    s_executingAssembly = new AssemblyInfo(Assembly.GetCallingAssembly());

                return s_executingAssembly;
            }
        }

        // Static Methods

        /// <summary>Loads the specified assembly that is embedded as a resource in the assembly.</summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <remarks>This cannot be used to load GSF.Core itself.</remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void LoadAssemblyFromResource(string assemblyName)
        {
            // Hooks into assembly resolve event for current domain so it can load assembly from embedded resource.
            if (!s_addedResolver)
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromResource;
                s_addedResolver = true;
            }

            // Loads the assembly (This will invoke event that will resolve assembly from resource.).
            AppDomain.CurrentDomain.Load(assemblyName);
        }

        private static Assembly ResolveAssemblyFromResource(object sender, ResolveEventArgs e)
        {
            Assembly resourceAssembly;
            string shortName = e.Name.Split(',')[0];

            if ((object)s_assemblyCache == null)
                s_assemblyCache = new Dictionary<string, Assembly>();

            resourceAssembly = s_assemblyCache[shortName];

            if ((object)resourceAssembly == null)
            {
                // Loops through all of the resources in the executing assembly.
                foreach (string name in Assembly.GetEntryAssembly().GetManifestResourceNames())
                {
                    // Sees if the embedded resource name matches the assembly it is trying to load.
                    if (string.Compare(FilePath.GetFileNameWithoutExtension(name), EntryAssembly.RootNamespace + "." + shortName, true) == 0)
                    {
                        // If so, loads embedded resource assembly into a binary buffer.
                        System.IO.Stream resourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(name);
                        byte[] buffer = new byte[resourceStream.Length];
                        resourceStream.Read(buffer, 0, (int)resourceStream.Length);
                        resourceStream.Close();

                        // Loads assembly from binary buffer.
                        resourceAssembly = Assembly.Load(buffer);
                        s_assemblyCache.Add(shortName, resourceAssembly);
                        break;
                    }
                }
            }

            return resourceAssembly;
        }

        #endregion
    }
}