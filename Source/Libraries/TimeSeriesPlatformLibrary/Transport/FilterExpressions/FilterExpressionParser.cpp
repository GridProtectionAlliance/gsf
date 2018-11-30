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
#include "tree/ParseTreeWalker.h"
#include "../../Common/Convert.h"

#include <boost/algorithm/string.hpp>

using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;
using namespace antlr4;
using namespace antlr4::tree;
using namespace boost;

// Mapped type for boost UUID (ANTLR4 also defines a Guid type)
typedef GSF::TimeSeries::Guid guid;

static guid ParseGuidLiteral(string guidLiteral)
{
    // Remove any quotes from GUID (boost currently only handles optional braces),
    // ANTLR grammar already ensures GUID starting with quote also ends with one
    if (guidLiteral.front() == '\'')
    {
        guidLiteral.erase(0, 1);
        guidLiteral.erase(guidLiteral.size() - 1);
    }

    return ToGuid(guidLiteral.c_str());
}

time_t ParseDateTimeLiteral(string time)
{
    // Remove any surrounding '#' symbols from date/time, ANTLR grammar already
    // ensures date/time starting with '#' symbol will also end with one
    if (time.front() == '#')
    {
        time.erase(0, 1);
        time.erase(time.size() - 1);
    }

    // Need a "generic" ParseTimestamp function...
    //    istringstream in { time };
    //    sys_seconds timestamp;
    //
    //    // Try parsing several date-time formats formatted timestamp string
    //    // using the Hinnant date library: https://github.com/HowardHinnant/date
    //
    //    // Need to try several date/time format variations per:
    //    // https://howardhinnant.github.io/date/date.html#from_stream_formatting
    //    /*
    //        istringstream in { "2018-01-12 12:05:14" };
    //        sys_seconds timestamp;
    //    
    //        in >> parse("%Y-%m-%dT%T%z", timestamp);
    //
    //        if (bool(in))    
    //        {
    //            std::cout << "Parsed XML time: " << system_clock::to_time_t(timestamp) << "\n";
    //        }
    //        else
    //        {
    //            in.clear();
    //            in.str("2018-01-12 12:05:14");
    //            in >> parse("%F%n %T", timestamp);
    //    
    //            if (bool(in))    
    //            {
    //                std::cout << "Parsed std time: " << system_clock::to_time_t(timestamp) << "\n";
    //            }
    //            else
    //            {
    //                std::cout << "Failed to parse time...\n";
    //            }
    //        }
    //     */
    //
    //    // Try XML format, e.g.: 2018-03-14T19:23:11.665-04:00:
    //    in >> parse("%Y-%m-%dT%T%z", timestamp);
    
    return ParseXMLTimestamp(time.c_str());
}

FilterExpressionParser::FilterExpressionParser(const string& filterExpression) :
    m_inputStream(filterExpression),
    m_lexer(nullptr),
    m_tokens(nullptr),
    m_parser(nullptr),
    m_dataset(nullptr),
    m_primaryMeasurementTableName("ActiveMeasurements"),
    m_signalIDColumnName("SignalID"),
    m_measurementKeyColumnName("ID"),
    m_pointTagColumnName("PointTag")
{
    m_lexer = new FilterExpressionSyntaxLexer(&m_inputStream);
    m_tokens = new CommonTokenStream(m_lexer);
    m_parser = new FilterExpressionSyntaxParser(m_tokens);
}

FilterExpressionParser::~FilterExpressionParser()
{
    delete m_lexer;
    delete m_tokens;
    delete m_parser;
}

bool FilterExpressionParser::TryGetExpr(const ParserRuleContext* context, ExpressionPtr& expression) const
{
    return TryGetValue<const ParserRuleContext*, ExpressionPtr>(m_expressions, context, expression, nullptr);
}

void FilterExpressionParser::AddExpr(const ParserRuleContext* context, const ExpressionPtr& expression)
{
    m_expressions.insert(pair<const ParserRuleContext*, ExpressionPtr>(context, expression));
}

void FilterExpressionParser::MapMeasurement(const DataTablePtr& measurements, const int32_t signalIDColumnIndex, const string& columnName, const string& mappingValue)
{
    const auto column = measurements->Column(columnName);

    if (column == nullptr)
        return;

    const int32_t columnIndex = column->Index();

    for (int32_t i = 0; i < measurements->RowCount(); i++)
    {
        const auto row = measurements->Row(i);

        if (row != nullptr)
        {
            if (iequals(mappingValue, row->ValueAsString(columnIndex)))
            {
                m_signalIDs.push_back(row->ValueAsGuid(signalIDColumnIndex));
                return;
            }
        }
    }
}

const DataSetPtr& FilterExpressionParser::CurrentDataSet() const
{
    return m_dataset;
}

void FilterExpressionParser::AssignDataSet(const DataSetPtr& dataset)
{
    m_dataset = dataset;
}

const string& FilterExpressionParser::GetPrimaryMeasurementTableName() const
{
    return m_primaryMeasurementTableName;
}

void FilterExpressionParser::SetPrimaryMeasurementTableName(const string& tableName)
{
    m_primaryMeasurementTableName = tableName;
}

const string& FilterExpressionParser::GetSignalIDColumnName() const
{
    return m_signalIDColumnName;
}

void FilterExpressionParser::SetSignalIDColumnName(const string& columnName)
{
    m_signalIDColumnName = columnName;
}

const string& FilterExpressionParser::GetMeasurementKeyColumnName() const
{
    return m_measurementKeyColumnName;
}

void FilterExpressionParser::SetMeasurementKeyColumnName(const string& columnName)
{
    m_measurementKeyColumnName = columnName;
}

