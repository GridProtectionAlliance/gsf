//******************************************************************************************************
//  RazorEngine.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  05/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a Razor engine instance for the specified <typeparamref name="TLanguage"/> constraint.
    /// </summary>
    /// <typeparam name="TLanguage"><see cref="LanguageConstraint"/> for the Razor engine.</typeparam>
    public class RazorEngine<TLanguage> : IRazorEngine where TLanguage : LanguageConstraint, new()
    {
        #region [ Members ]

        // Fields
        private readonly IRazorEngineService m_engineService;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RazorEngine{TLanguage}"/> instance.
        /// </summary>
        /// <param name="templatePath">Template path for view files.</param>
        public RazorEngine(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
                throw new ArgumentNullException(nameof(templatePath));

            TLanguage languageType = new TLanguage();

            switch (languageType.ResolutionMode)
            {
                case RazorViewResolutionMode.ResolvePath:
                case RazorViewResolutionMode.WatchingResolvePath:
                    if (!Directory.Exists(templatePath))
                        throw new DirectoryNotFoundException();

                    if (languageType.ResolutionMode == RazorViewResolutionMode.ResolvePath)
                    {
                        m_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                        {
                            Language = languageType.TargetLanguage,
                            TemplateManager = new ResolvePathTemplateManager(new[] { templatePath }),
                            Debug = false
                        });
                    }
                    else
                    {
                        // The watching resolve path template manager should not be used in production since
                        // assemblies cannot be unloaded from an AppDomain. Every time a change to a .cshtml
                        // file is picked up by the watcher it is compiled and loaded into the AppDomain and
                        // the old one cannot be removed (.NET restriction), the net result is a memory leak
                        InvalidatingCachingProvider cachingProvider = new InvalidatingCachingProvider();

                        m_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                        {
                            Language = languageType.TargetLanguage,
                            CachingProvider = cachingProvider,
                            TemplateManager = new WatchingResolvePathTemplateManager(new[] { templatePath }, cachingProvider),
                            Debug = true
                        });
                    }
                    break;
                case RazorViewResolutionMode.EmbeddedResource:
                    m_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                    {
                        Language = languageType.TargetLanguage,
                        TemplateManager = new DelegateTemplateManager(name =>
                        {
                            string resourceName = name;

                            if (!resourceName.Contains("."))
                                resourceName = templatePath + name;

                            Stream stream = WebExtensions.OpenEmbeddedResourceStream(resourceName);

                            if ((object)stream == null)
                                return "";

                            using (StreamReader reader = new StreamReader(stream))
                                return reader.ReadToEnd();
                        }),
                        Debug = false
                    });
                    break;
            }

            TemplatePath = templatePath;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the template path defined for this <see cref="RazorEngine{TLanguage}"/>.
        /// </summary>
        public string TemplatePath
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="IRazorEngineService"/> instance used by the <see cref="RazorEngine{TLanguage}"/>.
        /// </summary>
        public IRazorEngineService EngineService => m_engineService;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="RazorEngine{TLanguage}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RazorEngine{TLanguage}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_engineService?.Dispose();
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets a given key from the <see cref="ITemplateManager" /> implementation. See <see cref="ITemplateManager.GetKey(String,ResolveType,ITemplateKey)" />.
        /// </summary>
        /// <returns></returns>
        public ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return m_engineService.GetKey(name, resolveType, context);
        }

        /// <summary>Checks if a given template is already cached.</summary>
        /// <returns></returns>
        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            return m_engineService.IsTemplateCached(key, modelType);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            m_engineService.AddTemplate(key, templateSource);
        }

        /// <summary>Compiles the specified template and caches it.</summary>
        public void Compile(ITemplateKey key, Type modelType = null)
        {
            m_engineService.Compile(key, modelType);
        }

        /// <summary>
        /// Runs the given cached template. When the cache does not contain the template it will be compiled and cached beforehand.
        /// </summary>
        public void RunCompile(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            m_engineService.RunCompile(key, writer, modelType, model, viewBag);
        }

        /// <summary>Runs the given cached template.</summary>
        public void Run(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            m_engineService.Run(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// Kicks off a task to pre-compile Razor templates.
        /// </summary>
        /// <param name="exceptionHandler">Exception handler used to report issues, if any.</param>
        public Task PreCompile(Action<Exception> exceptionHandler = null)
        {
            return PreCompile(null, exceptionHandler);
        }

        /// <summary>
        /// Kicks off a task to pre-compile Razor templates.
        /// </summary>
        /// <param name="modelType">The type of the model used for the application.</param>
        /// <param name="exceptionHandler">Exception handler used to report issues, if any.</param>
        public Task PreCompile(Type modelType, Action<Exception> exceptionHandler = null)
        {
            return Task.Run(() =>
            {
                TLanguage languageType = new TLanguage();

                if (languageType.ResolutionMode == RazorViewResolutionMode.EmbeddedResource)
                {
                    foreach (string fileName in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(fileName => fileName.StartsWith(TemplatePath)))
                    {
                        try
                        {
                            m_engineService.Compile(fileName, modelType);
                        }
                        catch (Exception ex)
                        {
                            if ((object)exceptionHandler != null)
                                exceptionHandler(new InvalidOperationException($"Failed to pre-compile razor template \"{fileName}\": {ex.Message}", ex));
                        }
                    }
                }
                else
                {
                    string webRootFolder = FilePath.AddPathSuffix(TemplatePath);
                    string[] razorFiles = FilePath.GetFileList($"{webRootFolder}*.{(languageType.TargetLanguage == Language.CSharp ? "cs" : "vb")}html");

                    foreach (string fileName in razorFiles)
                    {
                        try
                        {
                            m_engineService.Compile(FilePath.GetFileName(fileName), modelType);
                        }
                        catch (Exception ex)
                        {
                            if ((object)exceptionHandler != null)
                                exceptionHandler(new InvalidOperationException($"Failed to pre-compile razor template \"{fileName}\": {ex.Message}", ex));
                        }
                    }
                }
            });
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static RazorEngine<TLanguage> s_defaultEngine;
        private static readonly Dictionary<string, RazorEngine<TLanguage>> s_configuredEngines;

        // Static Properties

        /// <summary>
        /// Gets default configured razor engine instance.
        /// </summary>
        public static RazorEngine<TLanguage> Default => s_defaultEngine ?? (s_defaultEngine = GetConfiguredEngine());

        // Static Constructor
        static RazorEngine()
        {
            s_configuredEngines = new Dictionary<string, RazorEngine<TLanguage>>(StringComparer.OrdinalIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Gets a shared <see cref="RazorEngine{TLanguage}"/> instance created based on configured template path defined in the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Settings category to use for template path settings; defaults to "systemSettings".</param>
        /// <returns>Shared <see cref="RazorEngine{TLanguage}"/> instance created based on configured template path.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public static RazorEngine<TLanguage> GetConfiguredEngine(string settingsCategory = null)
        {
            lock (typeof(LanguageConstraint))
            {
                if (string.IsNullOrWhiteSpace(settingsCategory))
                    settingsCategory = "systemSettings";

                return s_configuredEngines.GetOrAdd(settingsCategory, category =>
                {
                    string templatePath = null;

                    // Get configured template path
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[category];

                    TLanguage languageType = new TLanguage();

                    switch (languageType.ResolutionMode)
                    {
                        case RazorViewResolutionMode.ResolvePath:
                        case RazorViewResolutionMode.WatchingResolvePath:
                            if (Common.GetApplicationType() == ApplicationType.Web)
                            {
                                settings.Add("TemplatePath", "~/Views/Shared", "Path for data context based razor field templates.");
                                templatePath = HostingEnvironment.MapPath(settings["TemplatePath"].Value).EnsureEnd('/');
                            }
                            else
                            {
                                settings.Add("TemplatePath", "Eval(systemSettings.WebRootPath)", "Path for data context based razor field templates.");
                                templatePath = FilePath.GetAbsolutePath(settings["TemplatePath"].Value);
                            }
                            break;
                        case RazorViewResolutionMode.EmbeddedResource:
                            settings.Add("EmbeddedTemplatePath", "GSF.Web.Model.Views.", "Embedded name space path for data context based razor field templates.");
                            templatePath = settings["EmbeddedTemplatePath"].Value.EnsureEnd('.');
                            break;
                    }

                    return new RazorEngine<TLanguage>(templatePath);
                });
            }
        }

        #endregion
    }
}
