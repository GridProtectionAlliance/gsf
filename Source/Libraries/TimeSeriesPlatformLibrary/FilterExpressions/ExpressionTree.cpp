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
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::FilterExpressions;

const int32_t GSF::FilterExpressions::ExpressionValueTypeLength = static_cast<int32_t>(ExpressionValueType::Undefined) + 1;

const char* GSF::FilterExpressions::ExpressionValueTypeAcronym[] =
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

const char* GSF::FilterExpressions::EnumName(const ExpressionValueType valueType)
{
    return ExpressionValueTypeAcronym[static_cast<int32_t>(valueType)];
}

const char* GSF::FilterExpressions::ExpressionUnaryTypeAcronym[] =
{
    "+",
    "-",
    "~"
};

const char* GSF::FilterExpressions::EnumName(const ExpressionUnaryType unaryType)
{
    return ExpressionUnaryTypeAcronym[static_cast<int32_t>(unaryType)];
}

const char* GSF::FilterExpressions::ExpressionOperatorTypeAcronym[] =
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
    "^",
    "<",
    "<=",
    ">",
    ">=",
    "=",
    "===",
    "<>",
    "!==",
    "IS NULL",
    "IS NOT NULL",
    "LIKE",
    "LIKE BINARY",
    "NOT LIKE",
    "NOT LIKE BINARY",
    "AND",
    "OR"
};

const char* GSF::FilterExpressions::EnumName(const ExpressionOperatorType operatorType)
{
    return ExpressionOperatorTypeAcronym[static_cast<int32_t>(operatorType)];
}

bool GSF::FilterExpressions::IsIntegerType(const ExpressionValueType valueType)
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

bool GSF::FilterExpressions::IsNumericType(const ExpressionValueType valueType)
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

TimeInterval ParseTimeInterval(const string& intervalName)
{
    if (IsEqual(intervalName, "DAY"))
        return TimeInterval::Day;

    if (IsEqual(intervalName, "HOUR"))
        return TimeInterval::Hour;

    if (IsEqual(intervalName, "MINUTE"))
        return TimeInterval::Minute;

    if (IsEqual(intervalName, "SECOND"))
        return TimeInterval::Second;

    if (IsEqual(intervalName, "MONTH"))
        return TimeInterval::Month;

    if (IsEqual(intervalName, "YEAR"))
        return TimeInterval::Year;

    if (IsEqual(intervalName, "MILLISECOND"))
        return TimeInterval::Millisecond;

    if (IsEqual(intervalName, "DAYOFYEAR"))
        return TimeInterval::DayOfYear;

    if (IsEqual(intervalName, "WEEK"))
        return TimeInterval::Week;

    if (IsEqual(intervalName, "WEEKDAY"))
        return TimeInterval::WeekDay;

    throw ExpressionTreeException("Time interval \"" + intervalName + "\" is not recognized");
}

ExpressionTreeException::ExpressionTreeException(string message) noexcept :
    m_message(std::move(message))
{
}

const char* ExpressionTreeException::what() const noexcept
{
    return &m_message[0];
}

Expression::Expression(const ExpressionType type) :
    Type(type)
{
}

Expression::~Expression() = default;

ValueExpression::ValueExpression(const ExpressionValueType valueType, Object value, bool const valueIsNullable) :  // NOLINT(modernize-pass-by-value)
    Expression(ExpressionType::Value),
    Value(std::move(value)),
    ValueType(valueType),
    ValueIsNullable(valueIsNullable)
{
}

void ValueExpression::ValidateValueType(const ExpressionValueType valueType) const
{
    if (ValueType != valueType)
        throw ExpressionTreeException("Cannot read expression value as \"" + string(EnumName(valueType)) + "\", type is \"" + string(EnumName(ValueType)) + "\"");
}

