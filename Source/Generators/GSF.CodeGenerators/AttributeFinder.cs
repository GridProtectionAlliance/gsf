﻿//******************************************************************************************************
//  AttributeFinder.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  07/08/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GSF.CodeGenerators;

public sealed class AttributeFinder<TDeclarationSyntax>(string attributeFullName) : ISyntaxContextReceiver where TDeclarationSyntax : BaseTypeDeclarationSyntax
{
    public List<AttributeSyntax> Attributes = [];

    public bool HasAttributes => Attributes.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (DeclarationHasAttributes(context.Node))
            Attributes.AddRange(GetSemanticTargetForGeneration(context));
    }

    private static bool DeclarationHasAttributes(SyntaxNode node)
    {
        return node is TDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private IEnumerable<AttributeSyntax> GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        List<AttributeSyntax> attributes = [];
        TDeclarationSyntax declarationSyntax = (TDeclarationSyntax)context.Node;

        foreach (AttributeSyntax attributeSyntax in declarationSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

            if (string.Equals(attributeFullName, attributeContainingTypeSymbol.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        return attributes;
    }
}

public static class AttributeSyntaxExtensions
{
    public static string[] GetArgumentValues(this AttributeSyntax attribute)
    {
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList?.Arguments ?? default;
        return arguments.Select(argument => argument.Expression.NormalizeWhitespace().ToFullString()).ToArray();
    }
}