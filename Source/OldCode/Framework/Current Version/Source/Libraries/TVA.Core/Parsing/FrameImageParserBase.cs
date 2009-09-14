//*******************************************************************************************************
//  FrameImageParserBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/28/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/20/2008 - J. Ritchie Carroll
//       Adapted for more generalized use via the following related base classes:
//          BinaryImageParserBase => FrameImageParserBase => MultiSourceFrameImageParserBase.
//  9/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
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