bool ValueExpression::IsNull() const
{
    if (!ValueIsNullable)
        return false;

    switch (ValueType)
    {
        case ExpressionValueType::Boolean:
            return !ValueAsNullableBoolean().HasValue();
        case ExpressionValueType::Int32:
            return !ValueAsNullableInt32().HasValue();
        case ExpressionValueType::Int64:
            return !ValueAsNullableInt64().HasValue();
        case ExpressionValueType::Decimal:
            return !ValueAsNullableDecimal().HasValue();
        case ExpressionValueType::Double:
            return !ValueAsNullableDouble().HasValue();
        case ExpressionValueType::String:
            return !ValueAsNullableString().HasValue();
        case ExpressionValueType::Guid:
            return !ValueAsNullableGuid().HasValue();
        case ExpressionValueType::DateTime:
            return !ValueAsNullableDateTime().HasValue();
        case ExpressionValueType::Undefined:
            return true;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

string ValueExpression::ToString() const
{
    switch (ValueType)
    {
        case ExpressionValueType::Boolean:
            return GSF::ToString(ValueAsNullableBoolean());
        case ExpressionValueType::Int32:
            return GSF::ToString(ValueAsNullableInt32());
        case ExpressionValueType::Int64:
            return GSF::ToString(ValueAsNullableInt64());
        case ExpressionValueType::Decimal:
            return GSF::ToString(ValueAsNullableDecimal());
        case ExpressionValueType::Double:
            return GSF::ToString(ValueAsNullableDouble());
        case ExpressionValueType::String:
            return GSF::ToString(ValueAsNullableString());
        case ExpressionValueType::Guid:
            return GSF::ToString(ValueAsNullableGuid());
        case ExpressionValueType::DateTime:
            return GSF::ToString(ValueAsNullableDateTime());
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

datetime_t ValueExpression::ValueAsDateTime() const
{
    ValidateValueType(ExpressionValueType::DateTime);

    if (ValueIsNullable)
        return Cast<Nullable<datetime_t>>(Value).GetValueOrDefault();

    return Cast<datetime_t>(Value);
}

Nullable<datetime_t> ValueExpression::ValueAsNullableDateTime() const
{
    ValidateValueType(ExpressionValueType::DateTime);

    if (ValueIsNullable)
        return Cast<Nullable<datetime_t>>(Value);

    return Cast<datetime_t>(Value);
}

UnaryExpression::UnaryExpression(const ExpressionUnaryType unaryType, ExpressionPtr value) :
    Expression(ExpressionType::Unary),
    UnaryType(unaryType),
    Value(std::move(value))
{
}

ColumnExpression::ColumnExpression(DataColumnPtr dataColumn) :
    Expression(ExpressionType::Column),
    DataColumn(std::move(dataColumn))
{
}

OperatorExpression::OperatorExpression(const ExpressionOperatorType operatorType, ExpressionPtr leftValue, ExpressionPtr rightValue) :
    Expression(ExpressionType::Operator),
    OperatorType(operatorType),
    LeftValue(std::move(leftValue)),
    RightValue(std::move(rightValue))
{
}

InListExpression::InListExpression(ExpressionPtr value, ExpressionCollectionPtr arguments, const bool hasNotKeyword, const bool exactMatch) :
    Expression(ExpressionType::InList),
    Value(std::move(value)),
    Arguments(std::move(arguments)),
    HasNotKeyword(hasNotKeyword),
    ExactMatch(exactMatch)
{
}

FunctionExpression::FunctionExpression(const ExpressionFunctionType functionType, ExpressionCollectionPtr arguments) :
    Expression(ExpressionType::Function),
    FunctionType(functionType),
    Arguments(std::move(arguments))
{
}

ValueExpressionPtr ExpressionTree::Evaluate(const ExpressionPtr& expression, const ExpressionValueType targetValueType) const
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
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, UnaryBool(unaryValue->ValueAsBoolean(), unaryExpression->UnaryType));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, Unary<int32_t>(unaryValue->ValueAsInt32(), unaryExpression->UnaryType));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, Unary<int64_t>(unaryValue->ValueAsInt64(), unaryExpression->UnaryType));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, UnaryFloat<decimal_t>(unaryValue->ValueAsDecimal(), unaryExpression->UnaryType, unaryValueType));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, UnaryFloat<float64_t>(unaryValue->ValueAsDouble(), unaryExpression->UnaryType, unaryValueType));
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
                value = NullValue(ExpressionValueType::Int64);
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
    const bool hasNotKeyword = inListExpression->HasNotKeyword;
    const bool exactMatch = inListExpression->ExactMatch;

    // If in list test value is Null, result is Null
    if (inListValue->IsNull())
        return NullValue(inListValue->ValueType);

    const int32_t argumentCount = inListExpression->Arguments->size();

    for (int32_t i = 0; i < argumentCount; i++)
    {
        const ValueExpressionPtr argumentValue = Evaluate(inListExpression->Arguments->at(i));
        const ExpressionValueType valueType = DeriveComparisonOperationValueType(ExpressionOperatorType::Equal, inListValue->ValueType, argumentValue->ValueType);
        const ValueExpressionPtr result = Equal(inListValue, argumentValue, valueType, exactMatch);

        if (result->ValueAsBoolean())
            return hasNotKeyword ? ExpressionTree::False : ExpressionTree::True;
    }

    return hasNotKeyword ? ExpressionTree::True : ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::EvaluateFunction(const ExpressionPtr& expression) const
{
    const FunctionExpressionPtr functionExpression = CastSharedPtr<FunctionExpression>(expression);
    const ExpressionCollectionPtr arguments = functionExpression->Arguments;

    switch (functionExpression->FunctionType)
    {
        case ExpressionFunctionType::Abs:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Abs\" function expects 1 argument, received " + ToString(arguments->size()));

            return Abs(Evaluate(arguments->at(0), ExpressionValueType::Double));
        case ExpressionFunctionType::Ceiling:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Ceiling\" function expects 1 argument, received " + ToString(arguments->size()));

            return Ceiling(Evaluate(arguments->at(0), ExpressionValueType::Double));
        case ExpressionFunctionType::Coalesce:
            if (arguments->size() < 2)
                throw ExpressionTreeException("\"Coalesce\" function expects at least 2 arguments, received " + ToString(arguments->size()));

            // Not pre-evaluating Coalesce arguments - arguments will be evaluated only up to first non-null value
            return Coalesce(arguments);
        case ExpressionFunctionType::Convert:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"Convert\" function expects 2 arguments, received " + ToString(arguments->size()));

            return Convert(Evaluate(arguments->at(0)), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::Contains:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"Contains\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return Contains(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return Contains(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::DateAdd:
            if (arguments->size() != 3)
                throw ExpressionTreeException("\"DateAdd\" function expects 3 arguments, received " + ToString(arguments->size()));

            return DateAdd(Evaluate(arguments->at(0), ExpressionValueType::DateTime), Evaluate(arguments->at(1), ExpressionValueType::Int32), Evaluate(arguments->at(2), ExpressionValueType::String));
        case ExpressionFunctionType::DateDiff:
            if (arguments->size() != 3)
                throw ExpressionTreeException("\"DateDiff\" function expects 3 arguments, received " + ToString(arguments->size()));

            return DateDiff(Evaluate(arguments->at(0), ExpressionValueType::DateTime), Evaluate(arguments->at(1), ExpressionValueType::DateTime), Evaluate(arguments->at(2), ExpressionValueType::String));
        case ExpressionFunctionType::DatePart:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"DatePart\" function expects 2 arguments, received " + ToString(arguments->size()));

            return DatePart(Evaluate(arguments->at(0), ExpressionValueType::DateTime), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::EndsWith:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"EndsWith\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return EndsWith(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return EndsWith(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::Floor:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Floor\" function expects 1 argument, received " + ToString(arguments->size()));

            return Floor(Evaluate(arguments->at(0), ExpressionValueType::Double));
        case ExpressionFunctionType::IIf:
            if (arguments->size() != 3)
                throw ExpressionTreeException("\"IIf\" function expects 3 arguments, received " + ToString(arguments->size()));

            // Not pre-evaluating IIf result value arguments - only evaluating desired path
            return IIf(Evaluate(arguments->at(0), ExpressionValueType::Boolean), arguments->at(1), arguments->at(2));
        case ExpressionFunctionType::IndexOf:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"IndexOf\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return IndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return IndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::IsDate:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"IsDate\" function expects 1 argument, received " + ToString(arguments->size()));

            return IsDate(Evaluate(arguments->at(0), ExpressionValueType::Boolean));
        case ExpressionFunctionType::IsInteger:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"IsInteger\" function expects 1 argument, received " + ToString(arguments->size()));

            return IsInteger(Evaluate(arguments->at(0), ExpressionValueType::Boolean));
        case ExpressionFunctionType::IsGuid:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"IsGuid\" function expects 1 argument, received " + ToString(arguments->size()));

            return IsGuid(Evaluate(arguments->at(0), ExpressionValueType::Boolean));
        case ExpressionFunctionType::IsNull:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"IsNull\" function expects 2 arguments, received " + ToString(arguments->size()));

            return IsNull(Evaluate(arguments->at(0)), Evaluate(arguments->at(1)));
        case ExpressionFunctionType::IsNumeric:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"IsNumeric\" function expects 1 argument, received " + ToString(arguments->size()));

            return IsNumeric(Evaluate(arguments->at(0), ExpressionValueType::Boolean));
        case ExpressionFunctionType::LastIndexOf:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"LastIndexOf\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return LastIndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return LastIndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::Len:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Len\" function expects 1 argument, received " + ToString(arguments->size()));

            return Len(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::Lower:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Lower\" function expects 1 argument, received " + ToString(arguments->size()));

            return Lower(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::MaxOf:
            if (arguments->size() < 2)
                throw ExpressionTreeException("\"MaxOf\" function expects at least 2 arguments, received " + ToString(arguments->size()));

            return MaxOf(arguments);
        case ExpressionFunctionType::MinOf:
            if (arguments->size() < 2)
                throw ExpressionTreeException("\"MinOf\" function expects at least 2 arguments, received " + ToString(arguments->size()));

            return MinOf(arguments);
        case ExpressionFunctionType::NthIndexOf:
            if (arguments->size() < 3 || arguments->size() > 4)
                throw ExpressionTreeException("\"NthIndexOf\" function expects 3 or 4 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 3)
                return NthIndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Int32), NullValue(ExpressionValueType::Boolean));

            return NthIndexOf(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Int32), Evaluate(arguments->at(3), ExpressionValueType::Boolean));
        case ExpressionFunctionType::Now:
            if (!arguments->empty())
                throw ExpressionTreeException("\"Now\" function expects 0 arguments, received " + ToString(arguments->size()));

            return Now();
        case ExpressionFunctionType::Power:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"Power\" function expects 2 arguments, received " + ToString(arguments->size()));

            return Power(Evaluate(arguments->at(0), ExpressionValueType::Double), Evaluate(arguments->at(1), ExpressionValueType::Int32));
        case ExpressionFunctionType::RegExMatch:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"RegExMatch\" function expects 2 arguments, received " + ToString(arguments->size()));

            return RegExMatch(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::RegExVal:
            if (arguments->size() != 2)
                throw ExpressionTreeException("\"RegExVal\" function expects 2 arguments, received " + ToString(arguments->size()));

            return RegExVal(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String));
        case ExpressionFunctionType::Replace:
            if (arguments->size() < 3 || arguments->size() > 4)
                throw ExpressionTreeException("\"Replace\" function expects 3 or 4 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 3)
                return Replace(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return Replace(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::String), Evaluate(arguments->at(3), ExpressionValueType::Boolean));
        case ExpressionFunctionType::Reverse:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Reverse\" function expects 1 argument, received " + ToString(arguments->size()));

            return Reverse(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::Round:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Round\" function expects 1 argument, received " + ToString(arguments->size()));

            return Round(Evaluate(arguments->at(0), ExpressionValueType::Double));
        case ExpressionFunctionType::Split:
            if (arguments->size() < 3 || arguments->size() > 4)
                throw ExpressionTreeException("\"Split\" function expects 3 or 4 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 3)
                return Split(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Int32), NullValue(ExpressionValueType::Boolean));

            return Split(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Int32), Evaluate(arguments->at(3), ExpressionValueType::Boolean));
        case ExpressionFunctionType::Sqrt:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Sqrt\" function expects 1 argument, received " + ToString(arguments->size()));

            return Sqrt(Evaluate(arguments->at(0), ExpressionValueType::Double));
        case ExpressionFunctionType::StartsWith:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"StartsWith\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return StartsWith(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return StartsWith(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::StrCount:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"StrCount\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return StrCount(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return StrCount(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::StrCmp:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"StrCmp\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return StrCmp(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), NullValue(ExpressionValueType::Boolean));

            return StrCmp(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::String), Evaluate(arguments->at(2), ExpressionValueType::Boolean));
        case ExpressionFunctionType::SubStr:
            if (arguments->size() < 2 || arguments->size() > 3)
                throw ExpressionTreeException("\"SubStr\" function expects 2 or 3 arguments, received " + ToString(arguments->size()));

            if (arguments->size() == 2)
                return SubStr(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::Int32), NullValue(ExpressionValueType::Int32));

            return SubStr(Evaluate(arguments->at(0), ExpressionValueType::String), Evaluate(arguments->at(1), ExpressionValueType::Int32), Evaluate(arguments->at(2), ExpressionValueType::Int32));
        case ExpressionFunctionType::Trim:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Trim\" function expects 1 argument, received " + ToString(arguments->size()));

            return Trim(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::TrimLeft:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"TrimLeft\" function expects 1 argument, received " + ToString(arguments->size()));

            return TrimLeft(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::TrimRight:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"TrimRight\" function expects 1 argument, received " + ToString(arguments->size()));

            return TrimRight(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::Upper:
            if (arguments->size() != 1)
                throw ExpressionTreeException("\"Upper\" function expects 1 argument, received " + ToString(arguments->size()));

            return Upper(Evaluate(arguments->at(0), ExpressionValueType::String));
        case ExpressionFunctionType::UtcNow:
            if (!arguments->empty())
                throw ExpressionTreeException("\"UtcNow\" function expects 0 arguments, received " + ToString(arguments->size()));

            return UtcNow();
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
        case ExpressionOperatorType::BitwiseXor:
            return BitwiseXor(leftValue, rightValue, valueType);
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
        case ExpressionOperatorType::EqualExactMatch:
            return Equal(leftValue, rightValue, valueType, true);
        case ExpressionOperatorType::NotEqual:
            return NotEqual(leftValue, rightValue, valueType);
        case ExpressionOperatorType::NotEqualExactMatch:
            return NotEqual(leftValue, rightValue, valueType, true);
        case ExpressionOperatorType::IsNull:
            return IsNull(leftValue);
        case ExpressionOperatorType::IsNotNull:
            return IsNotNull(leftValue);
        case ExpressionOperatorType::Like:
            return Like(leftValue, rightValue);
        case ExpressionOperatorType::LikeExactMatch:
            return Like(leftValue, rightValue, true);
        case ExpressionOperatorType::NotLike:
            return NotLike(leftValue, rightValue);
        case ExpressionOperatorType::NotLikeExactMatch:
            return NotLike(leftValue, rightValue, true);
        case ExpressionOperatorType::And:
            return And(leftValue, rightValue);
        case ExpressionOperatorType::Or:
            return Or(leftValue, rightValue);
        default:
            throw ExpressionTreeException("Unexpected operator type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveOperationValueType(const ExpressionOperatorType operationType, const ExpressionValueType leftValueType, const ExpressionValueType rightValueType) const
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
        case ExpressionOperatorType::BitwiseXor:
            return DeriveIntegerOperationValueType(operationType, leftValueType, rightValueType);        
        case ExpressionOperatorType::LessThan:
        case ExpressionOperatorType::LessThanOrEqual:
        case ExpressionOperatorType::GreaterThan:
        case ExpressionOperatorType::GreaterThanOrEqual:
        case ExpressionOperatorType::Equal:
        case ExpressionOperatorType::EqualExactMatch:
        case ExpressionOperatorType::NotEqual:
        case ExpressionOperatorType::NotEqualExactMatch:
            return DeriveComparisonOperationValueType(operationType, leftValueType, rightValueType);
        case ExpressionOperatorType::And:
        case ExpressionOperatorType::Or:
            return DeriveBooleanOperationValueType(operationType, leftValueType, rightValueType);
        case ExpressionOperatorType::BitShiftLeft:
        case ExpressionOperatorType::BitShiftRight:
        case ExpressionOperatorType::IsNull:
        case ExpressionOperatorType::IsNotNull:
        case ExpressionOperatorType::Like:
        case ExpressionOperatorType::LikeExactMatch:
        case ExpressionOperatorType::NotLike:
        case ExpressionOperatorType::NotLikeExactMatch:
            return leftValueType;
        default:
            throw ExpressionTreeException("Unexpected expression operator type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveArithmeticOperationValueType(const ExpressionOperatorType operationType, const ExpressionValueType leftValueType, const ExpressionValueType rightValueType) const
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
                    if (operationType == ExpressionOperatorType::Add)
                        return ExpressionValueType::String;
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
                    if (operationType == ExpressionOperatorType::Add)
                        return ExpressionValueType::String;
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
                    if (operationType == ExpressionOperatorType::Add)
                        return ExpressionValueType::String;
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
                    if (operationType == ExpressionOperatorType::Add)
                        return ExpressionValueType::String;
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
                    if (operationType == ExpressionOperatorType::Add)
                        return ExpressionValueType::String;
                case ExpressionValueType::Guid:
                case ExpressionValueType::DateTime:
                    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"Double\" and \"" + string(EnumName(rightValueType)) + "\"");
                default:
                    throw ExpressionTreeException("Unexpected expression value type encountered");
            }
        case ExpressionValueType::String:
            if (operationType == ExpressionOperatorType::Add)
                return ExpressionValueType::String;
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
            throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftValueType)) + "\" and \"" + string(EnumName(rightValueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ExpressionValueType ExpressionTree::DeriveIntegerOperationValueType(const ExpressionOperatorType operationType, const ExpressionValueType leftValueType, const ExpressionValueType rightValueType) const
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

ExpressionValueType ExpressionTree::DeriveComparisonOperationValueType(const ExpressionOperatorType operationType, const ExpressionValueType leftValueType, const ExpressionValueType rightValueType) const
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

ExpressionValueType ExpressionTree::DeriveBooleanOperationValueType(const ExpressionOperatorType operationType, const ExpressionValueType leftValueType, const ExpressionValueType rightValueType) const
{
    if (leftValueType == ExpressionValueType::Boolean && rightValueType == ExpressionValueType::Boolean)
        return ExpressionValueType::Boolean;

    throw ExpressionTreeException("Cannot perform \"" + string(EnumName(operationType)) + "\" operation on \"" + string(EnumName(leftValueType)) + "\" and \"" + string(EnumName(rightValueType)) + "\"");
}

ValueExpressionPtr ExpressionTree::Abs(const ValueExpressionPtr& sourceValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Abs\" function argument must be numeric");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(sourceValue->ValueType);

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, sourceValue->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, abs(sourceValue->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, abs(sourceValue->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, abs(sourceValue->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, abs(sourceValue->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Ceiling(const ValueExpressionPtr& sourceValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Ceiling\" function argument must be numeric");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    if (IsIntegerType(sourceValue->ValueType))
        return sourceValue;

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, ceil(sourceValue->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, ceil(sourceValue->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Coalesce(const ExpressionCollectionPtr& arguments) const
{
    ValueExpressionPtr testValue = Evaluate(arguments->at(0));

    if (!testValue->IsNull())
        return testValue;

    for (size_t i = 1; i < arguments->size(); i++)
    {
        ValueExpressionPtr listValue = Evaluate(arguments->at(i));

        if (!listValue->IsNull())
            return listValue;
    }

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
    if (GSF::StartsWith(targetTypeName, "System.") && targetTypeName.size() > 7)
        targetTypeName = targetTypeName.substr(7);

    ExpressionValueType targetValueType = ExpressionValueType::Undefined;
    bool foundValueType = false;

    for (int32_t i = 0; i < ExpressionValueTypeLength; i++)
    {
        if (GSF::IsEqual(targetTypeName, ExpressionValueTypeAcronym[i]))
        {
            targetValueType = static_cast<ExpressionValueType>(i);
            foundValueType = true;
            break;
        }
    }

    if (!foundValueType)
    {
        // Handle a few common exceptions
        if (GSF::IsEqual(targetTypeName, "Single") || GSF::StartsWith(targetTypeName, "float"))
        {
            targetValueType = ExpressionValueType::Double;
            foundValueType = true;
        }
        else if (GSF::IsEqual(targetTypeName, "bool"))
        {
            targetValueType = ExpressionValueType::Boolean;
            foundValueType = true;
        }
        else if (GSF::StartsWith(targetTypeName, "Int") || GSF::StartsWith(targetTypeName, "UInt"))
        {
            targetValueType = ExpressionValueType::Int64;
            foundValueType = true;
        }
    }

    if (!foundValueType || targetValueType == ExpressionValueType::Undefined)
        throw ExpressionTreeException("Specified \"Convert\" function target type \"" + targetType->ValueAsString() + "\", second argument, is not supported");

    return Convert(sourceValue, targetValueType);
}

ValueExpressionPtr ExpressionTree::Contains(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Contains\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Contains\" function test value, second argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (testValue->IsNull())
        return ExpressionTree::False;

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::Contains(sourceValue->ValueAsString(), testValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::DateAdd(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& addValue, const ValueExpressionPtr& intervalType) const
{
    if (sourceValue->ValueType != ExpressionValueType::DateTime && sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DateAdd\" source value, first argument, must be a date-time");

    if (!IsIntegerType(addValue->ValueType))
        throw ExpressionTreeException("\"DateAdd\" function add value, second argument, must be an integer type");

    if (addValue->IsNull())
        throw ExpressionTreeException("\"DateAdd\" function add value, second argument, is null");

    if (intervalType->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DateAdd\" function interval type, third argument, must be a string");

    if (intervalType->IsNull())
        throw ExpressionTreeException("\"DateAdd\" function interval type, third argument, is null");

    // DateTime parameters should support strings as well as literals
    const ValueExpressionPtr dateValue = Convert(sourceValue, ExpressionValueType::DateTime);
    const TimeInterval interval = ParseTimeInterval(intervalType->ValueAsString());

    // If source value is Null, result is Null
    if (dateValue->IsNull())
        return dateValue;

    int32_t value;

    switch (addValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            value = addValue->ValueAsBoolean() ? 1 : 0;
            break;
        case ExpressionValueType::Int32:
            value = addValue->ValueAsInt32();
            break;
        case ExpressionValueType::Int64:
            value = static_cast<int32_t>(addValue->ValueAsInt64());
            break;
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }

    return NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, GSF::DateAdd(dateValue->ValueAsDateTime(), value, interval));
}

ValueExpressionPtr ExpressionTree::DateDiff(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ValueExpressionPtr& intervalType) const
{
    if (leftValue->ValueType != ExpressionValueType::DateTime && leftValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DateDiff\" left value, first argument, must be a date-time");

    if (rightValue->ValueType != ExpressionValueType::DateTime && rightValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DateDiff\" right value, second argument, must be a date-time");

    if (intervalType->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DateDiff\" function interval type, third argument, must be a string");

    if (intervalType->IsNull())
        throw ExpressionTreeException("\"DateDiff\" function interval type, third argument, is null");

    // DateTime parameters should support strings as well as literals
    const ValueExpressionPtr leftDateValue = Convert(leftValue, ExpressionValueType::DateTime);
    const ValueExpressionPtr rightDateValue = Convert(rightValue, ExpressionValueType::DateTime);
    const TimeInterval interval = ParseTimeInterval(intervalType->ValueAsString());

    // If either test value is Null, result is Null
    if (leftDateValue->IsNull() || rightDateValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::DateDiff(leftDateValue->ValueAsDateTime(), rightDateValue->ValueAsDateTime(), interval));
}

ValueExpressionPtr ExpressionTree::DatePart(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& intervalType) const
{
    if (sourceValue->ValueType != ExpressionValueType::DateTime && sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DatePart\" source value, first argument, must be a date-time");

    if (intervalType->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"DatePart\" function interval type, second argument, must be a string");

    if (intervalType->IsNull())
        throw ExpressionTreeException("\"DatePart\" function interval type, second argument, is null");

    // DateTime parameters should support strings as well as literals
    const ValueExpressionPtr dateValue = Convert(sourceValue, ExpressionValueType::DateTime);
    const TimeInterval interval = ParseTimeInterval(intervalType->ValueAsString());

    // If source value is Null, result is Null
    if (dateValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::DatePart(dateValue->ValueAsDateTime(), interval));
}

ValueExpressionPtr ExpressionTree::EndsWith(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"EndsWith\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"EndsWith\" function test value, second argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (testValue->IsNull())
        return ExpressionTree::False;

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::EndsWith(sourceValue->ValueAsString(), testValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::Floor(const ValueExpressionPtr& sourceValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Floor\" function argument must be numeric");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    if (IsIntegerType(sourceValue->ValueType))
        return sourceValue;

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, floor(sourceValue->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, floor(sourceValue->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const
{
    if (testValue->ValueType != ExpressionValueType::Boolean)
        throw ExpressionTreeException("\"IIf\" function test value, first argument, must be a boolean");

    // Null test expression evaluates to false, that is, right expression
    return testValue->ValueAsBoolean() ? Evaluate(leftResultValue) : Evaluate(rightResultValue);
}

ValueExpressionPtr ExpressionTree::IndexOf(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"IndexOf\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"IndexOf\" function test value, second argument, must be a string");

    if (testValue->IsNull())
        throw ExpressionTreeException("\"IndexOf\" function test value, second argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::IndexOf(sourceValue->ValueAsString(), testValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::IsDate(const ValueExpressionPtr& testValue) const
{
    if (testValue->IsNull())
        return ExpressionTree::False;

    if (testValue->ValueType == ExpressionValueType::DateTime)
        return ExpressionTree::True;

    datetime_t timestamp;

    if (testValue->ValueType == ExpressionValueType::String && TryParseTimestamp(testValue->ValueAsString().c_str(), timestamp))
        return ExpressionTree::True;

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::IsInteger(const ValueExpressionPtr& testValue) const
{
    if (testValue->IsNull())
        return ExpressionTree::False;

    if (IsIntegerType(testValue->ValueType))
        return ExpressionTree::True;

    if (testValue->ValueType == ExpressionValueType::String)
    {
        try
        {
            stoll(testValue->ValueAsString());
            return ExpressionTree::True;
        }
        catch (...)
        {
            return ExpressionTree::False;
        }
    }

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::IsGuid(const ValueExpressionPtr& testValue) const
{
    if (testValue->IsNull())
        return ExpressionTree::False;

    if (testValue->ValueType == ExpressionValueType::Guid)
        return ExpressionTree::True;

    if (testValue->ValueType == ExpressionValueType::String)
    {
        try
        {
            ParseGuid(testValue->ValueAsString().c_str());
            return ExpressionTree::True;
        }
        catch (...)
        {
            return ExpressionTree::False;
        }
    }

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::IsNull(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue) const
{
    if (defaultValue->IsNull())
        throw ExpressionTreeException("\"IsNull\" default value, second argument, is null");

    if (testValue->IsNull())
        return defaultValue;

    return testValue;
}

ValueExpressionPtr ExpressionTree::IsNumeric(const ValueExpressionPtr& testValue) const
{
    if (testValue->IsNull())
        return ExpressionTree::False;

    if (IsNumericType(testValue->ValueType))
        return ExpressionTree::True;

    if (testValue->ValueType == ExpressionValueType::String)
    {
        try
        {
            stod(testValue->ValueAsString());
            return ExpressionTree::True;
        }
        catch (...)
        {
            return ExpressionTree::False;
        }
    }

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::LastIndexOf(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"LastIndexOf\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"LastIndexOf\" function test value, second argument, must be a string");

    if (testValue->IsNull())
        throw ExpressionTreeException("\"LastIndexOf\" function test value, second argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::LastIndexOf(sourceValue->ValueAsString(), testValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::Len(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Len\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, static_cast<int32_t>(sourceText.size()));
}

ValueExpressionPtr ExpressionTree::Lower(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Lower\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::ToLower(sourceText));
}

ValueExpressionPtr ExpressionTree::MaxOf(const ExpressionCollectionPtr& arguments) const
{
    ValueExpressionPtr testValue = Evaluate(arguments->at(0));

    for (size_t i = 1; i < arguments->size(); i++)
    {
        const ValueExpressionPtr nextValue = Evaluate(arguments->at(i));
        const ExpressionValueType valueType = DeriveOperationValueType(ExpressionOperatorType::GreaterThan, testValue->ValueType, nextValue->ValueType);
        const ValueExpressionPtr result = GreaterThan(nextValue, testValue, valueType);

        if (result->ValueAsBoolean() || (testValue->IsNull() && !nextValue->IsNull()))
            testValue = nextValue;
    }

    return testValue;
}

ValueExpressionPtr ExpressionTree::MinOf(const ExpressionCollectionPtr& arguments) const
{
    ValueExpressionPtr testValue = Evaluate(arguments->at(0));

    for (size_t i = 1; i < arguments->size(); i++)
    {
        const ValueExpressionPtr nextValue = Evaluate(arguments->at(i));
        const ExpressionValueType valueType = DeriveOperationValueType(ExpressionOperatorType::LessThan, testValue->ValueType, nextValue->ValueType);
        const ValueExpressionPtr result = LessThan(nextValue, testValue, valueType);

        if (result->ValueAsBoolean() || (testValue->IsNull() && !nextValue->IsNull()))
            testValue = nextValue;
    }

    return testValue;
}

ValueExpressionPtr ExpressionTree::Now() const
{
    return NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, GSF::Now());
}

ValueExpressionPtr ExpressionTree::NthIndexOf(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"NthIndexOf\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"NthIndexOf\" function test value, second argument, must be a string");

    if (testValue->IsNull())
        throw ExpressionTreeException("\"NthIndexOf\" function test value, second argument, is null");

    if (!IsIntegerType(indexValue->ValueType))
        throw ExpressionTreeException("\"NthIndexOf\" function index value, third argument, must be an integer type");

    if (indexValue->IsNull())
        throw ExpressionTreeException("\"NthIndexOf\" function index value, third argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Int32);

    int32_t index;

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

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::IndexOf(sourceValue->ValueAsString(), testValue->ValueAsString(), index, Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::Power(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& exponentValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Power\" function source value, first argument, must be numeric");

    if (!IsNumericType(exponentValue->ValueType))
        throw ExpressionTreeException("\"Power\" function exponent value, second argument, must be numeric");

    // If source value or exponent value is Null, result is Null
    if (sourceValue->IsNull() || exponentValue->IsNull())
        return NullValue(sourceValue->ValueType);    

    const ExpressionValueType valueType = DeriveArithmeticOperationValueType(ExpressionOperatorType::Multiply, sourceValue->ValueType, exponentValue->ValueType);
    const ValueExpressionPtr value = Convert(sourceValue, valueType);
    const ValueExpressionPtr exponent = Convert(exponentValue, valueType);

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, pow(value->ValueAsBoolean(), exponent->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, pow(value->ValueAsInt32(), exponent->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, pow(value->ValueAsInt64(), exponent->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, pow(value->ValueAsDecimal(), exponent->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, pow(value->ValueAsDouble(), exponent->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::RegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("RegExMatch", regexValue, testValue, false);
}

ValueExpressionPtr ExpressionTree::RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const
{
    return EvaluateRegEx("RegExVal", regexValue, testValue, true);
}

ValueExpressionPtr ExpressionTree::Replace(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& replaceValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Replace\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Replace\" function test value, second argument, must be a string");

    if (replaceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Replace\" function replace value, third argument, must be a string");

    if (testValue->IsNull())
        throw ExpressionTreeException("\"Replace\" function test value, second argument, is null");

    if (replaceValue->IsNull())
        throw ExpressionTreeException("\"Replace\" function replace value, third argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::Replace(sourceValue->ValueAsString(), testValue->ValueAsString(), replaceValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::Reverse(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Reverse\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    string value = sourceValue->ValueAsString();
    value.reserve();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, value);
}

ValueExpressionPtr ExpressionTree::Round(const ValueExpressionPtr& sourceValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Round\" function argument must be numeric");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    if (IsIntegerType(sourceValue->ValueType))
        return sourceValue;

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, round(sourceValue->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, round(sourceValue->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Split(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& delimiterValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Split\" function source value, first argument, must be a string");

    if (delimiterValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Split\" function delimiter value, second argument, must be a string");

    if (delimiterValue->IsNull())
        throw ExpressionTreeException("\"Split\" function delimiter value, second argument, is null");

    if (!IsIntegerType(indexValue->ValueType))
        throw ExpressionTreeException("\"Split\" function index value, third argument, must be an integer type");

    if (indexValue->IsNull())
        throw ExpressionTreeException("\"Split\" function index value, third argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return sourceValue;

    int32_t index;

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

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::Split(sourceValue->ValueAsString(), delimiterValue->ValueAsString(), index, Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::Sqrt(const ValueExpressionPtr& sourceValue) const
{
    if (!IsNumericType(sourceValue->ValueType))
        throw ExpressionTreeException("\"Sqrt\" function argument must be numeric");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(sourceValue->ValueType);

    switch (sourceValue->ValueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, sqrt(sourceValue->ValueAsBoolean()));
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, sqrt(sourceValue->ValueAsInt32()));
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, sqrt(sourceValue->ValueAsInt64()));
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, sqrt(sourceValue->ValueAsDecimal()));
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, sqrt(sourceValue->ValueAsDouble()));
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::StartsWith(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"StartsWith\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"StartsWith\" function test value, second argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    if (testValue->IsNull())
        return ExpressionTree::False;

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::StartsWith(sourceValue->ValueAsString(), testValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::StrCount(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& testValue, const ValueExpressionPtr& ignoreCase) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"StrCount\" function source value, first argument, must be a string");

    if (testValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"StrCount\" function test value, second argument, must be a string");

    if (sourceValue->IsNull() || testValue->IsNull())
        return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, 0);

    const string& findValue = testValue->ValueAsString();

    if (findValue.empty())
        return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, 0);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::Count(sourceValue->ValueAsString(), findValue, Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::StrCmp(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ValueExpressionPtr& ignoreCase) const
{
    if (leftValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Contains\" function left value, first argument, must be a string");

    if (rightValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Contains\" function right value, second argument, must be a string");

    if (leftValue->IsNull() && rightValue->IsNull())
        return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, 0);

    if (leftValue->IsNull())
        return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, 1);

    if (rightValue->IsNull())
        return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, -1);

    return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, GSF::Compare(leftValue->ValueAsString(), rightValue->ValueAsString(), Convert(ignoreCase, ExpressionValueType::Boolean)->ValueAsBoolean()));
}

ValueExpressionPtr ExpressionTree::SubStr(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"SubStr\" function source value, first argument, must be a string");

    if (!IsIntegerType(indexValue->ValueType))
        throw ExpressionTreeException("\"SubStr\" function index value, second argument, must be an integer type");

    if (!IsIntegerType(lengthValue->ValueType))
        throw ExpressionTreeException("\"SubStr\" function length value, third argument, must be an integer type");

    if (indexValue->IsNull())
        throw ExpressionTreeException("\"SubStr\" function index value, second argument, is null");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

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
        return NewSharedPtr<ValueExpression>(ExpressionValueType::String, string(sourceText.substr(index, length)));

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, string(sourceText.substr(index)));
}

ValueExpressionPtr ExpressionTree::Trim(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Trim\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::Trim(sourceText));
}

ValueExpressionPtr ExpressionTree::TrimLeft(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"TrimLeft\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::TrimLeft(sourceText));
}

ValueExpressionPtr ExpressionTree::TrimRight(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"TrimRight\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::TrimRight(sourceText));
}

ValueExpressionPtr ExpressionTree::Upper(const ValueExpressionPtr& sourceValue) const
{
    if (sourceValue->ValueType != ExpressionValueType::String)
        throw ExpressionTreeException("\"Upper\" function source value, first argument, must be a string");

    // If source value is Null, result is Null
    if (sourceValue->IsNull())
        return NullValue(ExpressionValueType::String);

    const string sourceText = sourceValue->ValueAsString();

    return NewSharedPtr<ValueExpression>(ExpressionValueType::String, GSF::ToUpper(sourceText));
}

ValueExpressionPtr ExpressionTree::UtcNow() const
{
    return NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, GSF::UtcNow());
}

ValueExpressionPtr ExpressionTree::Multiply(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() * right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() * right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() * right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, left->ValueAsDecimal() * right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, left->ValueAsDouble() * right->ValueAsDouble());
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply multiplication \"*\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Divide(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
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
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() / right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() / right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, left->ValueAsDecimal() / right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, left->ValueAsDouble() / right->ValueAsDouble());
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply division \"/\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Modulus(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() % right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() % right->ValueAsInt64());
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

ValueExpressionPtr ExpressionTree::Add(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() + right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() + right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() + right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, left->ValueAsDecimal() + right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, left->ValueAsDouble() + right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::String, left->ValueAsString() + right->ValueAsString());
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply addition \"+\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Subtract(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() - right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() - right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() - right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Decimal, left->ValueAsDecimal() - right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Double, left->ValueAsDouble() - right->ValueAsDouble());
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
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, leftValue->ValueAsBoolean() << shiftValue);
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, leftValue->ValueAsInt32() << shiftValue);
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, leftValue->ValueAsInt64() << shiftValue);
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
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, leftValue->ValueAsBoolean() >> shiftValue);
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, leftValue->ValueAsInt32() >> shiftValue);
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, leftValue->ValueAsInt64() >> shiftValue);
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

ValueExpressionPtr ExpressionTree::BitwiseAnd(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() & right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() & right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() & right->ValueAsInt64());
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

ValueExpressionPtr ExpressionTree::BitwiseOr(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() | right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() | right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() | right->ValueAsInt64());
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

ValueExpressionPtr ExpressionTree::BitwiseXor(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(valueType);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() ^ right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int32, left->ValueAsInt32() ^ right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Int64, left->ValueAsInt64() ^ right->ValueAsInt64());
        case ExpressionValueType::Decimal:
        case ExpressionValueType::Double:
        case ExpressionValueType::String:
        case ExpressionValueType::Guid:
        case ExpressionValueType::DateTime:
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply bitwise \"^\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() < right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() < right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() < right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() < right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() < right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::Compare(left->ValueAsString(), right->ValueAsString()) < 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() < right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() < right->ValueAsDateTime());
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply less than \"<\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::LessThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() <= right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() <= right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() <= right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() <= right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() <= right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::Compare(left->ValueAsString(), right->ValueAsString()) <= 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() <= right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() <= right->ValueAsDateTime());
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply less than or equal \"<=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() > right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() > right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() > right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() > right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() > right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::Compare(left->ValueAsString(), right->ValueAsString()) > 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() > right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() > right->ValueAsDateTime());
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than \">\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::GreaterThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() >= right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() >= right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() >= right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() >= right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() >= right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::Compare(left->ValueAsString(), right->ValueAsString()) >= 0);
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() >= right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() >= right->ValueAsDateTime());
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply greater than or equal \">=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Equal(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType, const bool exactMatch) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() == right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() == right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() == right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() == right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() == right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, GSF::IsEqual(left->ValueAsString(), right->ValueAsString(), !exactMatch));
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() == right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() == right->ValueAsDateTime());
        case ExpressionValueType::Undefined:
            throw ExpressionTreeException("Cannot apply equal \"=\" operator to \"" + string(EnumName(valueType)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected expression value type encountered");
    }
}

ValueExpressionPtr ExpressionTree::NotEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const ExpressionValueType valueType, const bool exactMatch) const
{
    // If left or right value is Null, result is Null
    if (leftValue->IsNull() || rightValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr left = Convert(leftValue, valueType);
    const ValueExpressionPtr right = Convert(rightValue, valueType);

    switch (valueType)
    {
        case ExpressionValueType::Boolean:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsBoolean() != right->ValueAsBoolean());
        case ExpressionValueType::Int32:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt32() != right->ValueAsInt32());
        case ExpressionValueType::Int64:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsInt64() != right->ValueAsInt64());
        case ExpressionValueType::Decimal:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDecimal() != right->ValueAsDecimal());
        case ExpressionValueType::Double:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDouble() != right->ValueAsDouble());
        case ExpressionValueType::String:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, !GSF::IsEqual(left->ValueAsString(), right->ValueAsString(), !exactMatch));
        case ExpressionValueType::Guid:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsGuid() != right->ValueAsGuid());
        case ExpressionValueType::DateTime:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, left->ValueAsDateTime() != right->ValueAsDateTime());
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

