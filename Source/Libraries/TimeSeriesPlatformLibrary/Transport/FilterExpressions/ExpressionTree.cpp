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
#include <regex>

using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

const int32_t GSF::TimeSeries::Transport::ExpressionDataTypeLength = static_cast<int32_t>(ExpressionDataType::Undefined) + 1;

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
    "Undefined"
};

const char* GSF::TimeSeries::Transport::EnumName(ExpressionDataType type)
{
    return ExpressionDataTypeAcronym[static_cast<int32_t>(type)];
}

const char* GSF::TimeSeries::Transport::ExpressionUnaryTypeAcronym[] =
{
    "+",
    "-",
    "~"
};

const char* GSF::TimeSeries::Transport::EnumName(ExpressionUnaryType type)
{
    return ExpressionUnaryTypeAcronym[static_cast<int32_t>(type)];
}

const char* GSF::TimeSeries::Transport::ExpressionOperatorTypeAcronym[] =
{
    "*",
    "/",
    "%",
    "+",
    "-",
    "<<",
    ">>",
    "&",
    "|",
    "<",
    "<=",
    ">",
    ">=",
    "=",
    "<>",
    "IS NULL",
    "IS NOT NULL",
    "LIKE",
    "NOT LIKE",
    "AND",
    "OR"
};