const string& FilterExpressionParser::GetPointTagColumnName() const
{
    return m_pointTagColumnName;
}

void FilterExpressionParser::SetPointTagColumnName(const string& columnName)
{
    m_pointTagColumnName = columnName;
}

void FilterExpressionParser::Evaluate()
{
    if (m_dataset == nullptr)
        throw RuntimeException("Dataset is undefined");

    m_signalIDs.clear();
    m_expressions.clear();

    ParseTreeWalker walker;
    walker.walk(this, m_parser->parse());
}

const vector<guid>& FilterExpressionParser::FilteredSignalIDs() const
{
    return m_signalIDs;
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
    identifierStatement
     : GUID_LITERAL
     | MEASUREMENT_KEY_LITERAL
     | POINT_TAG_LITERAL
     ;
 */
void FilterExpressionParser::exitIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext* context)
{
    if (context->GUID_LITERAL())
    {
        m_signalIDs.push_back(ParseGuidLiteral(context->GUID_LITERAL()->getText()));
        return;
    }

    const auto measurements = m_dataset->Table(m_primaryMeasurementTableName);

    if (measurements == nullptr)
        return;

    const auto signalIDColumn = measurements->Column(m_signalIDColumnName);

    if (signalIDColumn == nullptr)
        return;

    const int32_t signalIDColumnIndex = signalIDColumn->Index();

    if (context->MEASUREMENT_KEY_LITERAL())
    {
        MapMeasurement(measurements, signalIDColumnIndex, m_measurementKeyColumnName, context->MEASUREMENT_KEY_LITERAL()->getText());
        return;
    }

    if (context->POINT_TAG_LITERAL())
        MapMeasurement(measurements, signalIDColumnIndex, m_pointTagColumnName, context->POINT_TAG_LITERAL()->getText());
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
    const ExpressionPtr result = NewSharedPtr<Expression>();
    ExpressionPtr value;

    result->Context = context;

    if (TryGetExpr(context->literalValue(), value))
    {
        result->Value = value->Value;
        result->Type = value->Type;
        AddExpr(context, result);
        return;
    }

    if (TryGetExpr(context->columnName(), value))
    {
        result->Value = value->Value;
        result->Type = value->Type;
        AddExpr(context, result);
        return;
    }

    const auto unaryOperator = context->unaryOperator();

    if (unaryOperator != nullptr && TryGetExpr(context->expression(0), value))
    {

        return;
    }

    if (context->expression().size() == 2)
    {
        
    }

    if (TryGetExpr(context->functionName(), value))
    {
        return;
    }
}

/*
    literalValue
     : INTEGER_LITERAL
     | NUMERIC_LITERAL
     | STRING_LITERAL
     | DATETIME_LITERAL
     | GUID_LITERAL
     | BOOLEAN_LITERAL
     | K_NULL
     ;
 */
void FilterExpressionParser::exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext* context)
{
    const ExpressionPtr result = NewSharedPtr<Expression>();

    result->Context = context;

    if (context->INTEGER_LITERAL())
    {
        const double_t value = stod(context->INTEGER_LITERAL()->getText());

        if (value > Int64::MaxValue)
        {
            result->Value = value;
            result->Type = DataType::Double;
        }
        else if (value > Int32::MaxValue)
        {
            result->Value = static_cast<int64_t>(value);
            result->Type = DataType::Int64;
        }
        else
        {
            result->Value = static_cast<int32_t>(value);
            result->Type = DataType::Int32;
        }
        
        AddExpr(context, result);
        return;
    }

    if (context->NUMERIC_LITERAL())
    {
        const string literal = context->NUMERIC_LITERAL()->getText();
        
        if (literal.find('e') || literal.find('E'))
        {
            // Real literals using scientific notation are parsed as double
            result->Value = stod(literal);
            result->Type = DataType::Double;
        }
        else
        {
            // Real literals without scientific notation are parsed as decimal, if
            // the number fails to parse as decimal, then it is parsed as a double
            try
            {
                result->Value = decimal_t(literal);
                result->Type = DataType::Decimal;
            }
            catch (const std::runtime_error&)
            {
                result->Value = stod(literal);
                result->Type = DataType::Double;
            }
        }

        AddExpr(context, result);
        return;
    }

    if (context->STRING_LITERAL())
    {
        result->Value = context->STRING_LITERAL()->getText();
        result->Type = DataType::String;
        AddExpr(context, result);
        return;
    }

    if (context->DATETIME_LITERAL())
    {
        result->Value = ParseDateTimeLiteral(context->DATETIME_LITERAL()->getText());
        result->Type = DataType::DateTime;
        AddExpr(context, result);
        return;
    }

    if (context->GUID_LITERAL())
    {
        result->Value = ParseGuidLiteral(context->GUID_LITERAL()->getText());
        result->Type = DataType::Guid;
        AddExpr(context, result);
        return;
    }

    if (context->BOOLEAN_LITERAL())
    {        
        result->Value = iequals(context->BOOLEAN_LITERAL()->getText(), "true");
        result->Type = DataType::Boolean;
        AddExpr(context, result);
        return;
    }

    if (context->K_NULL())
    {
        result->Value = nullptr;
        result->Type = DataType::Null;
        AddExpr(context, result);
    }
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
     : K_COALESCE
     | K_CONVERT
     | K_IIF
     | K_LEN
     | K_ISNULL
     | K_REGEXP
     | K_SUBSTR
     | K_SUBSTRING
     | K_TRIM
     ;
 */
void FilterExpressionParser::exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext* context)
{
}
