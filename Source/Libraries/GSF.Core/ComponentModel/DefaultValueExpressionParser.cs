//******************************************************************************************************
//  DefaultValueExpressionParser.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  04/09/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ExpressionEvaluator;
using GSF.Identity;
using GSF.Reflection;

// ReSharper disable UnusedMember.Local
// ReSharper disable StaticMemberInGenericType
namespace GSF.ComponentModel
{
    /// <summary>
    /// Represents a parser for <see cref="DefaultValueExpressionAttribute"/> instances.
    /// </summary>
    public class DefaultValueExpressionParser : DefaultValueExpressionParser<object>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DefaultValueExpressionParser"/>.
        /// </summary>
        /// <param name="expression">C# expression to be parsed.</param>
        public DefaultValueExpressionParser(string expression) : base(expression)
        {
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly TypeRegistry s_defaultTypeRegistry;
        private static readonly Regex s_findThisKeywords;

        // Static Constructor
        static DefaultValueExpressionParser()
        {
            // Setup default type registry for parsing default value expression attributes
            s_defaultTypeRegistry = new TypeRegistry();
            s_defaultTypeRegistry.RegisterDefaultTypes();
            s_defaultTypeRegistry.RegisterType<Guid>();
            s_defaultTypeRegistry.RegisterType<UserInfo>();
            s_defaultTypeRegistry.RegisterType("Common", typeof(Common));

            // Define a regular expression to find "this" keywords
            s_findThisKeywords = new Regex(@"(^this(?=[^\w]))|((?<=[^\w])this(?=[^\w]))|(^this$)", RegexOptions.Compiled | RegexOptions.Multiline);
        }

        // Static Properties

        /// <summary>
        /// Gets the default <see cref="TypeRegistry"/> instance used for evaluating <see cref="DefaultValueExpressionAttribute"/> instances.
        /// </summary>
        public static TypeRegistry DefaultTypeRegistry => s_defaultTypeRegistry;

        // Static Methods

        /// <summary>
        /// Replaces references to "this" keyword with a specified <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="expression">Expression to search.</param>
        /// <param name="fieldName">Replacement value for "this" keyword usages.</param>
        /// <returns>An expression with "this" keywords replaced with specified <paramref name="fieldName"/>.</returns>
        public static string ReplaceThisKeywords(string expression, string fieldName)
        {
            lock (s_findThisKeywords)
                return s_findThisKeywords.Replace(expression, fieldName);
        }

        #endregion
    }

    /// <summary>
    /// Represents a typed parser for <see cref="DefaultValueExpressionAttribute"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of expression to be parsed.</typeparam>
    public class DefaultValueExpressionParser<T> : CompiledExpression<T>
    {
        #region [ Members ]

        // Nested Types
        private class MinimumScope : DefaultValueExpressionScopeBase<T> { }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DefaultValueExpressionParser{T}"/>.
        /// </summary>
        /// <param name="expression">C# expression to be parsed.</param>
        public DefaultValueExpressionParser(string expression) : base(expression)
        {
            TypeRegistry = DefaultValueExpressionParser.DefaultTypeRegistry;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the expression with the provided parameter expression <paramref name="scope"/> and optional <paramref name="typeRegistry"/>.
        /// </summary>
        /// <param name="scope">Parameter expression used to provide context to parsed instances.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="DefaultValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="DefaultValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        public void Parse(Expression scope, TypeRegistry typeRegistry = null)
        {
            if ((object)scope == null)
                throw new ArgumentNullException(nameof(scope));

            if ((object)typeRegistry != null)
                TypeRegistry = typeRegistry;

            BuildTree(scope);
        }

        #endregion

        #region [ Static ]

        // Static Fields

        // Cached expression value dictionary is defined per type T to reduce contention
        private static readonly Dictionary<PropertyInfo, object> s_cachedExpressionValues;
        private static readonly MethodInfo s_addCachedValueMethod;
        private static readonly MethodInfo s_getCachedValueMethod;

        // Static Constructor
        static DefaultValueExpressionParser()
        {
            // Define a table of cached default value expression values
            s_cachedExpressionValues = new Dictionary<PropertyInfo, object>();
            Type expressionParser = typeof(DefaultValueExpressionParser<T>);
            s_addCachedValueMethod = expressionParser.GetMethod("AddCachedValue", BindingFlags.Static | BindingFlags.NonPublic);
            s_getCachedValueMethod = expressionParser.GetMethod("GetCachedValue", BindingFlags.Static | BindingFlags.NonPublic);
        }

        // Static Methods

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> object parameter
        /// applying any specified <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/>
        /// instances that are declared on the type <typeparamref name="T"/> properties.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="DefaultValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="DefaultValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will create new
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/> attributes.
        /// The newly created object will automatically have applied any defined default values as specified by
        /// the encountered attributes.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        public static Func<T> CreateInstance(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null)
        {
            return () => CreateInstance<MinimumScope>(properties, typeRegistry)(new MinimumScope());
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="DefaultValueExpressionScopeBase{T}"/> object parameter applying any
        /// specified <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/>
        /// instances that are declared on the type <typeparamref name="T"/> properties.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="DefaultValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="DefaultValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will create new
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/> attributes.
        /// The newly created object will automatically have applied any defined default values as specified by
        /// the encountered attributes. The generated delegate takes a parameter to a contextual object useful
        /// for providing extra runtime data to <see cref="DefaultValueExpressionAttribute"/> attributes; the
        /// parameter must be derived from <see cref="DefaultValueExpressionScopeBase{T}"/>. Any public fields,
        /// methods or properties defined in the derived class will be automatically accessible from the
        /// expressions declared in the <see cref="DefaultValueExpressionAttribute"/> attributes. By default,
        /// the expressions will have access to the current <typeparamref name="T"/> instance by referencing the
        /// <c>this</c> keyword, which is an alias to <see cref="DefaultValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="DefaultValueExpressionScopeBase{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Func<TExpressionScope, T> CreateInstance<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : DefaultValueExpressionScopeBase<T>
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);

            if ((object)constructor == null)
                return scope => { throw new InvalidOperationException($"No parameterless constructor exists for type \"{type.FullName}\"."); };

            if ((object)properties == null)
                properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead && property.CanWrite);

