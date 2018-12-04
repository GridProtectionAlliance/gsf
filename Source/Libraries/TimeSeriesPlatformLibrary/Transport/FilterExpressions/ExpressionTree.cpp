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
using namespace GSF::TimeSeries::Transport;

const char* GSF::TimeSeries::Transport::ExpressionDataTypeAcronym[] =
{
    "Boolean",
    "Int32",
    "Int64",
    "Decimal",
    "Double",
    "String",
    "Guid",
    "DateTime",
    "Null"
};

const char* GSF::TimeSeries::Transport::EnumName(ExpressionDataType type)
{
    return ExpressionDataTypeAcronym[static_cast<int32_t>(type)];
}

bool Transport::IsIntegerType(ExpressionDataType type)
{
    switch (type)
    {
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Int32:
        case ExpressionDataType::Int64:
            return true;
        default:
            return false;
    }
}

bool Transport::IsNumericType(ExpressionDataType type)
{
    switch (type)
    {
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Int32:
        case ExpressionDataType::Int64:
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
            return true;
        default:
            return false;
    }
}

const ValueExpressionPtr ExpressionTree::True = NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, true);

const ValueExpressionPtr ExpressionTree::False = NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, false);

const ValueExpressionPtr ExpressionTree::Null = NewSharedPtr<ValueExpression>(ExpressionDataType::Null, nullptr);

ExpressionTreeException::ExpressionTreeException(string message) noexcept :
    m_message(move(message))
{
}

const char* ExpressionTreeException::what() const noexcept
{
    return &m_message[0];
}

Expression::Expression(ExpressionType type, ExpressionDataType dataType, bool isNullable) :
    Type(type),
    DataType(dataType),
    IsNullable(isNullable)
{
}

ValueExpression::ValueExpression(ExpressionDataType dataType, const Object& value, bool isNullable) :
    Expression(ExpressionType::Value, dataType, isNullable),
    Value(value)
{
}

void ValueExpression::ValidateDataType(ExpressionDataType targetType) const
{
    if (DataType != targetType)
        throw ExpressionTreeException("Cannot read literal expression value as " + string(EnumName(targetType)) + ", data type is " + string(EnumName(DataType)));
}

