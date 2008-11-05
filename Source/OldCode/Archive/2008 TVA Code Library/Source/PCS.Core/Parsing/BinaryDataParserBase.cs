//*******************************************************************************************************
//  BinaryDataParserBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/28/2007 - Pinal C. Patel
//      Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//      Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//      instead of DesignMode property as the former is more accurate than the latter
//  09/11/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using PCS.Collections;
using PCS.Configuration;
using PCS.IO;

namespace PCS.Parsing
{
    /// <summary>
    /// An abstract class that can be used for parsing data from multiple sources appropriate output types.
    /// </summary>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier for the output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the output.</typeparam>
    public abstract class BinaryDataParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : Component, ISupportLifecycle, ISupportInitialize , IPersistSettings where TOutputType : IBinaryDataConsumer, IIdentifiableType<TTypeIdentifier>
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Container for Type information.
        /// </summary>
        private class TypeInfo
        {
            public Type RuntimeType;
            public TTypeIdentifier TypeID;
            public DefaultConstructor CreateNew;
        }

        /// <summary>
        /// Container for data to be parsed.
        /// </summary>
        private class DataContainer
        {
            public DataContainer(TSourceIdentifier source, byte[] data)
            {
                this.Source = source;
                this.Data = data;
            }

            public byte[] Data;
            public TSourceIdentifier Source;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="OptimizeParsing"/> property.
        /// </summary>
        public const bool DefaultOptimizeParsing = true;

        /// <summary>
        /// Specifies the default value for the <see cref="DataAssemblyAttempts"/> property.
        /// </summary>
        public const int DefaultDataAssemblyAttempts = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "BinaryDataParser";

        // Delegates
        private delegate TOutputType DefaultConstructor();

        // Events


        /// <summary>
        /// Occurs when data image cannot be deserialized to the <see cref="Type"/> that the data image was for 
        /// either because the data image was partial or corrupt.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1}.Argument"/> is the ID of the <see cref="Type"/> that the data image was for.
        /// </remarks>
        [Description("Occurs when data image cannot be deserialized to the Type that the data image was for either because the data image was partial or corrupt.")]
        public event EventHandler<EventArgs<TTypeIdentifier>> DataDiscarded;

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more object of the <see cref="Type"/> 
        /// that the data image was for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument1"/> is the ID of the source for the data image.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </para>
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public event EventHandler<EventArgs<TSourceIdentifier, List<TOutputType>>> DataParsed;

