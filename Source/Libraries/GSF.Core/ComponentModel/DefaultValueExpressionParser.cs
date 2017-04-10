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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ExpressionEvaluator;
using GSF.Identity;
using GSF.Reflection;

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
        private static readonly TypeRegistry s_typeRegistry;
        private static readonly Regex s_findThisKeywords;

        // Static Constructor
        static DefaultValueExpressionParser()
        {
            // Setup default type registry for parsing default value expression attributes
            s_typeRegistry = new TypeRegistry();
            s_typeRegistry.RegisterDefaultTypes();
            s_typeRegistry.RegisterType<Guid>();
            s_typeRegistry.RegisterType<UserInfo>();
            s_typeRegistry.RegisterType("Common", typeof(Common));

            // Define a regular expression to find "this" expressions
            s_findThisKeywords = new Regex(@"(^this(?=[^\w]))|((?<=[^\w])this(?=[^\w]))|(^this$)", RegexOptions.Compiled | RegexOptions.Multiline);
        }

        // Static Properties

        /// <summary>
        /// Gets the default <see cref="TypeRegistry"/> instance used for evaluating <see cref="DefaultValueExpressionAttribute"/> instances.
        /// </summary>
        public static TypeRegistry DefaultTypeRegistry => s_typeRegistry;

        // Static Methods

        /// <summary>
        /// Replaces references to "this" keyword with a specified <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="expression">Expression to search.</param>
        /// <param name="fieldName">Replacement value for "this" keyword usages.</param>
        /// <returns>An expression with "this" keyword replaced with specified <paramref name="fieldName"/>.</returns>
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
        public void Parse(ParameterExpression scope, TypeRegistry typeRegistry = null)
        {
            if ((object)scope == null)
                throw new ArgumentNullException(nameof(scope));

            if ((object)typeRegistry != null)
                TypeRegistry = typeRegistry;

            BuildTree(scope);
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
        /// <returns>
        /// Generated delegate that will create new <typeparamref name="T"/> instances with default values applied.
        /// </returns>
        /// <typeparam name="TExpressionScope"><see cref="DefaultValueExpressionScopeBase{T}"/> parameter type.</typeparam>
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

            // Create new instance and assign to local variable
            expressions.Add(Expression.Assign(newInstance, Expression.New(constructor)));

            // Assign new instance to "_instance" field of current scope parameter
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
                        expressions.Add(Expression.Call(newInstance, property.SetMethod, Expression.Convert(expressionParser.Expression, property.PropertyType)));
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

        #endregion
    }
}
