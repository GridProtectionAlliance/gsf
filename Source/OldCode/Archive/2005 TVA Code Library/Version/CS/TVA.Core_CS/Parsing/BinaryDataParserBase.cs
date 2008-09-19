//*******************************************************************************************************
//  TVA.Parsing.BinaryDataParserBase.vb - Base class for parsing binary data
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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Collections.Generic;
using TVA;
using TVA.IO;
using TVA.Collections;
using TVA.Configuration;

namespace TVA.Parsing
{
    [DefaultEvent("DataParsed")]
    public abstract partial class BinaryDataParserBase<TIdentifier, TOutput> : IPersistSettings, ISupportInitialize where TOutput : IBinaryDataConsumer
    {
        #region " Members "

        private class TypeInfo
        {
            public TIdentifier ID;
            public DefaultConstructor CreateNew;
            public Type RuntimeType;
        }

        private string m_idPropertyName;
        private bool m_optimizeParsing;
        private int m_unparsedDataReuseLimit;
        private bool m_persistSettings;
        private string m_settingsCategoryName;
        private Dictionary<TIdentifier, TypeInfo> m_outputTypes;
        private Dictionary<Guid, int> m_unparsedDataReuseCount;
        private ProcessQueue<IdentifiableItem<Guid, byte[]>> m_dataQueue;
        private delegate TOutput DefaultConstructor();

        #endregion

        #region " Events "

        /// <summary>
        /// Occurs when a data image has been parsed.
        /// </summary>
        public event EventHandler<GenericEventArgs<IdentifiableItem<Guid, List<TOutput>>>> DataParsed;

        /// <summary>
        /// Occurs when a matching output type is not found for parsing the data image.
        /// </summary>
        public event EventHandler<GenericEventArgs<TIdentifier>> OutputTypeNotFound;

        /// <summary>
        /// Occurs when unparsed data is reused and not discarded.
        /// </summary>
        public event EventHandler<GenericEventArgs<TIdentifier>> UnparsedDataReused;

        /// <summary>
        /// Occurs when unparsed data is discarded and not re-used.
        /// </summary>
        public event EventHandler<GenericEventArgs<TIdentifier>> UnparsedDataDiscarded;

        #endregion

        #region " Methods "

