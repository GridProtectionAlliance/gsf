//******************************************************************************************************
//  FilterExpressionParser.h - Gbtc
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

#ifndef __FILTER_EXPRESSION_PARSER_H
#define __FILTER_EXPRESSION_PARSER_H

//#include "../../Common/CommonTypes.h"
#include "FilterExpressionSyntaxBaseListener.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{

struct ExpressionType
{
    static const uint8_t Boolean = 0;
    static const uint8_t Numeric = 1;
    static const uint8_t String = 2;
    static const uint8_t DateTime = 3;
    static const uint8_t Null = 255;
};

struct Expression
{
    ExpressionType Type;
    antlr4::ParserRuleContext Context;
    antlrcpp::Any Value;
};

class FilterExpressionParser : public FilterExpressionSyntaxBaseListener
{
private:
    //std::map<antlr4::tree::ParseTree, Expression> m_expressions;

public:
    FilterExpressionParser();
    ~FilterExpressionParser();

    void exitParse(FilterExpressionSyntaxParser::ParseContext*) override;
    void exitError(FilterExpressionSyntaxParser::ErrorContext*) override;
    void exitExpression(FilterExpressionSyntaxParser::ExpressionContext*) override;
    void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext*) override;
    void exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext*) override;
    void exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext*) override;
    void exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext*) override;
};

}}}

#endif