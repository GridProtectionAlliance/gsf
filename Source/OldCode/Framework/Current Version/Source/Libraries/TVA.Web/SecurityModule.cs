//*******************************************************************************************************
//  SecurityModule.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/31/2010 - Pinal C. Patel
//       Generated original version of source code.
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
 Original Software Title: The TVA Open Source Phasor Data Concentrator
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using TVA.Configuration;
using TVA.IO;
using TVA.Security;
using System.Web.Hosting;

namespace TVA.Web
{
    #region [ Enumerations ]

    #endregion

    /// <summary>
    /// Represents an HTTP module that can be used to enable site-wide role-based security.
    /// </summary>
    public class SecurityModule : IHttpModule, IPersistSettings
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// A handler used to force the SessionStateModule to load session state.
        /// </summary>
        private class SessionEnabledHandler : IHttpHandler, IRequiresSessionState
        {
            public IHttpHandler OriginalHandler;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionEnabledHandler"/> class.
            /// </summary>
            /// <param name="originalHandler">The original handler object.</param>
            public SessionEnabledHandler(IHttpHandler originalHandler)
            {
                OriginalHandler = originalHandler;
            }

            /// <summary>
            /// This method will never get called.
            /// </summary>
            public void ProcessRequest(HttpContext context)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Returns false since class has a member.
            /// </summary>
            public bool IsReusable
            {
                get { return false; }
            }
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ProviderType"/> property.
        /// </summary>
        public const string DefaultProviderType = "TVA.Security.SecureUser, TVA.Security";

        /// <summary>
        /// Specifies the default value for the <see cref="Error401Page"/> property.
        /// </summary>
        public const string DefaultError401Page = "";

        /// <summary>
        /// Specifies the default value for the <see cref="Error403Page"/> property.
        /// </summary>
        public const string DefaultError403Page = "";

        /// <summary>
        /// Specifies the default value for the <see cref="IncludedResources"/> property.
        /// </summary>
        public const string DefaultIncludedResources = "*.*=*";

        /// <summary>
        /// Specifies the default value for the <see cref="ExcludedResources"/> property.
        /// </summary>
        public const string DefaultExcludedResources = "*.gif;*.jpg;*.png;*.css;*.js";

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "SecurityModule";

