//******************************************************************************************************
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

public class AttributeFinder<TSyntaxNode> : ISyntaxReceiver where TSyntaxNode : BaseTypeDeclarationSyntax
{
    private readonly string m_attributeName;
    private readonly string m_attributeFullName;

    public AttributeFinder(string attributeName)
    {
        m_attributeName = attributeName.Split('.').Last();

        if (m_attributeName.EndsWith("Attribute", StringComparison.Ordinal))
            m_attributeName = m_attributeName[..^9];

        m_attributeFullName = attributeName;
    }

    public List<AttributeSyntax> CandidateAttributes = [];

    public bool HasCandidateAttributes => CandidateAttributes.Count > 0;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TSyntaxNode declaration)
            return;

        if (declaration.AttributeLists.Count == 0)
            return;

        bool isCandidateMatch(string attributeCodeName)
        {
            return attributeCodeName.EndsWith(m_attributeName, StringComparison.Ordinal) ||
                   attributeCodeName.EndsWith($"{m_attributeName}Attribute", StringComparison.Ordinal);
        }

        CandidateAttributes.AddRange(declaration.AttributeLists
            .SelectMany(syntax => syntax.Attributes)
            .Where(syntax => isCandidateMatch(syntax.Name.NormalizeWhitespace().ToFullString()))
            .ToArray());
    }

    // Check if attribute matches the fully qualified name of the attribute, only then is candidate considered an exact match
    public bool IsMatch(Compilation compilation)
    {
        INamedTypeSymbol? attributeSymbol = compilation.GetTypeByMetadataName(m_attributeFullName);

        if (attributeSymbol == null)
            return false;

        string attributeSymbolFullName = attributeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (attributeSymbolFullName.StartsWith("global::") && !m_attributeFullName.StartsWith("global::"))
            attributeSymbolFullName = attributeSymbolFullName[8..];

        return string.Equals(m_attributeFullName, attributeSymbolFullName, StringComparison.Ordinal);
    }
}