const char* GSF::TimeSeries::Transport::EnumName(ExpressionOperatorType type)
{
    return ExpressionOperatorTypeAcronym[static_cast<int32_t>(type)];
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
        case ExpressionDataType::Undefined:
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
        case ExpressionDataType::Undefined:
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
    Expression(ExpressionType::Column, ExpressionDataType::Undefined, true),
    Column(column)
{
}

OperatorExpression::OperatorExpression(ExpressionOperatorType operatorType, const ExpressionPtr& leftValue, const ExpressionPtr& rightValue) :
    Expression(ExpressionType::Operator, ExpressionDataType::Undefined),
    OperatorType(operatorType),
    LeftValue(leftValue),
    RightValue(rightValue)
{
}

InListExpression::InListExpression(const ExpressionPtr& value, const ExpressionCollectionPtr& arguments, bool notInList) :
    Expression(ExpressionType::InList, ExpressionDataType::Boolean),
    Value(value),
    Arguments(arguments),
    NotInList(notInList)
{
}

FunctionExpression::FunctionExpression(ExpressionFunctionType functionType, const ExpressionCollectionPtr& arguments) :
    Expression(ExpressionType::Function, ExpressionDataType::Undefined),
    FunctionType(functionType),
    Arguments(arguments)
{
}

ValueExpressionPtr ExpressionTree::Evaluate(const ExpressionPtr& node, ExpressionDataType targetDataType) const
{
    if (node == nullptr)
        return NullValue(targetDataType);

    // All expression nodes should evaluate to a value expression
    switch (node->Type)
    {
        case ExpressionType::Value:
            // Change Undefined values to Nullable of target type
            if (node->DataType == ExpressionDataType::Undefined)
                return NullValue(targetDataType);

            return CastSharedPtr<ValueExpression>(node);
        case ExpressionType::Unary:
            return EvaluateUnary(node);
        case ExpressionType::Column:
            return EvaluateColumn(node);
        case ExpressionType::InList:
            return EvaluateInList(node);
        case ExpressionType::Function:
            return EvaluateFunction(node);
        case ExpressionType::Operator:
            return EvaluateOperator(node);
         default:
             throw ExpressionTreeException("Unexpected expression type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateUnary(const ExpressionPtr& node) const
{
    const UnaryExpressionPtr unaryNode = CastSharedPtr<UnaryExpression>(node);
    const ValueExpressionPtr unaryValue = Evaluate(unaryNode->Value);

    // If unary value is Null, result is Null
    if (unaryValue->IsNull())
        return NullValue(unaryValue->DataType);

    switch (unaryValue->DataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, ApplyIntegerUnaryOperation<int32_t>(unaryValue->ValueAsInt32(), unaryNode->UnaryType));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, ApplyIntegerUnaryOperation<int64_t>(unaryValue->ValueAsInt64(), unaryNode->UnaryType));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, ApplyNumericUnaryOperation<decimal_t>(unaryValue->ValueAsDecimal(), unaryNode->UnaryType, unaryValue->DataType));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, ApplyNumericUnaryOperation<float64_t>(unaryValue->ValueAsDouble(), unaryNode->UnaryType, unaryValue->DataType));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply unary \"" + string(EnumName(unaryNode->UnaryType)) + "\" operator to \"" + string(EnumName(unaryNode->DataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateColumn(const ExpressionPtr& node) const
{
    const ColumnExpressionPtr columnNode = CastSharedPtr<ColumnExpression>(node);
    const DataColumnPtr& column = columnNode->Column;

    if (column == nullptr)
        throw ExpressionTreeException("Encountered column expression with undefined data column reference.");

    const int32_t columnIndex = column->Index();

    Nullable<uint64_t> value64U;
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
                if (value64U.GetValueOrDefault() > static_cast<uint64_t>(Int64::MaxValue))
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

    // All literal expression values derived for columns are wrapped in Nullable<T>
    return NewSharedPtr<ValueExpression>(dataType, value, true);
}

ValueExpressionPtr ExpressionTree::EvaluateInList(const ExpressionPtr& node) const
{
    const InListExpressionPtr inListNode = CastSharedPtr<InListExpression>(node);
    const ValueExpressionPtr inListValue = Evaluate(inListNode->Value);
    const bool notInList = inListNode->NotInList;

    // If in list test value is Null, result is Null
    if (inListValue->IsNull())
        return NullValue(inListValue->DataType);

    const int32_t argumentCount = inListNode->Arguments->size();

    for (int32_t i = 0; i < argumentCount; i++)
    {
        const ValueExpressionPtr argumentValue = Evaluate(inListNode->Arguments->at(i));
        const ExpressionDataType dataType = DeriveEqualityOperationDataType(ExpressionOperatorType::Equal, inListValue->DataType, argumentValue->DataType);
        const ValueExpressionPtr result = Equal(inListValue, argumentValue, dataType);

        if (result->IsNullable)
        {
            Nullable<bool> value = Cast<Nullable<bool>>(result->Value);

            if (value.HasValue() && value.GetValueOrDefault())
                return notInList ? ExpressionTree::False : ExpressionTree::True;
        }
        else
        {
            if (Cast<bool>(result->Value))
                return notInList ? ExpressionTree::False : ExpressionTree::True;
        }
    }

    return notInList ? ExpressionTree::True : ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::EvaluateFunction(const ExpressionPtr& node) const
{
    const FunctionExpressionPtr functionNode = CastSharedPtr<FunctionExpression>(node);

    switch (functionNode->FunctionType)
    {
        case ExpressionFunctionType::Coalesce:
            if (functionNode->Arguments->size() != 2)
                throw ExpressionTreeException("\"Coalesce\"/\"IsNull\" function expects 2 arguments, received " + ToString(functionNode->Arguments->size()));

            return Coalesce(Evaluate(functionNode->Arguments->at(0)), Evaluate(functionNode->Arguments->at(1)));
        case ExpressionFunctionType::Convert:
            if (functionNode->Arguments->size() != 2)
                throw ExpressionTreeException("\"Convert\" function expects 2 arguments, received " + ToString(functionNode->Arguments->size()));

            return Convert(Evaluate(functionNode->Arguments->at(0)), Evaluate(functionNode->Arguments->at(1), ExpressionDataType::String));
        case ExpressionFunctionType::IIf:
            if (functionNode->Arguments->size() != 3)
                throw ExpressionTreeException("\"IIf\" function expects 3 arguments, received " + ToString(functionNode->Arguments->size()));

            // Not pre-evaluating IIf result value arguments - only evaluating desired path
            return IIf(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::Boolean), functionNode->Arguments->at(1), functionNode->Arguments->at(2));
        case ExpressionFunctionType::IsRegExMatch:
            if (functionNode->Arguments->size() != 2)
                throw ExpressionTreeException("\"IsRegExMatch\" function expects 2 arguments, received " + ToString(functionNode->Arguments->size()));

            return IsRegExMatch(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String), Evaluate(functionNode->Arguments->at(1), ExpressionDataType::String));
        case ExpressionFunctionType::Len:
            if (functionNode->Arguments->size() != 1)
                throw ExpressionTreeException("\"Len\" function expects 1 argument, received " + ToString(functionNode->Arguments->size()));

            return Len(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String));
        case ExpressionFunctionType::RegExVal:
            if (functionNode->Arguments->size() != 2)
                throw ExpressionTreeException("\"RegExVal\" function expects 2 arguments, received " + ToString(functionNode->Arguments->size()));

            return RegExVal(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String), Evaluate(functionNode->Arguments->at(1), ExpressionDataType::String));
        case ExpressionFunctionType::SubString:
            if (functionNode->Arguments->size() < 2 || functionNode->Arguments->size() > 3)
                throw ExpressionTreeException("\"SubString\" function expects 2 or 3 arguments, received " + ToString(functionNode->Arguments->size()));

            if (functionNode->Arguments->size() == 2)
                return SubString(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String), Evaluate(functionNode->Arguments->at(1), ExpressionDataType::Int32), NullValue(ExpressionDataType::Int32));

            return SubString(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String), Evaluate(functionNode->Arguments->at(1), ExpressionDataType::Int32), Evaluate(functionNode->Arguments->at(2), ExpressionDataType::Int32));
        case ExpressionFunctionType::Trim:
            if (functionNode->Arguments->size() != 1)
                throw ExpressionTreeException("\"Trim\" function expects 1 argument, received " + ToString(functionNode->Arguments->size()));

            return Trim(Evaluate(functionNode->Arguments->at(0), ExpressionDataType::String));
        default:
            throw ExpressionTreeException("Unexpected function type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateOperator(const ExpressionPtr& node) const
{
    const OperatorExpressionPtr operatorNode = CastSharedPtr<OperatorExpression>(node);
    const ValueExpressionPtr leftValue = Evaluate(operatorNode->LeftValue);
    const ValueExpressionPtr rightValue = Evaluate(operatorNode->RightValue);
    const ExpressionDataType dataType = DeriveOperationDataType(operatorNode->OperatorType, leftValue->DataType, rightValue->DataType);

    switch (operatorNode->OperatorType)
    {
        case ExpressionOperatorType::Multiply:
            return Multiply(leftValue, rightValue, dataType);
        case ExpressionOperatorType::Divide:
            return Divide(leftValue, rightValue, dataType);
        case ExpressionOperatorType::Modulus:
            return Modulus(leftValue, rightValue, dataType);
        case ExpressionOperatorType::Add:
            return Add(leftValue, rightValue, dataType);
        case ExpressionOperatorType::Subtract:
            return Subtract(leftValue, rightValue, dataType);
        case ExpressionOperatorType::BitShiftLeft:
            return BitShiftLeft(leftValue, rightValue);
        case ExpressionOperatorType::BitShiftRight:
            return BitShiftRight(leftValue, rightValue);
        case ExpressionOperatorType::BitwiseAnd:
            return BitwiseAnd(leftValue, rightValue, dataType);
        case ExpressionOperatorType::BitwiseOr:
            return BitwiseOr(leftValue, rightValue, dataType);
        case ExpressionOperatorType::LessThan:
            return LessThan(leftValue, rightValue, dataType);
        case ExpressionOperatorType::LessThanOrEqual:
            return LessThanOrEqual(leftValue, rightValue, dataType);
        case ExpressionOperatorType::GreaterThan:
            return GreaterThan(leftValue, rightValue, dataType);
        case ExpressionOperatorType::GreaterThanOrEqual:
            return GreaterThanOrEqual(leftValue, rightValue, dataType);
        case ExpressionOperatorType::Equal:
            return Equal(leftValue, rightValue, dataType);
        case ExpressionOperatorType::NotEqual:
            return NotEqual(leftValue, rightValue, dataType);
        case ExpressionOperatorType::IsNull:
            return IsNull(leftValue);
        case ExpressionOperatorType::IsNotNull:
            return IsNotNull(leftValue);
        case ExpressionOperatorType::Like:
            return Like(leftValue, rightValue);
        case ExpressionOperatorType::NotLike:
            return NotLike(leftValue, rightValue);
        case ExpressionOperatorType::And:
            return And(leftValue, rightValue);
        case ExpressionOperatorType::Or:
            return Or(leftValue, rightValue);
        default:
            throw ExpressionTreeException("Unexpected operator type encountered");
    }
}

template<typename T>
T ExpressionTree::ApplyIntegerUnaryOperation(const Nullable<T>& unaryValue, ExpressionUnaryType unaryOperation) const
{
    T value = unaryValue.GetValueOrDefault();

    switch (unaryOperation)
    {
        case ExpressionUnaryType::Plus:
            return +value;
        case ExpressionUnaryType::Minus:
            return -value;
        case ExpressionUnaryType::Not:
            return ~value;
        default:
            throw ExpressionTreeException("Unexpected unary type encountered");
    }
}

template<typename T>
T ExpressionTree::ApplyNumericUnaryOperation(const Nullable<T>& unaryValue, ExpressionUnaryType unaryOperation, ExpressionDataType dataType) const
{
    T value = unaryValue.GetValueOrDefault();

    switch (unaryOperation)
    {
        case ExpressionUnaryType::Plus:
            return +value;
        case ExpressionUnaryType::Minus:
            return -value;
        case ExpressionUnaryType::Not:
            throw ExpressionTreeException("Cannot apply unary \"~\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected unary type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    switch (operationType)
    {
        case ExpressionOperatorType::Multiply:
        case ExpressionOperatorType::Divide:
        case ExpressionOperatorType::Modulus:
        case ExpressionOperatorType::Add:
        case ExpressionOperatorType::Subtract:
            return DeriveArithmeticOperationDataType(operationType, leftDataType, rightDataType);        
        case ExpressionOperatorType::BitwiseAnd:
        case ExpressionOperatorType::BitwiseOr:
            return DeriveBitwiseOperationDataType(operationType, leftDataType, rightDataType);
        case ExpressionOperatorType::LessThan:
        case ExpressionOperatorType::LessThanOrEqual:
        case ExpressionOperatorType::GreaterThan:
        case ExpressionOperatorType::GreaterThanOrEqual:
            return DeriveComparisonOperationDataType(operationType, leftDataType, rightDataType);
        case ExpressionOperatorType::Equal:
        case ExpressionOperatorType::NotEqual:
            return DeriveEqualityOperationDataType(operationType, leftDataType, rightDataType);
        case ExpressionOperatorType::And:
        case ExpressionOperatorType::Or:
            return DeriveBooleanOperationDataType(operationType, leftDataType, rightDataType);
        case ExpressionOperatorType::BitShiftLeft:
        case ExpressionOperatorType::BitShiftRight:
        case ExpressionOperatorType::IsNull:
        case ExpressionOperatorType::IsNotNull:
        case ExpressionOperatorType::Like:
        case ExpressionOperatorType::NotLike:
            return leftDataType;
        default:
            throw ExpressionTreeException("Unexpected expression operator type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveArithmeticOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    switch (leftDataType)
    {
        case ExpressionDataType::Int32:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int64:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Decimal:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Decimal\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Double:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftDataType)) + "\" and \"" + string(EnumName(rightDataType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveBitwiseOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    switch (leftDataType)
    {
        case ExpressionDataType::Boolean:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                    return ExpressionDataType::Boolean;
                case ExpressionDataType::Int32:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int32:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int64:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::String:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftDataType)) + "\" and \"" + string(EnumName(rightDataType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveComparisonOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    switch (leftDataType)
    {
        case ExpressionDataType::Boolean:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightDataType)) + "\"");
        case ExpressionDataType::Int32:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::String:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int64:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::String:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Decimal:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::String:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Decimal\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Double:
            switch (rightDataType)
            {
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::String:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::String:
            return leftDataType;
        case ExpressionDataType::Guid:
            switch (rightDataType)
            {
                case ExpressionDataType::Guid:
                case ExpressionDataType::String:
                    return ExpressionDataType::Guid;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Guid\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::DateTime:
            switch (rightDataType)
            {
                case ExpressionDataType::DateTime:
                case ExpressionDataType::String:
                    return ExpressionDataType::DateTime;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::Guid:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"DateTime\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveEqualityOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    switch (leftDataType)
    {
        case ExpressionDataType::Boolean:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::String:
                    return ExpressionDataType::Boolean;
                case ExpressionDataType::Int32:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int32:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::String:
                    return ExpressionDataType::Int32;
                case ExpressionDataType::Int64:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Int64:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::String:
                    return ExpressionDataType::Int64;
                case ExpressionDataType::Decimal:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Decimal:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::String:
                    return ExpressionDataType::Decimal;
                case ExpressionDataType::Double:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Decimal\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::Double:
            switch (rightDataType)
            {
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::String:
                    return ExpressionDataType::Double;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::String:
            return leftDataType;
        case ExpressionDataType::Guid:
            switch (rightDataType)
            {
                case ExpressionDataType::Guid:
                case ExpressionDataType::String:
                    return ExpressionDataType::Guid;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Guid\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        case ExpressionDataType::DateTime:
            switch (rightDataType)
            {
                case ExpressionDataType::DateTime:
                case ExpressionDataType::String:
                    return ExpressionDataType::DateTime;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::Guid:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"DateTime\" and \"" + string(EnumName(rightDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ExpressionDataType ExpressionTree::DeriveBooleanOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const
{
    if (leftDataType == ExpressionDataType::Boolean && rightDataType == ExpressionDataType::Boolean)
        return ExpressionDataType::Boolean;

    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftDataType)) + "\" and \"" + string(EnumName(rightDataType)) + "\"");
}

const ValueExpressionPtr& ExpressionTree::Coalesce(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue) const
{
    if (testValue->DataType != defaultValue->DataType)
        throw ExpressionTreeException("\"Coalesce\"/\"IsNull\" function arguments must be the same type");

    if (testValue->IsNullable)
    {
        NullableType value = Cast<NullableType>(testValue->Value);

        if (value.HasValue())
            return testValue;

        return defaultValue;
    }

    return testValue;
}

ValueExpressionPtr ExpressionTree::Convert(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& targetType) const
{
    if (targetType->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"Convert\" function target type, second argument, must be string type");

    Nullable<string> targetTypeValue = targetType->ValueAsString();

    if (!targetTypeValue.HasValue())
        throw ExpressionTreeException("\"Convert\" function target type, second argument, is null");

    string targetTypeName = targetTypeValue.GetValueOrDefault();

    // Remove any "System." prefix: 01234567
    if (StartsWith(targetTypeName, "System.") && targetTypeName.size() > 7)
        targetTypeName = targetTypeName.substr(7);

    ExpressionDataType targetDataType = ExpressionDataType::Undefined;
    bool foundDataType = false;

    for (int32_t i = 0; i < ExpressionDataTypeLength; i++)
    {
        if (IsEqual(targetTypeName, ExpressionDataTypeAcronym[i]))
        {
            targetDataType = static_cast<ExpressionDataType>(i);
            foundDataType = true;
            break;
        }
    }

    if (!foundDataType || targetDataType == ExpressionDataType::Undefined)
        throw ExpressionTreeException("Specified \"Convert\" function target type \"" + targetTypeValue.GetValueOrDefault() + "\", second argument, is not supported");

    return Convert(sourceValue, targetDataType);
}

ValueExpressionPtr ExpressionTree::IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const
{
    if (testValue->DataType != ExpressionDataType::Boolean)
        throw ExpressionTreeException("\"IIf\" function test value, first argument, must be boolean type");

    if (leftResultValue->DataType != rightResultValue->DataType)
        throw ExpressionTreeException("\"IIf\" function result values, second and third arguments, must be the same type");

    if (testValue->IsNullable)
    {
        Nullable<bool> value = Cast<Nullable<bool>>(testValue->Value);

        if (value.HasValue())
            return value.GetValueOrDefault() ? Evaluate(leftResultValue) : Evaluate(rightResultValue);

        // Null test expression evaluates to false, that is, right expression
        return Evaluate(rightResultValue);
    }

    return Cast<bool>(testValue->Value) ? Evaluate(leftResultValue) : Evaluate(rightResultValue);
}

ValueExpressionPtr ExpressionTree::IsRegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("IsRegExMatch", regexValue, testValue, false);
}

ValueExpressionPtr ExpressionTree::Len(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"Len\" function source value, first argument, must be string type");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionDataType::Int32);

    const string sourceText = sourceValue->IsNullable ?
        Cast<Nullable<string>>(sourceValue->Value).GetValueOrDefault() :
        Cast<string>(sourceValue->Value);

    return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, sourceText.size());
}

ValueExpressionPtr ExpressionTree::RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("RegExVal", regexValue, testValue, true);
}

ValueExpressionPtr ExpressionTree::SubString(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const
{
    if (sourceValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"SubString\" function source value, first argument, must be string type");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionDataType::String);

    if (!IsIntegerType(indexValue->DataType))
        throw ExpressionTreeException("\"SubString\" function index value, second argument, must be an integer type");

    if (!IsIntegerType(lengthValue->DataType))
        throw ExpressionTreeException("\"SubString\" function length value, third argument, must be an integer type");

    int32_t index, length = -1;

    const string sourceText = sourceValue->IsNullable ?
        Cast<Nullable<string>>(sourceValue->Value).GetValueOrDefault() :
        Cast<string>(sourceValue->Value);

    if (indexValue->IsNullable)
    {
        NullableType value = Cast<NullableType>(indexValue->Value);

        if (!value.HasValue())
            throw ExpressionTreeException("\"SubString\" function index value, second argument, is null");

        switch (indexValue->DataType)
        {
            case ExpressionDataType::Boolean:
                index = Cast<Nullable<bool>>(indexValue->Value).GetValueOrDefault() ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                index = Cast<Nullable<int32_t>>(indexValue->Value).GetValueOrDefault();
                break;
            case ExpressionDataType::Int64:
                index = static_cast<int32_t>(Cast<Nullable<int64_t>>(indexValue->Value).GetValueOrDefault());
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }
    else
    {
        switch (indexValue->DataType)
        {
            case ExpressionDataType::Boolean:
                index = Cast<bool>(indexValue->Value) ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                index = Cast<int32_t>(indexValue->Value);
                break;
            case ExpressionDataType::Int64:
                index = static_cast<int32_t>(Cast<int64_t>(indexValue->Value));
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }

    if (lengthValue->IsNullable)
    {
        NullableType value = Cast<NullableType>(lengthValue->Value);

        if (value.HasValue())
        {
            switch (lengthValue->DataType)
            {
                case ExpressionDataType::Boolean:
                    length = Cast<Nullable<bool>>(lengthValue->Value).GetValueOrDefault() ? 1 : 0;
                    break;
                case ExpressionDataType::Int32:
                    length = Cast<Nullable<int32_t>>(lengthValue->Value).GetValueOrDefault();
                    break;
                case ExpressionDataType::Int64:
                    length = static_cast<int32_t>(Cast<Nullable<int64_t>>(lengthValue->Value).GetValueOrDefault());
                    break;
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
        }
    }
    else
    {
        switch (lengthValue->DataType)
        {
            case ExpressionDataType::Boolean:
                length = Cast<bool>(lengthValue->Value) ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                length = Cast<int32_t>(lengthValue->Value);
                break;
            case ExpressionDataType::Int64:
                length = static_cast<int32_t>(Cast<int64_t>(lengthValue->Value));
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }

    }

    if (length > -1)
        return NewSharedPtr<ValueExpression>(ExpressionDataType::String, sourceText.substr(index, length));

    return NewSharedPtr<ValueExpression>(ExpressionDataType::String, sourceText.substr(index));
}

ValueExpressionPtr ExpressionTree::Trim(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"Trim\" function source value, first argument, must be string type");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionDataType::String);

    const string sourceText = sourceValue->IsNullable ?
        Cast<Nullable<string>>(sourceValue->Value).GetValueOrDefault() :
        Cast<string>(sourceValue->Value);

    return NewSharedPtr<ValueExpression>(ExpressionDataType::String, GSF::TimeSeries::Trim(sourceText));
}

ValueExpressionPtr ExpressionTree::Multiply(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Multiply<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Multiply<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Multiply<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Multiply<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply multiplication \"*\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Divide(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Divide<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Divide<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Divide<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Divide<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply division \"/\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Modulus(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Modulus<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Modulus<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Modulus<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Modulus<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply modulus \"%\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Add(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Add<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Add<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Add<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Add<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply addition \"+\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Subtract(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Subtract<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Subtract<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Subtract<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Subtract<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply subtraction \"-\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitShiftLeft(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(leftValue->DataType);

    if (!IsIntegerType(rightValue->DataType))
        throw ExpressionTreeException("BitShift operation shift value must be an integer type");

    int32_t shiftValue;

    if (rightValue->IsNullable)
    {
        NullableType value = Cast<NullableType>(rightValue->Value);

        if (!value.HasValue())
            throw ExpressionTreeException("BitShift operation shift value is null");

        switch (rightValue->DataType)
        {
            case ExpressionDataType::Boolean:
                shiftValue = Cast<Nullable<bool>>(rightValue->Value).GetValueOrDefault() ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                shiftValue = Cast<Nullable<int32_t>>(rightValue->Value).GetValueOrDefault();
                break;
            case ExpressionDataType::Int64:
                shiftValue = static_cast<int32_t>(Cast<Nullable<int64_t>>(rightValue->Value).GetValueOrDefault());
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }
    else
    {
        switch (rightValue->DataType)
        {
            case ExpressionDataType::Boolean:
                shiftValue = Cast<bool>(rightValue->Value) ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                shiftValue = Cast<int32_t>(rightValue->Value);
                break;
            case ExpressionDataType::Int64:
                shiftValue = static_cast<int32_t>(Cast<int64_t>(rightValue->Value));
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }

    switch (leftValue->DataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, BitShiftLeft<int32_t>(leftValue->ValueAsInt32(), shiftValue));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, BitShiftLeft<int64_t>(leftValue->ValueAsInt64(), shiftValue));
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply left bit-shift \"<<\" operator to \"" + string(EnumName(leftValue->DataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitShiftRight(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(leftValue->DataType);

    if (!IsIntegerType(rightValue->DataType))
        throw ExpressionTreeException("BitShift operation shift value must be an integer type");

    int32_t shiftValue;

    if (rightValue->IsNullable)
    {
        NullableType value = Cast<NullableType>(rightValue->Value);

        if (!value.HasValue())
            throw ExpressionTreeException("BitShift operation shift value is null");

        switch (rightValue->DataType)
        {
            case ExpressionDataType::Boolean:
                shiftValue = Cast<Nullable<bool>>(rightValue->Value).GetValueOrDefault() ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                shiftValue = Cast<Nullable<int32_t>>(rightValue->Value).GetValueOrDefault();
                break;
            case ExpressionDataType::Int64:
                shiftValue = static_cast<int32_t>(Cast<Nullable<int64_t>>(rightValue->Value).GetValueOrDefault());
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }
    else
    {
        switch (rightValue->DataType)
        {
            case ExpressionDataType::Boolean:
                shiftValue = Cast<bool>(rightValue->Value) ? 1 : 0;
                break;
            case ExpressionDataType::Int32:
                shiftValue = Cast<int32_t>(rightValue->Value);
                break;
            case ExpressionDataType::Int64:
                shiftValue = static_cast<int32_t>(Cast<int64_t>(rightValue->Value));
                break;
            default:
                throw ExpressionTreeException("Unexpected expression data type encountered");
        }
    }

    switch (leftValue->DataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, BitShiftRight<int32_t>(leftValue->ValueAsInt32(), shiftValue));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, BitShiftRight<int64_t>(leftValue->ValueAsInt64(), shiftValue));
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply right bit-shift \">>\" operator to \"" + string(EnumName(leftValue->DataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitwiseAnd(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, BitwiseAnd<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, BitwiseAnd<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply bitwise \"&\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitwiseOr(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(dataType);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, BitwiseOr<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, BitwiseOr<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
        case ExpressionDataType::Double:
        case ExpressionDataType::Boolean:
        case ExpressionDataType::String:
        case ExpressionDataType::Guid:
        case ExpressionDataType::DateTime:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply bitwise \"|\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Compare(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()) < 0);
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThan<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply less than \"<\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Compare(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()) <= 0);
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, LessThanOrEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply less than or equal \"<=\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Compare(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()) > 0);
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThan<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than \">\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Compare(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()) >= 0);
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, GreaterThanOrEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Boolean:
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than or equal \">=\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Equal(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, IsEqual(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()));
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Equal<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply equal \"=\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::NotEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, dataType);
    const ValueExpressionPtr right = Convert(rightValue, dataType);

    switch (dataType)
    {
        case ExpressionDataType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, !IsEqual(left->ValueAsString().GetValueOrDefault(), right->ValueAsString().GetValueOrDefault()));
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, NotEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionDataType::Undefined:
            throw ExpressionTreeException("Cannot apply not equal \"<>\" operator to \"" + string(EnumName(dataType)) + "\" type");
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}

ValueExpressionPtr ExpressionTree::IsNull(const ValueExpressionPtr& leftValue) const
{
    return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, leftValue->IsNull());
}

ValueExpressionPtr ExpressionTree::IsNotNull(const ValueExpressionPtr& leftValue) const
{
    return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, !leftValue->IsNull());
}

ValueExpressionPtr ExpressionTree::Like(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    if (leftValue->DataType != ExpressionDataType::String || rightValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("Cannot perform \"LIKE\" operation on \"" + string(EnumName(leftValue->DataType)) + "\" and \"" + string(EnumName(rightValue->DataType)) + "\"");

    if (rightValue->IsNull())
        throw ExpressionTreeException("Right operand of \"LIKE\" expression is null");

    const string leftOperand = leftValue->IsNullable ?
        Cast<Nullable<string>>(leftValue->Value).GetValueOrDefault() :
        Cast<string>(leftValue->Value);

    const string rightOperand = rightValue->IsNullable ?
        Cast<Nullable<string>>(rightValue->Value).GetValueOrDefault() :
        Cast<string>(rightValue->Value);

    string testExpression = Replace(rightOperand, "%", "*", false);
    const bool startsWithWildcard = StartsWith(testExpression, "*", false);
    const bool endsWithWildcard = EndsWith(testExpression, "*", false);

    if (startsWithWildcard)
        testExpression = testExpression.substr(1);

    if (endsWithWildcard && !testExpression.empty())
        testExpression = testExpression.substr(0, testExpression.size() - 1);

    // "*" or "**" expression means match everything
    if (testExpression.empty())
        return ExpressionTree::True;

    // Wild cards in the middle of the string are not supported
    if (Contains(testExpression, "*", false))
        throw ExpressionTreeException("Right operand of \"LIKE\" expression \"" + rightOperand + "\" has an invalid pattern");

    if (startsWithWildcard && EndsWith(leftOperand, testExpression))
        return ExpressionTree::True;

    if (endsWithWildcard && StartsWith(leftOperand, testExpression))
        return ExpressionTree::True;

    if (startsWithWildcard && endsWithWildcard && Contains(leftOperand, testExpression))
        return ExpressionTree::True;

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::NotLike(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    const ValueExpressionPtr likeResult = Like(leftValue, rightValue);

    const bool result = likeResult->IsNullable ?
        Cast<Nullable<bool>>(likeResult->Value).GetValueOrDefault() :
        Cast<bool>(likeResult->Value);

    return result ? ExpressionTree::False : ExpressionTree::True;
}

ValueExpressionPtr ExpressionTree::And(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    if (leftValue->DataType != ExpressionDataType::Boolean || rightValue->DataType != ExpressionDataType::Boolean)
        throw ExpressionTreeException("Cannot perform \"AND\" operation on \"" + string(EnumName(leftValue->DataType)) + "\" and \"" + string(EnumName(rightValue->DataType)) + "\"");

    const bool left = leftValue->ValueAsBoolean().GetValueOrDefault();
    const bool right = rightValue->ValueAsBoolean().GetValueOrDefault();

    return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, left && right);
}

ValueExpressionPtr ExpressionTree::Or(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionDataType::Boolean);

    if (leftValue->DataType != ExpressionDataType::Boolean || rightValue->DataType != ExpressionDataType::Boolean)
        throw ExpressionTreeException("Cannot perform \"OR\" operation on \"" + string(EnumName(leftValue->DataType)) + "\" and \"" + string(EnumName(rightValue->DataType)) + "\"");

    const bool left = leftValue->ValueAsBoolean().GetValueOrDefault();
    const bool right = rightValue->ValueAsBoolean().GetValueOrDefault();

    return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, left || right);
}

template <typename T>
T ExpressionTree::Multiply(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() * rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::Divide(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() / rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::Modulus(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() % rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::Add(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() + rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::Subtract(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() - rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::BitShiftLeft(const Nullable<T>& operandValue, int32_t shiftValue)
{
    return operandValue.GetValueOrDefault() << shiftValue;
}

template <typename T>
T ExpressionTree::BitShiftRight(const Nullable<T>& operandValue, int32_t shiftValue)
{
    return operandValue.GetValueOrDefault() >> shiftValue;
}

template <typename T>
T ExpressionTree::BitwiseAnd(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() & rightValue.GetValueOrDefault();
}

template <typename T>
T ExpressionTree::BitwiseOr(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() | rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::LessThan(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() < rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::LessThanOrEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() <= rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::GreaterThan(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() > rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::GreaterThanOrEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() >= rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::Equal(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() == rightValue.GetValueOrDefault();
}

template <typename T>
bool ExpressionTree::NotEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue)
{
    return leftValue.GetValueOrDefault() != rightValue.GetValueOrDefault();
}

ValueExpressionPtr ExpressionTree::Convert(const ValueExpressionPtr& sourceValue, ExpressionDataType targetDataType) const
{
    // If source value is Null, result is Null, regardless of target type
    if (sourceValue->IsNull())
        return NullValue(targetDataType);

    Object targetValue;

    switch (sourceValue->DataType)
    {
        case ExpressionDataType::Boolean:
        {
            const bool result = sourceValue->IsNullable ?
                Cast<Nullable<bool>>(sourceValue->Value).GetValueOrDefault() : 
                Cast<bool>(sourceValue->Value);

            const int32_t value = result ? 1 : 0;

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value;
                    break;
                case ExpressionDataType::Int32:
                    targetValue = value;
                    break;
                case ExpressionDataType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Boolean\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Int32:
        {
            const int32_t value = sourceValue->IsNullable ?
                Cast<Nullable<int32_t>>(sourceValue->Value).GetValueOrDefault() :
                Cast<int32_t>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionDataType::Int32:
                    targetValue = value;
                    break;
                case ExpressionDataType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Int32\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Int64:
        {
            const int64_t value = sourceValue->IsNullable ?
                Cast<Nullable<int64_t>>(sourceValue->Value).GetValueOrDefault() :
                Cast<int64_t>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionDataType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionDataType::Int64:
                    targetValue = value;
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Int64\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Decimal:
        {
            const decimal_t value = sourceValue->IsNullable ?
                Cast<Nullable<decimal_t>>(sourceValue->Value).GetValueOrDefault() :
                Cast<decimal_t>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value == decimal_t(0);
                    break;
                case ExpressionDataType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionDataType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = value;
                    break;
                case ExpressionDataType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Decimal\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Double:
        {
            const float64_t value = sourceValue->IsNullable ?
                Cast<Nullable<float64_t>>(sourceValue->Value).GetValueOrDefault() :
                Cast<float64_t>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value == 0.0;
                    break;
                case ExpressionDataType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionDataType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = value;
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Double\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::String:
        {
            const string value = sourceValue->IsNullable ?
                Cast<Nullable<string>>(sourceValue->Value).GetValueOrDefault() :
                Cast<string>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    if (IsEqual(value, "true") || IsEqual(value, "1"))
                        targetValue = true;
                    else if (IsEqual(value, "false") || IsEqual(value, "0"))
                        targetValue = false;
                    else
                        throw ExpressionTreeException("\"String\" value not recognized as a valid \"Boolean\"");
                    break;
                case ExpressionDataType::Int32:
                    targetValue = stoi(value);
                    break;
                case ExpressionDataType::Int64:
                    targetValue = stoll(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = decimal_t(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = stod(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = value;
                    break;
                case ExpressionDataType::Guid:
                    targetValue = ToGuid(value.c_str());
                    break;
                case ExpressionDataType::DateTime:
                    targetValue = ParseTimestamp(value.c_str());
                    break;
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Guid:
        {
            const Guid value = sourceValue->IsNullable ?
                Cast<Nullable<Guid>>(sourceValue->Value).GetValueOrDefault() :
                Cast<Guid>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::String:
                    targetValue = ToString(value);
                    break;
                case ExpressionDataType::Guid:
                    targetValue = value;
                    break;
                case ExpressionDataType::Boolean:
                case ExpressionDataType::Int32:
                case ExpressionDataType::Int64:
                case ExpressionDataType::Decimal:
                case ExpressionDataType::Double:
                case ExpressionDataType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Guid\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::DateTime:
        {
            const time_t value = sourceValue->IsNullable ?
                Cast<Nullable<time_t>>(sourceValue->Value).GetValueOrDefault() :
                Cast<time_t>(sourceValue->Value);

            switch (targetDataType)
            {
                case ExpressionDataType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionDataType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionDataType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionDataType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionDataType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionDataType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionDataType::Guid:
                    throw ExpressionTreeException("Cannot convert \"DateTime\" data type to \"" + string(EnumName(targetDataType)) + "\"");
                case ExpressionDataType::DateTime:
                    targetValue = value;
                    break;
                default:
                    throw ExpressionTreeException("Unexpected expression data type encountered");
            }
            break;
        }
        case ExpressionDataType::Undefined:
            // Change Undefined values to Nullable of target type
            return NullValue(targetDataType);
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }

    return NewSharedPtr<ValueExpression>(targetDataType, targetValue);
}

ValueExpressionPtr ExpressionTree::EvaluateRegEx(const string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, bool returnMatchedValue) const
{
    if (regexValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"" + functionName + "\" function expression value, first argument, must be string type");

    if (testValue->DataType != ExpressionDataType::String)
        throw ExpressionTreeException("\"" + functionName + "\" function test value, second argument, must be string type");

    string expressionText, testText;

    if (regexValue->IsNullable)
    {
        Nullable<string> value = Cast<Nullable<string>>(regexValue->Value);

        if (!value.HasValue())
            return NullValue(ExpressionDataType::String);

        expressionText = value.GetValueOrDefault();
    }
    else
    {
        expressionText = Cast<string>(regexValue->Value);
    }

    if (testValue->IsNullable)
    {
        Nullable<string> value = Cast<Nullable<string>>(testValue->Value);

        if (!value.HasValue())
            return NullValue(ExpressionDataType::String);

        testText = value.GetValueOrDefault();
    }
    else
    {
        testText = Cast<string>(testValue->Value);
    }

    cmatch match;
    const regex expression(expressionText);
    const bool result = regex_match(testText.c_str(), match, expression);

    if (returnMatchedValue)
    {
        // RegExVal returns any matched value, otherwise empty string
        if (result)
            return NewSharedPtr<ValueExpression>(ExpressionDataType::String, string(match[0]));

        return ExpressionTree::EmptyString;
    }

    // IsRegExMatch returns boolean result for if there was a matched value
    return result ? ExpressionTree::True : ExpressionTree::False;
}

ExpressionTree::ExpressionTree(string measurementTableName, const DataTablePtr& measurements) :
    MeasurementTableName(move(measurementTableName)),
    Measurements(measurements)
{
}

ValueExpressionPtr ExpressionTree::Evaluate(const DataRowPtr& row)
{
    m_currentRow = row;
    return Evaluate(Root);
}

const ValueExpressionPtr ExpressionTree::True = NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, true);

const ValueExpressionPtr ExpressionTree::False = NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, false);

const ValueExpressionPtr ExpressionTree::EmptyString = NewSharedPtr<ValueExpression>(ExpressionDataType::String, string());

ValueExpressionPtr ExpressionTree::NullValue(ExpressionDataType targetDataType)
{
    // Change Undefined values to Nullable of target type
    switch (targetDataType)
    {
        case ExpressionDataType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Boolean, Nullable<bool>(nullptr), true);
        case ExpressionDataType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, Nullable<int32_t>(nullptr), true);
        case ExpressionDataType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, Nullable<int64_t>(nullptr), true);
        case ExpressionDataType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, Nullable<decimal_t>(nullptr), true);
        case ExpressionDataType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Double, Nullable<float64_t>(nullptr), true);
        case ExpressionDataType::String:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::String, Nullable<string>(nullptr), true);
        case ExpressionDataType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::Guid, Nullable<Guid>(nullptr), true);
        case ExpressionDataType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionDataType::DateTime, Nullable<time_t>(nullptr), true);
        default:
            throw ExpressionTreeException("Unexpected expression data type encountered");
    }
}