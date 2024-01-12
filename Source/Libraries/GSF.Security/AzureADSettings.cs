//******************************************************************************************************
//  AzureADSettings.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/15/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GSF.Configuration;
using GSF.IO;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using File = System.IO.File;

namespace GSF.Security;

/// <summary>
/// Represents settings needed for Azure Active Directory (AD) authentication.
/// </summary>
public class AzureADSettings
{
    /// <summary>
    /// Default category for Azure AD settings.
    /// </summary>
    public const string DefaultSettingsCategory = "AzureAd";

    /// <summary>
    /// Default value for <see cref="Instance"/> property.
    /// </summary>
    public const string DefaultInstance = "https://login.microsoftonline.com/";

    /// <summary>
    /// Default value for <see cref="TenantID"/> property.
    /// </summary>
    public const string DefaultTenantID = "";

    /// <summary>
    /// Default value for <see cref="ClientID"/> property.
    /// </summary>
    public const string DefaultClientID = "";

    /// <summary>
    /// Default value for <see cref="RedirectURI"/> property.
    /// </summary>
    public const string DefaultRedirectURI = "/";

    /// <summary>
    /// Default value for <see cref="CallbackPath"/> property.
    /// </summary>
    public const string DefaultCallbackPath = "/signin-oidc";

    /// <summary>
    /// Default value for <see cref="SignedOutCallbackPath"/> property.
    /// </summary>
    public const string DefaultSignedOutCallbackPath = "/signout-oidc";

    /// <summary>
    /// Default value for <see cref="Enabled"/> property.
    /// </summary>
    public const bool DefaultEnabled = false;

    private const string DefaultAzureADConfigSource = "appsettings.json";

