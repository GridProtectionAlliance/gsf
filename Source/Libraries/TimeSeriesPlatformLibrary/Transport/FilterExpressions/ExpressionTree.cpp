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
#include <utility>

using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

const int32_t GSF::TimeSeries::Transport::ExpressionValueTypeLength = static_cast<int32_t>(ExpressionValueType::Undefined) + 1;

const char* GSF::TimeSeries::Transport::ExpressionValueTypeAcronym[] =
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

const char* GSF::TimeSeries::Transport::EnumName(ExpressionValueType valueType)
{
    return ExpressionValueTypeAcronym[static_cast<int32_t>(valueType)];
}

const char* GSF::TimeSeries::Transport::ExpressionUnaryTypeAcronym[] =
{
    "+",
    "-",
    "~"
};

const char* GSF::TimeSeries::Transport::EnumName(ExpressionUnaryType unaryType)
{
    return ExpressionUnaryTypeAcronym[static_cast<int32_t>(unaryType)];
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

const char* GSF::TimeSeries::Transport::EnumName(ExpressionOperatorType operatorType)
{
    return ExpressionOperatorTypeAcronym[static_cast<int32_t>(operatorType)];
}

bool Transport::IsIntegerType(ExpressionValueType valueType)
{
    switch (valueType)
    {
        case ExpressionValueType::Boolean:
        case ExpressionValueType::Int32:
        case ExpressionValueType::Int64:
            return true;
        default:
            return false;
    }
}

bool Transport::IsNumericType(ExpressionValueType valueType)
{
    switch (valueType)
    {
        case ExpressionValueType::Boolean:
        case ExpressionValueType::Int32:
        case ExpressionValueType::Int64:
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
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

Expression::Expression(ExpressionType type) :
    Type(type)
{
}

ValueExpression::ValueExpression(ExpressionValueType valueType, const Object& value, bool valueIsNullable) :
    Expression(ExpressionType::Value),
    Value(value),
    ValueType(valueType),
    ValueIsNullable(valueIsNullable)
{
}

void ValueExpression::ValidateValueType(ExpressionValueType valueType) const
{
    if (ValueType != valueType)
        throw ExpressionTreeException("Cannot read expression value as \"" + string(EnumName(valueType)) + "\", type is \"" + string(EnumName(ValueType)) + "\"");
}

bool ValueExpression::IsNull() const
{
    if (ValueIsNullable)
        return !Cast<NullableType>(Value).HasValue();

    return false;
}

string ValueExpression::ToString() const
{
    switch (ValueType)
    {
        case ExpressionValueType::Boolean:
            return GSF::TimeSeries::ToString(ValueAsNullableBoolean());
        case ExpressionValueType::Int32:
            return GSF::TimeSeries::ToString(ValueAsNullableInt32());
        case ExpressionValueType::Int64:
            return GSF::TimeSeries::ToString(ValueAsNullableInt64());
        case ExpressionValueType::Decimal:
            return GSF::TimeSeries::ToString(ValueAsNullableDecimal());
        case ExpressionValueType::Double:
            return GSF::TimeSeries::ToString(ValueAsNullableDouble());
        case ExpressionValueType::String:
            return GSF::TimeSeries::ToString(ValueAsNullableString());
        case ExpressionValueType::Guid:
            return GSF::TimeSeries::ToString(ValueAsNullableGuid());
        case ExpressionValueType::DateTime:
            return GSF::TimeSeries::ToString(ValueAsNullableDateTime());
        case ExpressionValueType::Undefined:
            return nullptr;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

bool ValueExpression::ValueAsBoolean() const
{
    ValidateValueType(ExpressionValueType::Boolean);

    if (ValueIsNullable)
        return Cast<Nullable<bool>>(Value).GetValueOrDefault();

    return Cast<bool>(Value);
}

Nullable<bool> ValueExpression::ValueAsNullableBoolean() const
{
    ValidateValueType(ExpressionValueType::Boolean);

    if (ValueIsNullable)
        return Cast<Nullable<bool>>(Value);

    return Cast<bool>(Value);
}

int32_t ValueExpression::ValueAsInt32() const
{
    ValidateValueType(ExpressionValueType::Int32);

    if (ValueIsNullable)
        return Cast<Nullable<int32_t>>(Value).GetValueOrDefault();

    return Cast<int32_t>(Value);
}

Nullable<int32_t> ValueExpression::ValueAsNullableInt32() const
{
    ValidateValueType(ExpressionValueType::Int32);

    if (ValueIsNullable)
        return Cast<Nullable<int32_t>>(Value);

    return Cast<int32_t>(Value);
}

int64_t ValueExpression::ValueAsInt64() const
{
    ValidateValueType(ExpressionValueType::Int64);

    if (ValueIsNullable)
        return Cast<Nullable<int64_t>>(Value).GetValueOrDefault();

    return Cast<int64_t>(Value);
}

Nullable<int64_t> ValueExpression::ValueAsNullableInt64() const
{
    ValidateValueType(ExpressionValueType::Int64);

    if (ValueIsNullable)
        return Cast<Nullable<int64_t>>(Value);

    return Cast<int64_t>(Value);
}

decimal_t ValueExpression::ValueAsDecimal() const
{
    ValidateValueType(ExpressionValueType::Decimal);

    if (ValueIsNullable)
        return Cast<Nullable<decimal_t>>(Value).GetValueOrDefault();

    return Cast<decimal_t>(Value);
}

Nullable<decimal_t> ValueExpression::ValueAsNullableDecimal() const
{
    ValidateValueType(ExpressionValueType::Decimal);

    if (ValueIsNullable)
        return Cast<Nullable<decimal_t>>(Value);

    return Cast<decimal_t>(Value);
}

float64_t ValueExpression::ValueAsDouble() const
{
    ValidateValueType(ExpressionValueType::Double);

    if (ValueIsNullable)
        return Cast<Nullable<float64_t>>(Value).GetValueOrDefault();

    return Cast<float64_t>(Value);
}

Nullable<float64_t> ValueExpression::ValueAsNullableDouble() const
{
    ValidateValueType(ExpressionValueType::Double);

    if (ValueIsNullable)
        return Cast<Nullable<float64_t>>(Value);

    return Cast<float64_t>(Value);
}

string ValueExpression::ValueAsString() const
{
    ValidateValueType(ExpressionValueType::String);

    if (ValueIsNullable)
        return Cast<Nullable<string>>(Value).GetValueOrDefault();

    return Cast<string>(Value);
}

Nullable<string> ValueExpression::ValueAsNullableString() const
{
    ValidateValueType(ExpressionValueType::String);

    if (ValueIsNullable)
        return Cast<Nullable<string>>(Value);

    return Cast<string>(Value);
}

Guid ValueExpression::ValueAsGuid() const
{
    ValidateValueType(ExpressionValueType::Guid);

    if (ValueIsNullable)
        return Cast<Nullable<Guid>>(Value).GetValueOrDefault();

    return Cast<Guid>(Value);
}

Nullable<Guid> ValueExpression::ValueAsNullableGuid() const
{
    ValidateValueType(ExpressionValueType::Guid);

    if (ValueIsNullable)
        return Cast<Nullable<Guid>>(Value);

    return Cast<Guid>(Value);
}

time_t ValueExpression::ValueAsDateTime() const
{
    ValidateValueType(ExpressionValueType::DateTime);

    if (ValueIsNullable)
        return Cast<Nullable<time_t>>(Value).GetValueOrDefault();

    return Cast<time_t>(Value);
}

Nullable<time_t> ValueExpression::ValueAsNullableDateTime() const
{
    ValidateValueType(ExpressionValueType::DateTime);

    if (ValueIsNullable)
        return Cast<Nullable<time_t>>(Value);

    return Cast<time_t>(Value);
}

UnaryExpression::UnaryExpression(ExpressionUnaryType unaryType, const ExpressionPtr& value) :
    Expression(ExpressionType::Unary),
    UnaryType(unaryType),
    Value(value)
{
}

ColumnExpression::ColumnExpression(const DataColumnPtr& dataColumn) :
    Expression(ExpressionType::Column),
    DataColumn(dataColumn)
{
}

OperatorExpression::OperatorExpression(ExpressionOperatorType operatorType, const ExpressionPtr& leftValue, const ExpressionPtr& rightValue) :
    Expression(ExpressionType::Operator),
    OperatorType(operatorType),
    LeftValue(leftValue),
    RightValue(rightValue)
{
}

InListExpression::InListExpression(const ExpressionPtr& value, ExpressionCollectionPtr arguments, bool notInList) :
    Expression(ExpressionType::InList),
    Value(value),
    Arguments(std::move(arguments)),
    NotInList(notInList)
{
}

FunctionExpression::FunctionExpression(ExpressionFunctionType functionType, ExpressionCollectionPtr arguments) :
    Expression(ExpressionType::Function),
    FunctionType(functionType),
    Arguments(std::move(arguments))
{
}

ValueExpressionPtr ExpressionTree::Evaluate(const ExpressionPtr& expression, ExpressionValueType targetValueType) const
{
    if (expression == nullptr)
        return NullValue(targetValueType);

    // All expression nodes should evaluate to a value expression
    switch (expression->Type)
    {
        case ExpressionType::Value:
        {
            const ValueExpressionPtr valueExpression = CastSharedPtr<ValueExpression>(expression);

            // Change Undefined NULL values to Nullable of target type
            if (valueExpression->ValueType == ExpressionValueType::Undefined)
                return NullValue(targetValueType);

            return valueExpression;
        }
        case ExpressionType::Unary:
            return EvaluateUnary(expression);
        case ExpressionType::Column:
            return EvaluateColumn(expression);
        case ExpressionType::InList:
            return EvaluateInList(expression);
        case ExpressionType::Function:
            return EvaluateFunction(expression);
        case ExpressionType::Operator:
            return EvaluateOperator(expression);
        default:
            throw ExpressionTreeException("Unexpected expression type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateUnary(const ExpressionPtr& expression) const
{
    const UnaryExpressionPtr unaryExpression = CastSharedPtr<UnaryExpression>(expression);
    const ValueExpressionPtr unaryValue = Evaluate(unaryExpression->Value);
    const ExpressionValueType unaryValueType = unaryValue->ValueType;

    // If unary value is Null, result is Null
    if (unaryValue->IsNull())
        return NullValue(unaryValueType);

    switch (unaryValueType)
    {
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, ApplyIntegerUnaryOperation<int32_t>(unaryValue->ValueAsInt32(), unaryExpression->UnaryType));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, ApplyIntegerUnaryOperation<int64_t>(unaryValue->ValueAsInt64(), unaryExpression->UnaryType));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, ApplyFloatingPointUnaryOperation<decimal_t>(unaryValue->ValueAsDecimal(), unaryExpression->UnaryType, unaryValueType));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, ApplyFloatingPointUnaryOperation<float64_t>(unaryValue->ValueAsDouble(), unaryExpression->UnaryType, unaryValueType));
        case ExpressionValueType::Boolean:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply unary \"" + string(EnumName(unaryExpression->UnaryType)) + "\" operator to \"" + string(EnumName(unaryValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateColumn(const ExpressionPtr& expression) const
{
    const ColumnExpressionPtr columnExpression = CastSharedPtr<ColumnExpression>(expression);
    const DataColumnPtr& column = columnExpression->DataColumn;

    if (column == nullptr)
        throw ExpressionTreeException("Encountered column expression with undefined data column reference.");

    const int32_t columnIndex = column->Index();
    ExpressionValueType valueType;
    Object value;

    // Map column DataType to ExpressionType, storing equivalent Nullable<T> literal value
    switch (column->Type())
    {
        case DataType::String:
            valueType = ExpressionValueType::String;
            value = m_currentRow->ValueAsString(columnIndex);
            break;
        case DataType::Boolean:
            valueType = ExpressionValueType::Boolean;
            value = m_currentRow->ValueAsBoolean(columnIndex);
            break;
        case DataType::DateTime:
            valueType = ExpressionValueType::DateTime;
            value = m_currentRow->ValueAsDateTime(columnIndex);
            break;
        case DataType::Single:
            valueType = ExpressionValueType::Double;
            value = CastAsNullable<double>(m_currentRow->ValueAsSingle(columnIndex));
            break;
        case DataType::Double:
            valueType = ExpressionValueType::Double;
            value = m_currentRow->ValueAsDouble(columnIndex);
            break;
        case DataType::Decimal:
            valueType = ExpressionValueType::Decimal;
            value = m_currentRow->ValueAsDecimal(columnIndex);
            break;
        case DataType::Guid:
            valueType = ExpressionValueType::Guid;
            value = m_currentRow->ValueAsGuid(columnIndex);
            break;
        case DataType::Int8:
            valueType = ExpressionValueType::Int32;
            value = CastAsNullable<int32_t>(m_currentRow->ValueAsInt8(columnIndex));
            break;
        case DataType::Int16:
            valueType = ExpressionValueType::Int32;
            value = CastAsNullable<int32_t>(m_currentRow->ValueAsInt16(columnIndex));
            break;
        case DataType::Int32:
            valueType = ExpressionValueType::Int32;
            value = m_currentRow->ValueAsInt32(columnIndex);
            break;
        case DataType::UInt8:
            valueType = ExpressionValueType::Int32;
            value = CastAsNullable<int32_t>(m_currentRow->ValueAsUInt8(columnIndex));
            break;
        case DataType::UInt16:
            valueType = ExpressionValueType::Int32;
            value = CastAsNullable<int32_t>(m_currentRow->ValueAsUInt16(columnIndex));
            break;
        case DataType::Int64:;
            valueType = ExpressionValueType::Int64;
            value = m_currentRow->ValueAsInt64(columnIndex);
            break;
        case DataType::UInt32:
            valueType = ExpressionValueType::Int64;
            value = CastAsNullable<int64_t>(m_currentRow->ValueAsUInt32(columnIndex));
            break;
        case DataType::UInt64:
        {
            Nullable<uint64_t> value64U = m_currentRow->ValueAsUInt64(columnIndex);

            if (value64U.HasValue())
            {
                if (value64U.GetValueOrDefault() > static_cast<uint64_t>(Int64::MaxValue))
                {
                    valueType = ExpressionValueType::Double;
                    value = CastAsNullable<double>(value64U);
                }
                else
                {
                    valueType = ExpressionValueType::Int64;
                    value = CastAsNullable<int64_t>(value64U);
                }
            }
            else
            {
                valueType = ExpressionValueType::Int64;
                value = Nullable<int64_t>(nullptr);
            }

            break;
        }
        default:
            throw ExpressionTreeException("Unexpected column data type encountered");
    }

    // All literal expression values derived for columns are wrapped in Nullable<T>
    return NewSharedPtr<ValueExpression>(valueType, value, true);
}

ValueExpressionPtr ExpressionTree::EvaluateInList(const ExpressionPtr& expression) const
{
    const InListExpressionPtr inListExpression = CastSharedPtr<InListExpression>(expression);
    const ValueExpressionPtr inListValue = Evaluate(inListExpression->Value);
    const bool notInList = inListExpression->NotInList;

    // If in list test value is Null, result is Null
    if (inListValue->IsNull())
        return NullValue(inListValue->ValueType);

    const int32_t argumentCount = inListExpression->Arguments->size();

    for (int32_t i = 0; i < argumentCount; i++)
    {
        const ValueExpressionPtr argumentValue = Evaluate(inListExpression->Arguments->at(i));
        const ExpressionValueType valueType = DeriveComparisonOperationValueType(ExpressionOperatorType::Equal, inListValue->ValueType, argumentValue->ValueType);
        const ValueExpressionPtr result = Equal(inListValue, argumentValue, valueType);

        if (result->ValueAsBoolean())
            return notInList ? ExpressionTree::False : ExpressionTree::True;
    }

    return notInList ? ExpressionTree::True : ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::EvaluateFunction(const ExpressionPtr& expression) const
{
    const FunctionExpressionPtr functionExpression = CastSharedPtr<FunctionExpression>(expression);
    const ExpressionCollectionPtr arguments = functionExpression->Arguments;

    switch (functionExpression->FunctionType)
    {
        case ExpressionFunctionType::Coalesce:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"Coalesce\"/\"IsNull\" function expects 2 arguments, received " + ToString(arguments->size()));

            return Coalesce(Evaluate(arguments->at(0)), Evaluate(arguments->at(1)));
        case ExpressionFunctionType::Convert:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"Convert\" function expects 2 arguments, received " + ToString(arguments->size()));

            return Convert(Evaluate(arguments->at(0)), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::IIf:
            if (arguments->size() != 3)
                throw ExpressionTreeException("\"IIf\" function expects 3 arguments, received " + ToString(arguments->size()));

            // Not pre-evaluating IIf result value arguments - only evaluating desired path
            return IIf(Evaluate(arguments->at(0), ExpressionValueType::Boolean), arguments->at(1), arguments->at(2));
        case ExpressionFunctionType::IsRegExMatch:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"IsRegExMatch\" function expects 2 arguments, received " + ToString(arguments->size()));

            return IsRegExMatch(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::Len:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Len\" function expects 1 argument, received " + ToString(arguments->size()));

            return Len(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::RegExVal:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"RegExVal\" function expects 2 arguments, received " + ToString(arguments->size()));

            return RegExVal(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::SubString:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"SubString\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return SubString(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::Int32), NullValue(ExpressionValueType::Int32));

            return SubString(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::Int32), Evaluate(arguments->at(2), ExpressionValueType::Int32));
        case ExpressionFunctionType::Trim:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Trim\" function expects 1 argument, received " + ToString(arguments->size()));

            return Trim(Evaluate(arguments->at(0), ExpressionValueType::String));
        default:
            throw ExpressionTreeException("Unexpected function type encountered");
    }
}

ValueExpressionPtr ExpressionTree::EvaluateOperator(const ExpressionPtr& expression) const
{
    const OperatorExpressionPtr operatorExpression = CastSharedPtr<OperatorExpression>(expression);
    const ValueExpressionPtr leftValue = Evaluate(operatorExpression->LeftValue);
    const ValueExpressionPtr rightValue = Evaluate(operatorExpression->RightValue);
    const ExpressionValueType valueType = DeriveOperationValueType(operatorExpression->OperatorType, leftValue->ValueType, rightValue->ValueType);

    switch (operatorExpression->OperatorType)
    {
        case ExpressionOperatorType::Multiply:
            return Multiply(leftValue, rightValue, valueType);
        case ExpressionOperatorType::Divide:
            return Divide(leftValue, rightValue, valueType);
        case ExpressionOperatorType::Modulus:
            return Modulus(leftValue, rightValue, valueType);
        case ExpressionOperatorType::Add:
            return Add(leftValue, rightValue, valueType);
        case ExpressionOperatorType::Subtract:
            return Subtract(leftValue, rightValue, valueType);
        case ExpressionOperatorType::BitShiftLeft:
            return BitShiftLeft(leftValue, rightValue);
        case ExpressionOperatorType::BitShiftRight:
            return BitShiftRight(leftValue, rightValue);
        case ExpressionOperatorType::BitwiseAnd:
            return BitwiseAnd(leftValue, rightValue, valueType);
        case ExpressionOperatorType::BitwiseOr:
            return BitwiseOr(leftValue, rightValue, valueType);
        case ExpressionOperatorType::LessThan:
            return LessThan(leftValue, rightValue, valueType);
        case ExpressionOperatorType::LessThanOrEqual:
            return LessThanOrEqual(leftValue, rightValue, valueType);
        case ExpressionOperatorType::GreaterThan:
            return GreaterThan(leftValue, rightValue, valueType);
        case ExpressionOperatorType::GreaterThanOrEqual:
            return GreaterThanOrEqual(leftValue, rightValue, valueType);
        case ExpressionOperatorType::Equal:
            return Equal(leftValue, rightValue, valueType);
        case ExpressionOperatorType::NotEqual:
            return NotEqual(leftValue, rightValue, valueType);
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
T ExpressionTree::ApplyIntegerUnaryOperation(const T& unaryValue, ExpressionUnaryType unaryOperation)
{
    switch (unaryOperation)
    {
        case ExpressionUnaryType::Plus:
            return +unaryValue;
        case ExpressionUnaryType::Minus:
            return -unaryValue;
        case ExpressionUnaryType::Not:
            return ~unaryValue;
        default:
            throw ExpressionTreeException("Unexpected unary type encountered");
    }
}

template<typename T>
T ExpressionTree::ApplyFloatingPointUnaryOperation(const T& unaryValue, ExpressionUnaryType unaryOperation, ExpressionValueType unaryValueType)
{
    switch (unaryOperation)
    {
        case ExpressionUnaryType::Plus:
            return +unaryValue;
        case ExpressionUnaryType::Minus:
            return -unaryValue;
        case ExpressionUnaryType::Not:
            throw ExpressionTreeException("Cannot apply unary \"~\" operator to \"" + string(EnumName(unaryValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected unary type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const
{
    switch (operationType)
    {
        case ExpressionOperatorType::Multiply:
        case ExpressionOperatorType::Divide:
        case ExpressionOperatorType::Add:
        case ExpressionOperatorType::Subtract:
            return DeriveArithmeticOperationValueType(operationType, leftValueType, rightValueType);        
        case ExpressionOperatorType::Modulus:
        case ExpressionOperatorType::BitwiseAnd:
        case ExpressionOperatorType::BitwiseOr:
            return DeriveIntegerOperationValueType(operationType, leftValueType, rightValueType);        
        case ExpressionOperatorType::LessThan:
        case ExpressionOperatorType::LessThanOrEqual:
        case ExpressionOperatorType::GreaterThan:
        case ExpressionOperatorType::GreaterThanOrEqual:
        case ExpressionOperatorType::Equal:
        case ExpressionOperatorType::NotEqual:
            return DeriveComparisonOperationValueType(operationType, leftValueType, rightValueType);
        case ExpressionOperatorType::And:
        case ExpressionOperatorType::Or:
            return DeriveBooleanOperationValueType(operationType, leftValueType, rightValueType);
        case ExpressionOperatorType::BitShiftLeft:
        case ExpressionOperatorType::BitShiftRight:
        case ExpressionOperatorType::IsNull:
        case ExpressionOperatorType::IsNotNull:
        case ExpressionOperatorType::Like:
        case ExpressionOperatorType::NotLike:
            return leftValueType;
        default:
            throw ExpressionTreeException("Unexpected expression operator type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveArithmeticOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const
{
    switch (leftValueType)
    {
        case ExpressionValueType::Boolean:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                    return ExpressionValueType::Boolean;
                case ExpressionValueType::Int32:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int32:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int64:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Decimal:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Decimal\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Double:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftValueType)) + "\" and \"" + string(EnumName(rightValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveIntegerOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const
{
    switch (leftValueType)
    {
        case ExpressionValueType::Boolean:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                    return ExpressionValueType::Boolean;
                case ExpressionValueType::Int32:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int32:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int64:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::String:
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftValueType)) + "\" and \"" + string(EnumName(rightValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveComparisonOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const
{
    switch (leftValueType)
    {
        case ExpressionValueType::Boolean:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::String:
                    return ExpressionValueType::Boolean;
                case ExpressionValueType::Int32:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Boolean\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int32:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::String:
                    return ExpressionValueType::Int32;
                case ExpressionValueType::Int64:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int32\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Int64:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::String:
                    return ExpressionValueType::Int64;
                case ExpressionValueType::Decimal:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Int64\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Decimal:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::String:
                    return ExpressionValueType::Decimal;
                case ExpressionValueType::Double:
                    return ExpressionValueType::Double;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Decimal\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::Double:
            switch (rightValueType)
            {
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::String:
                    return ExpressionValueType::Double;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::String:
            return leftValueType;
        case ExpressionValueType::Guid:
            switch (rightValueType)
            {
                case ExpressionValueType::Guid:
                case ExpressionValueType::String:
                    return ExpressionValueType::Guid;
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Guid\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::DateTime:
            switch (rightValueType)
            {
                case ExpressionValueType::DateTime:
                case ExpressionValueType::String:
                    return ExpressionValueType::DateTime;
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::Guid:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"DateTime\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveBooleanOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const
{
    if (leftValueType == ExpressionValueType::Boolean && rightValueType == ExpressionValueType::Boolean)
        return ExpressionValueType::Boolean;

    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftValueType)) + "\" and \"" + string(EnumName(rightValueType)) + "\"");
}

const ValueExpressionPtr& ExpressionTree::Coalesce(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue)
{
    if (testValue->ValueType != defaultValue->ValueType)
        throw ExpressionTreeException("\"Coalesce\"/\"IsNull\" function arguments must be the same type");

    if (defaultValue->IsNull())
        throw ExpressionTreeException("\"Coalesce\"/\"IsNull\" default value, second argument, is null");

    if (testValue->IsNull())
        return defaultValue;

    return testValue;
}

ValueExpressionPtr ExpressionTree::Convert(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& targetType) const
{
    if (targetType->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Convert\" function target type, second argument, must be a string");

    if (targetType->IsNull())
        throw ExpressionTreeException("\"Convert\" function target type, second argument, is null");

    string targetTypeName = targetType->ValueAsString();

    // Remove any "System." prefix: 01234567
    if (StartsWith(targetTypeName, "System.") && targetTypeName.size() > 7)
        targetTypeName = targetTypeName.substr(7);

    ExpressionValueType targetValueType = ExpressionValueType::Undefined;
    bool foundValueType = false;

    for (int32_t i = 0; i < ExpressionValueTypeLength; i++)
    {
        if (IsEqual(targetTypeName, ExpressionValueTypeAcronym[i]))
        {
            targetValueType = static_cast<ExpressionValueType>(i);
            foundValueType = true;
            break;
        }
    }

    if (!foundValueType || targetValueType == ExpressionValueType::Undefined)
        throw ExpressionTreeException("Specified \"Convert\" function target type \"" + targetType->ValueAsString() + "\", second argument, is not supported");

    return Convert(sourceValue, targetValueType);
}

ValueExpressionPtr ExpressionTree::IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const
{
    if (testValue->ValueType != ExpressionValueType::Boolean)
        throw ExpressionTreeException("\"IIf\" function test value, first argument, must be a boolean");

    // Null test expression evaluates to false, that is, right expression
    return testValue->ValueAsBoolean() ? Evaluate(leftResultValue) : Evaluate(rightResultValue);
}

ValueExpressionPtr ExpressionTree::IsRegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("IsRegExMatch", regexValue, testValue, false);
}

ValueExpressionPtr ExpressionTree::Len(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Len\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, sourceText.size());
}

ValueExpressionPtr ExpressionTree::RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("RegExVal", regexValue, testValue, true);
}

ValueExpressionPtr ExpressionTree::SubString(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"SubString\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    if (!IsIntegerType(indexValue->ValueType))
        throw ExpressionTreeException("\"SubString\" function index value, second argument, must be an integer");

    if (!IsIntegerType(lengthValue->ValueType))
        throw ExpressionTreeException("\"SubString\" function length value, third argument, must be an integer");

    if (indexValue->IsNull())
        throw ExpressionTreeException("\"SubString\" function index value, second argument, is null");

    int32_t index, length = -1;

    const string sourceText = sourceValue->ValueAsString();

    switch (indexValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            index = indexValue->ValueAsBoolean() ? 1 : 0;
            break;
        case ExpressionValueType::Int32:
            index = indexValue->ValueAsInt32();
            break;
        case ExpressionValueType::Int64:
            index = static_cast<int32_t>(indexValue->ValueAsInt64());
            break;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }

    if (!lengthValue->IsNull())
    {
        switch (lengthValue->ValueType)
        {
            case ExpressionValueType::Boolean:
                length = lengthValue->ValueAsBoolean() ? 1 : 0;
                break;
            case ExpressionValueType::Int32:
                length = lengthValue->ValueAsInt32();
                break;
            case ExpressionValueType::Int64:
                length = static_cast<int32_t>(lengthValue->ValueAsInt64());
                break;
            default:
                throw ExpressionTreeException("Unexpected expression value type encountered");
        }
    }

    if (length > -1)
        return NewSharedPtr<ValueExpression>(ExpressionValueType::String, sourceText.substr(index, length));

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, sourceText.substr(index));
}

ValueExpressionPtr ExpressionTree::Trim(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Trim\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::TimeSeries::Trim(sourceText));
}

ValueExpressionPtr ExpressionTree::Multiply(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Multiply<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Multiply<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Multiply<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, Multiply<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, Multiply<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply multiplication \"*\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Divide(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
        {
            const int32_t leftInt = left->ValueAsBoolean() ? 1 : 0;
            const int32_t rightInt = right->ValueAsBoolean() ? 1 : 0;
            bool result;

            if (rightInt == 0)
                result = false;
            else
                result = leftInt / rightInt != 0;

            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, result);
        }
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Divide<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Divide<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, Divide<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, Divide<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply division \"/\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Modulus(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
        {
            const int32_t leftInt = left->ValueAsBoolean() ? 1 : 0;
            const int32_t rightInt = right->ValueAsBoolean() ? 1 : 0;
            bool result;

            if (rightInt == 0)
                result = false;
            else
                result = leftInt % rightInt != 0;

            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, result);
        }
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Modulus<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Modulus<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply modulus \"%\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Add(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Add<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Add<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Add<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, Add<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, Add<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply addition \"+\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Subtract(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Subtract<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Subtract<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Subtract<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, Subtract<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, Subtract<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply subtraction \"-\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitShiftLeft(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(leftValue->ValueType);

    if (!IsIntegerType(rightValue->ValueType))
        throw ExpressionTreeException("BitShift operation shift value must be an integer");

    if (rightValue->IsNull())
        throw ExpressionTreeException("BitShift operation shift value is null");

    int32_t shiftValue;

    switch (rightValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            shiftValue = rightValue->ValueAsBoolean() ? 1 : 0;
            break;
        case ExpressionValueType::Int32:
            shiftValue = rightValue->ValueAsInt32();
            break;
        case ExpressionValueType::Int64:
            shiftValue = static_cast<int32_t>(rightValue->ValueAsInt64());
            break;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }

    switch (leftValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, BitShiftLeft<bool>(leftValue->ValueAsBoolean(), shiftValue));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, BitShiftLeft<int32_t>(leftValue->ValueAsInt32(), shiftValue));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, BitShiftLeft<int64_t>(leftValue->ValueAsInt64(), shiftValue));
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply left bit-shift \"<<\" operator to \"" + string(EnumName(leftValue->ValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitShiftRight(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(leftValue->ValueType);

    if (!IsIntegerType(rightValue->ValueType))
        throw ExpressionTreeException("BitShift operation shift value must be an integer");

    if (rightValue->IsNull())
        throw ExpressionTreeException("BitShift operation shift value is null");

    int32_t shiftValue;

    switch (rightValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            shiftValue = rightValue->ValueAsBoolean() ? 1 : 0;
            break;
        case ExpressionValueType::Int32:
            shiftValue = rightValue->ValueAsInt32();
            break;
        case ExpressionValueType::Int64:
            shiftValue = static_cast<int32_t>(rightValue->ValueAsInt64());
            break;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }

    switch (leftValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, BitShiftRight<bool>(leftValue->ValueAsBoolean(), shiftValue));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, BitShiftRight<int32_t>(leftValue->ValueAsInt32(), shiftValue));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, BitShiftRight<int64_t>(leftValue->ValueAsInt64(), shiftValue));
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply right bit-shift \">>\" operator to \"" + string(EnumName(leftValue->ValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitwiseAnd(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, BitwiseAnd<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, BitwiseAnd<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, BitwiseAnd<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply bitwise \"&\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::BitwiseOr(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, BitwiseOr<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, BitwiseOr<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, BitwiseOr<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply bitwise \"|\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Compare(left->ValueAsString(), right->ValueAsString()) < 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThan<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply less than \"<\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Compare(left->ValueAsString(), right->ValueAsString()) <= 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, LessThanOrEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply less than or equal \"<=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Compare(left->ValueAsString(), right->ValueAsString()) > 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThan<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than \">\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Compare(left->ValueAsString(), right->ValueAsString()) >= 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GreaterThanOrEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than or equal \">=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Equal(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, IsEqual(left->ValueAsString(), right->ValueAsString()));
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Equal<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply equal \"=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::NotEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<bool>(left->ValueAsBoolean(), right->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<int32_t>(left->ValueAsInt32(), right->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<int64_t>(left->ValueAsInt64(), right->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<decimal_t>(left->ValueAsDecimal(), right->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<float64_t>(left->ValueAsDouble(), right->ValueAsDouble()));
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, !IsEqual(left->ValueAsString(), right->ValueAsString()));
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<Guid>(left->ValueAsGuid(), right->ValueAsGuid()));
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, NotEqual<time_t>(left->ValueAsDateTime(), right->ValueAsDateTime()));
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply not equal \"<>\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::IsNull(const ValueExpressionPtr& leftValue) const
{
    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, leftValue->IsNull());
}

ValueExpressionPtr ExpressionTree::IsNotNull(const ValueExpressionPtr& leftValue) const
{
    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, !leftValue->IsNull());
}

ValueExpressionPtr ExpressionTree::Like(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (leftValue->ValueType != ExpressionValueType::String || rightValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("Cannot perform \"LIKE\" operation on \"" + string(EnumName(leftValue->ValueType)) + "\" and \"" + string(EnumName(rightValue->ValueType)) + "\"");

    if (rightValue->IsNull())
        throw ExpressionTreeException("Right operand of \"LIKE\" expression is null");

    const string leftOperand = leftValue->ValueAsString();
    const string rightOperand = rightValue->ValueAsString();

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
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr likeResult = Like(leftValue, rightValue);

    return likeResult->ValueAsBoolean() ? ExpressionTree::False : ExpressionTree::True;
}

ValueExpressionPtr ExpressionTree::And(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (leftValue->ValueType != ExpressionValueType::Boolean || rightValue->ValueType != ExpressionValueType::Boolean)
        throw ExpressionTreeException("Cannot perform \"AND\" operation on \"" + string(EnumName(leftValue->ValueType)) + "\" and \"" + string(EnumName(rightValue->ValueType)) + "\"");

    const bool left = leftValue->ValueAsBoolean();
    const bool right = rightValue->ValueAsBoolean();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left && right);
}

ValueExpressionPtr ExpressionTree::Or(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (leftValue->ValueType != ExpressionValueType::Boolean || rightValue->ValueType != ExpressionValueType::Boolean)
        throw ExpressionTreeException("Cannot perform \"OR\" operation on \"" + string(EnumName(leftValue->ValueType)) + "\" and \"" + string(EnumName(rightValue->ValueType)) + "\"");

    const bool left = leftValue->ValueAsBoolean();
    const bool right = rightValue->ValueAsBoolean();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left || right);
}

template <typename T>
T ExpressionTree::Multiply(const T& leftValue, const T& rightValue)
{
    return leftValue * rightValue;
}

template <typename T>
T ExpressionTree::Divide(const T& leftValue, const T& rightValue)
{
    return leftValue / rightValue;
}

template <typename T>
T ExpressionTree::Modulus(const T& leftValue, const T& rightValue)
{
    return leftValue % rightValue;
}

template <typename T>
T ExpressionTree::Add(const T& leftValue, const T& rightValue)
{
    return leftValue + rightValue;
}

template <typename T>
T ExpressionTree::Subtract(const T& leftValue, const T& rightValue)
{
    return leftValue - rightValue;
}

template <typename T>
T ExpressionTree::BitShiftLeft(const T& operandValue, int32_t shiftValue)
{
    return operandValue << shiftValue;
}

template <typename T>
T ExpressionTree::BitShiftRight(const T& operandValue, int32_t shiftValue)
{
    return operandValue >> shiftValue;
}

template <typename T>
T ExpressionTree::BitwiseAnd(const T& leftValue, const T& rightValue)
{
    return leftValue & rightValue;
}

template <typename T>
T ExpressionTree::BitwiseOr(const T& leftValue, const T& rightValue)
{
    return leftValue | rightValue;
}

template <typename T>
bool ExpressionTree::LessThan(const T& leftValue, const T& rightValue)
{
    return leftValue < rightValue;
}

template <typename T>
bool ExpressionTree::LessThanOrEqual(const T& leftValue, const T& rightValue)
{
    return leftValue <= rightValue;
}

template <typename T>
bool ExpressionTree::GreaterThan(const T& leftValue, const T& rightValue)
{
    return leftValue > rightValue;
}

template <typename T>
bool ExpressionTree::GreaterThanOrEqual(const T& leftValue, const T& rightValue)
{
    return leftValue >= rightValue;
}

template <typename T>
bool ExpressionTree::Equal(const T& leftValue, const T& rightValue)
{
    return leftValue == rightValue;
}

template <typename T>
bool ExpressionTree::NotEqual(const T& leftValue, const T& rightValue)
{
    return leftValue != rightValue;
}

ValueExpressionPtr ExpressionTree::Convert(const ValueExpressionPtr& sourceValue, ExpressionValueType targetValueType) const
{
    // If source value is Null, result is Null, regardless of target type
    if (sourceValue->IsNull())
        return NullValue(targetValueType);

    Object targetValue;

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Boolean:
        {
            const bool result = sourceValue->ValueAsBoolean();
            const int32_t value = result ? 1 : 0;

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = result;
                    break;
                case ExpressionValueType::Int32:
                    targetValue = value;
                    break;
                case ExpressionValueType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Boolean\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Int32:
        {
            const int32_t value = sourceValue->ValueAsInt32();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionValueType::Int32:
                    targetValue = value;
                    break;
                case ExpressionValueType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Int32\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Int64:
        {
            const int64_t value = sourceValue->ValueAsInt64();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionValueType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionValueType::Int64:
                    targetValue = value;
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Int64\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Decimal:
        {
            const decimal_t value = sourceValue->ValueAsDecimal();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = value == decimal_t(0);
                    break;
                case ExpressionValueType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionValueType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = value;
                    break;
                case ExpressionValueType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Decimal\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Double:
        {
            const float64_t value = sourceValue->ValueAsDouble();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = value == 0.0;
                    break;
                case ExpressionValueType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionValueType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = value;
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Double\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::String:
        {
            const string value = sourceValue->ValueAsString();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    if (IsEqual(value, "true") || IsEqual(value, "1"))
                        targetValue = true;
                    else if (IsEqual(value, "false") || IsEqual(value, "0"))
                        targetValue = false;
                    else
                        throw ExpressionTreeException("\"String\" value not recognized as a valid \"Boolean\"");
                    break;
                case ExpressionValueType::Int32:
                    targetValue = stoi(value);
                    break;
                case ExpressionValueType::Int64:
                    targetValue = stoll(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = decimal_t(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = stod(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = value;
                    break;
                case ExpressionValueType::Guid:
                    targetValue = ToGuid(value.c_str());
                    break;
                case ExpressionValueType::DateTime:
                    targetValue = ParseTimestamp(value.c_str());
                    break;
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Guid:
        {
            const Guid value = sourceValue->ValueAsGuid();

            switch (targetValueType)
            {
                case ExpressionValueType::String:
                    targetValue = ToString(value);
                    break;
                case ExpressionValueType::Guid:
                    targetValue = value;
                    break;
                case ExpressionValueType::Boolean:
                case ExpressionValueType::Int32:
                case ExpressionValueType::Int64:
                case ExpressionValueType::Decimal:
                case ExpressionValueType::Double:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot convert \"Guid\" to \"" + string(EnumName(targetValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::DateTime:
        {
            const time_t value = sourceValue->ValueAsDateTime();

            switch (targetValueType)
            {
                case ExpressionValueType::Boolean:
                    targetValue = value == 0;
                    break;
                case ExpressionValueType::Int32:
                    targetValue = static_cast<int32_t>(value);
                    break;
                case ExpressionValueType::Int64:
                    targetValue = static_cast<int64_t>(value);
                    break;
                case ExpressionValueType::Decimal:
                    targetValue = static_cast<decimal_t>(value);
                    break;
                case ExpressionValueType::Double:
                    targetValue = static_cast<float64_t>(value);
                    break;
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                    throw ExpressionTreeException("Cannot convert \"DateTime\" to \"" + string(EnumName(targetValueType)) + "\"");
                case ExpressionValueType::DateTime:
                    targetValue = value;
                    break;
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
            break;
        }
        case ExpressionValueType::Undefined:
            // Change Undefined values to Nullable of target type
            return NullValue(targetValueType);
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }

    return NewSharedPtr<ValueExpression>(targetValueType, targetValue);
}

ValueExpressionPtr ExpressionTree::EvaluateRegEx(const string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, bool returnMatchedValue) const
{
    if (regexValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"" + functionName + "\" function expression value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"" + functionName + "\" function test value, second argument, must be a string");

    if (regexValue->IsNull() || testValue->IsNull())
        return NullValue(returnMatchedValue ? ExpressionValueType::String : ExpressionValueType::Boolean);

    const string expressionText = regexValue->ValueAsString();
    const string testText = testValue->ValueAsString();
    const regex expression(expressionText);

    cmatch match;
    const bool result = regex_match(testText.c_str(), match, expression);

    if (returnMatchedValue)
    {
        // RegExVal returns any matched value, otherwise empty string
        if (result)
            return NewSharedPtr<ValueExpression>(ExpressionValueType::String, string(match[0]));

        return ExpressionTree::EmptyString;
    }

    // IsRegExMatch returns boolean result for if there was a matched value
    return result ? ExpressionTree::True : ExpressionTree::False;
}

ExpressionTree::ExpressionTree(const DataTablePtr& measurements) :
    Measurements(measurements),
    TopLimit(-1)
{
}

ValueExpressionPtr ExpressionTree::Evaluate(const DataRowPtr& row)
{
    m_currentRow = row;
    return Evaluate(Root);
}

const ValueExpressionPtr ExpressionTree::True = NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, true);

const ValueExpressionPtr ExpressionTree::False = NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, false);

const ValueExpressionPtr ExpressionTree::EmptyString = NewSharedPtr<ValueExpression>(ExpressionValueType::String, string());

ValueExpressionPtr ExpressionTree::NullValue(ExpressionValueType targetValueType)
{
    // Change Undefined values to Nullable of target type
    switch (targetValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, Nullable<bool>(nullptr), true);
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Nullable<int32_t>(nullptr), true);
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Nullable<int64_t>(nullptr), true);
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, Nullable<decimal_t>(nullptr), true);
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, Nullable<float64_t>(nullptr), true);
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::String, Nullable<string>(nullptr), true);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Guid, Nullable<Guid>(nullptr), true);
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, Nullable<time_t>(nullptr), true);
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}