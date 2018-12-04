//******************************************************************************************************
//  ExpressionTree.cpp - Gbtc
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
//  12/02/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "ExpressionTree.h"


using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace Transport;

const LiteralExpressionPtr ExpressionTree::True = NewSharedPtr<LiteralExpression>(ExpressionDataType::Boolean, true);

const LiteralExpressionPtr ExpressionTree::False = NewSharedPtr<LiteralExpression>(ExpressionDataType::Boolean, false);

const LiteralExpressionPtr ExpressionTree::Null = NewSharedPtr<LiteralExpression>(ExpressionDataType::Null, nullptr);

ExpressionTreeException::ExpressionTreeException(string message) noexcept :
    m_message(std::move(message))
{
}

const char* ExpressionTreeException::what() const noexcept
{
    return &m_message[0];
}

Expression::Expression(ExpressionType type, ExpressionDataType dataType) :
    Type(type),
    DataType(dataType)
{
}

LiteralExpression::LiteralExpression(ExpressionDataType dataType, const Object& value) :
    Expression(ExpressionType::Literal, dataType),
    Value(value)
{
}

UnaryExpression::UnaryExpression(ExpressionUnaryType unaryType, const ExpressionPtr& value) :
    Expression(ExpressionType::Unary, value->DataType),
    UnaryType(unaryType),
    Value(value)
{
}

ColumnExpression::ColumnExpression(ExpressionDataType dataType, int32_t columnIndex) :
    Expression(ExpressionType::Column, dataType),
    ColumnIndex(columnIndex)
{
}

OperatorExpression::OperatorExpression(ExpressionDataType dataType, ExpressionOperatorType operatorType) :
    Expression(ExpressionType::Operator, dataType),
    OperatorType(operatorType),
    Left(nullptr),
    Right(nullptr)
{
}

FunctionExpression::FunctionExpression(ExpressionDataType dataType, ExpressionFunctionType functionType, const std::vector<ExpressionPtr>& arguments) :
    Expression(ExpressionType::Function, dataType),
    FunctionType(functionType),
    Arguments(arguments)
{
}

ExpressionTree::ExpressionTree(std::string measurementTableName, const DataTablePtr& measurements) :
    MeasurementTableName(std::move(measurementTableName)),
    Measurements(measurements)
{
}

ExpressionPtr ExpressionTree::Evaluate(const ExpressionPtr& node)
{
    if (node == nullptr)
        return ExpressionTree::False;

    switch (node->Type)
    {
        case ExpressionType::Literal:
            return node;
        case ExpressionType::Unary:
            return EvaluateUnary(node);
        case ExpressionType::Column:
            return EvaluateColumn(node);
        case ExpressionType::Function:
            return EvaluateFunction(node);
        case ExpressionType::Operator:
            return EvaluateOperator(node);
         default:
             throw ExpressionTreeException("Unexpected expression type encountered");
    }
}

ExpressionPtr ExpressionTree::EvaluateUnary(const ExpressionPtr& node)
{
    const UnaryExpressionPtr unaryNode = CastSharedPtr<UnaryExpression>(node);
    return node;
}

template<typename T, typename U>
Nullable<T> CastAsNullable(Nullable<U> source)
{
    if (source.HasValue())
        return static_cast<T>(source.Value);

    return nullptr;
}

ExpressionPtr ExpressionTree::EvaluateColumn(const ExpressionPtr& node) const
{
    const ColumnExpressionPtr columnNode = CastSharedPtr<ColumnExpression>(node);
    const int32_t columnIndex = columnNode->ColumnIndex;
    const DataColumnPtr& column = m_currentRow->Parent()->Column(columnIndex);

    if (column)
    {
        Nullable<uint64_t> value64U = nullptr;
        ExpressionDataType dataType;
        Object value;

        switch (column->Type())
        {
            case DataType::String:
                dataType = ExpressionDataType::String;
                value = string(m_currentRow->ValueAsString(columnIndex));
                break;
            case DataType::Boolean:
                dataType = ExpressionDataType::Boolean;
                value = m_currentRow->ValueAsBoolean(columnIndex);
                break;
            case DataType::DateTime:
                dataType = ExpressionDataType::DateTime;
                value = m_currentRow->ValueAsDateTime(columnIndex);
                break;
            case DataType::Single:
                dataType = ExpressionDataType::Double;
                value = CastAsNullable<double>(m_currentRow->ValueAsSingle(columnIndex));
                break;
            case DataType::Double:
                dataType = ExpressionDataType::Double;
                value = m_currentRow->ValueAsDouble(columnIndex);
                break;
            case DataType::Decimal:
                dataType = ExpressionDataType::Decimal;
                value = m_currentRow->ValueAsDecimal(columnIndex);
                break;
            case DataType::Guid:
                dataType = ExpressionDataType::Guid;
                value = m_currentRow->ValueAsGuid(columnIndex);
                break;
            case DataType::Int8:
                dataType = ExpressionDataType::Int32;
                value = CastAsNullable<int32_t>(m_currentRow->ValueAsInt8(columnIndex));
                break;
            case DataType::Int16:
                dataType = ExpressionDataType::Int32;
                value = CastAsNullable<int32_t>(m_currentRow->ValueAsInt16(columnIndex));
                break;
            case DataType::Int32:
                dataType = ExpressionDataType::Int32;
                value = m_currentRow->ValueAsInt32(columnIndex);
                break;
            case DataType::UInt8:
                dataType = ExpressionDataType::Int32;
                value = CastAsNullable<int32_t>(m_currentRow->ValueAsUInt8(columnIndex));
                break;
            case DataType::UInt16:
                dataType = ExpressionDataType::Int32;
                value = CastAsNullable<int32_t>(m_currentRow->ValueAsUInt16(columnIndex));
                break;
            case DataType::Int64:;
                dataType = ExpressionDataType::Int64;
                value = m_currentRow->ValueAsInt64(columnIndex);
                break;
            case DataType::UInt32:
                dataType = ExpressionDataType::Int64;
                value = CastAsNullable<int64_t>(m_currentRow->ValueAsUInt32(columnIndex));
                break;
            case DataType::UInt64:
                value64U = m_currentRow->ValueAsUInt64(columnIndex);

                if (value64U.HasValue())
                {
                    if (value64U.Value > Int64::MaxValue)
                    {
                        dataType = ExpressionDataType::Double;
                        value = CastAsNullable<double>(value64U);
                    }
                    else
                    {
                        dataType = ExpressionDataType::Int64;
                        value = value64U;
                    }
                }
                else
                {
                    dataType = ExpressionDataType::Int64;
                    value = ExpressionTree::Null;
                }
                break;
            default:
                throw ExpressionTreeException("Unexpected column data type encountered");
        }

        return NewSharedPtr<LiteralExpression>(dataType, value);
    }

    throw ExpressionTreeException("Failed to find data column for index " + to_string(columnNode->ColumnIndex));
}