            List<Expression> expressions = new List<Expression>();
            ParameterExpression newInstance = Expression.Variable(type);
            ParameterExpression scopeParameter = Expression.Parameter(typeof(TExpressionScope));
            DefaultValueAttribute defaultValueAttribute;
            DefaultValueExpressionAttribute defaultValueExpressionAttribute;
            EvaluationOrderAttribute evaluationOrderAttribute;

            // Sort properties by any specified evaluation order
            properties = properties.OrderBy(property => property.TryGetAttribute(out evaluationOrderAttribute) ? evaluationOrderAttribute.OrderIndex : 0);

            // Create new instance and assign to local variable
            expressions.Add(Expression.Assign(newInstance, Expression.New(constructor)));

            // Assign new instance to "Instance" field of scope parameter
            expressions.Add(Expression.Assign(Expression.Field(scopeParameter, typeof(TExpressionScope).GetField("Instance")), newInstance));

            // Find any defined default value attributes for properties and assign them to new instance
            foreach (PropertyInfo property in properties)
            {
                if (property.TryGetAttribute(out defaultValueAttribute))
                {
                    try
                    {
                        expressions.Add(Expression.Call(newInstance, property.SetMethod, Expression.Constant(defaultValueAttribute.Value, property.PropertyType)));
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error evaluating default value attribute for property \"{type.FullName}.{property.Name}\": {ex.Message}", property.Name, ex); };
                    }
                }
                else if (property.TryGetAttribute(out defaultValueExpressionAttribute))
                {
                    try
                    {
                        // Replace all references to "this" with "Instance"
                        string expression = DefaultValueExpressionParser.ReplaceThisKeywords(defaultValueExpressionAttribute.Expression, "Instance");

                        // Parse default value expression
                        DefaultValueExpressionParser expressionParser = new DefaultValueExpressionParser(expression);
                        expressionParser.Parse(scopeParameter, typeRegistry);

                        UnaryExpression getParsedValue = Expression.Convert(expressionParser.Expression, property.PropertyType);

                        if (defaultValueExpressionAttribute.Cached)
                        {
                            ConstantExpression propertyInfo = Expression.Constant(property, typeof(PropertyInfo));
                            ParameterExpression parsedValue = Expression.Variable(property.PropertyType);
                            ParameterExpression cachedValue = Expression.Variable(typeof(object));

                            BlockExpression addParsedValueToCache = Expression.Block(new[] { parsedValue },
                                Expression.Assign(parsedValue, getParsedValue),
                                Expression.Call(s_addCachedValueMethod, propertyInfo, Expression.Convert(parsedValue, typeof(object))),
                                Expression.Call(newInstance, property.SetMethod, parsedValue)
                            );

                            MethodCallExpression setCachedValue = Expression.Call(newInstance, property.SetMethod, Expression.Convert(cachedValue, property.PropertyType));

                            expressions.Add(Expression.Block(new[] { cachedValue },
                                Expression.Assign(cachedValue, Expression.Call(s_getCachedValueMethod, propertyInfo)),
                                Expression.IfThenElse(Expression.Equal(cachedValue, Expression.Constant(null)), addParsedValueToCache, setCachedValue)
                            ));
                        }
                        else
                        {
                            expressions.Add(Expression.Call(newInstance, property.SetMethod, getParsedValue));
                        }
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing default value expression attribute for property \"{type.FullName}.{property.Name}\": {ex.Message}", property.Name, ex); };
                    }
                }
            }

            // Return new instance
            expressions.Add(newInstance);

            // Return a delegate to compiled function block
            return Expression.Lambda<Func<TExpressionScope, T>>(Expression.Block(new[] { newInstance }, expressions), scopeParameter).Compile();
        }

        private static void AddCachedValue(PropertyInfo property, object value)
        {
            lock (s_cachedExpressionValues)
                s_cachedExpressionValues.Add(property, value);
        }

        private static object GetCachedValue(PropertyInfo property)
        {
            object value;

            lock (s_cachedExpressionValues)
                s_cachedExpressionValues.TryGetValue(property, out value);

            return value;
        }

        #endregion
    }
}
