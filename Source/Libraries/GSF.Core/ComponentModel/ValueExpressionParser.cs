//******************************************************************************************************
//  ValueExpressionParser.cs - Gbtc
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
using GSF.Collections;
using GSF.Identity;
using GSF.Reflection;

// ReSharper disable UnusedMember.Local
// ReSharper disable StaticMemberInGenericType
namespace GSF.ComponentModel
{
    /// <summary>
    /// Represents a parser for <see cref="ValueExpressionAttributeBase"/> instances.
    /// </summary>
    public class ValueExpressionParser : ValueExpressionParser<object>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ValueExpressionParser"/>.
        /// </summary>
        /// <param name="expression">C# expression to be parsed.</param>
        public ValueExpressionParser(string expression) : base(expression)
        {
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly TypeRegistry s_defaultTypeRegistry;
        private static readonly Regex s_findThisKeywords;

        // Static Constructor
        static ValueExpressionParser()
        {
            // Setup default type registry for parsing value expression attributes
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
        /// Gets the default <see cref="TypeRegistry"/> instance used for evaluating <see cref="ValueExpressionAttributeBase"/> instances.
        /// </summary>
        public static TypeRegistry DefaultTypeRegistry => s_defaultTypeRegistry;

        // Static Methods

        /// <summary>
        /// Returns a flag that determines if the "this" keyword exists with the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">Expression to search.</param>
        /// <returns><c>true</c> if "this" keyword exists in expression; otherwise, <c>false</c>.</returns>
        public static bool HasThisKeywords(string expression)
        {
            lock (s_findThisKeywords)
                return s_findThisKeywords.IsMatch(expression);
        }

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
    /// Represents a typed parser for <see cref="ValueExpressionAttributeBase"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of expression to be parsed.</typeparam>
    public class ValueExpressionParser<T> : CompiledExpression<T>
    {
        #region [ Members ]

        // Nested Types
        private class MinimumScope : ValueExpressionScopeBase<T> { }

        private class EvaluationOrderException : Exception
        {
            public EvaluationOrderException(string message) : base(message)
            {
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ValueExpressionParser{T}"/>.
        /// </summary>
        /// <param name="expression">C# expression to be parsed.</param>
        public ValueExpressionParser(string expression) : base(expression)
        {
            TypeRegistry = ValueExpressionParser.DefaultTypeRegistry;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the expression with the provided parameter expression <paramref name="scope"/> and optional <paramref name="typeRegistry"/>.
        /// </summary>
        /// <param name="scope">Parameter expression used to provide context to parsed instances.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="ValueExpressionAttributeBase"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
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
        static ValueExpressionParser()
        {
            // Define a table of cached default value expression values
            s_cachedExpressionValues = new Dictionary<PropertyInfo, object>();
            Type expressionParser = typeof(ValueExpressionParser<T>);
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
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
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
            Func<MinimumScope, T> createInstanceFunction = CreateInstance<MinimumScope>(properties, typeRegistry);
            return () => createInstanceFunction(new MinimumScope());
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> object parameter
        /// applying any specified <see cref="DefaultValueAttribute"/> or <typeparamref name="TValueExpressionAttribute"/>
        /// instances that are declared on the type <typeparamref name="T"/> properties.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will create new
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="DefaultValueAttribute"/> or <typeparamref name="TValueExpressionAttribute"/> attributes.
        /// The newly created object will automatically have applied any defined default values as specified by
        /// the encountered attributes.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static Func<T> CreateInstanceForType<TValueExpressionAttribute>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : ValueExpressionAttributeBase
        {
            Func<MinimumScope, T> createInstanceFunction = CreateInstanceForType<TValueExpressionAttribute, MinimumScope>(properties, typeRegistry);
            return () => createInstanceFunction(new MinimumScope());
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any specified
        /// <see cref="UpdateValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance is accepted
        /// as the parameter to the returned delegate <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="UpdateValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will update
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <see cref="UpdateValueExpressionAttribute"/> attributes. The updated object will automatically
        /// have applied any defined update values as specified by the encountered attributes.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        public static Action<T> UpdateInstance(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null)
        {
            Action<MinimumScope> updateInstanceFunction = UpdateInstance<MinimumScope>(properties, typeRegistry);
            return instance => updateInstanceFunction(new MinimumScope { Instance = instance });
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any specified
        /// <typeparamref name="TValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance is accepted
        /// as the parameter to the returned delegate <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will update
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <typeparamref name="TValueExpressionAttribute"/> attributes. The updated object will automatically
        /// have applied any defined update values as specified by the encountered attributes.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="ValueExpressionAttributeBase"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static Action<T> UpdateInstanceForType<TValueExpressionAttribute>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : ValueExpressionAttributeBase
        {
            Action<MinimumScope> updateInstanceFunction = UpdateInstanceForType<TValueExpressionAttribute, MinimumScope>(properties, typeRegistry);
            return instance => updateInstanceFunction(new MinimumScope { Instance = instance });
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any
        /// specified <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/>
        /// instances that are declared on the type <typeparamref name="T"/> properties.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="DefaultValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will create new
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/> attributes.
        /// The newly created object will automatically have applied any defined default values as specified by
        /// the encountered attributes. The generated delegate takes a parameter to a contextual object useful
        /// for providing extra runtime data to <see cref="DefaultValueExpressionAttribute"/> attributes; the
        /// parameter must be derived from <see cref="ValueExpressionScopeBase{T}"/>. Any public fields,
        /// methods or properties defined in the derived class will be automatically accessible from the
        /// expressions declared in the <see cref="DefaultValueExpressionAttribute"/> attributes. By default,
        /// the expressions will have access to the current <typeparamref name="T"/> instance by referencing the
        /// <c>this</c> keyword, which is an alias to <see cref="ValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="ValueExpressionScopeBase{T}"/> parameter type.</typeparam>
        public static Func<TExpressionScope, T> CreateInstance<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : ValueExpressionScopeBase<T>
        {
            return CreateInstanceForType<DefaultValueExpressionAttribute, TExpressionScope>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any
        /// specified <see cref="DefaultValueAttribute"/> or <typeparamref name="TValueExpressionAttribute"/>
        /// instances that are declared on the type <typeparamref name="T"/> properties.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will create new
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="DefaultValueAttribute"/> or <typeparamref name="TValueExpressionAttribute"/> attributes.
        /// The newly created object will automatically have applied any defined default values as specified by
        /// the encountered attributes. The generated delegate takes a parameter to a contextual object useful
        /// for providing extra runtime data to <typeparamref name="TValueExpressionAttribute"/> attributes; the
        /// parameter must be derived from <see cref="ValueExpressionScopeBase{T}"/>. Any public fields,
        /// methods or properties defined in the derived class will be automatically accessible from the
        /// expressions declared in the <typeparamref name="TValueExpressionAttribute"/> attributes. By default,
        /// the expressions will have access to the current <typeparamref name="T"/> instance by referencing the
        /// <c>this</c> keyword, which is an alias to <see cref="ValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="ValueExpressionAttributeBase"/> parameter type.</typeparam>
        /// <typeparam name="TExpressionScope"><see cref="ValueExpressionScopeBase{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Func<TExpressionScope, T> CreateInstanceForType<TValueExpressionAttribute, TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : ValueExpressionAttributeBase where TExpressionScope : ValueExpressionScopeBase<T>
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(Type.EmptyTypes);

            if ((object)constructor == null)
                return scope => { throw new InvalidOperationException($"No parameterless constructor exists for type \"{typeof(T).FullName}\"."); };

            if ((object)properties == null)
                properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead && property.CanWrite);

            List<Expression> expressions = new List<Expression>();
            ParameterExpression newInstance = Expression.Variable(typeof(T));
            ParameterExpression scopeParameter = Expression.Parameter(typeof(TExpressionScope));
            DefaultValueAttribute defaultValueAttribute;
            TValueExpressionAttribute valueExpressionAttribute;

            // Sort properties by any specified evaluation order
            properties = properties.OrderBy(property => property.TryGetAttribute(out valueExpressionAttribute) ? valueExpressionAttribute.EvaluationOrder : 0);

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
                        return scope => { throw new ArgumentException($"Error evaluating \"DefaultValueAttribute\" for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message}", property.Name, ex); };
                    }
                }
                else if (property.TryGetAttribute(out valueExpressionAttribute))
                {
                    try
                    {
                        expressions.Add(AssignParsedValueExpression(valueExpressionAttribute, typeRegistry, property, scopeParameter, newInstance));
                    }
                    catch (EvaluationOrderException ex)
                    {
                        // Need to wrap exceptions in order to keep original call stack
                        return scope => { throw new InvalidOperationException(ex.Message, ex); };
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing \"{typeof(TValueExpressionAttribute).Name}\" value for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message}", property.Name, ex); };
                    }
                }
            }

            // Return new instance
            expressions.Add(newInstance);

            // Return a delegate to compiled function block
            return Expression.Lambda<Func<TExpressionScope, T>>(Expression.Block(new[] { newInstance }, expressions), scopeParameter).Compile();
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any specified
        /// <see cref="UpdateValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="ValueExpressionScopeBase{T}.Instance"/> property prior to call.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="UpdateValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will update
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <see cref="UpdateValueExpressionAttribute"/> attributes. The updated object will automatically
        /// have applied any defined update values as specified by the encountered attributes. The generated
        /// delegate takes a parameter to a contextual object useful for providing extra runtime data to
        /// <see cref="UpdateValueExpressionAttribute"/> attributes; the parameter must be derived from
        /// <see cref="ValueExpressionScopeBase{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <see cref="UpdateValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="ValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="ValueExpressionScopeBase{T}"/> parameter type.</typeparam>
        public static Action<TExpressionScope> UpdateInstance<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : ValueExpressionScopeBase<T>
        {
            return UpdateInstanceForType<UpdateValueExpressionAttribute, TExpressionScope>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="ValueExpressionScopeBase{T}"/> object parameter applying any specified
        /// <typeparamref name="TValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="ValueExpressionScopeBase{T}.Instance"/> property prior to call.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will update
        /// objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <typeparamref name="TValueExpressionAttribute"/> attributes. The updated object will automatically
        /// have applied any defined update values as specified by the encountered attributes. The generated
        /// delegate takes a parameter to a contextual object useful for providing extra runtime data to
        /// <typeparamref name="TValueExpressionAttribute"/> attributes; the parameter must be derived from
        /// <see cref="ValueExpressionScopeBase{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <typeparamref name="TValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="ValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="ValueExpressionAttributeBase"/> parameter type.</typeparam>
        /// <typeparam name="TExpressionScope"><see cref="ValueExpressionScopeBase{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Action<TExpressionScope> UpdateInstanceForType<TValueExpressionAttribute, TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : ValueExpressionAttributeBase where TExpressionScope : ValueExpressionScopeBase<T>
        {
            if ((object)properties == null)
                properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead && property.CanWrite);

            List<Expression> expressions = new List<Expression>();
            ParameterExpression instance = Expression.Variable(typeof(T));
            ParameterExpression scopeParameter = Expression.Parameter(typeof(TExpressionScope));
            TValueExpressionAttribute valueExpressionAttribute;

            // Sort properties by any specified evaluation order
            properties = properties.OrderBy(property => property.TryGetAttribute(out valueExpressionAttribute) ? valueExpressionAttribute.EvaluationOrder : 0);

            // Get "Instance" field of scope parameter and assign to local variable
            expressions.Add(Expression.Assign(instance, Expression.Field(scopeParameter, typeof(TExpressionScope).GetField("Instance"))));

            // Find any defined default value attributes for properties and assign them to new instance
            foreach (PropertyInfo property in properties)
            {
                if (property.TryGetAttribute(out valueExpressionAttribute))
                {
                    try
                    {
                        expressions.Add(AssignParsedValueExpression(valueExpressionAttribute, typeRegistry, property, scopeParameter, instance));
                    }
                    catch (EvaluationOrderException ex)
                    {
                        // Need to wrap exceptions in order to keep original call stack
                        return scope => { throw new InvalidOperationException(ex.Message, ex); };
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing \"{typeof(TValueExpressionAttribute).Name}\" value for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message}", property.Name, ex); };
                    }
                }
            }

            // Return a delegate to compiled function block
            return Expression.Lambda<Action<TExpressionScope>>(Expression.Block(new[] { instance }, expressions), scopeParameter).Compile();
        }

        private static Expression AssignParsedValueExpression(ValueExpressionAttributeBase valueExpressionAttribute, TypeRegistry typeRegistry, PropertyInfo property, ParameterExpression scopeParameter, ParameterExpression instance)
        {
            string expression = valueExpressionAttribute.Expression;

            // Check for "this" keywords in expression
            if (ValueExpressionParser.HasThisKeywords(expression))
            {
                if (valueExpressionAttribute.EvaluationOrder < 1)
                    throw new EvaluationOrderException($"Value expression attribute for property \"{typeof(T).FullName}.{property.Name}\" references the \"this\" keyword and must specify a positive \"EvaluationOrder\".");

                // Replace all references to "this" with "Instance"
                expression = ValueExpressionParser.ReplaceThisKeywords(expression, "Instance");
            }

            // Parse value expression
            ValueExpressionParser expressionParser = new ValueExpressionParser(expression);
            expressionParser.Parse(scopeParameter, typeRegistry);

            UnaryExpression getParsedValue = Expression.Convert(expressionParser.Expression, property.PropertyType);

            if (valueExpressionAttribute.Cached)
            {
                ConstantExpression propertyInfo = Expression.Constant(property, typeof(PropertyInfo));
                ParameterExpression parsedValue = Expression.Variable(property.PropertyType);
                ParameterExpression cachedValue = Expression.Variable(typeof(Tuple<bool, object>));

                // ReSharper disable PossibleNullReferenceException
                MethodInfo getTupleItem1 = typeof(Tuple<bool, object>).GetProperty("Item1").GetMethod;
                MethodInfo getTupleItem2 = typeof(Tuple<bool, object>).GetProperty("Item2").GetMethod;
                // ReSharper restore PossibleNullReferenceException

                BlockExpression addParsedValueToCache = Expression.Block(new[] {parsedValue},
                    Expression.Assign(parsedValue, getParsedValue),
                    Expression.Call(s_addCachedValueMethod, propertyInfo, Expression.Convert(parsedValue, typeof(object))),
                    Expression.Call(instance, property.SetMethod, parsedValue)
                );

                MethodCallExpression setCachedValue = Expression.Call(instance, property.SetMethod, Expression.Convert(Expression.Call(cachedValue, getTupleItem2), property.PropertyType));

                return Expression.Block(new[] {cachedValue},
                    Expression.Assign(cachedValue, Expression.Call(s_getCachedValueMethod, propertyInfo)),
                    Expression.IfThenElse(Expression.IsTrue(Expression.Call(cachedValue, getTupleItem1)), setCachedValue, addParsedValueToCache)
                );
            }

            return Expression.Call(instance, property.SetMethod, getParsedValue);
        }

        // Function referenced through reflection - see s_addCachedValueMethod
        private static void AddCachedValue(PropertyInfo property, object value)
        {
            lock (s_cachedExpressionValues)
                s_cachedExpressionValues.GetOrAdd(property, value);
        }

        // Function referenced through reflection - see s_getCachedValueMethod
        private static Tuple<bool, object> GetCachedValue(PropertyInfo property)
        {
            bool exists;
            object value;

            lock (s_cachedExpressionValues)
                exists = s_cachedExpressionValues.TryGetValue(property, out value);

            return new Tuple<bool, object>(exists, value);
        }

        #endregion
    }
}
