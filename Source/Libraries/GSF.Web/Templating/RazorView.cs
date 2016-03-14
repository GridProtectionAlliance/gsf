//******************************************************************************************************
//  RazorView.cs - Gbtc
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
//  01/13/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using GSF.Web.Model;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

// ReSharper disable StaticMemberInGenericType
namespace GSF.Web.Templating
{
    /// <summary>
    /// Defines resolutions modes for Razor view template files.
    /// </summary>
    public enum RazorViewResolutionMode
    {
        /// <summary>
        /// Resolves template files from a path.
        /// </summary>
        ResolvePath,
        /// <summary>
        /// Resolves template files from a path and watches for changes.
        /// </summary>
        /// <remarks>
        /// This should only be used in debug mode.
        /// </remarks>
        WatchingResolvePath,
        /// <summary>
        /// Resolves template files from an embedded resource.
        /// </summary>
        EmbeddedResource
    }

    /// <summary>
    /// Defines a language constraint for a <see cref="RazorView{TLanguage}"/>.
    /// </summary>
    public abstract class LanguageContraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageContraint"/> implementation.
        /// </summary>
        public abstract Language TargetLanguage
        {
            get;
        }

        /// <summary>
        /// Gets resolution mode of <see cref="RazorView{TLanguage}"/>.
        /// </summary>
        public virtual RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.ResolvePath;
    }

    /// <summary>
    /// Defines a C# based language constraint.
    /// </summary>
    public class CSharp : LanguageContraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageContraint"/> implementation - C#.
        /// </summary>
        public override Language TargetLanguage => Language.CSharp;
    }

    /// <summary>
    /// Defines a C# based language constraint in debug mode.
    /// </summary>
    public class CSharpDebug : CSharp
    {
        /// <summary>
        /// Gets resolution mode of <see cref="RazorView{TLanguage}"/> - watching resolve path for debugging operations.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.WatchingResolvePath;
    }

    /// <summary>
    /// Defines a C# based language constraint for embedded resources.
    /// </summary>
    public class CSharpEmbeddedResource : CSharp
    {
        /// <summary>
        /// Gets resolution mode of <see cref="RazorView{TLanguage}"/> - embedded resources.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.EmbeddedResource;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint.
    /// </summary>
    public class VisualBasic : LanguageContraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageContraint"/> implementation - Visual Basic.
        /// </summary>
        public override Language TargetLanguage => Language.VisualBasic;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint in debug mode.
    /// </summary>
    public class VisualBasicDebug : VisualBasic
    {
        /// <summary>
        /// Gets resolution mode of <see cref="RazorView{TLanguage}"/> - watching resolve path for debugging operations.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.WatchingResolvePath;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint for embedded resources.
    /// </summary>
    public class VisualBasicEmbeddedResource : CSharp
    {
        /// <summary>
        /// Gets resolution mode of <see cref="RazorView{TLanguage}"/> - embedded resources.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.EmbeddedResource;
    }

    /// <summary>
    /// Defines a view class for data context based Razor template implementations.
    /// </summary>
    /// <typeparam name="TLanguage"><see cref="LanguageContraint"/> type to use for Razor views.</typeparam>
    public class RazorView<TLanguage> where TLanguage : LanguageContraint, new()
    {
        #region [ Members ]

        // Fields
        private readonly DynamicViewBag m_viewBag = new DynamicViewBag();
        private Dictionary<string, string> m_parameters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RazorView{TLanguage}"/>.
        /// </summary>
        public RazorView()
        {
        }

        /// <summary>
        /// Creates a new <see cref="RazorView{TLanguage}"/> with the specified exception handler.
        /// </summary>
        /// <param name="templateName">Name of template file, typically a .cshtml or .vbhtml file.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions, if any.</param>
        public RazorView(string templateName, Action<Exception> exceptionHandler = null)
        {
            TemplateName = templateName;
            ExceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Creates a new <see cref="RazorView{TLanguage}"/> with the specified parameters.
        /// </summary>
        /// <param name="templateName">Name of template file, typically a .cshtml or .vbhtml file.</param>
        /// <param name="model">Reference to model to use when rendering template.</param>
        /// <param name="modelType">Type of <paramref name="model"/>.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions, if any.</param>
        public RazorView(string templateName, object model, Type modelType, Action<Exception> exceptionHandler = null) : this(templateName, exceptionHandler)
        {
            Model = model;
            ModelType = modelType;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets name of template file.
        /// </summary>
        public string TemplateName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets reference to model to use when rendering template.
        /// </summary>
        public object Model
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets type of <see cref="Model"/>.
        /// </summary>
        public Type ModelType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets delegate used to handle exceptions.
        /// </summary>
        public Action<Exception> ExceptionHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets reference to view bag used when rendering template.
        /// </summary>
        public dynamic ViewBag => m_viewBag;

        /// <summary>
        /// Gets query string parameter specified by <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Name of query string parameter to retrieve.</param>
        /// <returns>Query string parameter specified by <paramref name="key"/>.</returns>
        public string this[string key] => Parameters[key];

        /// <summary>
        /// Gets a dictionary of query string parameters passed to rendered view.
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get
            {
                if ((object)m_parameters == null)
                {
                    HttpRequestMessage request = ViewBag.Request;
                    m_parameters = HttpUtility.ParseQueryString(request.RequestUri.Query).ToDictionary();
                }

                return m_parameters;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compiles and executes view template.
        /// </summary>
        /// <returns>Rendered result.</returns>
        public string Execute()
        {
            using (DataContext<TLanguage> dataContext = new DataContext<TLanguage>(exceptionHandler: ExceptionHandler))
            {
                m_viewBag.AddValue("DataContext", dataContext);
                return s_engineService.RunCompile(TemplateName, ModelType, Model, m_viewBag);
            }
        }

        /// <summary>
        /// Compiles and executes view template for specified request message and post data.
        /// </summary>
        /// <returns>Rendered result.</returns>
        public string Execute(HttpRequestMessage requestMessage, dynamic postData)
        {
            using (DataContext<TLanguage> dataContext = new DataContext<TLanguage>(exceptionHandler: ExceptionHandler))
            {
                m_viewBag.AddValue("DataContext", dataContext);
                m_viewBag.AddValue("Request", requestMessage);

                if ((object)postData == null)
                {
                    m_viewBag.AddValue("IsPost", false);
                }
                else
                {
                    m_viewBag.AddValue("IsPost", true);
                    m_viewBag.AddValue("PostData", postData);
                }

                return s_engineService.RunCompile(TemplateName, ModelType, Model, m_viewBag);
            }
        }

        /// <summary>
        /// Asynchronously compiles and executes view template for specified request message and post data.
        /// </summary>
        /// <returns>Task that will provide rendered result.</returns>
        public Task ExecuteAsync(HttpRequestMessage requestMessage, dynamic postData)
        {
            return Task.Run(() => Execute(requestMessage, postData));
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly IRazorEngineService s_engineService;

        /// <summary>
        /// Defines default settings path for template files.
        /// </summary>
        public static readonly string TemplatePath;

        // Static Constructor
        static RazorView()
        {
            TLanguage languageType = new TLanguage();

            // Get configured template path
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];

            switch (languageType.ResolutionMode)
            {
                case RazorViewResolutionMode.ResolvePath:
                case RazorViewResolutionMode.WatchingResolvePath:
                    if (Common.GetApplicationType() == ApplicationType.Web)
                    {
                        systemSettings.Add("TemplatePath", "~/Views/Shared/Templates/", "Path for data context based razor field templates.");
                        TemplatePath = HostingEnvironment.MapPath(systemSettings["TemplatePath"].Value).EnsureEnd('/');
                    }
                    else
                    {
                        systemSettings.Add("TemplatePath", "wwwroot/Templates/", "Path for data context based razor field templates.");
                        TemplatePath = FilePath.GetAbsolutePath(systemSettings["TemplatePath"].Value);
                    }

                    if (languageType.ResolutionMode == RazorViewResolutionMode.ResolvePath)
                    {
                        s_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                        {
                            Language = languageType.TargetLanguage,
                            TemplateManager = new ResolvePathTemplateManager(new[] { TemplatePath }),
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

                        s_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                        {
                            Language = languageType.TargetLanguage,
                            CachingProvider = cachingProvider,
                            TemplateManager = new WatchingResolvePathTemplateManager(new[] { TemplatePath }, cachingProvider),
                            Debug = true
                        });
                    }
                    break;
                case RazorViewResolutionMode.EmbeddedResource:
                    systemSettings.Add("EmbeddedTemplatePath", "GSF.Web.Templating.Views.", "Embedded name space path for data context based razor field templates.");
                    TemplatePath = systemSettings["EmbeddedTemplatePath"].Value.EnsureEnd('.');

                    s_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
                    {
                        Language = languageType.TargetLanguage,
                        TemplateManager = new DelegateTemplateManager(name =>
                        {
                            string resourceName = name;

                            if (!resourceName.Contains("."))
                                resourceName = TemplatePath + name;

                            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

                            if ((object)stream == null)
                                return "";

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                return reader.ReadToEnd();
                            }
                        }),
                        Debug = false
                    });

                    

                    break;
                default:
                    throw new ArgumentOutOfRangeException("");
            }
        }

        /// <summary>
        /// Kicks off a task to pre-compile Razor templates.
        /// </summary>
        /// <param name="exceptionHandler">Exception handler used to report issues, if any.</param>
        public static Task PreCompile(Action<Exception> exceptionHandler = null)
        {
            return Task.Run(() =>
            {
                TLanguage languageType = new TLanguage();

                if (languageType.ResolutionMode == RazorViewResolutionMode.EmbeddedResource)
                {
                    foreach (string fileName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                    {
                        if (fileName.StartsWith(TemplatePath))
                        {
                            try
                            {
                                s_engineService.Compile(fileName.Substring(TemplatePath.Length));
                            }
                            catch (Exception ex)
                            {
                                if ((object)exceptionHandler != null)
                                    exceptionHandler(new InvalidOperationException($"Failed to pre-compile razor template \"{fileName}\": {ex.Message}", ex));
                            }
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
                            s_engineService.Compile(FilePath.GetFileName(fileName));
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
    }
}
