//******************************************************************************************************
//  Nullable.h - Gbtc
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
//  12/03/2018 - J. Ritchie Carroll
//       Generated original version of source code based on following stack overflow discussion:
//          https://stackoverflow.com/questions/2537942/nullable-values-in-c
//
//******************************************************************************************************

#ifndef __NULLABLE_H
#define __NULLABLE_H

#include <cstddef>
#include "Convert.h"

class NullableType
{
public:
    virtual ~NullableType() = default;
    
    virtual bool HasValue() const
    {
        return false;
    };
};

template <typename T>
class Nullable final : public NullableType
{
public:
    Nullable();
    Nullable(const T &value);
    Nullable(nullptr_t nullpointer);
    const Nullable<T> & operator=(const Nullable<T> &value);
    const Nullable<T> & operator=(const T &value);
    const Nullable<T> & operator=(nullptr_t nullpointer);
    bool HasValue() const override;
    const T & GetValueOrDefault() const;
    const T & GetValueOrDefault(const T &def) const;
    bool TryGetValue(T &value) const;

public:
    class NullableValue final
    {
    public:
        friend class Nullable;

    private:
        NullableValue();
        NullableValue(const T &value);

    public:
        NullableValue & operator=(const NullableValue &) = delete;
        operator const T &() const;
        const T & operator*() const;
        const T * operator&() const;

        // https://stackoverflow.com/questions/42183631/inability-to-overload-dot-operator-in-c
        const T * operator->() const;

    public:
        template <typename T2>
        friend bool operator==(const Nullable<T2> &op1, const Nullable<T2> &op2);

        template <typename T2>
        friend bool operator==(const Nullable<T2> &op, const T2 &value);

        template <typename T2>
        friend bool operator==(const T2 &value, const Nullable<T2> &op);

        template <typename T2>
        friend bool operator==(const Nullable<T2> &op, nullptr_t nullpointer);

        template <typename T2>
        friend bool operator!=(const Nullable<T2> &op1, const Nullable<T2> &op2);

        template <typename T2>
        friend bool operator!=(const Nullable<T2> &op, const T2 &value);

        template <typename T2>
        friend bool operator!=(const T2 &value, const Nullable<T2> &op);

        template <typename T2>
        friend bool operator==(nullptr_t nullpointer, const Nullable<T2> &op);

        template <typename T2>
        friend bool operator!=(const Nullable<T2> &op, nullptr_t nullpointer);

        template <typename T2>
        friend bool operator!=(nullptr_t nullpointer, const Nullable<T2> &op);

    private:
        void checkHasValue() const;

    private:
        bool m_hasValue;
        T m_value;
    };

public:
    NullableValue Value;
};

template <typename T>
Nullable<T>::NullableValue::NullableValue()
    : m_hasValue(false), m_value(T()) { }

template <typename T>
Nullable<T>::NullableValue::NullableValue(const T &value)
    : m_hasValue(true), m_value(value) { }

template <typename T>
Nullable<T>::NullableValue::operator const T &() const
{
    checkHasValue();
    return m_value;
}

template <typename T>
const T & Nullable<T>::NullableValue::operator*() const
{
    checkHasValue();
    return m_value;
}

template <typename T>
const T * Nullable<T>::NullableValue::operator&() const
{
    checkHasValue();
    return &m_value;
}

template <typename T>
const T * Nullable<T>::NullableValue::operator->() const
{
    checkHasValue();
    return &m_value;
}

template <typename T>
void Nullable<T>::NullableValue::checkHasValue() const
{
    if (!m_hasValue)
        throw std::exception("Nullable object must have a value");
}

template <typename T>
bool Nullable<T>::HasValue() const { return Value.m_hasValue; }

template <typename T>
const T & Nullable<T>::GetValueOrDefault() const
{
    return Value.m_value;
}

template <typename T>
const T & Nullable<T>::GetValueOrDefault(const T &def) const
{
    if (Value.m_hasValue)
        return Value.m_value;
    else
        return def;
}

template <typename T>
bool Nullable<T>::TryGetValue(T &value) const
{
    value = Value.m_value;
    return Value.m_hasValue;
}

template <typename T>
Nullable<T>::Nullable() { }