bool ValueExpression::IsNull() const
{
    switch (DataType)
    {
        case ExpressionDataType::Boolean:
            return !ValueAsBoolean().HasValue();
        case ExpressionDataType::Int32:
            return !ValueAsInt32().HasValue();
        case ExpressionDataType::Int64:
            return !ValueAsInt64().HasValue();
        case ExpressionDataType::Decimal:
            return !ValueAsDecimal().HasValue();
        case ExpressionDataType::Double:
            return !ValueAsDouble().HasValue();
        case ExpressionDataType::String:
            return !ValueAsString().HasValue();
        case ExpressionDataType::Guid:
            return !ValueAsGuid().HasValue();
        case ExpressionDataType::DateTime:
            return !ValueAsDateTime().HasValue();
        case ExpressionDataType::Null:
            return true;
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

string ValueExpression::ToString() const
{
    switch (DataType)
    {
        case ExpressionDataType::Boolean:
            return GSF::TimeSeries::ToString(ValueAsBoolean());
        case ExpressionDataType::Int32:
            return GSF::TimeSeries::ToString(ValueAsInt32());
        case ExpressionDataType::Int64:
            return GSF::TimeSeries::ToString(ValueAsInt64());
        case ExpressionDataType::Decimal:
            return GSF::TimeSeries::ToString(ValueAsDecimal());
        case ExpressionDataType::Double:
            return GSF::TimeSeries::ToString(ValueAsDouble());
        case ExpressionDataType::String:
            return GSF::TimeSeries::ToString(ValueAsString());
        case ExpressionDataType::Guid:
            return GSF::TimeSeries::ToString(ValueAsGuid());
        case ExpressionDataType::DateTime:
            return GSF::TimeSeries::ToString(ValueAsDateTime());
        case ExpressionDataType::Null:
            return nullptr;
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

Nullable<bool> ValueExpression::ValueAsBoolean() const
{
    ValidateDataType(ExpressionDataType::Boolean);

    if (IsNullable)
        return Cast<Nullable<bool>>(Value);

    return Cast<bool>(Value);
}

Nullable<int32_t> ValueExpression::ValueAsInt32() const
{
    ValidateDataType(ExpressionDataType::Int32);

    if (IsNullable)
        return Cast<Nullable<int32_t>>(Value);

    return Cast<int32_t>(Value);
}

Nullable<int64_t> ValueExpression::ValueAsInt64() const
{
    ValidateDataType(ExpressionDataType::Int64);

    if (IsNullable)
        return Cast<Nullable<int64_t>>(Value);

    return Cast<int64_t>(Value);
}

Nullable<decimal_t> ValueExpression::ValueAsDecimal() const
{
    ValidateDataType(ExpressionDataType::Decimal);

    if (IsNullable)
        return Cast<Nullable<decimal_t>>(Value);

    return Cast<decimal_t>(Value);
}

Nullable<float64_t> ValueExpression::ValueAsDouble() const
{
    ValidateDataType(ExpressionDataType::Double);

    if (IsNullable)
        return Cast<Nullable<float64_t>>(Value);

    return Cast<float64_t>(Value);
}

Nullable<string> ValueExpression::ValueAsString() const
{
    ValidateDataType(ExpressionDataType::String);

    if (IsNullable)
        return Cast<Nullable<string>>(Value);

    return Cast<string>(Value);
}

Nullable<Guid> ValueExpression::ValueAsGuid() const
{
    ValidateDataType(ExpressionDataType::Guid);

    if (IsNullable)
        return Cast<Nullable<Guid>>(Value);

    return Cast<Guid>(Value);
}

Nullable<time_t> ValueExpression::ValueAsDateTime() const
{
    ValidateDataType(ExpressionDataType::DateTime);

    if (IsNullable)
        return Cast<Nullable<time_t>>(Value);

    return Cast<time_t>(Value);
}

UnaryExpression::UnaryExpression(ExpressionUnaryType unaryType, const ExpressionPtr& value) :
    Expression(ExpressionType::Unary, value->DataType, value->IsNullable),
    UnaryType(unaryType),
    Value(value)
{
}

ColumnExpression::ColumnExpression(const DataColumnPtr& column) :
    Expression(ExpressionType::Column, ExpressionDataType::Null, true),
    Column(column)
{
}

OperatorExpression::OperatorExpression(ExpressionDataType dataType, ExpressionOperatorType operatorType) :
    Expression(ExpressionType::Operator, dataType),
    OperatorType(operatorType),
    Left(nullptr),
    Right(nullptr)
{
}

FunctionExpression::FunctionExpression(ExpressionDataType dataType, ExpressionFunctionType functionType, const vector<ExpressionPtr>& arguments) :
    Expression(ExpressionType::Function, dataType),
    FunctionType(functionType),
    Arguments(arguments)
{
}

ExpressionTree::ExpressionTree(string measurementTableName, const DataTablePtr& measurements) :
    MeasurementTableName(move(measurementTableName)),
    Measurements(measurements)
{
}

ExpressionPtr ExpressionTree::Evaluate(const ExpressionPtr& node)
{
    if (node == nullptr)
        return ExpressionTree::False;

    // All expression nodes should evaluate to a value expression
    // Only operator expressions have left and right nodes
    switch (node->Type)
    {
        case ExpressionType::Value:
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

ExpressionPtr ExpressionTree::EvaluateColumn(const ExpressionPtr& node) const
{
    const ColumnExpressionPtr columnNode = CastSharedPtr<ColumnExpression>(node);
    const DataColumnPtr& column = columnNode->Column;

    if (column == nullptr)
        throw ExpressionTreeException("Encountered column expression with undefined data column reference.");

    const int32_t columnIndex = column->Index();

    Nullable<uint64_t> value64U = nullptr;
    ExpressionDataType dataType;
    Object value;

    // Map column DataType to ExpressionType, storing equivalent Nullable<T> literal value
    switch (column->Type())
    {
        case DataType::String:
            dataType = ExpressionDataType::String;
            value = m_currentRow->ValueAsString(columnIndex);
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
                if (value64U.Value > static_cast<uint64_t>(Int64::MaxValue))
                {
                    dataType = ExpressionDataType::Double;
                    value = CastAsNullable<double>(value64U);
                }
                else
                {
                    dataType = ExpressionDataType::Int64;
                    value = CastAsNullable<int64_t>(value64U);
                }
            }
            else
            {
                dataType = ExpressionDataType::Int64;
                value = Nullable<int64_t>(nullptr);
            }
            break;
        default:
            throw ExpressionTreeException("Unexpected column data type encountered");
    }

    // All literal expressions derived for columns are wrapped in Nullable<T>
    return NewSharedPtr<ValueExpression>(dataType, value, true);
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

    // Final expression should have a boolean data type (it's part of a WHERE clause)
    if (result->DataType == ExpressionDataType::Boolean)
        return Cast<bool>(CastSharedPtr<ValueExpression>(result)->Value);

    throw ExpressionTreeException("Final expression tree evaluation did not result in a boolean value, result data type is " + string(EnumName(result->DataType)));
}