ExpressionPtr ExpressionTree::EvaluateFunction(const ExpressionPtr& node)
{
    const FunctionExpressionPtr functionNode = CastSharedPtr<FunctionExpression>(node);

    switch (functionNode->FunctionType)
    {
        case ExpressionFunctionType::Coalesce:
            if (functionNode->Arguments.size() != 2)
                throw ExpressionTreeException("Coalesce/IsNull function expects 2 arguments, received " + to_string(functionNode->Arguments.size()));

            break;
        case ExpressionFunctionType::Convert:
            if (functionNode->Arguments.size() != 2)
                throw ExpressionTreeException("Convert function expects 2 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        case ExpressionFunctionType::ImmediateIf:
            if (functionNode->Arguments.size() != 3)
                throw ExpressionTreeException("IIf function expects 3 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        case ExpressionFunctionType::Len:
            if (functionNode->Arguments.size() != 1)
                throw ExpressionTreeException("Len expects 3 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        case ExpressionFunctionType::RegExp:
            if (functionNode->Arguments.size() != 2)
                throw ExpressionTreeException("RegExp function expects 2 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        case ExpressionFunctionType::SubString:
            if (functionNode->Arguments.size() < 2 || functionNode->Arguments.size() > 3)
                throw ExpressionTreeException("SubString function expects 2 or 3 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        case ExpressionFunctionType::Trim:
            if (functionNode->Arguments.size() != 1)
                throw ExpressionTreeException("Trim function expects 3 arguments, received " + to_string(functionNode->Arguments.size()));
            break;
        default:
            throw ExpressionTreeException("Unexpected function type encountered");
    }
}

ExpressionPtr ExpressionTree::EvaluateOperator(const ExpressionPtr& node)
{
    const OperatorExpressionPtr operatorNode = CastSharedPtr<OperatorExpression>(node);

    ExpressionPtr left = Evaluate(operatorNode->Left);
    ExpressionPtr right = Evaluate(operatorNode->Right);

    switch (operatorNode->OperatorType)
    {
        case ExpressionOperatorType::Multiply:
            break;
        case ExpressionOperatorType::Divide:
            break;
        case ExpressionOperatorType::Modulus:
            break;
        case ExpressionOperatorType::Add:
            break;
        case ExpressionOperatorType::Subtract:
            break;
        case ExpressionOperatorType::BitShiftLeft:
            break;
        case ExpressionOperatorType::BitShiftRight:
            break;
        case ExpressionOperatorType::BitwiseAnd:
            break;
        case ExpressionOperatorType::BitwiseOr:
            break;
        case ExpressionOperatorType::LessThan:
            break;
        case ExpressionOperatorType::LessThanOrEqual:
            break;
        case ExpressionOperatorType::GreaterThan:
            break;
        case ExpressionOperatorType::GreaterThanOrEqual:
            break;
        case ExpressionOperatorType::Equal:
            break;
        case ExpressionOperatorType::NotEqual:
            break;
        case ExpressionOperatorType::IsNull:
            break;
        case ExpressionOperatorType::IsNotNull:
            break;
        case ExpressionOperatorType::And:
            break;
        case ExpressionOperatorType::Or:
            break;
        default:
            throw ExpressionTreeException("Unexpected operator type encountered");
    }
}

bool ExpressionTree::Evaluate(const DataRowPtr& row)
{
    m_currentRow = row;

    const ExpressionPtr result = Evaluate(Root);

    // Final expression should have a boolean data type
    if (result->DataType == ExpressionDataType::Boolean)
        return Cast<bool>(CastSharedPtr<LiteralExpression>(result)->Value);

    throw ExpressionTreeException("Final expression tree evaluation did not result in a boolean value");
}