template <typename T>
Nullable<T>::Nullable(nullptr_t nullpointer) { (void)nullpointer; }

template <typename T>
Nullable<T>::Nullable(const T &value)
    : Value(value) { }

template <typename T2>
bool operator==(const Nullable<T2> &op1, const Nullable<T2> &op2)
{
    if (op1.Value.m_hasValue != op2.Value.m_hasValue)
        return false;

    if (op1.Value.m_hasValue)
        return op1.Value.m_value == op2.Value.m_value;
    else
        return true;
}

template <typename T2>
bool operator==(const Nullable<T2> &op, const T2 &value)
{
    if (!op.Value.m_hasValue)
        return false;

    return op.Value.m_value == value;
}

template <typename T2>
bool operator==(const T2 &value, const Nullable<T2> &op)
{
    if (!op.Value.m_hasValue)
        return false;

    return op.Value.m_value == value;
}

template <typename T2>
bool operator==(const Nullable<T2> &op, nullptr_t nullpointer)
{
    (void)nullpointer;
    return !op.Value.m_hasValue;
}

template <typename T2>
bool operator==(nullptr_t nullpointer, const Nullable<T2> &op)
{
    (void)nullpointer;
    return !op.Value.m_hasValue;
}

template <typename T2>
bool operator!=(const Nullable<T2> &op1, const Nullable<T2> &op2)
{
    if (op1.Value.m_hasValue != op2.Value.m_hasValue)
        return true;

    if (op1.Value.m_hasValue)
        return op1.Value.m_value != op2.Value.m_value;
    else
        return false;
}

template <typename T2>
bool operator!=(const Nullable<T2> &op, const T2 &value)
{
    if (!op.Value.m_hasValue)
        return true;

    return op.Value.m_value != value;
}

template <typename T2>
bool operator!=(const T2 &value, const Nullable<T2> &op)
{
    if (!op.Value.m_hasValue)
        return false;

    return op.Value.m_value != value;
}

template <typename T2>
bool operator!=(const Nullable<T2> &op, nullptr_t nullpointer)
{
    (void)nullpointer;
    return op.Value.m_hasValue;
}

template <typename T2>
bool operator!=(nullptr_t nullpointer, const Nullable<T2> &op)
{
    (void)nullpointer;
    return op.Value.m_hasValue;
}

template <typename T>
const Nullable<T> & Nullable<T>::operator=(const Nullable<T> &value)
{
    Value.m_hasValue = value.Value.m_hasValue;
    Value.m_value = value.Value.m_value;
    return *this;
}

template <typename T>
const Nullable<T> & Nullable<T>::operator=(const T &value)
{
    Value.m_hasValue = true;
    Value.m_value = value;
    return *this;
}

template <typename T>
const Nullable<T> & Nullable<T>::operator=(nullptr_t nullpointer)
{
    (void)nullpointer;
    Value.m_hasValue = false;
    Value.m_value = T();
    return *this;
}

template<typename T, typename U>
Nullable<T> CastAsNullable(const Nullable<U>& source)
{
    if (source.HasValue())
        return static_cast<T>(source.Value);

    return nullptr;
}

template<typename T>
std::string ToString(Nullable<T> value)
{
    if (value.HasValue())
        return GSF::TimeSeries::ToString(value.Value);

    return {};
}

inline std::string ToString(Nullable<std::string> value)
{
    if (value.HasValue())
        return value.Value;

    return {};
}

inline std::string ToString(Nullable<bool> value)
{
    if (value.HasValue())
        return static_cast<bool>(value.Value) ? "true" : "false";

    return {};
}

inline std::string ToString(Nullable<GSF::TimeSeries::decimal_t> value)
{
    if (value.HasValue())
        return static_cast<GSF::TimeSeries::decimal_t>(value.Value).str();

    return {};
}

inline std::string ToString(Nullable<GSF::TimeSeries::Guid> value)
{
    if (value.HasValue())
        return GSF::TimeSeries::ToString(static_cast<GSF::TimeSeries::Guid>(value.Value));

    return {};
}

inline std::string ToString(Nullable<time_t> value)
{
    if (value.HasValue())
        return GSF::TimeSeries::ToString(static_cast<time_t>(value.Value));

    return {};
}

#endif