ValueExpressionPtr ExpressionTree::Like(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const bool exactMatch) const
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

    string testExpression = GSF::Replace(rightOperand, "%", "*", false);
    const bool startsWithWildcard = GSF::StartsWith(testExpression, "*", false);
    const bool endsWithWildcard = GSF::EndsWith(testExpression, "*", false);
    const bool ignoreCase = !exactMatch;

    if (startsWithWildcard)
        testExpression = testExpression.substr(1);

    if (endsWithWildcard && !testExpression.empty())
        testExpression = testExpression.substr(0, testExpression.size() - 1);

    // "*" or "**" expression means match everything
    if (testExpression.empty())
        return ExpressionTree::True;

    // Wild cards in the middle of the string are not supported
    if (GSF::Contains(testExpression, "*", false))
        throw ExpressionTreeException("Right operand of \"LIKE\" expression \"" + rightOperand + "\" has an invalid pattern");

    if (startsWithWildcard && GSF::EndsWith(leftOperand, testExpression, ignoreCase))
        return ExpressionTree::True;

    if (endsWithWildcard && GSF::StartsWith(leftOperand, testExpression, ignoreCase))
        return ExpressionTree::True;

    if (startsWithWildcard && endsWithWildcard && GSF::Contains(leftOperand, testExpression, ignoreCase))
        return ExpressionTree::True;

    return ExpressionTree::False;
}

