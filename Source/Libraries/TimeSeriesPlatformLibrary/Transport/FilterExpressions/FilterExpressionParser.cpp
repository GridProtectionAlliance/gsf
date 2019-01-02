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

using namespace std;
using namespace GSF::Data;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;
using namespace antlr4;
using namespace antlr4::tree;
using namespace boost;

// Mapped type for TimeSeries Guid (ANTLR4 also defines a Guid type)
typedef GSF::TimeSeries::Guid guid;

static string ParseStringLiteral(string stringLiteral)  // NOLINT
{
    // Remove any surrounding quotes from string, ANTLR grammar already
    // ensures strings starting with quote also ends with one
    if (stringLiteral.front() == '\'')
    {
        stringLiteral.erase(0, 1);
        stringLiteral.erase(stringLiteral.size() - 1);
    }

    return stringLiteral;
}

static guid ParseGuidLiteral(string guidLiteral)
{
    // Remove any quotes from GUID (boost currently only handles optional braces),
    // ANTLR grammar already ensures GUID starting with quote also ends with one
    if (guidLiteral.front() == '\'')
    {
        guidLiteral.erase(0, 1);
        guidLiteral.erase(guidLiteral.size() - 1);
    }

    return ParseGuid(guidLiteral.c_str());
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

    return ParseTimestamp(time.c_str());
}

FilterExpressionParserException::FilterExpressionParserException(string message) noexcept :
    m_message(std::move(message))
{
}

const char* FilterExpressionParserException::what() const noexcept
{
    return &m_message[0];
}

FilterExpressionParser::FilterExpressionParser(const string& filterExpression) :
    m_inputStream(filterExpression),
    m_dataset(nullptr),
    m_primaryMeasurementTableName("ActiveMeasurements")
{
    m_lexer = new FilterExpressionSyntaxLexer(&m_inputStream);
    m_tokens = new CommonTokenStream(m_lexer);
    m_parser = new FilterExpressionSyntaxParser(m_tokens);

    MeasurementTableIDFieldsPtr measurementTableIDFields = NewSharedPtr<MeasurementTableIDFields>();

    measurementTableIDFields->SignalIDFieldName = "SignalID";
    measurementTableIDFields->MeasurementKeyFieldName = "ID";
    measurementTableIDFields->PointTagFieldName = "PointTag";

    SetMeasurementTableIDFields(m_primaryMeasurementTableName, measurementTableIDFields);
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
    // Track expression in parser rule context map
    m_expressions.insert(pair<const ParserRuleContext*, ExpressionPtr>(context, expression));

    // Update active expression tree root
    m_activeExpressionTree->Root = expression;
}

