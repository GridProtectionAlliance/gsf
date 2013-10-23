//******************************************************************************************************
//  SignalRoute.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/21/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Routing
{
    /// <summary>
    /// Represents a route that time-series signals
    /// take to reach their destinations for processing.
    /// </summary>
    public class SignalRoute
    {
        #region [ Members ]

        // Fields
        private MethodInfo m_processingMethod;
        private Type m_listType;
        private Type m_signalType;

        private IAdapter m_adapter;
        private IList m_list;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SignalRoute"/> class.
        /// </summary>
        /// <param name="processingMethod">The method used for processing time-series signals.</param>
        public SignalRoute(MethodInfo processingMethod)
            : this(processingMethod, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SignalRoute"/> class.
        /// </summary>
        /// <param name="processingMethod">The method used for processing time-series signals.</param>
        /// <param name="adapter">The adapter that is receiving the signal.</param>
        public SignalRoute(MethodInfo processingMethod, IAdapter adapter)
        {
            m_processingMethod = processingMethod;
            m_listType = GetListType(processingMethod);
            m_adapter = adapter;

            if ((object)m_listType != null)
                m_signalType = m_listType.GetGenericArguments()[0];
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SignalRoute"/> class.
        /// </summary>
        /// <param name="route">The route to be copied.</param>
        public SignalRoute(SignalRoute route)
            : this(route, route.Adapter)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SignalRoute"/> class.
        /// </summary>
        /// <param name="route">The route to be copied.</param>
        /// <param name="adapter">The adapter that is receiving the signal.</param>
        public SignalRoute(SignalRoute route, IAdapter adapter)
        {
            m_processingMethod = route.ProcessingMethod;
            m_listType = route.ListType;
            m_signalType = route.SignalType;
            m_adapter = adapter;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the method used to process the time-series signals.
        /// </summary>
        public MethodInfo ProcessingMethod
        {
            get
            {
                return m_processingMethod;
            }
        }

        /// <summary>
        /// Gets the type of the list containing time-series signals that is
        /// passed into the <see cref="ProcessingMethod"/> for processing.
        /// </summary>
        public Type ListType
        {
            get
            {
                return m_listType;
            }
        }

        /// <summary>
        /// Gets the type of signals that can be processed via this route.
        /// </summary>
        public Type SignalType
        {
            get
            {
                return m_signalType;
            }
        }

        /// <summary>
        /// Gets the adapter that is receiving the signal.
        /// </summary>
        public IAdapter Adapter
        {
            get
            {
                return m_adapter;
            }
        }
        
        /// <summary>
        /// Gets the list that contains the signals to be processed by the <see cref="ProcessingMethod"/>.
        /// </summary>
        public IList List
        {
            get
            {
                // Instantiate the list on request
                if ((object)m_list == null && (object)m_listType != null)
                    m_list = (IList)Activator.CreateInstance(m_listType);

                return m_list;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Invokes the processing method for this route and
        /// clears out the list of signals to be processed.
        /// </summary>
        public void Invoke()
        {
            if ((object)m_list != null)
            {
                if ((object)m_processingMethod != null && (object)m_adapter != null)
                    m_processingMethod.Invoke(m_adapter, new object[] { m_list });

                m_list.Clear();
            }
        }

        /// <summary>
        /// Attempts to create a generic route, based on the current route,
        /// that will process the given <paramref name="timeSeriesEntityType"/>.
        /// </summary>
        /// <param name="timeSeriesEntityType">The type of time-series signals to be processed by the generic route.</param>
        /// <returns>A generic route that will process the given type; or null if no such route can be made.</returns>
        public SignalRoute MakeGenericSignalRoute(Type timeSeriesEntityType)
        {
            List<Type> possibleMatches;
            Type[] substitutions;
            Type definitionType;
            Type baseType;

            if ((object)m_processingMethod == null || (object)m_listType == null)
                return null;

            // Only generic method definitions can
            // be used to create generic methods
            if (!m_processingMethod.IsGenericMethodDefinition)
                return null;

            definitionType = m_processingMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];
            substitutions = new Type[m_processingMethod.GetGenericArguments().Length];

            if (definitionType.IsInterface)
            {
                // Only interfaces that extend ITimeSeriesEntity
                // can be used to match the method signature
                possibleMatches = timeSeriesEntityType.GetInterfaces()
                    .Where(interfaceType => s_fundamentalBaseType.IsAssignableFrom(interfaceType))
                    .ToList();
            }
            else
            {
                // Only base types of the timeSeriesEntityType
                // can be used to match the method signature
                baseType = timeSeriesEntityType;
                possibleMatches = new List<Type>();

                while (s_fundamentalBaseType.IsAssignableFrom(baseType))
                {
                    possibleMatches.Add(baseType);

                    // Null reference cannot occur because at some point
                    // in the hierarchy, we will arrive at System.Object,
                    // which does not implement ITimeSeriesEntity
                    baseType = baseType.BaseType;
                }
            }

            foreach (Type possibleMatch in possibleMatches)
            {
                // Analyze the types to determine whether they are compatible and to
                // determine what substitutions are necessary to create a generic method
                if (TryGetSubstitutions(definitionType, possibleMatch, substitutions))
                    return new SignalRoute(m_processingMethod.MakeGenericMethod(substitutions), m_adapter);

                Array.Clear(substitutions, 0, substitutions.Length);
            }

            return null;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Type s_fundamentalBaseType = typeof(ITimeSeriesEntity);
        private static readonly Type s_genericListDefinition = typeof(List<object>).GetGenericTypeDefinition();

        // Static Methods

        // Attempts to get the type of the list needed to pass into the
        // processing method to process signals. Returns null if the
        // signature does not match a signal processing method.
        private static Type GetListType(MethodInfo processingMethod)
        {
            ParameterInfo[] parameters;
            Type parameterType;
            Type innerType;
            Type listType;

            if ((object)processingMethod == null)
                return null;

            parameters = processingMethod.GetParameters();

            // Processing methods should have a single parameter
            if (parameters.Length != 1)
                return null;

            parameterType = parameters[0].ParameterType;

            // The parameter must be a generic type
            if (!parameterType.IsGenericType)
                return null;

            // All generic types have at least one generic argument
            innerType = parameterType.GetGenericArguments()[0];
            listType = s_genericListDefinition.MakeGenericType(innerType);

            // Processing method must take a List<innerType> as its only argument
            if (!parameterType.IsAssignableFrom(listType))
                return null;

            // The inner type must implement ITimeSeriesEntity
            if (!s_fundamentalBaseType.IsAssignableFrom(innerType))
                return null;

            return listType;
        }

        // Recursively attempts to match the given type definition to the concrete type
        // and fills the given array with type substitutions needed to make the types match.
        // The array of substitutions should be filled with null prior to calling this method.
        private static bool TryGetSubstitutions(Type definitionType, Type concreteType, Type[] substitutions)
        {
            int genericParameterPosition;
            Type[] innerDefinitionTypes;
            Type[] innerPossibleTypes;

            if (definitionType.IsGenericParameter)
            {
                genericParameterPosition = definitionType.GenericParameterPosition;

                // If a substitution has yet to be made for this parameter,
                // make the substitution and determine whether the concrete
                // type violates the constraints imposed on the generic parameter
                if ((object)substitutions[genericParameterPosition] == null)
                {
                    substitutions[genericParameterPosition] = concreteType;
                    return definitionType.GetGenericParameterConstraints().All(constraint => constraint.IsAssignableFrom(concreteType));
                }

                // If a substitution has already been made for this
                // parameter, make sure it matches the concrete type
                return (substitutions[genericParameterPosition] == concreteType);
            }

            // If the definition type is concrete, but the
            // types don't match, then they are not compatible
            if (!definitionType.ContainsGenericParameters)
                return (definitionType == concreteType);

            // At this point we know that the definition type has to be generic.
            // If the concrete type is not generic, then the types cannot be compatible
            if (!concreteType.IsGenericType)
                return false;

            // If the type definitions don't match, the types are not compatible
            if (definitionType.GetGenericTypeDefinition() != concreteType.GetGenericTypeDefinition())
                return false;

            // Get the generic type arguments for both types
            innerDefinitionTypes = definitionType.GetGenericArguments();
            innerPossibleTypes = concreteType.GetGenericArguments();

            for (int i = 0; i < innerDefinitionTypes.Length; i++)
            {
                // Recursively compare the generic type arguments to determine whether they are compatible
                if (!TryGetSubstitutions(innerDefinitionTypes[i], innerPossibleTypes[i], substitutions))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