        /// <summary>
        /// Gets or sets the name of the property that identifies the output type.
        /// </summary>
        /// <value></value>
        /// <returns>Name of the property that identifies the output type.</returns>
        public string IDPropertyName
        {
            get
            {
                return m_idPropertyName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_idPropertyName = value;
                }
                else
                {
                    throw new ArgumentNullException("IDPropertyName");
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if parsing is to be done in an optimal mode.
        /// </summary>
        /// <value></value>
        /// <returns>True if parsing is to be done in an optimal mode; otherwise False.</returns>
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
        /// Gets or sets the number of attempts to be made to parse the unparsed data by reusing it.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public int UnparsedDataReuseLimit
        {
            get
            {
                return m_unparsedDataReuseLimit;
            }
            set
            {
                m_unparsedDataReuseLimit = value;
            }
        }

        /// <summary>
        /// Gets the queue to which data to be parsed is to be added.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The TVA.Collections.ProcessQueue(Of TVA.IdentifiableItem(Of Guid, Byte())) to which data to be parsed is
        /// to be added.
        /// </returns>
        [Browsable(false)]
        public ProcessQueue<IdentifiableItem<Guid, byte[]>> DataQueue
        {
            get
            {
                return m_dataQueue;
            }
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        public void Start()
        {
            System.Reflection.Assembly asm = null;
            ConstructorInfo typeCtor = null;
            string dllDirectory = FilePath.AbsolutePath("");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("InMemory"), AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("Helper");
            TypeBuilder typeBuilder = modBuilder.DefineType("ClassFactory");
            List<TypeInfo> outputTypes = new List<TypeInfo>(); // Temporarily hold output types until their IDs are determined.

            if (Common.GetApplicationType() == ApplicationType.Web)
            {
                // In case of a web application, we need to look in the bin directory for DLLs.
                dllDirectory = FilePath.AddPathSuffix(dllDirectory + "bin");
            }

            // Process all assemblies in the application bin directory.
            foreach (string dll in Directory.GetFiles(dllDirectory, "*.dll"))
            {
                try
                {
                    // Load the assembly in the curent app domain.
                    asm = System.Reflection.Assembly.LoadFrom(dll);

                    // Process all of the public types in the assembly.
                    foreach (Type asmType in asm.GetExportedTypes())
                    {
                        typeCtor = asmType.GetConstructor(Type.EmptyTypes);
                        if ((typeCtor != null) && !asmType.IsAbstract && asmType.GetRootType() == typeof(TOutput))
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
                                System.Reflection.Emit.ILGenerator ilGen = dynamicTypeCtor.GetILGenerator();
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Nop);
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Newobj, typeCtor);
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Ret);
                            }
                            else
                            {
                                // Invokation approach: DynamicMethod + Delegate
                                // This method is very fast compared to rest of the approaches, but not as fast as the one above.
                                DynamicMethod dynamicTypeCtor = new DynamicMethod("DefaultConstructor", asmType, Type.EmptyTypes, asmType.Module, true);
                                System.Reflection.Emit.ILGenerator ilGen = dynamicTypeCtor.GetILGenerator();
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Nop);
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Newobj, typeCtor);
                                ilGen.Emit(System.Reflection.Emit.OpCodes.Ret);

                                // Create a delegate to the constructor that'll be called to create a new instance of the type.
                                outputType.CreateNew = (DefaultConstructor)(dynamicTypeCtor.CreateDelegate(typeof(DefaultConstructor)));
                            }

                            // We'll hold all of the matching types in this list temporarily until their IDs are determined.
                            outputTypes.Add(outputType);
                        }
                    }
                }
                catch
                {
                    // Absorb any exception we might encounter while loading an assembly or processing it.
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

            PropertyInfo idProperty;
            foreach (TypeInfo outputType in outputTypes)
            {
                // Now, we'll go though all of the output types we've found and instantiate an instance of each in order
                // to get the identifier for each of the type. This will help lookup of the type to be used when parsing
                // the data.
                TOutput instance = outputType.CreateNew();
                idProperty = outputType.RuntimeType.GetProperty(m_idPropertyName, typeof(TIdentifier));
                if (idProperty != null)
                {
                    // The output type does expose a property with the specified name and return type.
                    outputType.ID = (TIdentifier)(idProperty.GetValue(instance, null));
                    if (!m_outputTypes.ContainsKey(outputType.ID))
                    {
                        m_outputTypes.Add(outputType.ID, outputType);
                    }
                }
            }

            m_dataQueue.Start();
        }

        /// <summary>
        /// Stops the parser.
        /// </summary>
        public void Stop()
        {
            m_dataQueue.Stop();     // Stop processing of queued data.
            m_outputTypes.Clear();  // Clear the cached packet type available.
        }

        public abstract TIdentifier GetTypeID(byte[] binaryImage, int startIndex);

        #region " IPersistSettings "

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

        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategoryName = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
            }
        }

        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                if (settings.Count > 0)
                {
                    IDPropertyName = settings["IDPropertyName"].GetTypedValue(m_idPropertyName);
                    OptimizeParsing = settings["OptimizeParsing"].GetTypedValue(m_optimizeParsing);
                    UnparsedDataReuseLimit = settings["UnparsedDataReuseLimit"].GetTypedValue(m_unparsedDataReuseLimit);
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                    CategorizedSettingsElement element;
                    settings.Clear();

                    element = settings["IDPropertyName", true];
                    element.Value = m_idPropertyName;
                    element.Description = "Name of the property that identifies the output type.";

                    element = settings["OptimizeParsing", true];
                    element.Value = m_optimizeParsing.ToString();
                    element.Description = "True if parsing is to be done in an optimal mode; otherwise False.";

                    element = settings["UnparsedDataReuseLimit", true];
                    element.Value = m_unparsedDataReuseLimit.ToString();
                    element.Description = "Number of times unparsed data can be reused before being discarded.";

                    TVA.Configuration.Common.SaveSettings();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        #endregion

        #region " ISupportInitialize "

        public void BeginInit()
        {
            // We don't need to do anything before the component is initialized.				
        }

        public void EndInit()
        {
            // Load settings from the config file.
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime) LoadSettings();
        }

        #endregion

        private void ParseData(IdentifiableItem<Guid, byte[]>[] item)
        {
            int cursor;
            TOutput instance;
            TIdentifier typeID;
            TypeInfo outputType;

            for (int i = 0; i <= item.Length - 1; i++)
            {
                // We have defined the process queue that holds the data to be parsed of type "Many-at-Once", so we'll
                // most likely get multiple data images that we must process and we'll do just that...!
                if (item[i].Item != null && item[i].Item.Length > 0)
                {
                    // This data image is valid (i.e. has data in it), so we'll go on to process it. By "processing the
                    // data image" we mean that we'll take the data in the image and use it to initialize an appropriate
                    // instance of a type that will represent the data, hence making the binary data more meaningful and
                    // useful.
                    List<TOutput> output = new List<TOutput>();

                    cursor = 0;
                    while (cursor < item[i].Item.Length)
                    {
                        typeID = GetTypeID(item[i].Item, cursor); // <- Necessary overhead :(

                        if (m_outputTypes.TryGetValue(typeID, out outputType))
                        {
                            // We have type that can be instantiated and initialized with the data from this image.
                            try
                            {
                                instance = outputType.CreateNew();
                                cursor += instance.Initialize(item[i].Item, cursor); // Returns the number of bytes used.
                                output.Add(instance);
                                m_unparsedDataReuseCount[item[i].Source] = 0; // <- Necessary overhead :(
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

                                m_unparsedDataReuseCount.TryGetValue(item[i].Source, out reuseCount);

                                if (reuseCount < m_unparsedDataReuseLimit)
                                {
                                    // We can try to reuse the unparsed data make use of it.
                                    bool insertUnusedData = true;

                                    // First, we extract the unparsed data from the data image.
                                    byte[] unusedData = new byte[item[i].Item.Length - cursor];
                                    Array.Copy(item[i].Item, cursor, unusedData, 0, unusedData.Length);

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
                                                byte[] mergedImage = new byte[item[k].Item.Length + unusedData.Length];
                                                Array.Copy(unusedData, 0, mergedImage, 0, unusedData.Length);
                                                Array.Copy(item[k].Item, 0, mergedImage, unusedData.Length, item[k].Item.Length);

                                                item[k].Item = mergedImage;

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
                                        m_dataQueue.Insert(0, new IdentifiableItem<Guid, byte[]>(item[i].Source, unusedData));
                                    }

                                    reuseCount++;
                                    m_unparsedDataReuseCount[item[i].Source] = reuseCount;
                                    cursor = item[i].Item.Length; // Move on to the next data image.
                                    if (UnparsedDataReused != null)
                                        UnparsedDataReused(this, new GenericEventArgs<TIdentifier>(typeID));
                                }
                                else
                                {
                                    cursor = item[i].Item.Length; // Move on to the next data image.

                                    m_unparsedDataReuseCount[item[i].Source] = 0;
                                    if (UnparsedDataDiscarded != null)
                                        UnparsedDataDiscarded(this, new GenericEventArgs<TIdentifier>(typeID));
                                }
                            }
                        }
                        else
                        {
                            // If we come accross data in the image we cannot convert to a type than, we are going
                            // to have to discard the remainder of the image because we will now know where the
                            // the next valid block of data is within the image.

                            cursor = item[i].Item.Length; // Move on to the next data image.
                            if (OutputTypeNotFound != null)
                                OutputTypeNotFound(this, new GenericEventArgs<TIdentifier>(typeID));
                        }
                    }

                    if (DataParsed != null)
                        DataParsed(this, new GenericEventArgs<IdentifiableItem<Guid, List<TOutput>>>(new IdentifiableItem<Guid, List<TOutput>>(item[i].Source, output)));
                }
            }

        }

        #endregion
    }
}