ValueExpressionPtr ExpressionTree::NotLike(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, const bool exactMatch) const
{
    // If left value is Null, result is Null
    if (leftValue->IsNull())
        return NullValue(ExpressionValueType::Boolean);

    const ValueExpressionPtr likeResult = Like(leftValue, rightValue, exactMatch);

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

template<class T>
T ExpressionTree::Unary(const T& unaryValue, const ExpressionUnaryType unaryOperation)
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

template<class T>
T ExpressionTree::UnaryFloat(const T& unaryValue, const ExpressionUnaryType unaryOperation, const ExpressionValueType unaryValueType)
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

bool ExpressionTree::UnaryBool(bool unaryValue, const ExpressionUnaryType unaryOperation)
{
    switch (unaryOperation)
    {
        case ExpressionUnaryType::Not:
            return !unaryValue;
        case ExpressionUnaryType::Plus:
        case ExpressionUnaryType::Minus:
            throw ExpressionTreeException("Cannot apply unary \"" + string(EnumName(unaryOperation)) + "\" operator to \"" + string(EnumName(ExpressionValueType::Boolean)) + "\"");
        default:
            throw ExpressionTreeException("Unexpected unary type encountered");
    }
}

ValueExpressionPtr ExpressionTree::Convert(const ValueExpressionPtr& sourceValue, const ExpressionValueType targetValueType) const
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
                    targetValue = value == 0LL;
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
                    targetValue = value == 0.0; //-V550
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
                    targetValue = ParseBoolean(value);
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
                    targetValue = ParseGuid(value.c_str());
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
            switch (targetValueType)
            {
                case ExpressionValueType::String:
                    targetValue = sourceValue->ToString();
                    break;
                case ExpressionValueType::Guid:
                    targetValue = sourceValue->ValueAsGuid();
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
            const datetime_t result = sourceValue->ValueAsDateTime();
            const time_t value = to_time_t(result);

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
                case ExpressionValueType::DateTime:
                    targetValue = result;
                    break;
                case ExpressionValueType::Guid:
                    throw ExpressionTreeException("Cannot convert \"DateTime\" to \"" + string(EnumName(targetValueType)) + "\"");
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

ValueExpressionPtr ExpressionTree::EvaluateRegEx(const string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, const bool returnMatchedValue) const
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

    smatch match;
    const bool result = regex_search(testText, match, expression);

    if (returnMatchedValue)
    {
        // RegExVal returns any matched value, otherwise empty string
        if (result)
            return NewSharedPtr<ValueExpression>(ExpressionValueType::String, match.str(0));

        return ExpressionTree::EmptyString;
    }

    // RegExMatch returns boolean result for if there was a matched value
    return result ? ExpressionTree::True : ExpressionTree::False;
}

ExpressionTree::ExpressionTree(DataTablePtr table) :
    m_table(std::move(table)),
    TopLimit(-1)
{
}

const DataTablePtr& ExpressionTree::Table() const
{
    return m_table;
}

ValueExpressionPtr ExpressionTree::Evaluate(const DataRowPtr& row)
{
    m_currentRow = row;
    return Evaluate(Root);
}

const ValueExpressionPtr ExpressionTree::True = NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, true);

const ValueExpressionPtr ExpressionTree::False = NewSharedPtr<ValueExpression>(ExpressionValueType::Boolean, false);

const ValueExpressionPtr ExpressionTree::EmptyString = NewSharedPtr<ValueExpression>(ExpressionValueType::String, string());

ValueExpressionPtr ExpressionTree::NullValue(const ExpressionValueType targetValueType)
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
            return NewSharedPtr<ValueExpression>(ExpressionValueType::DateTime, Nullable<datetime_t>(nullptr), true);
        default:
            return NewSharedPtr<ValueExpression>(ExpressionValueType::Undefined, nullptr);
    }
}