        /// <summary>
        /// Occurs when matching <see cref="Type"/> for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1}.Argument"/> is the ID of the <see cref="Type"/> that the data image was for.
        /// </remarks>
        [Description("Occurs when matching Type for deserializing the data image cound not be found.")]
        public event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        // Fields
        private bool m_optimizeParsing;
        private int m_dataAssemblyAttempts;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private ProcessQueue<DataContainer> m_unparsedDataQueue;
        private Dictionary<TTypeIdentifier, TypeInfo> m_outputTypes;
        private Dictionary<TSourceIdentifier, int> m_assemblyAttemptTracker;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataParserBase{TSourceIdentifier, TTypeIdentifier, TOutputType}"/> class.
        /// </summary>
        public BinaryDataParserBase()
        {
            m_optimizeParsing = DefaultOptimizeParsing;
            m_dataAssemblyAttempts = DefaultDataAssemblyAttempts;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_outputTypes = new Dictionary<TTypeIdentifier, TypeInfo>();
            m_assemblyAttemptTracker = new Dictionary<TSourceIdentifier, int>();
            m_unparsedDataQueue = ProcessQueue<DataContainer>.CreateRealTimeQueue(ParseData);
        }
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates if data is to be parsed in an optimal mode.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultOptimizeParsing),
        Description("Indicates whether the data is to be parsed in an optimal mode.")]
        public bool OptimizeParsing
        {
            get
            {
                return m_optimizeParsing;
            }
            set
            {
                m_optimizeParsing = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of attempts to be made for assembling partial data images before parsing it.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultDataAssemblyAttempts),
        Description("Number of attempts to be made for assembling partial data images before parsing it.")]
        public int DataAssemblyAttempts
        {
            get
            {
                return m_dataAssemblyAttempts;
            }
            set
            {
                m_dataAssemblyAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of data parser object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of data parser object are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of data parser object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of data parser object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser object is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_unparsedDataQueue.Enabled;
            }
            set
            {
                m_unparsedDataQueue.Enabled = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Starts the data parser.
        /// </summary>
        public void Start()
        {
            Initialize();   // Initialize if uninitialized.

            ConstructorInfo typeCtor = null;
            string dllDirectory = FilePath.GetAbsolutePath("");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("InMemory"), AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("Helper");
            TypeBuilder typeBuilder = modBuilder.DefineType("ClassFactory");
            List<TypeInfo> outputTypes = new List<TypeInfo>(); // Temporarily hold output types until their IDs are determined.

            foreach (Type asmType in typeof(TOutputType).LoadImplementations())
            {
                typeCtor = asmType.GetConstructor(Type.EmptyTypes);
                if (typeCtor != null)
                {
                    // The type meets the following criteria:
                    // - has a default public constructor
                    // - is not abstract and can be instantiated.
                    // - root type is same as the type specified for the output

                    TypeInfo outputType = new TypeInfo();
                    outputType.RuntimeType = asmType;

                    // We employ 2 of the best peforming ways of instantiating objects using reflection.
                    // See: http://blogs.msdn.com/haibo_luo/archive/2005/11/17/494009.aspx
                    if (m_optimizeParsing)
                    {
                        // Invokation approach: Reflection.Emit + Delegate
                        // This is hands-down that most fastest way of instantiating objects using reflection.
                        MethodBuilder dynamicTypeCtor = typeBuilder.DefineMethod(asmType.Name, MethodAttributes.Public | MethodAttributes.Static, asmType, Type.EmptyTypes);
                        ILGenerator ilGen = dynamicTypeCtor.GetILGenerator();
                        ilGen.Emit(OpCodes.Nop);
                        ilGen.Emit(OpCodes.Newobj, typeCtor);
                        ilGen.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        // Invokation approach: DynamicMethod + Delegate
                        // This method is very fast compared to rest of the approaches, but not as fast as the one above.
                        DynamicMethod dynamicTypeCtor = new DynamicMethod("DefaultConstructor", asmType, Type.EmptyTypes, asmType.Module, true);
                        ILGenerator ilGen = dynamicTypeCtor.GetILGenerator();
                        ilGen.Emit(OpCodes.Nop);
                        ilGen.Emit(OpCodes.Newobj, typeCtor);
                        ilGen.Emit(OpCodes.Ret);

                        // Create a delegate to the constructor that'll be called to create a new instance of the type.
                        outputType.CreateNew = (DefaultConstructor)(dynamicTypeCtor.CreateDelegate(typeof(DefaultConstructor)));
                    }

                    // We'll hold all of the matching types in this list temporarily until their IDs are determined.
                    outputTypes.Add(outputType);
                }
            }

            if (m_optimizeParsing)
            {
                // The reason we have to do this over here is because we can create a type only once. This is the type
                // that has all the constructors created above for the various class matching the requirements for the
                // output type.
                Type bakedType = typeBuilder.CreateType(); // This can be done only once!!!
                foreach (TypeInfo outputType in outputTypes)
                {
                    outputType.CreateNew = (DefaultConstructor)(System.Delegate.CreateDelegate(typeof(DefaultConstructor), bakedType.GetMethod(outputType.RuntimeType.Name)));
                }
            }

            foreach (TypeInfo outputType in outputTypes)
            {
                // Now, we'll go though all of the output types we've found and instantiate an instance of each in order
                // to get the identifier for each of the type. This will help lookup of the type to be used when parsing
                // the data.
                TOutputType instance = outputType.CreateNew();
                outputType.TypeID = instance.TypeID;
                if (!m_outputTypes.ContainsKey(outputType.TypeID))
                {
                    m_outputTypes.Add(outputType.TypeID, outputType);
                }
            }

            m_unparsedDataQueue.Start();
        }

        /// <summary>
        /// Stops the data parser.
        /// </summary>
        public void Stop()
        {
            m_unparsedDataQueue.Stop();     // Stop processing of queued data.
            m_outputTypes.Clear();  // Clear the cached packet type available.
        }

        /// <summary>
        /// Queues data for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="data">Data to be parsed.</param>
        public void Parse(TSourceIdentifier source, byte[] data)
        { 
            m_unparsedDataQueue.Add(new DataContainer(source,data));
        }

        /// <summary>
        /// Initializes the data parser object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the data parser 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves settings for the data parser object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["OptimizeParsing", true].Update(m_optimizeParsing, "True if data parsing is to be done in an optimal mode; otherwise False.");
                settings["DataAssemblyAttempts", true].Update(m_dataAssemblyAttempts, "Number of attempts to be made for assembling partial data images before parsing it.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the data parser object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                OptimizeParsing = settings["OptimizeParsing", true].ValueAs(m_optimizeParsing);
                DataAssemblyAttempts = settings["DataAssemblyAttempts", true].ValueAs(m_dataAssemblyAttempts);
            }
        }

        /// <summary>
        /// Performs necessary operations before the data parser object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the data parser object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the data parser object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the data parser object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                
            }
        }

        /// <summary>
        /// Raises the <see cref="DataDiscarded"/>
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataDiscarded(EventArgs<TTypeIdentifier> e)
        {
            if (DataDiscarded != null)
                DataDiscarded(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataParsed(EventArgs<TSourceIdentifier, List<TOutputType>> e)
        {
            if (DataParsed != null)
                DataParsed(this, e);
        }


        /// <summary>
        /// Raises the <see cref="OutputTypeNotFound"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnOutputTypeNotFound(EventArgs<TTypeIdentifier> e)
        {
            if (OutputTypeNotFound != null)
                OutputTypeNotFound(this, e);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the data parser object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    SaveSettings(); 
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_unparsedDataQueue != null)
                            m_unparsedDataQueue.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets ID of the <see cref="Type"/> the data image is for.
        /// </summary>
        /// <param name="binaryImage">The data image whose target type is to be determined.</param>
        /// <param name="startIndex">The index in the data image from where the data is valid.</param>
        /// <returns>ID of the <see cref="Type"/> the data image is for.</returns>
        protected abstract TTypeIdentifier ExtractTypeID(byte[] binaryImage, int startIndex);

        private void ParseData(DataContainer[] item)
        {
            int cursor;
            TOutputType instance;
            TTypeIdentifier typeID;
            TypeInfo outputType;

            for (int i = 0; i <= item.Length - 1; i++)
            {
                // We have defined the process queue that holds the data to be parsed of type "Many-at-Once", so we'll
                // most likely get multiple data images that we must process and we'll do just that...!
                if (item[i].Data != null && item[i].Data.Length > 0)
                {
                    // This data image is valid (i.e. has data in it), so we'll go on to process it. By "processing the
                    // data image" we mean that we'll take the data in the image and use it to initialize an appropriate
                    // instance of a type that will represent the data, hence making the binary data more meaningful and
                    // useful.
                    List<TOutputType> output = new List<TOutputType>();

                    cursor = 0;
                    while (cursor < item[i].Data.Length)
                    {
                        typeID = ExtractTypeID(item[i].Data, cursor); // <- Necessary overhead :(

                        if (m_outputTypes.TryGetValue(typeID, out outputType))
                        {
                            // We have type that can be instantiated and initialized with the data from this image.
                            try
                            {
                                instance = outputType.CreateNew();
                                cursor += instance.Initialize(item[i].Data, cursor);    // Returns the number of bytes used.
                                output.Add(instance);
                                m_assemblyAttemptTracker[item[i].Source] = 0;       // <- Necessary overhead :(
                            }
                            catch
                            {
                                // We might encounter an exception when a given type is trying to initialize the
                                // instance from the provided data, and that data is either partial or malformed.
                                // So if the image is partial and we combine it with future data from the same
                                // source, we will be able to succesfully use all of the data given the option to
                                // reuse data is turned on. However, if the image data is malformed, we will not be
                                // able to use it even if we combine it with future data from the same source, and
                                // therefore we must give up reusing this "unparsed" data after so many attempts
                                // to re-use it.
                                // In order to achieve this, we use a dictionary to keep track of how many times
                                // unparsed data from a given source has been reused. If the data has not been reused
                                // up to the specified limit for reusing the data, we'll reuse the data, or else we'll
                                // discard it.
                                int reuseCount;

                                m_assemblyAttemptTracker.TryGetValue(item[i].Source, out reuseCount);

                                if (reuseCount < m_dataAssemblyAttempts)
                                {
                                    // We can try to reuse the unparsed data make use of it.
                                    bool insertUnusedData = true;

                                    // First, we extract the unparsed data from the data image.
                                    byte[] unusedData = new byte[item[i].Data.Length - cursor];
                                    Array.Copy(item[i].Data, cursor, unusedData, 0, unusedData.Length);

                                    if (i < (item.Length - 1))
                                    {
                                        // This isn't the last data image in the batch, so we'll look for the first data
                                        // image that is from the same source as this data image.
                                        for (int k = i + 1; k <= item.Length - 1; k++)
                                        {
                                            if (item[k].Source.Equals(item[i].Source))
                                            {
                                                // We found a data image from the same source so we'll merge the unparsed
                                                // data with the data in that data image and hopefully now we'll be able
                                                // to parse the combined data.
                                                byte[] mergedImage = new byte[item[k].Data.Length + unusedData.Length];
                                                Array.Copy(unusedData, 0, mergedImage, 0, unusedData.Length);
                                                Array.Copy(item[k].Data, 0, mergedImage, unusedData.Length, item[k].Data.Length);

                                                item[k].Data = mergedImage;

                                                insertUnusedData = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (insertUnusedData)
                                    {
                                        // We could not find a data image from the same source as the current data image
                                        // so we'll just insert this data in the queue, so by the time this data is
                                        // processed in the next batch, we might have data from the same source that we
                                        // might be able to combine and use.
                                        m_unparsedDataQueue.Insert(0, new DataContainer(item[i].Source, unusedData));
                                    }

                                    reuseCount++;
                                    m_assemblyAttemptTracker[item[i].Source] = reuseCount;
                                    cursor = item[i].Data.Length; // Move on to the next data image.
                                }
                                else
                                {
                                    cursor = item[i].Data.Length; // Move on to the next data image.

                                    m_assemblyAttemptTracker[item[i].Source] = 0;
                                    OnDataDiscarded(new EventArgs<TTypeIdentifier>(typeID));
                                }
                            }
                        }
                        else
                        {
                            // If we come accross data in the image we cannot convert to a type than, we are going
                            // to have to discard the remainder of the image because we will now know where the
                            // the next valid block of data is within the image.

                            cursor = item[i].Data.Length; // Move on to the next data image.
                            OnOutputTypeNotFound(new EventArgs<TTypeIdentifier>(typeID));
                        }
                    }

                    OnDataParsed(new EventArgs<TSourceIdentifier, List<TOutputType>>(item[i].Source, output));
                }
            }
        }

        #endregion
   }
}