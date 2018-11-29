//******************************************************************************************************
//  FilterExpressionParser.cpp - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "FilterExpressionParser.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;
using namespace antlr4;

FilterExpressionParser::FilterExpressionParser()
{
}


FilterExpressionParser::~FilterExpressionParser()
{
}

/*
    parse
     : ( filterExpressionStatementList | error ) EOF
     ;
 */
void FilterExpressionParser::exitParse(FilterExpressionSyntaxParser::ParseContext* context)
{
}

/*
    expression
     : literalValue
     | columnName
     | unaryOperator expression
     | expression ( '*' | '/' | '%' ) expression
     | expression ( '+' | '-' ) expression
     | expression ( '<<' | '>>' | '&' | '|' ) expression
     | expression ( '<' | '<=' | '>' | '>=' ) expression
     | expression ( '=' | '==' | '!=' | '<>' ) expression
     | expression K_IS K_NOT? K_NULL
     | expression K_NOT? K_IN ( '(' ( expression ( ',' expression )* )? ')' )
     | expression K_NOT? K_LIKE expression
     | expression K_AND expression
     | expression K_OR expression
     | functionName '(' ( expression ( ',' expression )* | '*' )? ')'
     | '(' expression ')'
     ;
 */
void FilterExpressionParser::exitExpression(FilterExpressionSyntaxParser::ExpressionContext* context)
{
    const ExpressionPtr expression = NewSharedPtr<Expression>();

    expression->Context = context;

    auto iterator = m_expressions.find(context->literalValue());

    if (iterator != m_expressions.end())
    {
        const ExpressionPtr literalValue = iterator->second;

        expression->Type = literalValue->Type;
        expression->Value = literalValue->Value;

        m_expressions.insert(pair<ParserRuleContext*, ExpressionPtr>(context, expression));
        return;
    }

    iterator = m_expressions.find(context->columnName());

    if (iterator != m_expressions.end())
    {
        const ExpressionPtr columnName = iterator->second;

        expression->Type = columnName->Type;
        expression->Value = columnName->Value;

        m_expressions.insert(pair<ParserRuleContext*, ExpressionPtr>(context, expression));
        return;
    }

    iterator = m_expressions.find(context->unaryOperator());

    if (iterator != m_expressions.end())
    {
        const ExpressionPtr unaryOperator = iterator->second;
        return;
    }

    iterator = m_expressions.find(context->functionName());

    if (iterator != m_expressions.end())
    {
        const ExpressionPtr functionName = iterator->second;
        return;
    }
}

/*
    literalValue
     : NUMERIC_LITERAL
     | STRING_LITERAL
     | DATETIME_LITERAL
     | K_NULL
     ;
 */
void FilterExpressionParser::exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext* context)
{
}

/*
    columnName
     : IDENTIFIER
     ;
 */
void FilterExpressionParser::exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext* context)
{
}

/*
    unaryOperator
     : '-'
     | '+'
     | '~'
     | K_NOT
     ;
 */
void FilterExpressionParser::exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext* context)
{
}

/*
    functionName
     : K_CONVERT
     | K_IIF
     | K_LEN
     | K_ISNULL
     | K_REGEXP
     | K_SUBSTRING
     | K_TRIM
     ;
 */
void FilterExpressionParser::exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext* context)
{
}