    private const string DefaultAzureADSettings = @$"{{
  ""{DefaultSettingsCategory}"": {{
    ""Instance"": ""{DefaultInstance}"",
    ""TenantId"": ""[Enter the tenantId here]"",

    // Client ID (application ID) obtained from the Azure portal
    ""ClientId"": ""[Enter the Client Id here]"",
    ""RedirectURI"": ""{DefaultRedirectURI}"",
    ""CallbackPath"": ""{DefaultCallbackPath}"",
    ""SignedOutCallbackPath"": ""{DefaultSignedOutCallbackPath}"",
    ""Enabled"":  false
  }}
}}";

    /// <summary>
    /// Gets or sets the Azure AD instance URL.
    /// </summary>
    public string Instance { get; set; } = DefaultInstance;

    /// <summary>
    /// Gets or sets the Azure AD tenant ID.
    /// </summary>
    public string TenantID { get; set; } = DefaultTenantID;

    /// <summary>
    /// Gets or sets the Azure AD client ID.
    /// </summary>
    public string ClientID { get; set; } = DefaultClientID;

    /// <summary>
    /// Gets or sets the redirect URI where authentication responses can be received by the application.
    /// </summary>
    public string RedirectURI { get; set; } = DefaultRedirectURI;

    /// <summary>
    /// Gets or sets the Azure AD call-back path.
    /// </summary>
    public string CallbackPath { get; set; } = DefaultCallbackPath;

    /// <summary>
    /// Gets or sets the Azure AD signed out call-back path.
    /// </summary>
    public string SignedOutCallbackPath { get; set; } = DefaultSignedOutCallbackPath;

    /// <summary>
    /// Gets the flag that determines if Azure AD is enabled.
    /// </summary>
    public bool Enabled { get; set; } = DefaultEnabled;

    /// <summary>
    /// Gets the Azure AD authority (Instance + TenantID).
    /// </summary>
    public Uri Authority => new($"{Instance}{TenantID}");

    /// <summary>
    /// Gets the last exception, if any, encountered after getting a new Graph service client.
    /// </summary>
    public Exception LastException { get; set; }

    /// <summary>
    /// Gets a new Graph service client.
    /// </summary>
    /// <param name="settingsCategory">Settings category to use for determine configuration location.</param>
    /// <param name="forceRefresh">Set to <c>true</c> to force refresh of Azure AD token.</param>
    /// <returns>New Graph service client when Azure AD is enabled; otherwise, <c>null</c>.</returns>
    public GraphServiceClient GetGraphClient(string settingsCategory = null, bool forceRefresh = false)
    {
        const string AzureADSecretKey = "AzureADSecret";

        if (!Enabled)
            return null;
        
        if (string.IsNullOrEmpty(settingsCategory))
            settingsCategory = SecurityProviderBase.DefaultSettingsCategory;

        // Make sure default settings exist
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[settingsCategory];

        settings.Add(AzureADSecretKey, $"env({nameof(AzureADSecretKey)})", "Defines the Azure AD secret value to be used for user info and group lookups, post authentication.");
        config.Save(ConfigurationSaveMode.Modified);

        string secret = settings[AzureADSecretKey].ValueAs("");

        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException($"Cannot create GraphServiceClient: No Azure AD secret value is defined in \"{settingsCategory}\" settings category.");

        IConfidentialClientApplication clientApplication = ConfidentialClientApplicationBuilder.Create(ClientID)
            .WithClientSecret(secret)
            .WithAuthority(Authority)
            .Build();
                        
        clientApplication.AddInMemoryTokenCache();

        LastException = null;

        return new GraphServiceClient("https://graph.microsoft.com/V1.0/", new DelegateAuthenticationProvider(requestMessage =>
        {
            try
            {
                // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                AuthenticationResult result = clientApplication.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).WithForceRefresh(forceRefresh).ExecuteAsync().Result;

                // Add the access token in the Authorization header of the API request.
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            }
            catch (AggregateException ex)
            {
                throw new InvalidOperationException(string.Join("; ", ex.Flatten().InnerExceptions.Select(inex => inex.Message)), ex);
            }
            catch (Exception ex)
            {
                LastException = new InvalidOperationException($"Failed to get client token: {ex.Message}", ex);
            }

            return Task.FromResult(0);
        }));
    }

    /// <summary>
    /// Loads Azure AD settings. Source based on target configuration.
    /// </summary>
    /// <param name="settingsCategory">Settings category to use for determine configuration location.</param>
    /// <returns>Loaded <see cref="AzureADSettings"/> settings instance.</returns>
    public static AzureADSettings Load(string settingsCategory = null)
    {
        const string AzureADConfigSource = "AzureADConfigSource";

        if (string.IsNullOrEmpty(settingsCategory))
            settingsCategory = SecurityProviderBase.DefaultSettingsCategory;

        // Make sure default settings exist
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[settingsCategory];

        settings.Add(AzureADConfigSource, DefaultAzureADConfigSource, "Azure AD configuration source. Either 'appsettings.json' file path or settings category to use.");
        string configSource = settings[AzureADConfigSource].ValueAs(DefaultAzureADConfigSource).Trim();

        return configSource.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? 
            LoadFromAppSettings(configSource) : 
            LoadFromConfig(configSource);
    }

    /// <summary>
    /// Loads Azure AD settings from the specified JSON application settings file.
    /// </summary>
    /// <param name="filepath">JSON settings file to load. Defaults to local "appsettings.json".</param>
    /// <returns>Loaded <see cref="AzureADSettings"/> settings instance.</returns>
    public static AzureADSettings LoadFromAppSettings(string filepath = null)
    {
        if (string.IsNullOrEmpty(filepath))
            filepath = "appsettings.json";

        filepath = FilePath.GetAbsolutePath(filepath);

        // Make sure default settings exist
        if (!File.Exists(filepath))
            File.WriteAllText(filepath, DefaultAzureADSettings);

        dynamic settings = JsonConvert.DeserializeObject(File.ReadAllText(filepath));
        settings = settings?.AzureAd;

        return new AzureADSettings
        {
            Instance = settings?.Instance ?? DefaultInstance,
            TenantID = settings?.TenantId ?? DefaultTenantID,
            ClientID = settings?.ClientId ?? DefaultClientID,
            RedirectURI = settings?.RedirectURI ?? DefaultRedirectURI,
            CallbackPath = settings?.CallbackPath ?? DefaultCallbackPath,
            SignedOutCallbackPath = settings?.SignedOutCallbackPath ?? DefaultSignedOutCallbackPath,
            Enabled = settings?.Enabled ?? DefaultEnabled
        };
    }

    /// <summary>
    /// Loads Azure AD settings from categorized settings.
    /// </summary>
    /// <param name="settingsCategory">Settings category to use for settings load.</param>
    /// <returns>Loaded <see cref="AzureADSettings"/> settings instance.</returns>
    public static AzureADSettings LoadFromConfig(string settingsCategory = null)
    {
        if (string.IsNullOrEmpty(settingsCategory))
            settingsCategory = DefaultSettingsCategory;

        // Make sure default settings exist
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[settingsCategory];

        settings.Add(nameof(Instance), DefaultInstance, "Azure AD instance URL.");
        settings.Add(nameof(TenantID), DefaultTenantID, "Azure AD tenant ID.");
        settings.Add(nameof(ClientID), DefaultClientID, "Azure AD client ID.");
        settings.Add(nameof(RedirectURI), DefaultRedirectURI, "URI where authentication responses can be received by the application");
        settings.Add(nameof(CallbackPath), DefaultCallbackPath, "Azure AD call-back path.");
        settings.Add(nameof(SignedOutCallbackPath), DefaultSignedOutCallbackPath, "Azure AD signed out call-back path.");
        settings.Add(nameof(Enabled), DefaultEnabled, "Flag that determines if Azure AD is enabled.");

        return new AzureADSettings
        {
            Instance = settings[nameof(Instance)].ValueAs(DefaultInstance),
            TenantID = settings[nameof(TenantID)].ValueAs(DefaultTenantID),
            ClientID = settings[nameof(ClientID)].ValueAs(DefaultClientID),
            RedirectURI = settings[nameof(RedirectURI)].ValueAs(DefaultRedirectURI),
            CallbackPath = settings[nameof(CallbackPath)].ValueAs(DefaultCallbackPath),
            SignedOutCallbackPath = settings[nameof(SignedOutCallbackPath)].ValueAs(DefaultSignedOutCallbackPath),
            Enabled = settings[nameof(Enabled)].ValueAs(DefaultEnabled),
        };
    }
}