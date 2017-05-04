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
// ReSharper disable PossibleNullReferenceException
namespace GSF.ComponentModel
{
    /// <summary>
    /// Represents a parser for <see cref="IValueExpressionAttribute"/> instances.
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

        /// <summary>
        /// Creates a new <see cref="ValueExpressionParser"/> from the specified
        /// <paramref name="valueExpressionAttribute"/> and <paramref name="property"/>
        /// parameters deriving the base expression value from
        /// <see cref="IValueExpressionAttribute.GetPropertyUpdateValue"/>.
        /// </summary>
        /// <param name="valueExpressionAttribute">Source <see cref="IValueExpressionAttribute"/> instance.</param>
        /// <param name="property">Source <see cref="PropertyInfo"/> instance.</param>
        public ValueExpressionParser(IValueExpressionAttribute valueExpressionAttribute, PropertyInfo property) : base(valueExpressionAttribute, property)
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
            s_defaultTypeRegistry.RegisterType("StringExtensions", typeof(StringExtensions));

            // Define a regular expression to find "this" keywords
            s_findThisKeywords = new Regex(@"(^this(?=[^\w]))|((?<=[^\w])this(?=[^\w]))|(^this$)", RegexOptions.Compiled | RegexOptions.Multiline);
        }

        // Static Properties

        /// <summary>
        /// Gets the default <see cref="TypeRegistry"/> instance used for evaluating <see cref="IValueExpressionAttribute"/> instances.
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

        /// <summary>
        /// Derives an expression based on <paramref name="member"/> info with any <c>this</c>
        /// keywords properly referencing <see cref="ValueExpressionScopeBase{T}.Instance"/> value.
        /// </summary>
        /// <param name="expression">Expression to derive, typically from <see cref="IValueExpressionAttribute.GetPropertyUpdateValue"/> or <see cref="IValueExpressionAttribute.GetExpressionUpdateValue"/>.</param>
        /// <param name="valueExpressionAttribute">Associated <see cref="IValueExpressionAttribute"/> instance.</param>
        /// <param name="member">Associated <see cref="MemberInfo"/> instance, typically target property for <paramref name="valueExpressionAttribute"/>.</param>
        /// <param name="typeName">Modeled type name, e.g., typeof&lt;T&gt;.FullName.</param>
        /// <returns>Derived expression with any <c>this</c> keywords properly referencing <see cref="ValueExpressionScopeBase{T}.Instance"/> value.</returns>
        /// <exception cref="EvaluationOrderException">
        /// Specified <paramref name="expression"/> references the <c>this</c> keyword and must specify a positive <see cref="IValueExpressionAttribute.EvaluationOrder"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="expression"/> cannot be <c>null</c>.</exception>
        public static string DeriveExpression(string expression, IValueExpressionAttribute valueExpressionAttribute, MemberInfo member, string typeName)
        {
            if ((object)expression == null)
                throw new ArgumentNullException(nameof(expression));

            // Check for "this" keywords in expression
            if (HasThisKeywords(expression))
            {
                if (valueExpressionAttribute.EvaluationOrder < 1)
                    throw new EvaluationOrderException($"Value expression attribute for property \"{typeName}.{member.Name}\" references the \"this\" keyword and must specify a positive \"EvaluationOrder\".");

                // Replace all references to "this" with "Instance"
                expression = ReplaceThisKeywords(expression, "Instance");
            }

            return expression;
        }

        #endregion
    }

    /// <summary>
    /// Represents a typed parser for <see cref="IValueExpressionAttribute"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of expression to be parsed.</typeparam>
    public class ValueExpressionParser<T> : CompiledExpression<T>
    {
        #region [ Members ]

        // Nested Types
        private class MinimumScope : ValueExpressionScopeBase<T> { }

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

        /// <summary>
        /// Creates a new <see cref="ValueExpressionParser"/> from the specified
        /// <paramref name="valueExpressionAttribute"/> and <paramref name="property"/>
        /// parameters deriving the base expression value from
        /// <see cref="IValueExpressionAttribute.GetPropertyUpdateValue"/>.
        /// </summary>
        /// <param name="valueExpressionAttribute">Source <see cref="IValueExpressionAttribute"/> instance.</param>
        /// <param name="property">Source <see cref="PropertyInfo"/> instance.</param>
        public ValueExpressionParser(IValueExpressionAttribute valueExpressionAttribute, PropertyInfo property) : base(DeriveExpression(null, valueExpressionAttribute, property))
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the expression with the provided parameter expression <paramref name="scope"/> and optional <paramref name="typeRegistry"/>.
        /// </summary>
        /// <param name="scope">Parameter expression used to provide context to parsed instances.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="IValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <param name="isCall"><c>true</c> if parsing an action; otherwise, <c>false</c> for a function.</param>
        public void Parse(Expression scope, TypeRegistry typeRegistry = null, bool isCall = false)
        {
            if ((object)scope == null)
                throw new ArgumentNullException(nameof(scope));

            if ((object)typeRegistry != null)
                TypeRegistry = typeRegistry;

            BuildTree(scope, isCall);
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
        /// Validates that any static type <typeparamref name="T"/> functionality is initialized.
        /// </summary>
        /// <remarks>
        /// As long as type <typeparamref name="T"/> defines a parameterless constructor, this method
        /// will create an instance of the modeled class so that any defined static functionality will
        /// be initialized. Calling this method in advance of any of the static create or update delegate
        /// generation functions will allow modeled types to self-register any custom symbols and types
        /// that may be used during evaluation of value expressions attributes, e.g.:
        /// <code>
        /// static MyModel()
        /// {
        ///     TableOperations&lt;MyModel&gt;.TypeRegistry.RegisterType&lt;MyType&gt;();
        /// }
        /// </code>
        /// </remarks>
        /// <returns>
        /// <c>true</c> if type <typeparamref name="T"/> supports a parameterless constructor and was
        /// successfully initialized; otherwise, <c>false</c>.
        /// </returns>
        public static bool InitializeType()
        {
            if ((object)typeof(T).GetConstructor(Type.EmptyTypes) != null)
                return (object)Activator.CreateInstance<T>() != null;

            return false;
        }

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
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in a newly created model.
        /// </note>
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
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in a newly created model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static Func<T> CreateInstanceForType<TValueExpressionAttribute>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute
        {
            Func<MinimumScope, T> createInstanceFunction = CreateInstanceForType<TValueExpressionAttribute, MinimumScope>(properties, typeRegistry);
            return () => createInstanceFunction(new MinimumScope());
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> applying any
        /// specified <see cref="UpdateValueExpressionAttribute"/> instances that are declared on the type
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
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in an existing model.
        /// </note>
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
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> applying any
        /// specified <typeparamref name="TValueExpressionAttribute"/> instances that are declared on the type
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
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in an existing model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="IValueExpressionAttribute"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static Action<T> UpdateInstanceForType<TValueExpressionAttribute>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute
        {
            Action<MinimumScope> updateInstanceFunction = UpdateInstanceForType<TValueExpressionAttribute, MinimumScope>(properties, typeRegistry);
            return instance => updateInstanceFunction(new MinimumScope { Instance = instance });
        }

        /// <summary>
        /// Generates a delegate that will execute expression assignments on an instance of type <typeparamref name="T"/>
        /// where expressions are <see cref="TypeConvertedValueExpressionAttribute"/> instances that are declared
        /// on the type <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance is accepted
        /// as the parameter to the returned delegate <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="TypeConvertedValueExpressionAttribute"/> instances,
        /// or <c>null</c> to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// This function is useful for generating a delegate to a compiled function that will execute expression
        /// assignments on objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <see cref="TypeConvertedValueExpressionAttribute"/> attributes. Note that the expression in the
        /// <see cref="TypeConvertedValueExpressionAttribute"/> attribute is expected to evaluate to a property
        /// such that it can be assigned the target type <typeparamref name="T"/> property value.
        /// <note type="note">
        /// This function will assign current modeled property values back to expressions, this is often useful
        /// when a model is being synchronized to an external source, e.g., user interface elements.
        /// </note>
        /// </para>
        /// <para>
        /// This method is the inverse call for <see cref="UpdateProperties"/>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will execute expression assignments on <typeparamref name="T"/> instances.
        /// </returns>
        public static Action<T> UpdateExpressions(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null)
        {
            Action<MinimumScope> updateExpressionsFunction = UpdateExpressions<MinimumScope>(properties, typeRegistry);
            return instance => updateExpressionsFunction(new MinimumScope { Instance = instance });
        }

        /// <summary>
        /// Generates a delegate that will execute expression assignments on an instance of type <typeparamref name="T"/>
        /// where expressions are <typeparamref name="TValueExpressionAttribute"/> instances that are declared
        /// on the type <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance is accepted
        /// as the parameter to the returned delegate <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will execute expression
        /// assignments on objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <typeparamref name="TValueExpressionAttribute"/> attributes. Note that the expression in the
        /// <typeparamref name="TValueExpressionAttribute"/> attribute is expected to evaluate to a property
        /// such that it can be assigned the target type <typeparamref name="T"/> property value.
        /// <note type="note">
        /// This function will assign current modeled property values back to expressions, this is often useful
        /// when a model is being synchronized to an external source, e.g., user interface elements.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will execute expression assignments on <typeparamref name="T"/> instances.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="IValueExpressionAttribute"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static Action<T> UpdateExpressionsForType<TValueExpressionAttribute>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute
        {
            Action<MinimumScope> updateExpressionsFunction = UpdateExpressionsForType<TValueExpressionAttribute, MinimumScope>(properties, typeRegistry);
            return instance => updateExpressionsFunction(new MinimumScope { Instance = instance });
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> assigning values
        /// from <see cref="TypeConvertedValueExpressionAttribute"/> instances that are declared on the
        /// type <typeparamref name="T"/> properties to the property values. Target <typeparamref name="T"/>
        /// instance is accepted as the parameter to the returned delegate <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="TypeConvertedValueExpressionAttribute"/> instances, or
        /// <c>null</c> to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// This function is useful for generating a delegate to a compiled function that will update objects
        /// of type <typeparamref name="T"/> where properties of the type of have been decorated with
        /// <see cref="TypeConvertedValueExpressionAttribute"/> attributes. The updated object will automatically
        /// have applied any defined update values as specified by the encountered attributes.
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in an existing model.
        /// </note>
        /// </para>
        /// <para>
        /// This method is the inverse call for <see cref="UpdateExpressions"/>. Internally the method simply calls
        /// <see cref="UpdateInstanceForType{T}"/> for type <see cref="TypeConvertedValueExpressionAttribute"/>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        public static Action<T> UpdateProperties(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null)
        {
            return UpdateInstanceForType<TypeConvertedValueExpressionAttribute>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="IValueExpressionScope{T}"/> object parameter applying any
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
        /// parameter must be derived from <see cref="IValueExpressionScope{T}"/>. Any public fields,
        /// methods or properties defined in the derived class will be automatically accessible from the
        /// expressions declared in the <see cref="DefaultValueExpressionAttribute"/> attributes. By default,
        /// the expressions will have access to the current <typeparamref name="T"/> instance by referencing the
        /// <c>this</c> keyword, which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>.
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in a newly created model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        public static Func<TExpressionScope, T> CreateInstance<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : IValueExpressionScope<T>
        {
            return CreateInstanceForType<DefaultValueExpressionAttribute, TExpressionScope>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will create new instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="IValueExpressionScope{T}"/> object parameter applying any
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
        /// parameter must be derived from <see cref="IValueExpressionScope{T}"/>. Any public fields,
        /// methods or properties defined in the derived class will be automatically accessible from the
        /// expressions declared in the <typeparamref name="TValueExpressionAttribute"/> attributes. By default,
        /// the expressions will have access to the current <typeparamref name="T"/> instance by referencing the
        /// <c>this</c> keyword, which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>.
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in a newly created model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="IValueExpressionAttribute"/> parameter type.</typeparam>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Func<TExpressionScope, T> CreateInstanceForType<TValueExpressionAttribute, TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute where TExpressionScope : IValueExpressionScope<T>
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

            // Assign new instance to "Instance" property of scope parameter
            MethodInfo setInstance = typeof(TExpressionScope).GetProperty("Instance").SetMethod;
            expressions.Add(Expression.Call(scopeParameter, setInstance, newInstance));

            // Find any defined value attributes for properties and assign them to new instance
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
                    string expression = null;

                    try
                    {
                        expressions.Add(AssignParsedValueExpression(valueExpressionAttribute, typeRegistry, property, scopeParameter, newInstance, out expression));
                    }
                    catch (EvaluationOrderException ex)
                    {
                        // Need to wrap exceptions in order to keep original call stack
                        return scope => { throw new InvalidOperationException(ex.Message, ex); };
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing \"{typeof(TValueExpressionAttribute).Name}\" for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message} for expression \"{expression ?? "undefined"}\"", property.Name, ex); };
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
        /// contextual <see cref="IValueExpressionScope{T}"/> object parameter applying any specified
        /// <see cref="UpdateValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="IValueExpressionScope{T}.Instance"/> property prior to call.
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
        /// <see cref="IValueExpressionScope{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <see cref="UpdateValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>.
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in an existing model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with update expression values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        public static Action<TExpressionScope> UpdateInstance<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : IValueExpressionScope<T>
        {
            return UpdateInstanceForType<UpdateValueExpressionAttribute, TExpressionScope>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will update an instance of type <typeparamref name="T"/> accepting a
        /// contextual <see cref="IValueExpressionScope{T}"/> object parameter applying any specified
        /// <typeparamref name="TValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="IValueExpressionScope{T}.Instance"/> property prior to call.
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
        /// <see cref="IValueExpressionScope{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <typeparamref name="TValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>.
        /// <note type="note">
        /// This function will assign evaluated expression values to properties in an existing model.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will update <typeparamref name="T"/> instances with expression values applied.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="IValueExpressionAttribute"/> parameter type.</typeparam>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Action<TExpressionScope> UpdateInstanceForType<TValueExpressionAttribute, TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute where TExpressionScope : IValueExpressionScope<T>
        {
            if ((object)properties == null)
                properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead && property.CanWrite);

            List<Expression> expressions = new List<Expression>();
            ParameterExpression instance = Expression.Variable(typeof(T));
            ParameterExpression scopeParameter = Expression.Parameter(typeof(TExpressionScope));
            TValueExpressionAttribute valueExpressionAttribute;

            // Sort properties by any specified evaluation order
            properties = properties.OrderBy(property => property.TryGetAttribute(out valueExpressionAttribute) ? valueExpressionAttribute.EvaluationOrder : 0);

            // Get "Instance" property of scope parameter and assign to local variable
            MethodInfo getInstance = typeof(TExpressionScope).GetProperty("Instance").GetMethod;
            expressions.Add(Expression.Assign(instance, Expression.Call(scopeParameter, getInstance)));

            // Find any defined value attributes for properties and assign them to instance
            foreach (PropertyInfo property in properties)
            {
                if (property.TryGetAttribute(out valueExpressionAttribute))
                {
                    string expression = null;

                    try
                    {
                        expressions.Add(AssignParsedValueExpression(valueExpressionAttribute, typeRegistry, property, scopeParameter, instance, out expression));
                    }
                    catch (EvaluationOrderException ex)
                    {
                        // Need to wrap exceptions in order to keep original call stack
                        return scope => { throw new InvalidOperationException(ex.Message, ex); };
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing \"{typeof(TValueExpressionAttribute).Name}\" for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message} for expression \"{expression ?? "undefined"}\"", property.Name, ex); };
                    }
                }
            }

            // Return a delegate to compiled function block
            return Expression.Lambda<Action<TExpressionScope>>(Expression.Block(new[] { instance }, expressions), scopeParameter).Compile();
        }

        /// <summary>
        /// Generates a delegate that will execute expression assignments on an instance of type <typeparamref name="T"/>
        /// accepting a contextual <see cref="IValueExpressionScope{T}"/> object parameter where expressions
        /// are <see cref="TypeConvertedValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="IValueExpressionScope{T}.Instance"/> property prior to call.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <see cref="TypeConvertedValueExpressionAttribute"/> instances,
        /// or <c>null</c> to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will execute expression
        /// assignments on objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <see cref="TypeConvertedValueExpressionAttribute"/> attributes. The generated delegate takes a parameter
        /// to a contextual object useful for providing extra runtime data to the expressions defined in attributes
        /// of type <see cref="TypeConvertedValueExpressionAttribute"/>; the contextual parameter must be derived
        /// from <see cref="IValueExpressionScope{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <see cref="TypeConvertedValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>. Note that the expression in
        /// the <see cref="TypeConvertedValueExpressionAttribute"/> attribute is expected to evaluate to a property
        /// such that it can be assigned the target type <typeparamref name="T"/> property value.
        /// <note type="note">
        /// This function will assign current modeled property values back to expressions, this is often useful
        /// when a model is being synchronized to an external source, e.g., user interface elements.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will execute expression assignments on <typeparamref name="T"/> instances.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        public static Action<TExpressionScope> UpdateExpressions<TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TExpressionScope : IValueExpressionScope<T>
        {
            return UpdateExpressionsForType<TypeConvertedValueExpressionAttribute, TExpressionScope>(properties, typeRegistry);
        }

        /// <summary>
        /// Generates a delegate that will execute expression assignments on an instance of type <typeparamref name="T"/>
        /// accepting a contextual <see cref="IValueExpressionScope{T}"/> object parameter where expressions
        /// are <typeparamref name="TValueExpressionAttribute"/> instances that are declared on the type
        /// <typeparamref name="T"/> properties. Target <typeparamref name="T"/> instance needs to be
        /// assigned to the <see cref="IValueExpressionScope{T}.Instance"/> property prior to call.
        /// </summary>
        /// <param name="properties">Specific properties to target, or <c>null</c> to target all properties.</param>
        /// <param name="typeRegistry">
        /// Type registry to use when parsing <typeparamref name="TValueExpressionAttribute"/> instances, or <c>null</c>
        /// to use <see cref="ValueExpressionParser.DefaultTypeRegistry"/>.
        /// </param>
        /// <remarks>
        /// This function is useful for generating a delegate to a compiled function that will execute expression
        /// assignments on objects of type <typeparamref name="T"/> where properties of the type of have been decorated
        /// with <typeparamref name="TValueExpressionAttribute"/> attributes. The generated delegate takes a parameter
        /// to a contextual object useful for providing extra runtime data to the expressions defined in attributes
        /// of type <typeparamref name="TValueExpressionAttribute"/>; the contextual parameter must be derived from
        /// <see cref="IValueExpressionScope{T}"/>. Any public fields, methods or properties defined in the
        /// derived class will be automatically accessible from the expressions declared in the
        /// <typeparamref name="TValueExpressionAttribute"/> attributes. By default, the expressions will have
        /// access to the current <typeparamref name="T"/> instance by referencing the <c>this</c> keyword,
        /// which is an alias to <see cref="IValueExpressionScope{T}.Instance"/>. Note that the expression in
        /// the <typeparamref name="TValueExpressionAttribute"/> attribute is expected to evaluate to a property
        /// such that it can be assigned the target type <typeparamref name="T"/> property value.
        /// <note type="note">
        /// This function will assign current modeled property values back to expressions, this is often useful
        /// when a model is being synchronized to an external source, e.g., user interface elements.
        /// </note>
        /// </remarks>
        /// <returns>
        /// Generated delegate that will execute expression assignments on <typeparamref name="T"/> instances.
        /// </returns>
        /// <typeparam name="TValueExpressionAttribute"><see cref="IValueExpressionAttribute"/> parameter type.</typeparam>
        /// <typeparam name="TExpressionScope"><see cref="IValueExpressionScope{T}"/> parameter type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Action<TExpressionScope> UpdateExpressionsForType<TValueExpressionAttribute, TExpressionScope>(IEnumerable<PropertyInfo> properties = null, TypeRegistry typeRegistry = null) where TValueExpressionAttribute : Attribute, IValueExpressionAttribute where TExpressionScope : IValueExpressionScope<T>
        {
            if ((object)properties == null)
                properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead);

            List<Expression> expressions = new List<Expression>();
            ParameterExpression scopeParameter = Expression.Parameter(typeof(TExpressionScope));
            TValueExpressionAttribute valueExpressionAttribute;

            // Sort properties by any specified evaluation order
            properties = properties.OrderBy(property => property.TryGetAttribute(out valueExpressionAttribute) ? valueExpressionAttribute.EvaluationOrder : 0);

            // Find any defined expression attributes for properties and execute them
            foreach (PropertyInfo property in properties)
            {
                if (property.TryGetAttribute(out valueExpressionAttribute))
                {
                    string expression = null;

                    try
                    {
                        // Derive left assignment operand from raw expression, i.e., not from GetPropertyUpdateValue:
                        string leftOperand = DeriveExpression(valueExpressionAttribute.Expression, valueExpressionAttribute, property);
                        string rightOperand = DeriveExpression(valueExpressionAttribute.GetExpressionUpdateValue(property), valueExpressionAttribute, property);

                        // Parse expression update expression
                        expression = $"{leftOperand} = {rightOperand}";
                        ValueExpressionParser expressionParser = new ValueExpressionParser(expression);
                        expressionParser.Parse(scopeParameter, typeRegistry, true);

                        if ((object)expressionParser.Expression == null)
                            throw new InvalidOperationException("Failed to compile");

                        expressions.Add(expressionParser.Expression);
                    }
                    catch (EvaluationOrderException ex)
                    {
                        // Need to wrap exceptions in order to keep original call stack
                        return scope => { throw new InvalidOperationException(ex.Message, ex); };
                    }
                    catch (Exception ex)
                    {
                        return scope => { throw new ArgumentException($"Error parsing \"{typeof(TValueExpressionAttribute).Name}\" for property \"{typeof(T).FullName}.{property.Name}\": {ex.Message} for expression \"{expression ?? "undefined"}\"", property.Name, ex); };
                    }
                }
            }

            // Return a delegate to compiled function block
            if (expressions.Count > 0)
                return Expression.Lambda<Action<TExpressionScope>>(Expression.Block(expressions), scopeParameter).Compile();

            return scope => { };
        }

        private static Expression AssignParsedValueExpression(IValueExpressionAttribute valueExpressionAttribute, TypeRegistry typeRegistry, PropertyInfo property, ParameterExpression scopeParameter, ParameterExpression instance, out string expression)
        {
            // Parse value expression
            expression = DeriveExpression(null, valueExpressionAttribute, property);
            ValueExpressionParser expressionParser = new ValueExpressionParser(expression);
            expressionParser.Parse(scopeParameter, typeRegistry);

            if ((object)expressionParser.Expression == null)
                throw new InvalidOperationException("Failed to compile");

            UnaryExpression getParsedValue = Expression.Convert(expressionParser.Expression, property.PropertyType);

            if (valueExpressionAttribute.Cached)
            {
                ConstantExpression propertyInfo = Expression.Constant(property, typeof(PropertyInfo));
                ParameterExpression parsedValue = Expression.Variable(property.PropertyType);
                ParameterExpression cachedValue = Expression.Variable(typeof(Tuple<bool, object>));

                MethodInfo getTupleItem1 = typeof(Tuple<bool, object>).GetProperty("Item1").GetMethod;
                MethodInfo getTupleItem2 = typeof(Tuple<bool, object>).GetProperty("Item2").GetMethod;

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

        private static string DeriveExpression(string expression, IValueExpressionAttribute valueExpressionAttribute, PropertyInfo property)
        {
            return ValueExpressionParser.DeriveExpression(expression ?? valueExpressionAttribute.GetPropertyUpdateValue(property), valueExpressionAttribute, property, typeof(T).FullName);
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
