//******************************************************************************************************
//  OIDCSecurityProvider.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/05/2021 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable 67
#pragma warning disable 169

namespace GSF.Security
{
    /// <summary>
    /// Defines a JSON token response for the <see cref="OIDCSecurityProvider"/>.
    /// </summary>
    internal class TokenResponse
    {
        #pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Access token.
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// ID token.
        /// </summary>
        public string id_token { get; set; }

        #pragma warning restore IDE1006
    }

    /// <summary>
    /// Represents an <see cref="ISecurityProvider"/> that uses openID Connect
    /// </summary>
    /// <example>
    /// Required config file entries (automatically added):
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ProviderType" value="GSF.Security.OIDCSecurityProvider, GSF.Security" description="The type to be used for enforcing security."
    ///         encrypted="false" />
    ///       <add name="ClientID" value="xxxx-xxxx-xxxx" description="Defines the ClientID as required per OpenID Connect Standard." encrypted="false" />
    ///       <add name="Scope" value="user" description="Defines the Scope as required per OpenID Connect Standard." encrypted="false" />
    ///       <add name="AuthorizationEndpoint" value="user" description="Defines the Endpoint to redirect the user for Authorization." encrypted="false" />
    ///       <add name="RedirectURI" value="https://localhost:8986/" description="Defines the URI the User get's redirected to after signing in." encrypted="false" />
    ///       <add name="ClientSecret" value="sssss-ssssss-sssss" description="Defines the Client Secret to encrypt User Information." encrypted="false" />
    ///       <add name="SelfVerifiedNonce="aefgdfhf" description="Defines the Client Secret to encrypt User Information." encrypted="false" />
    ///       <add name="TokenEndpoint" value="user" description="Defines the Endpoint to get the User Token from." encrypted="false" />
    ///       <add name="ShowDetailedError" value="true" description="Indicates if the Login Page should display detailed Debuging Information when OAuth Fails." encrypted="false" />
    ///     </securityProvider>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class OIDCSecurityProvider : SecurityProviderBase
    {
        #region [ Members ]

        // Constants
        private const string ResponseType = "code";                          // Response Type the SecurityProvider expects
        private const int NonceSlidingExpiration = 600;                      // Time in Seconds that a Nonce is valid for. This prevents replay attacks

        /// <summary>
        /// Defines the provider ID for the <see cref="AdoSecurityProvider"/>.
        /// </summary>
        public const int ProviderID = 3;

        // Fields
        private string m_clientRequestUri;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoSecurityProvider"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        public OIDCSecurityProvider(string username)
            : base(username, false, false, false)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="OIDCUserData"/> object containing information about the user.
        /// </summary>
        public new OIDCUserData UserData 
        { 
            get => base.UserData as OIDCUserData; 
            protected set => base.UserData = value;
        }

        /// <summary>
        /// Gets last exception reported by the <see cref="AdoSecurityProvider"/>.
        /// </summary>
        public Exception LastException { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="LogAuthenticationAttempt"/> and <see cref="LogError"/> should
        /// write to the database. Defaults to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Setting this flag to <c>false</c> may be necessary in cases where a database has been setup to use authentication
        /// but does not include an "AccessLog" or "ErrorLog" table.
        /// </remarks>
        public bool UseDatabaseLogging { get; set; }

        /// <summary>
        /// The ClienID used to identify this Application with the Authorization Server
        /// </summary>
        public string ClientID { get; set; }

        /// <summary>
        /// The Scope used to obtain UserInformation from the Authorization Server
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// A Nonce that has been verified manually and never expires. This is used for allowing Server-server logons.
        /// </summary>
        public string SelfVerifiedNonce { get; set; }
        /// <summary>
        /// The Endpoint used to redirect the User
        /// </summary>
        public string AuthorizationEndpoint { get; set; }

        /// <summary>
        /// The Endpoint to get the User Token
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// The URI the User get's redirected to after signing in.
        /// </summary>
        public string RedirectURI { get; set; }

        /// <summary>
        /// The ClientSecret used to encrypt the user data
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The Claim used to get the Roles for the user
        /// </summary>
        public string RolesClaim { get; set; }

        /// <summary>
        /// Gets the flag that indicates whether the user 
        /// needs to be redirected after the Authentication attempt. 
        /// </summary>
        public override bool IsRedirectRequested => !string.IsNullOrEmpty(m_clientRequestUri);

        /// <summary>
        /// Gets the URI that user will be redirected to if <see cref="IsRedirectRequested"/> is set.
        /// </summary>
        public override string RequestedRedirect 
        {
            get 
            {
                string val = m_clientRequestUri;
                m_clientRequestUri = "";
                return val;
            }
        }

        /// <summary>
        /// Indicates if the Login Page should display detailed Debugging Information when OAuth Fails.
        /// </summary>
        public bool ShowDetailedError { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads saved security provider settings from the config file if the <see cref="SecurityProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SecurityProviderBase.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void LoadSettings()
        {
            base.LoadSettings();

            // Make sure default settings exist
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings.Add(nameof(ClientID), "", "Defines the ClientID as required per OpenID Connect Standard.");
            settings.Add(nameof(Scope), "", "Defines the Scope as required per OpenID Connect Standard.");
            settings.Add(nameof(AuthorizationEndpoint), "", "Defines the Endpoint to redirect the user for Authorization.");
            settings.Add(nameof(RedirectURI), "", "Defines the URI the User get's redirected to after signing in.");
            settings.Add(nameof(ClientSecret), "", "Defines the Client Secret as required per OpenID Connect Standard for decrypting User Information.");
            settings.Add(nameof(TokenEndpoint), "", "Defines the Endpoint to use to grab the User Token.");
            settings.Add(nameof(RolesClaim), "roles", "Defines the claim used to identify the users roles.");
            settings.Add(nameof(SelfVerifiedNonce), "", "Defines a nonce that is verified manually.");
            settings.Add(nameof(ShowDetailedError), "false", "Indicates if the Login Page should display detailed Debuging Information when OAuth Fails.");

            ClientID = settings[nameof(ClientID)].ValueAs(ClientID);
            Scope = settings[nameof(Scope)].ValueAs(Scope);
            AuthorizationEndpoint = settings[nameof(AuthorizationEndpoint)].ValueAs(AuthorizationEndpoint);
            RedirectURI = settings[nameof(RedirectURI)].ValueAs(RedirectURI);
            ClientSecret = settings[nameof(ClientSecret)].ValueAs(ClientSecret);
            TokenEndpoint = settings[nameof(TokenEndpoint)].ValueAs(TokenEndpoint);
            RolesClaim = settings[nameof(RolesClaim)].ValueAs(RolesClaim);
            SelfVerifiedNonce = settings[nameof(SelfVerifiedNonce)].ValueAs(SelfVerifiedNonce);
            ShowDetailedError = settings[nameof(ShowDetailedError)].ValueAs(ShowDetailedError);
        }

        /// <summary>
        /// Not implemented by <see cref="OIDCSecurityProvider"/>; always returns <c>false</c>.
        /// </summary>
        /// <returns>true if <see cref="SecurityProviderBase.UserData"/> is refreshed, otherwise false.</returns>
        public override bool RefreshData() => false;
       
        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public override bool Authenticate()
        {
            // Reset authenticated state and failure reason
            AuthenticationFailureReason = null;
            IsUserAuthenticated = false;

            try
            {
                RequestToken(base.UserData.Username);

                if (UserData.Roles.Count == 0)
                {
                    AuthenticationFailureReason = $"User \"{UserData.LoginID}\" has not been assigned any roles and therefore has no rights. Contact your administrator.";
                    IsUserAuthenticated = false;
                }
                else
                {
                    IsUserAuthenticated = !string.IsNullOrEmpty(UserData.Nonce);
                }


                // Log user authentication result
                LogAuthenticationAttempt(IsUserAuthenticated);
            }
            catch (Exception ex)
            {
                // Writing data will fail for read-only databases;
                // all we can do is track last exception in this case
                LastException = ex;
                Log.Publish(MessageLevel.Warning, MessageFlags.SecurityMessage, "Authenticate", "Failed to log authentication attempt to database.", "Database or AccessLog table may be read-only or inaccessible.", ex);
            }

            return IsUserAuthenticated;
        }

        /// <summary>
        /// Logs user authentication attempt.
        /// </summary>
        /// <param name="loginSuccess">true if user authentication was successful, otherwise false.</param>
        protected virtual void LogAuthenticationAttempt(bool loginSuccess)
        {
            if (UserData is null || string.IsNullOrWhiteSpace(UserData.Username))
                return;

            string message = $"User \"{UserData.Username}\" login attempt {(loginSuccess ? "succeeded using OpenID Connect" : "failed")}.";
            EventLogEntryType entryType = loginSuccess ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit;

            // Suffix authentication failure reason on failed logins if available
            if (!loginSuccess && !string.IsNullOrWhiteSpace(AuthenticationFailureReason))
                message = string.Concat(message, " ", AuthenticationFailureReason);

            // Attempt to write success or failure to the event log
            try
            {
                Log.Publish(MessageLevel.Info, MessageFlags.SecurityMessage, "AuthenticationAttempt", message);
                LogEvent(ApplicationName, message, entryType, 1);
            }
            catch (Exception ex)
            {
                LogError(ex.Source, ex.ToString());
            }

            // Attempt to write success or failure to the database - we allow caller to catch any possible exceptions here so that
            // database exceptions can be tracked separately (via LastException property) from other login exceptions, e.g., when
            // a read-only database is being used or current user only has read-only access to database.
            if (!string.IsNullOrWhiteSpace(SettingsCategory) && UseDatabaseLogging)
            {
                using AdoDataConnection connection = new(SettingsCategory);
                connection.ExecuteNonQuery("INSERT INTO AccessLog (UserName, AccessGranted) VALUES ({0}, {1})", UserData.Username, loginSuccess ? 1 : 0);
            }
        }

        /// <summary>
        /// Logs information about an encountered exception to the backend data store.
        /// </summary>
        /// <param name="source">Source of the exception.</param>
        /// <param name="message">Detailed description of the exception.</param>
        /// <returns>true if logging was successful, otherwise false.</returns>
        protected virtual bool LogError(string source, string message)
        {
            if (string.IsNullOrWhiteSpace(SettingsCategory) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(message))
                return false;

            Log.Publish(MessageLevel.Error, MessageFlags.SecurityMessage, source, message);

            if (!UseDatabaseLogging)
                return false;

            try
            {
                using AdoDataConnection connection = new(SettingsCategory);                
                connection.ExecuteNonQuery("INSERT INTO ErrorLog (Source, Message) VALUES ({0}, {1})", source, message);
                return true;
            }
            catch (Exception ex)
            {
                // Writing data will fail for read-only databases;
                // all we can do is track last exception in this case
                LastException = ex;
                Log.Publish(MessageLevel.Warning, MessageFlags.SecurityMessage, "LogErrorToDatabase", "Failed to log error to database.", "Database or ErrorLog table may be read-only or inaccessible.", ex);
            }
            
            return false;
        }

        /// <summary>
        /// Exchange Authorization Code for a Token
        /// </summary>
        /// <param name="code"> The Authorization Code returned by the Auth Server</param>
        private async Task<TokenResponse> GetTokenAsync(string code)
        {
            Dictionary<string, string> postParams = new() {
                { "grant_type", "authorization_code" },
                { "client_id", ClientID },
                { "client_secret", ClientSecret },
                { "code", code },
                { "redirect_uri", RedirectURI }
            };

            void ConfigureRequest(HttpRequestMessage request)
            {
                request.RequestUri = new Uri(TokenEndpoint);
                request.Method = HttpMethod.Post;               
                request.Content = new FormUrlEncodedContent(postParams);
            }

            using (HttpRequestMessage request = new())
            {
                ConfigureRequest(request);

                using HttpResponseMessage response = await Client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return null;

                return JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        private JObject DecodeJWT(string token)
        {
            if (!token.Contains("."))
                throw new InvalidExpressionException("A valid JWT token requires at least one '.'");

            const int JOSEHeaderIndex = 0;
            const int PayloadIndex = 1;
            //const int SignatureIndex = 2;

            string[] splitToken = token.Split('.');

            if (splitToken.Length <= PayloadIndex)
                throw new FormatException("JWT token has no payload");

            string joseHeader = splitToken[JOSEHeaderIndex];
            byte[] headerData = Convert.FromBase64String(joseHeader);
            joseHeader = Encoding.UTF8.GetString(headerData);
            JObject header = JObject.Parse(joseHeader);

            if (header.TryGetValue("enc", out _))
                throw new FormatException("JWE Tokens are not supported");

            // TODO: Implement signature verifications based on config file setting
            //if (SignatureIndex < splitToken.Length)
            //{
            //    // We are not validating signatures to allow openXDA to self-sign tokens
            //    void ValidateToken(string _) { }
            //    string signature = splitToken[SignatureIndex];
            //    ValidateToken(signature);
            //}

            string payload = splitToken[PayloadIndex];
            byte[] payloadData = Convert.FromBase64String(payload);
            string payloadContent = Encoding.UTF8.GetString(payloadData);

            if (header.TryGetValue("cty", out JToken cty) && cty.ToString().ToLower() == "jwt")
                return DecodeJWT(payloadContent);

            return JObject.Parse(payloadContent);
        }

        /// <summary>
        /// Performs a translation of the default login page to a different endpoint.
        /// </summary>
        /// <param name="loginUrl"> The URI of the login page specified in the AppSettings </param>
        /// <param name="uri"> The URI originally requested. </param>
        /// <param name="encodedPath"> The URI requested by the client </param>
        /// <param name="referrer"> The Referrer as specified in the request header </param>
        /// <returns> The URI to be redirected to</returns>
        public override string TranslateRedirect(string loginUrl, Uri uri, string encodedPath, string referrer)
        {
            Uri redirectURi = new(RedirectURI);

            if (redirectURi.AbsolutePath == uri.AbsolutePath && (uri.Query.Contains("code=") || uri.Query.Contains("error=")))
            {
                if (LastException is not null)
                    return $"{loginUrl}?oidcError={WebUtility.UrlEncode(LastException.Message)}";

                //This is a redirect issue and needs to display error page
                if (ShowDetailedError && uri.Query.Contains("error_description="))
                    return $"{loginUrl}?oidcError={WebUtility.UrlEncode(System.Web.HttpUtility.ParseQueryString(uri.Query).Get("error_description"))}";
                
                return $"{loginUrl}?oidcError={WebUtility.UrlEncode("A Server Error Occured.")}";
            }

            byte[] nonce = new byte[16];
            Cryptography.Random.GetBytes(nonce);

            s_nonceCache.Add(BitConverter.ToString(nonce).Replace("-", ""), encodedPath, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(NonceSlidingExpiration) });

            StringBuilder redirect = new();
            redirect.Append(AuthorizationEndpoint);
            redirect.Append($"?response_type={ResponseType}");
            redirect.Append($"&scope={Scope}");
            redirect.Append($"&client_id={ClientID}");
            redirect.Append($"&redirect_uri={RedirectURI}");
            redirect.Append($"&nonce={BitConverter.ToString(nonce).Replace("-", "")}");

            return redirect.ToString();
        }

        /// <summary>
        /// Not implemented by <see cref="OIDCSecurityProvider"/>; always returns <c>false</c>.
        /// </summary>
        /// <param name="securityAnswer">Answer to the user's security question.</param>
        /// <returns>true if the password is reset, otherwise false.</returns>
        /// <remarks></remarks>
        public override bool ResetPassword(string securityAnswer) => 
            false;

        /// <summary>
        /// Not implemented by <see cref="OIDCSecurityProvider"/>; always returns <c>false</c>.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        public override bool ChangePassword(string oldPassword, string newPassword) => 
            false;

        /// <summary>
        /// Not implemented by <see cref="OIDCSecurityProvider"/>; always returns <c>false</c>.
        /// </summary>
        public override bool CanRefreshData => false;

        /// <summary>
        /// Obtains the Token from the OIDC Server using a code.
        /// </summary>
        private void RequestToken(string code)
        {
            if (string.IsNullOrEmpty(code))
                return;

            // Already have token for this user, do not re-initialize user data
            if (UserData?.Token is not null)
                return;

            m_clientRequestUri = "";

            // Get user token
            TokenResponse token = GetTokenAsync(code).GetAwaiter().GetResult();

            JObject tokenContent = DecodeJWT(token.id_token);

            // Translate UserDetails according to Token
            if (!tokenContent.TryGetValue("sub", out JToken sub))
                throw new Exception("Token data was not derived from valid JWT: missing 'sub'");

            if (!tokenContent.TryGetValue("nonce", out JToken nonce))
                throw new Exception("Token data was not derived from valid JWT: missing 'nonce'");

            // TODO: validate Nonce matches nonce send for this user

            if (!tokenContent.TryGetValue("name", out JToken userName))
                throw new Exception("Token data was not derived from valid JWT: missing 'name'");

            OIDCUserData userData = new(sub.ToString())
            {
                Username = userName.ToString(),
                Nonce = nonce.ToString(),
                Token = token
            };

            // Initialize user data.
            userData.Initialize();

            if (tokenContent.TryGetValue("given_name", out JToken firstName))
                userData.FirstName = firstName.ToString();

            if (tokenContent.TryGetValue("family_name", out JToken lastName))
                userData.LastName = lastName.ToString();

            if (tokenContent.TryGetValue("phone_number", out JToken phoneNumber))
                userData.PhoneNumber = phoneNumber.ToString();

            if (tokenContent.TryGetValue("email", out JToken email))
                userData.EmailAddress = email.ToString();

            try
            {
                // Roles are obtained from a Claim
                userData.Roles = tokenContent.GetOrDefault(RolesClaim).ToString().Split(',').ToList();
            }
            catch (Exception ex)
            {
                LastException = ex;
                Log.Publish(MessageLevel.Warning, MessageFlags.SecurityMessage, "DecodingTokenError", "Failed to decode Roles Claim.", "Failed to decode Claim for roles.", ex);
                throw new Exception("Failed to Decode Roles Claim: " + ex.Message, ex);
            }

            userData.IsDefined = true;
            userData.IsDisabled = false;
            userData.IsLockedOut = false;

            UserData = userData;

            if (s_nonceCache.Contains(nonce.ToString()))
            {
                string base64Path = WebUtility.UrlDecode((string)s_nonceCache.Get(nonce.ToString()));
                byte[] pathBytes = Convert.FromBase64String(base64Path);
                m_clientRequestUri = Encoding.UTF8.GetString(pathBytes);
            }          
        }
        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(OIDCSecurityProvider), MessageClass.Component);

        private static readonly MemoryCache s_nonceCache = new("OIDC-NonceCache");

        private static readonly HttpClient Client = new();
       
        #endregion
    }
}
