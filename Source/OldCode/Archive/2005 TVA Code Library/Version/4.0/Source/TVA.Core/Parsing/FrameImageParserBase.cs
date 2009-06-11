//*******************************************************************************************************
//  FrameImageParserBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/28/2007 - Pinal C. Patel
//       Original version of source code generated
//  11/20/2008 - James R Carroll
//       Adapted for more generalized use via the following related base classes:
//          BinaryImageParserBase => FrameImageParserBase => MultiSourceFrameImageParserBase.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TVA.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of
    /// a binary data stream represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </para>
    /// <para>
    /// This class is more specific than the <see cref="BinaryImageParserBase"/> in that it can automate the parsing of
    /// a particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="ISupportFrameImage{TTypeIdentifier}"/> interface. The common method of identification is handled by
    /// creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily includes a
    /// TypeID property, but also should include any state information needed to parse a particular frame if
    /// necessary. Derived classes simply override the <see cref="ParseCommonHeader"/> function in order to parse
    /// the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    [Description("Defines the basic functionality for parsing a binary data stream represented as frames with common headers and returning the parsed data via an event."),
    DefaultEvent("DataParsed")]
    public abstract class FrameImageParserBase<TTypeIdentifier, TOutputType> : BinaryImageParserBase, IFrameImageParser<TTypeIdentifier, TOutputType> where TOutputType : ISupportFrameImage<TTypeIdentifier>
    {
        #region [ Members ]

        // Nested Types

        // Container for Type information.
        private class TypeInfo
        {
            public Type RuntimeType;
            public TTypeIdentifier TypeID;
            public DefaultConstructor CreateNew;
        }

        // Delegates
        private delegate TOutputType DefaultConstructor();

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one of the output types that the data
        /// image represents.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the object that was deserialized from the binary image.
        /// </remarks>
        public event EventHandler<EventArgs<TOutputType>> DataParsed;

        /// <summary>
        /// Occurs when matching an output type for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the output type that could not be found.
        /// </remarks>
        public event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        /// <summary>
        /// Occurs when more than one type has been defined that can deserialize the specified output type.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="Type"/> that defines a type ID that has already been defined.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the ID of the output type that was defined more than once.
        /// </remarks>
        public event EventHandler<EventArgs<Type, TTypeIdentifier>> DuplicateTypeHandlerEncountered;

        // Fields
        private Dictionary<TTypeIdentifier, TypeInfo> m_outputTypes;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        protected FrameImageParserBase()
        {
            m_outputTypes = new Dictionary<TTypeIdentifier, TypeInfo>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets current status of <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        [Browsable(false)]
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.Append("Total defined output types: ");
                status.Append(m_outputTypes.Count);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_outputTypes != null)
                        {
                            foreach (KeyValuePair<TTypeIdentifier, TypeInfo> item in m_outputTypes)
                            {
                                item.Value.CreateNew = null;
                            }

                            m_outputTypes.Clear();
                        }
                        m_outputTypes = null;
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
        /// Start the data parser.
        /// </summary>
        /// <remarks>
        /// This overload loads public types from assemblies in the application binaries directory that implement the parser's output type.
        /// </remarks>
        public override void Start()
        {
            Start(typeof(TOutputType).LoadImplementations());
        }

        /// <summary>
        /// Starts the data parser given the specified type implementations.
        /// </summary>
        /// <param name="implementations">Output type implementations to establish for the parser.</param>
        public virtual void Start(IEnumerable<Type> implementations)
        {
            // Call base class start method
            base.Start();

            ConstructorInfo typeCtor = null;
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("FrameParser"), AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("Helper");
            TypeBuilder typeBuilder = modBuilder.DefineType("ClassFactory");
            List<TypeInfo> outputTypes = new List<TypeInfo>(); // Temporarily hold output types until their IDs are determined.

            foreach (Type asmType in implementations)
            {
                // See if a parameterless constructor is available for this type
                typeCtor = asmType.GetConstructor(Type.EmptyTypes);

                // Since user can call this overload with any list of types, we double check the type criteria.
                // If output type is a class, see if current type derives from it, else if output type is an
                // interface, see if current type implements it.
                if (typeCtor != null && !asmType.IsAbstract && 
                (   
                   (typeof(TOutputType).IsClass && asmType.IsSubclassOf(typeof(TOutputType))) ||
                   (typeof(TOutputType).IsInterface && asmType.GetInterface(typeof(TOutputType).Name) != null))
                )
                {
                    // The type meets the following criteria:
                    //      - has a default public constructor
                    //      - is not abstract and can be instantiated.
                    //      - type is related to class or interface specified for the output
                    TypeInfo outputType = new TypeInfo();
                    outputType.RuntimeType = asmType;

                    // We employ the best peforming way of instantiating objects using reflection.
                    // See: http://blogs.msdn.com/haibo_luo/archive/2005/11/17/494009.aspx

                    // Invokation approach: Reflection.Emit + Delegate
                    MethodBuilder dynamicTypeCtor = typeBuilder.DefineMethod(asmType.Name, MethodAttributes.Public | MethodAttributes.Static, asmType, Type.EmptyTypes);
                    ILGenerator ilGen = dynamicTypeCtor.GetILGenerator();
                    ilGen.Emit(OpCodes.Nop);
                    ilGen.Emit(OpCodes.Newobj, typeCtor);
                    ilGen.Emit(OpCodes.Ret);

                    // We'll hold all of the matching types in this list temporarily until their IDs are determined.
                    outputTypes.Add(outputType);
                }
            }

            // The reason we have to do this here is because we can create a type only once. This is the type
            // that has all the constructors created above for the various class matching the requirements for
            // the output type.
            Type bakedType = typeBuilder.CreateType(); // This can be done only once!

            foreach (TypeInfo outputType in outputTypes)
            {
                outputType.CreateNew = (DefaultConstructor)(Delegate.CreateDelegate(typeof(DefaultConstructor), bakedType.GetMethod(outputType.RuntimeType.Name)));
            }

            foreach (TypeInfo outputType in outputTypes)
            {
                // Now, we'll go though all of the output types we've found and instantiate an instance of each in order to get
                // the identifier for each of the type. This will help lookup of the type to be used when parsing the data.
                TOutputType instance = outputType.CreateNew();
                outputType.TypeID = instance.TypeID;

                if (!m_outputTypes.ContainsKey(outputType.TypeID))
                    m_outputTypes.Add(outputType.TypeID, outputType);
                else
                    OnDuplicateTypeHandlerEncountered(outputType.RuntimeType, outputType.TypeID);
            }
        }

        /// <summary>
        /// Output type specific frame parsing algorithm.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseFrame(byte[] buffer, int offset, int length)
        {
            ICommonHeader<TTypeIdentifier> commonHeader;
            TypeInfo outputType;
            TOutputType instance;
            int parsedLength;

            // Extract the common header from the buffer image which includes the output type ID.
            // For any protocol data that is represented as frames of data in a stream, there will
            // be some set of common identification properties in the frame image, usually at the
            // top, that is common for all frame types.
            commonHeader = ParseCommonHeader(buffer, offset, length);

            // See if there was enough buffer to parse common header, if not exit and wait for more data
            if (commonHeader == null)
                return 0;

            // Lookup TypeID to see if it is a known type
            if (m_outputTypes.TryGetValue(commonHeader.TypeID, out outputType))
            {
                instance = outputType.CreateNew();
                instance.CommonHeader = commonHeader;
                parsedLength = instance.Initialize(buffer, offset, length);

                // Expose parsed type to consumer
                if (parsedLength > 0)
                    OnDataParsed(instance);
            }
            else
            {
                // We encountered an unrecognized data type that cannot be parsed
                if (ProtocolUsesSyncBytes)
                {
                    // Protocol uses synchronization bytes so we scan for them in the current buffer. This effectively
                    // scans through buffer to next frame...
                    int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset + 1, length - 1);

                    if (syncBytesPosition > -1)
                        return syncBytesPosition - offset;
                }
                
                // Without synchronization bytes we have no choice but to move onto the next buffer of data :(
                parsedLength = length;
                OnOutputTypeNotFound(commonHeader.TypeID);
                OnDataDiscarded(buffer.BlockCopy(offset, length));
            }

            return parsedLength;
        }
        
        /// <summary>
        /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented
        /// in the binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>)
        /// for the output types; this will primarily include an ID of the <see cref="Type"/> that the data image represents.  This parsing is
        /// only for common header information, actual parsing will be handled by output type via its <see cref="ISupportBinaryImage.Initialize"/>
        /// method. This header image should also be used to add needed complex state information about the output type being parsed if needed.
        /// </para>
        /// <para>
        /// If there is not enough buffer available to parse common header (as determined by <paramref name="length"/>), return null.  Also, if
        /// the protocol allows frame length to be determined at the time common header is being parsed and there is not enough buffer to parse
        /// the entire frame, it will be optimal to prevent further parsing by returning null.
        /// </para>
        /// </remarks>
        protected abstract ICommonHeader<TTypeIdentifier> ParseCommonHeader(byte[] buffer, int offset, int length);

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="obj">The object that was deserialized from binary image.</param>
        protected virtual void OnDataParsed(TOutputType obj)
        {
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TOutputType>(obj));
        }

        /// <summary>
        /// Raises the <see cref="OutputTypeNotFound"/> event.
        /// </summary>
        /// <param name="id">The ID of the output type that was not found.</param>
        protected virtual void OnOutputTypeNotFound(TTypeIdentifier id)
        {
            if (OutputTypeNotFound != null)
                OutputTypeNotFound(this, new EventArgs<TTypeIdentifier>(id));
        }

        /// <summary>
        /// Raises the <see cref="DuplicateTypeHandlerEncountered"/> event.
        /// </summary>
        /// <param name="duplicateType">The <see cref="Type"/> that defines a type ID that has already been defined.</param>
        /// <param name="id">The ID of the output type that was defined more than once.</param>
        protected virtual void OnDuplicateTypeHandlerEncountered(Type duplicateType, TTypeIdentifier id)
        {
            if (DuplicateTypeHandlerEncountered != null)
                DuplicateTypeHandlerEncountered(this, new EventArgs<Type, TTypeIdentifier>(duplicateType, id));
        }

        #endregion
    }
}