void FilterExpressionParser::MapMeasurement(const DataTablePtr& measurements, const int32_t signalIDColumnIndex, const string& columnName, const string& mappingValue)
{
    const DataColumnPtr& column = measurements->Column(columnName);

    if (column == nullptr)
        return;

    const int32_t columnIndex = column->Index();

    for (int32_t i = 0; i < measurements->RowCount(); i++)
    {
        const DataRowPtr& row = measurements->Row(i);

        if (row)
        {
            const Nullable<string> field = row->ValueAsString(columnIndex);

            if (field.HasValue() && IsEqual(mappingValue, field.GetValueOrDefault()))
            {
                const Nullable<guid> signalIDField = row->ValueAsGuid(signalIDColumnIndex);

                if (signalIDField.HasValue())
                {
                    const guid& signalID = signalIDField.GetValueOrDefault();

                    if (signalID != Empty::Guid && m_signalIDSet.insert(signalID).second)
                        m_signalIDs.push_back(signalID);
                }

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

MeasurementTableIDFieldsPtr FilterExpressionParser::GetMeasurementTableIDFields(const std::string& measurementTableName) const
{
    MeasurementTableIDFieldsPtr measurementTableFields;
    
    TryGetValue<const std::string, MeasurementTableIDFieldsPtr>(m_measurementTableIDFields, measurementTableName, measurementTableFields, nullptr);

    return measurementTableFields;
}

void FilterExpressionParser::SetMeasurementTableIDFields(const std::string& measurementTableName, const MeasurementTableIDFieldsPtr& measurementTableIDFields)
{
    m_measurementTableIDFields.insert_or_assign(measurementTableName, measurementTableIDFields);
}

const string& FilterExpressionParser::GetPrimaryMeasurementTableName() const
{
    return m_primaryMeasurementTableName;
}

void FilterExpressionParser::SetPrimaryMeasurementTableName(const string& tableName)
{
    m_primaryMeasurementTableName = tableName;
}

template<typename T>
static int32_t CompareValues(Nullable<T> leftNullable, Nullable<T> rightNullable)
{
    if (leftNullable.HasValue() && rightNullable.HasValue())
    {
        const T& leftValue = leftNullable.GetValueOrDefault();
        const T& rightValue = rightNullable.GetValueOrDefault();
        return leftValue < rightValue ? -1 : (leftValue > rightValue ? 1 : 0);
    }

    if (!leftNullable.HasValue() && !rightNullable.HasValue())
        return 0;

    return leftNullable.HasValue() ? 1 : -1;
}

void FilterExpressionParser::Evaluate()
{
    m_signalIDSet.clear();
    m_signalIDs.clear();
    m_expressionTrees.clear();
    m_expressions.clear();

    if (m_dataset == nullptr)
        throw FilterExpressionParserException("Cannot evaluate filter expression, no dataset has been defined");

    // Create parse tree and visit listener methods
    ParseTreeWalker walker;
    const auto parseTree = m_parser->parse();
    walker.walk(this, parseTree);

    // Each filter expression statement will have its own expression tree, evaluate each
    for (size_t x = 0; x < m_expressionTrees.size(); x++)
    {
        const ExpressionTreePtr& expressionTree = m_expressionTrees[x];
        const DataTablePtr& measurements = expressionTree->Measurements();
        const MeasurementTableIDFieldsPtr& measurementTableIDFields = GetMeasurementTableIDFields(measurements->Name());

        if (measurementTableIDFields == nullptr)
            throw FilterExpressionParserException("Failed to find ID fields record for measurement table \"" + measurements->Name() + "\"");

        const DataColumnPtr& signalIDColumn = measurements->Column(measurementTableIDFields->SignalIDFieldName);

        if (signalIDColumn == nullptr)
            throw FilterExpressionParserException("Failed to find signal ID field \"" + measurementTableIDFields->SignalIDFieldName + "\" for measurement table \"" + measurements->Name() + "\"");

        const int32_t signalIDColumnIndex = signalIDColumn->Index();
        vector<DataRowPtr> matchedRows;

        for (int32_t y = 0; y < measurements->RowCount(); y++)
        {
            if (expressionTree->TopLimit > -1 && static_cast<int32_t>(matchedRows.size()) >= expressionTree->TopLimit)
                break;

            const DataRowPtr& row = measurements->Row(y);

            if (row == nullptr)
                continue;

            const ValueExpressionPtr& resultExpression = expressionTree->Evaluate(row);

            // Final expression should have a boolean data type (it's part of a WHERE clause)
            if (resultExpression->ValueType != ExpressionValueType::Boolean)
                throw FilterExpressionParserException("Final expression tree evaluation did not result in a boolean value, result data type is \"" + string(EnumName(resultExpression->ValueType)) + "\"");

            // If final result is Null, i.e., has no value due to Null propagation, treat result as False
            if (resultExpression->ValueAsBoolean())
            {
                Nullable<guid> signalIDField = row->ValueAsGuid(signalIDColumnIndex);

                if (signalIDField.HasValue())
                {
                    const guid& signalID = signalIDField.GetValueOrDefault();

                    if (signalID != Empty::Guid && m_signalIDSet.insert(signalID).second)
                        matchedRows.push_back(row);
                }
            }
        }

        if (matchedRows.empty())
            continue;

        if (!expressionTree->OrderByTerms.empty())
        {
            std::sort(matchedRows.begin(), matchedRows.end(), [expressionTree](const DataRowPtr& leftMatchedRow, const DataRowPtr& rightMatchedRow)
            {
                for (size_t i = 0; i < expressionTree->OrderByTerms.size(); i++)
                {
                    const auto orderByTerm = expressionTree->OrderByTerms[i];
                    const DataColumnPtr& orderByColumn = orderByTerm.first;
                    const int32_t columnIndex = orderByColumn->Index();
                    const bool ascending = orderByTerm.second;
                    const DataRowPtr& leftRow = ascending ? leftMatchedRow : rightMatchedRow;
                    const DataRowPtr& rightRow = ascending ? rightMatchedRow : leftMatchedRow;
                    int32_t result;

                    switch (orderByColumn->Type())
                    {
                        case DataType::String:
                        {
                            auto leftNullable = leftRow->ValueAsString(columnIndex);
                            auto rightNullable = rightRow->ValueAsString(columnIndex);

                            if (leftNullable.HasValue() && rightNullable.HasValue())
                                result = Compare(leftNullable.GetValueOrDefault(), rightNullable.GetValueOrDefault());
                            else if (!leftNullable.HasValue() && !rightNullable.HasValue())
                                result = 0;
                            else
                                result = leftNullable.HasValue() ? 1 : -1;

                            break;
                        }
                        case DataType::Boolean:
                            result = CompareValues(leftRow->ValueAsBoolean(columnIndex), rightRow->ValueAsBoolean(columnIndex));
                            break;
                        case DataType::DateTime:
                            result = CompareValues(leftRow->ValueAsDateTime(columnIndex), rightRow->ValueAsDateTime(columnIndex));
                            break;
                        case DataType::Single:
                            result = CompareValues(leftRow->ValueAsSingle(columnIndex), rightRow->ValueAsSingle(columnIndex));
                            break;
                        case DataType::Double:
                            result = CompareValues(leftRow->ValueAsDouble(columnIndex), rightRow->ValueAsDouble(columnIndex));
                            break;
                        case DataType::Decimal:
                            result = CompareValues(leftRow->ValueAsDecimal(columnIndex), rightRow->ValueAsDecimal(columnIndex));
                            break;
                        case DataType::Guid:
                            result = CompareValues(leftRow->ValueAsGuid(columnIndex), rightRow->ValueAsGuid(columnIndex));
                            break;
                        case DataType::Int8:
                            result = CompareValues(leftRow->ValueAsInt8(columnIndex), rightRow->ValueAsInt8(columnIndex));
                            break;
                        case DataType::Int16:
                            result = CompareValues(leftRow->ValueAsInt16(columnIndex), rightRow->ValueAsInt16(columnIndex));
                            break;
                        case DataType::Int32:
                            result = CompareValues(leftRow->ValueAsInt32(columnIndex), rightRow->ValueAsInt32(columnIndex));
                            break;
                        case DataType::Int64:
                            result = CompareValues(leftRow->ValueAsInt64(columnIndex), rightRow->ValueAsInt64(columnIndex));
                            break;
                        case DataType::UInt8:
                            result = CompareValues(leftRow->ValueAsUInt8(columnIndex), rightRow->ValueAsUInt8(columnIndex));
                            break;
                        case DataType::UInt16:
                            result = CompareValues(leftRow->ValueAsUInt16(columnIndex), rightRow->ValueAsUInt16(columnIndex));
                            break;
                        case DataType::UInt32:
                            result = CompareValues(leftRow->ValueAsUInt32(columnIndex), rightRow->ValueAsUInt32(columnIndex));
                            break;
                        case DataType::UInt64:
                            result = CompareValues(leftRow->ValueAsUInt64(columnIndex), rightRow->ValueAsUInt64(columnIndex));
                            break;
                        default:
                            throw FilterExpressionParserException("Unexpected column data type encountered");
                    }

                    if (result < 0)
                        return true;

                    if (result > 0)
                        return false;

                    // Last compare result was equal, continue sort based on next order-by term
                }

                return false;
            });
        }

        for (size_t i = 0; i < matchedRows.size(); i++)
            m_signalIDs.push_back(matchedRows[i]->ValueAsGuid(signalIDColumnIndex).GetValueOrDefault());
    }
}

const vector<guid>& FilterExpressionParser::FilteredSignalIDs() const
{
    return m_signalIDs;
}

const unordered_set<guid>& FilterExpressionParser::FilteredSignalIDSet() const
{
    return m_signalIDSet;
}

/*
    filterStatement
     : K_FILTER ( K_TOP INTEGER_LITERAL )? tableName K_WHERE expression ( K_ORDER K_BY orderingTerm ( ',' orderingTerm )* )?
     ;

    orderingTerm
     : columnName ( K_ASC | K_DESC )?
     ;
 */
void FilterExpressionParser::enterFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext* context)
{
    // One filter expression can contain multiple filter statements separated by semi-colon,
    // so we track each as an independent expression tree
    m_expressions.clear();

    const string& measurementTableName = context->tableName()->getText();
    const DataTablePtr& measurements = m_dataset->Table(measurementTableName);

    if (measurements == nullptr)
        throw FilterExpressionParserException("Failed to find measurement table \"" + measurementTableName + "\"");

    m_activeExpressionTree = NewSharedPtr<ExpressionTree>(measurements);
    m_expressionTrees.push_back(m_activeExpressionTree);

    if (context->K_TOP() != nullptr)
        m_activeExpressionTree->TopLimit = stoi(context->INTEGER_LITERAL()->getText());

    if (context->K_ORDER() != nullptr && context->K_BY() != nullptr)
    {
        for (size_t i = 0; i < context->orderingTerm().size(); i++)
        {
            auto orderingTermContext = context->orderingTerm(i);
            const string& orderByColumnName = orderingTermContext->columnName()->getText();
            const DataColumnPtr& orderByColumn = measurements->Column(orderByColumnName);

            if (orderByColumn == nullptr)
                throw FilterExpressionParserException("Failed to find order by field \"" + orderByColumnName + "\" for measurement table \"" + measurements->Name() + "\"");

            m_activeExpressionTree->OrderByTerms.emplace_back(
                orderByColumn,
                orderingTermContext->K_DESC() == nullptr
            );
        }
    }
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
        const guid& signalID = ParseGuidLiteral(context->GUID_LITERAL()->getText());
        
        if (signalID != Empty::Guid && m_signalIDSet.insert(signalID).second)
            m_signalIDs.push_back(signalID);

        return;
    }

    const DataTablePtr& measurements = m_dataset->Table(m_primaryMeasurementTableName);

    if (measurements == nullptr)
        return;

    const MeasurementTableIDFieldsPtr& measurementTableIDFields = GetMeasurementTableIDFields(m_primaryMeasurementTableName);

    if (measurementTableIDFields == nullptr)
        return;

    const DataColumnPtr& signalIDColumn = measurements->Column(measurementTableIDFields->SignalIDFieldName);

    if (signalIDColumn == nullptr)
        return;

    const int32_t signalIDColumnIndex = signalIDColumn->Index();

    if (context->MEASUREMENT_KEY_LITERAL())
    {
        MapMeasurement(measurements, signalIDColumnIndex, measurementTableIDFields->MeasurementKeyFieldName, context->MEASUREMENT_KEY_LITERAL()->getText());
        return;
    }

    if (context->POINT_TAG_LITERAL())
        MapMeasurement(measurements, signalIDColumnIndex, measurementTableIDFields->PointTagFieldName, context->POINT_TAG_LITERAL()->getText());
}

/*
    unaryOperator
     : '-'
     | '+'
     | '~'
     | K_NOT
     ;

    functionName
     : K_COALESCE
     | K_CONVERT
     | K_IIF
     | K_ISNULL
     | K_ISREGEXMATCH
     | K_LEN
     | K_REGEXVAL
     | K_SUBSTR
     | K_SUBSTRING
     | K_TRIM
     ;

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
    ExpressionPtr value;

    // Check for literal value expressions
    const auto literalValueContext = context->literalValue();

    if (literalValueContext != nullptr)
    {
        if (TryGetExpr(literalValueContext, value))
        {
            AddExpr(context, value);
            return;
        }
        
        throw FilterExpressionParserException("Failed to find literal value expression \"" + literalValueContext->getText() + "\"");
    }

    // Check for column name expressions
    const auto columnNameContext = context->columnName();

    if (columnNameContext != nullptr)
    {
        if (TryGetExpr(context->columnName(), value))
        {
            AddExpr(context, value);
            return;
        }

        throw FilterExpressionParserException("Failed to find column name expression \"" + columnNameContext->getText() + "\"");
    }

    // Check for unary operator expressions
    const auto unaryOperatorContext = context->unaryOperator();

    if (unaryOperatorContext != nullptr)
    {
        if (context->expression().size() != 1)
            throw FilterExpressionParserException("Unary operator expression is undefined");

        if (TryGetExpr(context->expression(0), value))
        {
            ExpressionUnaryType unaryType;
            const string unaryOperator = unaryOperatorContext->getText();

            if (IsEqual(unaryOperator, "+", false))
                unaryType = ExpressionUnaryType::Plus;
            else if (IsEqual(unaryOperator, "-", false))
                unaryType = ExpressionUnaryType::Minus;
            else if (IsEqual(unaryOperator, "~", false) || IsEqual(unaryOperator, "NOT"))
                unaryType = ExpressionUnaryType::Not;
            else
                throw FilterExpressionParserException("Unexpected unary operator type \"" + unaryOperator + "\"");

            AddExpr(context, NewSharedPtr<UnaryExpression>(unaryType, value));
            return;
        }

        throw FilterExpressionParserException("Failed to find unary operator expression \"" + context->expression(0)->getText() + "\"");
    }

    // Check for function expressions
    const auto functionNameContext = context->functionName();

    if (functionNameContext != nullptr)
    {
        ExpressionFunctionType functionType;
        const string& functionName = functionNameContext->getText();

        if (IsEqual(functionName, "COALESCE") || IsEqual(functionName, "ISNULL"))
            functionType = ExpressionFunctionType::Coalesce;
        else if (IsEqual(functionName, "CONVERT"))
            functionType = ExpressionFunctionType::Convert;
        else if (IsEqual(functionName, "IIF"))
            functionType = ExpressionFunctionType::IIf;
        else if (IsEqual(functionName, "ISREGEXMATCH"))
            functionType = ExpressionFunctionType::IsRegExMatch;
        else if (IsEqual(functionName, "LEN"))
            functionType = ExpressionFunctionType::Len;
        else if (IsEqual(functionName, "REGEXVAL"))
            functionType = ExpressionFunctionType::RegExVal;
        else if (StartsWith(functionName, "SUBSTR"))
            functionType = ExpressionFunctionType::SubString;
        else if (IsEqual(functionName, "TRIM"))
            functionType = ExpressionFunctionType::Trim;
        else
            throw FilterExpressionParserException("Unexpected function type \"" + functionName + "\"");

        ExpressionCollectionPtr arguments = NewSharedPtr<ExpressionCollection>();
        const int32_t argumentCount = context->expression().size();

        for (int32_t i = 0; i < argumentCount; i++)
        {
            ExpressionPtr argument;

            if (TryGetExpr(context->expression(i), argument))
                arguments->push_back(argument);
            else
                throw FilterExpressionParserException("Failed to find argument expression " + ToString(i) + " for function \"" + functionName + "\"");
        }

        AddExpr(context, NewSharedPtr<FunctionExpression>(functionType, arguments));
        return;
    }

    // Check for IS NULL expressions
    const auto isKeywordContext = context->K_IS();
    const auto nullKeywordContext = context->K_NULL();
    const auto notKeywordContext = context->K_NOT();

    if (isKeywordContext != nullptr && nullKeywordContext != nullptr)
    {
        const ExpressionOperatorType operatorType = notKeywordContext == nullptr ? ExpressionOperatorType::IsNull : ExpressionOperatorType::IsNotNull;

        if (context->expression().size() != 1)
            throw FilterExpressionParserException("\"IS NULL\" expression is undefined");

        if (TryGetExpr(context->expression(0), value))
        {
            AddExpr(context, NewSharedPtr<OperatorExpression>(operatorType, value, nullptr));
            return;
        }

        throw FilterExpressionParserException("Failed to find \"IS NULL\" expression \"" + context->expression(0)->getText() + "\"");
    }

    // Check for IN expressions
    const auto inKeywordContext = context->K_IN();

    if (inKeywordContext != nullptr)
    {
        ExpressionCollectionPtr arguments = NewSharedPtr<ExpressionCollection>();
        const int32_t argumentCount = context->expression().size();

        if (context->expression().size() < 2)
            throw FilterExpressionParserException("Not enough expressions found for \"IN\" operation");

        for (int32_t i = 0; i < argumentCount; i++)
        {
            ExpressionPtr argument;

            if (TryGetExpr(context->expression(i), argument))
            {
                if (i == 0)
                    value = argument;
                else
                    arguments->push_back(argument);
            }
            else
            {
                throw FilterExpressionParserException("Failed to find argument expression " + ToString(i) + " for \"IN\" operation");
            }
        }

        AddExpr(context, NewSharedPtr<InListExpression>(value, arguments, notKeywordContext != nullptr));
        return;
    }

    // Check for operator expressions
    if (context->expression().size() == 2)
    {
        ExpressionPtr leftValue, rightValue;
        ExpressionOperatorType operatorType;

        if (!TryGetExpr(context->expression(0), leftValue))
            throw FilterExpressionParserException("Failed to find left operator expression \"" + context->expression(0)->getText() + "\"");

        if (!TryGetExpr(context->expression(1), rightValue))
            throw FilterExpressionParserException("Failed to find right operator expression \"" + context->expression(1)->getText() + "\"");

        if (context->children.size() < 3)
            throw FilterExpressionParserException("Operator expression is malformed");

        const string& operatorSymbol = context->children[1]->getText();

        // Check for arithmetic operations
        if (IsEqual(operatorSymbol, "*", false))
            operatorType = ExpressionOperatorType::Multiply;
        else if (IsEqual(operatorSymbol, "/", false))
            operatorType = ExpressionOperatorType::Divide;
        else if (IsEqual(operatorSymbol, "%", false))
            operatorType = ExpressionOperatorType::Modulus;
        else if (IsEqual(operatorSymbol, "+", false))
            operatorType = ExpressionOperatorType::Add;
        else if (IsEqual(operatorSymbol, "-", false))
            operatorType = ExpressionOperatorType::Subtract;

        // Check for bitwise operations
        else if (IsEqual(operatorSymbol, "<<", false))
            operatorType = ExpressionOperatorType::BitShiftLeft;
        else if (IsEqual(operatorSymbol, ">>", false))
            operatorType = ExpressionOperatorType::BitShiftRight;
        else if (IsEqual(operatorSymbol, "&", false))
            operatorType = ExpressionOperatorType::BitwiseAnd;
        else if (IsEqual(operatorSymbol, "|", false))
            operatorType = ExpressionOperatorType::BitwiseOr;

        // Check for comparison operations
        else if (IsEqual(operatorSymbol, "<", false))
            operatorType = ExpressionOperatorType::LessThan;
        else if (IsEqual(operatorSymbol, "<=", false))
            operatorType = ExpressionOperatorType::LessThanOrEqual;
        else if (IsEqual(operatorSymbol, ">", false))
            operatorType = ExpressionOperatorType::GreaterThan;
        else if (IsEqual(operatorSymbol, ">=", false))
            operatorType = ExpressionOperatorType::GreaterThanOrEqual;

        // Check for equality operations
        else if (IsEqual(operatorSymbol, "=", false) || IsEqual(operatorSymbol, "==", false))
            operatorType = ExpressionOperatorType::Equal;
        else if (IsEqual(operatorSymbol, "<>", false) || IsEqual(operatorSymbol, "!=", false))
            operatorType = ExpressionOperatorType::NotEqual;

        // Check for boolean operations
        else if (context->K_LIKE() != nullptr)
            operatorType = notKeywordContext == nullptr ? ExpressionOperatorType::Like : ExpressionOperatorType::NotLike;
        else if (context->K_AND() != nullptr)
            operatorType = ExpressionOperatorType::And;
        else if (context->K_OR() != nullptr)
            operatorType = ExpressionOperatorType::Or;
        else
            throw FilterExpressionParserException("Unexpected operator \"" + operatorSymbol + "\"");

        AddExpr(context, NewSharedPtr<OperatorExpression>(operatorType, leftValue, rightValue));
        return;
    }

    // Check for sub-expressions, i.e., "(" expression ")"
    if (!context->children.empty() && IsEqual(context->children[0]->getText(), "(", false) && context->expression().size() == 1)
    {
        if (TryGetExpr(context->expression(0), value))
        {
            AddExpr(context, value);
            return;
        }

        throw FilterExpressionParserException("Failed to find sub expression \"" + context->expression(0)->getText() + "\"");
    }

    throw FilterExpressionParserException("Unexpected expression \"" + context->getText() + "\"");
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
    ValueExpressionPtr result = nullptr;

    if (context->INTEGER_LITERAL())
    {
        const double_t value = stod(context->INTEGER_LITERAL()->getText());

        if (value > Int64::MaxValue)
            result = NewSharedPtr<ValueExpression>(ExpressionValueType::Double, value);
        else if (value > Int32::MaxValue)
            result = NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, static_cast<int64_t>(value));
        else
            result = NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, static_cast<int32_t>(value));
    }
    else if (context->NUMERIC_LITERAL())
    {
        const string& literal = context->NUMERIC_LITERAL()->getText();
        
        if (literal.find('e') || literal.find('E'))
        {
            // Real literals using scientific notation are parsed as double
            result = NewSharedPtr<ValueExpression>(ExpressionValueType::Double, stod(literal));
        }
        else
        {
            // Real literals without scientific notation are parsed as decimal, if
            // the number fails to parse as decimal, then it is parsed as a double
            try
            {
                result = NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, decimal_t(literal));
            }
            catch (const std::runtime_error&)
            {
                result = NewSharedPtr<ValueExpression>(ExpressionValueType::Double, stod(literal));
            }
        }
    }
    else if (context->STRING_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionValueType::String, ParseStringLiteral(context->STRING_LITERAL()->getText()));
    }
    else if (context->DATETIME_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, ParseDateTimeLiteral(context->DATETIME_LITERAL()->getText()));
    }
    else if (context->GUID_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionValueType::Guid, ParseGuidLiteral(context->GUID_LITERAL()->getText()));
    }
    else if (context->BOOLEAN_LITERAL())
    {
        result = IsEqual(context->BOOLEAN_LITERAL()->getText(), "true") ? ExpressionTree::True : ExpressionTree::False;
    }
    else if (context->K_NULL())
    {
        result = ExpressionTree::NullValue(ExpressionValueType::Undefined);
    }

    if (result)
        AddExpr(context, CastSharedPtr<Expression>(result));
}

/*
    columnName
     : IDENTIFIER
     ;
 */
void FilterExpressionParser::exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext* context)
{
    const string& columnName = context->IDENTIFIER()->getText();
    const DataColumnPtr& dataColumn = m_activeExpressionTree->Measurements()->Column(columnName);

    if (dataColumn == nullptr)
        throw FilterExpressionParserException("Failed to find column \"" + columnName + "\" in table \"" + m_activeExpressionTree->Measurements()->Name() + "\"");

    AddExpr(context, NewSharedPtr<ColumnExpression>(dataColumn));
}