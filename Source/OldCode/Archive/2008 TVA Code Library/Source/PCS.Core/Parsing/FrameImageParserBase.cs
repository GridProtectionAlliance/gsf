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
//  11/20/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Collections.Generic;

namespace PCS.Parsing
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
        /// image represented.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the object that was deserialized from the binary image.
        /// </remarks>
        public event EventHandler<EventArgs<TOutputType>> DataParsed;

        /// <summary>
        /// Occurs when matching a output type for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the output type that could not be found.
        /// </remarks>
        public event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        // Fields
        private Dictionary<TTypeIdentifier, TypeInfo> m_outputTypes;

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
        public void Start(IEnumerable<Type> implementations)
        {
            // Call base class start method, this ensures "Initialize" has been called
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
            int endOfBuffer = offset + length - 1;
            int parsedFrameLength;
            ICommonHeader<TTypeIdentifier> commonHeader;
            TypeInfo outputType;
            TOutputType instance;

            // Extract the common header from the buffer image which includes the output type ID.
            // For any protocol data that is represented as frames of data in a stream, there will
            // be some set of common identification properties in the frame image, usually at the
            // top, that is common for all frame types.
            parsedFrameLength = ParseCommonHeader(buffer, offset, length, out commonHeader);
            offset += parsedFrameLength;

            if (m_outputTypes.TryGetValue(commonHeader.TypeID, out outputType))
            {
                instance = outputType.CreateNew();
                instance.CommonHeader = commonHeader;
                parsedFrameLength += instance.Initialize(buffer, offset, endOfBuffer - offset + 1);
                
                // Expose parsed type to consumer
                OnDataParsed(instance);
            }
            else
            {
                // If we come across data in the image that we cannot convert to a type then we move on
                // to the next buffer of data :(
                parsedFrameLength = length;
                OnOutputTypeNotFound(commonHeader.TypeID);
            }

            return parsedFrameLength;
        }
        
        /// <summary>
        /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented in the binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <param name="commonHeader">The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>) for
        /// the output types via the <paramref name="commonHeader"/> parameter; this will primarily include an ID of the <see cref="Type"/> that the
        /// data image represents.  This parsing is only for common header information, actual parsing will be handled by output type via its
        /// <see cref="ISupportBinaryImage.Initialize"/> method. This header image should also be used to add needed complex state information
        /// about the output type being parsed if needed.
        /// </para>
        /// <para>
        /// This function should return total number of bytes that were parsed from the buffer. Consumers can choose to return "zero" if the output
        /// type <see cref="ISupportBinaryImage.Initialize"/> implementation expects the entire buffer image, however it will be optimal if
        /// the ParseCommonHeader method parses the header, and the Initialize method only parses the body of the image.
        /// </para>
        /// </remarks>
        protected abstract int ParseCommonHeader(byte[] buffer, int offset, int length, out ICommonHeader<TTypeIdentifier> commonHeader);

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="obj">Object deserialized from binary image.</param>
        protected virtual void OnDataParsed(TOutputType obj)
        {
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TOutputType>(obj));
        }

        /// <summary>
        /// Raises the <see cref="OutputTypeNotFound"/> event.
        /// </summary>
        /// <param name="id">ID of the output type that was not found.</param>
        protected virtual void OnOutputTypeNotFound(TTypeIdentifier id)
        {
            if (OutputTypeNotFound != null)
                OutputTypeNotFound(this, new EventArgs<TTypeIdentifier>(id));
        }

        #endregion
    }
}