        // Fields
        private string m_providerType;
        private string m_error401Page;
        private string m_error403Page;
        private Dictionary<string, string> m_includedResources;
        private string[] m_excludedResources;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private HttpApplication m_application;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityModule"/> class.
        /// </summary>
        public SecurityModule()
        {
            m_providerType = DefaultProviderType;
            m_error401Page = DefaultError401Page;
            m_error403Page = DefaultError403Page;
            m_includedResources = DefaultIncludedResources.ParseKeyValuePairs();
            m_excludedResources = DefaultExcludedResources.Split(';');
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Type"/> based on <see cref="SecurityProvider"/> to be used for enforcing security.
        /// </summary>
        public string ProviderType
        {
            get
            {
                return m_providerType;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_providerType = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL of a custom HTTP 401 error page.
        /// </summary>
        public string Error401Page
        {
            get
            {
                return m_error401Page;
            }
            set
            {
                m_error401Page = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL of a custom HTTP 403 error page.
        /// </summary>
        public string Error403Page
        {
            get
            {
                return m_error403Page;
            }
            set
            {
                m_error403Page = value;
            }
        }

        /// <summary>
        /// Gets or sets a semicolon delimited list of resources to be secured along with role names.
        /// </summary>
        public string IncludedResources
        {
            get
            {
                return m_includedResources.JoinKeyValuePairs();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    m_includedResources = new Dictionary<string, string>();
                else
                    m_includedResources = value.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets or sets a semicolon delimited list of resources to be excluded from being secured.
        /// </summary>
        public string ExcludedResources
        {
            get
            {
                return string.Join(";", m_excludedResources);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    m_excludedResources = new string[] { };
                else
                    m_excludedResources = value.Split(';');
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="SecurityModule"/> settings are to be saved to the config file.
        /// </summary>
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which <see cref="SecurityModule"/> settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="SecurityModule"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["ProviderType", true].Update(m_providerType);
                settings["Error401Page", true].Update(m_error401Page);
                settings["Error403Page", true].Update(m_error403Page);
                settings["IncludedResources", true].Update(IncludedResources);
                settings["ExcludedResources", true].Update(ExcludedResources);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="SecurityModule"/> settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("ProviderType", m_providerType, "The type to be used for enforcing security.");
                settings.Add("Error401Page", m_error401Page, "URL of a custom HTTP 401 error page.");
                settings.Add("Error403Page", m_error403Page, "URL of a custom HTTP 403 error page.");
                settings.Add("IncludedResources", IncludedResources, "Semicolon delimited list of resources to be secured along with role names.");
                settings.Add("ExcludedResources", ExcludedResources, "Semicolon delimited list of resources to be excluded from being secured.");
                ProviderType = settings["ProviderType"].ValueAs(m_providerType);
                Error401Page = settings["Error401Page"].ValueAs(m_error401Page);
                Error403Page = settings["Error403Page"].ValueAs(m_error403Page);
                IncludedResources = settings["IncludedResources"].ValueAs(IncludedResources);
                ExcludedResources = settings["ExcludedResources"].ValueAs(ExcludedResources);
            }
        }

        /// <summary>
        /// Initializes the <see cref="SecurityModule"/>.
        /// </summary>
        /// <param name="context">An <see cref="HttpApplication"/> object.</param>
        public void Init(HttpApplication context)
        {
            LoadSettings();
            m_application = context;
            m_application.PostMapRequestHandler += Application_PostMapRequestHandler;
            m_application.PostAcquireRequestState += Application_PostAcquireRequestState;
            m_application.PreRequestHandlerExecute += Application_PreRequestHandlerExecute;

            //if (!(HostingEnvironment.VirtualPathProvider is EmbeddedResourcePathProvider))
            //    HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourcePathProvider());
        }

        /// <summary>
        /// Releases the resources used by <see cref="SecurityModule"/>.
        /// </summary>
        public void Dispose()
        {
            SaveSettings();
            m_application.PostMapRequestHandler -= Application_PostMapRequestHandler;
            m_application.PostAcquireRequestState -= Application_PostAcquireRequestState;
            m_application.PreRequestHandlerExecute -= Application_PreRequestHandlerExecute;
        }

        private void Application_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (IsResourceExcluded())
                return;

            if (m_application.Context.Handler is IReadOnlySessionState ||
                m_application.Context.Handler is IRequiresSessionState)
                // no need to replace the current handler 
                return;

            // swap the current handler 
            m_application.Context.Handler = new SessionEnabledHandler(m_application.Context.Handler);
        }

        private void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            if (IsResourceExcluded())
                return;

            SessionEnabledHandler handler = HttpContext.Current.Handler as SessionEnabledHandler;
            if (handler != null)
                // set the original handler back 
                HttpContext.Current.Handler = handler.OriginalHandler;
        }

        private void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (IsResourceExcluded())
                return;

            if (SecurityProvider.Current == null)
                SecurityProvider.Current = Activator.CreateInstance(Type.GetType(m_providerType)) as SecurityProvider;

            if (!m_application.User.Identity.IsAuthenticated)
            {
                if (!ValidateCredentials())
                {
                    // Failed to authenticate user.
                    ReturnStatusCode401();
                    return;
                }
            }

            if (!IsResourceAccessible())
                ReturnStatusCode403();
        }

        private bool IsResourceExcluded()
        {
            string resource = m_application.Request.Url.Segments[m_application.Request.Url.Segments.Length - 1];
            // Check if resource is excluded explicitly by being included in ExcludedResources.
            foreach (string exclusion in m_excludedResources)
            {
                if (FilePath.IsFilePatternMatch(exclusion, resource, true))
                    return true;
            }
            // Check if resource is included explicitly by being included in IncludedResources.
            foreach (KeyValuePair<string, string> inclusion in m_includedResources)
            {
                if (FilePath.IsFilePatternMatch(inclusion.Key, resource, true))
                    return false;
            }

            return true;
        }

        private bool IsResourceAccessible()
        {
            // Check if the resource being accessed has a role-based access restriction on it.
            string resource = m_application.Request.Url.Segments[m_application.Request.Url.Segments.Length - 1];
            foreach (KeyValuePair<string, string> inclusion in m_includedResources)
            {
                if (FilePath.IsFilePatternMatch(inclusion.Key, resource, true) &&
                    (inclusion.Value.Trim() == "*" || m_application.User.IsInRole(inclusion.Value)))
                    return true;
            }

            return false;
        }

        private void ReturnStatusCode401()
        {
            if (!string.IsNullOrEmpty(m_error401Page))
                m_application.Response.Redirect(m_error401Page, true);

            StringBuilder response = new StringBuilder();
            response.Append("<html>\r\n");
            response.Append("<head>\r\n");
            response.Append("    <title>Error 401 - Unauthorized</title>\r\n");
            response.Append("</head>");
            response.Append("<body>\r\n");
            response.Append("    <h1>401 - Unauthorized</h1>\r\n");
            response.Append("    <blockquote>\r\n");
            response.Append("        <p>\r\n");
            response.Append("            The URL you've requested, requires a correct username and password. Either you \r\n");
            response.Append("            entered an incorrect username/password, or your browser doesn't support this feature.\r\n");
            response.Append("        </p>\r\n");
            response.Append("    </blockquote>\r\n");
            response.Append("</body>\r\n");
            response.Append("</html>\r\n");

            m_application.Response.StatusCode = 401;
            m_application.Response.StatusDescription = "Unauthorized";
            m_application.Response.TrySkipIisCustomErrors = true;
            m_application.Response.Write(response.ToString());
            m_application.Response.AppendHeader("WWW-Authenticate", string.Format("Basic Realm=\"{0}\"", Environment.UserDomainName));
            m_application.CompleteRequest();
        }

        private void ReturnStatusCode403()
        {
            if (!string.IsNullOrEmpty(m_error403Page))
                m_application.Response.Redirect(m_error403Page, true);

            StringBuilder response = new StringBuilder();
            response.Append("<html>\r\n");
            response.Append("<head>\r\n");
            response.Append("    <title>Error 403 - Forbidden</title>\r\n");
            response.Append("</head>");
            response.Append("<body>\r\n");
            response.Append("    <h1>403 - Forbidden</h1>\r\n");
            response.Append("    <blockquote>\r\n");
            response.Append("        <p>\r\n");
            response.Append("            You do not have permission to retrieve the requested URL. Please \r\n");
            response.Append("            contact the site administrator if you think this was a mistake.\r\n");
            response.Append("        </p>\r\n");
            response.Append("    </blockquote>\r\n");
            response.Append("</body>\r\n");
            response.Append("</html>\r\n");

            m_application.Response.StatusCode = 403;
            m_application.Response.StatusDescription = "Forbidden";
            m_application.Response.TrySkipIisCustomErrors = true;
            m_application.Response.Write(response.ToString());
            m_application.CompleteRequest();
        }

        private bool ValidateCredentials()
        {
            string authorization = m_application.Request.Headers["Authorization"].ToNonNullString().Trim();
            if (authorization.Length != 0 && authorization.StartsWith("Basic"))
            {
                // cut the word "basic" and decode from base64
                // get "username:password"
                byte[] tempConverted = Convert.FromBase64String(authorization.Substring(6));
                string userInfo = Encoding.ASCII.GetString(tempConverted);

                // get "username"
                // get "password"
                string[] credentials = userInfo.Split(':');
                if (credentials.Length == 2)
                {
                    string username = credentials[0];
                    string password = credentials[1];
                    SecurityProvider user = Activator.CreateInstance(Type.GetType(m_providerType), username) as SecurityProvider;
                    user.Initialize();
                    user.Authenticate(password);
                    SecurityProvider.Current = user;

                    return Thread.CurrentPrincipal.Identity.IsAuthenticated;
                }
            }
            return false;
        }

        #endregion
    }
}
