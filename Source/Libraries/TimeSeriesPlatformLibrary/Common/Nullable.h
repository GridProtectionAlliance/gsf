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

#include "Convert.h"

namespace GSF
{
    template<class T>
    class Nullable final // NOLINT
    {
    public:
        Nullable();
        Nullable(const T& value);
        Nullable(nullptr_t nullpointer);
        const Nullable<T>& operator=(const Nullable<T>& value); // NOLINT
        const Nullable<T>& operator=(const T& value); // NOLINT
        const Nullable<T>& operator=(nullptr_t nullpointer); // NOLINT
        bool HasValue() const;
        const T& GetValueOrDefault() const;
        const T& GetValueOrDefault(const T& def) const;
        bool TryGetValue(T& value) const;

        class NullableValue final // NOLINT
        {
        private:
            NullableValue();
            NullableValue(const T& value);

            void checkHasValue() const;

            bool m_hasValue;
            T m_value;

        public:
            NullableValue& operator=(const NullableValue&) = delete;
            operator const T&() const;
            const T& operator*() const;
            const T* operator&() const; // NOLINT

            // https://stackoverflow.com/questions/42183631/inability-to-overload-dot-operator-in-c
            const T* operator->() const;

            friend class Nullable;
        };

        NullableValue Value;
    };

    template<class T>
    Nullable<T>::NullableValue::NullableValue() : // NOLINT
        m_hasValue(false),
        m_value(T())
    {        
    }

    template<class T>
    Nullable<T>::NullableValue::NullableValue(const T& value):
        m_hasValue(true),
        m_value(value)
    {        
    }

    template<class T>
    Nullable<T>::NullableValue::operator const T&() const
    {
        checkHasValue();
        return m_value;
    }

    template<class T>
    const T& Nullable<T>::NullableValue::operator*() const
    {
        checkHasValue();
        return m_value;
    }

    template<class T> // NOLINT
    const T* Nullable<T>::NullableValue::operator&() const // NOLINT
    {
        checkHasValue();
        return &m_value;
    }

    template<class T>
    const T* Nullable<T>::NullableValue::operator->() const
    {
        checkHasValue();
        return &m_value;
    }

    template<class T>
    void Nullable<T>::NullableValue::checkHasValue() const
    {
        if (!m_hasValue)
            throw std::exception("Nullable object must have a value");
    }

    template<class T>
    bool Nullable<T>::HasValue() const { return Value.m_hasValue; }

    template<class T>
    const T& Nullable<T>::GetValueOrDefault() const
    {
        return Value.m_value;
    }

    template<class T>
    const T& Nullable<T>::GetValueOrDefault(const T& def) const
    {
        if (Value.m_hasValue)
            return Value.m_value;

        return def;
    }

    template<class T>
    bool Nullable<T>::TryGetValue(T& value) const
    {
        value = Value.m_value;
        return Value.m_hasValue;
    }

    template<class T>
    Nullable<T>::Nullable()
    {        
    }

    template<class T>
    Nullable<T>::Nullable(nullptr_t nullpointer)
    {
        (void)nullpointer;
    }

    template<class T>
    Nullable<T>::Nullable(const T& value)
        : Value(value)
    {        
    }

    template<class T2>
    bool operator==(const Nullable<T2> &op1, const Nullable<T2> &op2)
    {
        if (op1.Value.m_hasValue != op2.Value.m_hasValue)
            return false;

        if (op1.Value.m_hasValue)
            return op1.Value.m_value == op2.Value.m_value;
        
        return true;
    }

    template<class T2>
    bool operator==(const Nullable<T2> &op, const T2 &value)
    {
        if (!op.Value.m_hasValue)
            return false;

        return op.Value.m_value == value;
    }

    template<class T2>
    bool operator==(const T2 &value, const Nullable<T2> &op)
    {
        if (!op.Value.m_hasValue)
            return false;

        return op.Value.m_value == value;
    }

    template<class T2>
    bool operator==(const Nullable<T2> &op, nullptr_t nullpointer)
    {
        (void)nullpointer;
        return !op.Value.m_hasValue;
    }

    template<class T2>
    bool operator==(nullptr_t nullpointer, const Nullable<T2> &op)
    {
        (void)nullpointer;
        return !op.Value.m_hasValue;
    }

    template<class T2>
    bool operator!=(const Nullable<T2> &op1, const Nullable<T2> &op2)
    {
        if (op1.Value.m_hasValue != op2.Value.m_hasValue)
            return true;

        if (op1.Value.m_hasValue)
            return op1.Value.m_value != op2.Value.m_value;

        return false;
    }

    template<class T2>
    bool operator!=(const Nullable<T2> &op, const T2 &value)
    {
        if (!op.Value.m_hasValue)
            return true;

        return op.Value.m_value != value;
    }

    template<class T2>
    bool operator!=(const T2 &value, const Nullable<T2> &op)
    {
        if (!op.Value.m_hasValue)
            return false;

        return op.Value.m_value != value;
    }

    template<class T2>
    bool operator!=(const Nullable<T2> &op, nullptr_t nullpointer)
    {
        (void)nullpointer;
        return op.Value.m_hasValue;
    }

    template<class T2>
    bool operator!=(nullptr_t nullpointer, const Nullable<T2> &op)
    {
        (void)nullpointer;
        return op.Value.m_hasValue;
    }

    template<class T> // NOLINT
    const Nullable<T>& Nullable<T>::operator=(const Nullable<T>& value)
    {
        Value.m_hasValue = value.Value.m_hasValue;
        Value.m_value = value.Value.m_value;
        return *this;
    }

    template<class T> // NOLINT
    const Nullable<T>& Nullable<T>::operator=(const T& value)
    {
        Value.m_hasValue = true;
        Value.m_value = value;
        return *this;
    }

    template<class T>  // NOLINT
    const Nullable<T>& Nullable<T>::operator=(nullptr_t nullpointer)
    {
        (void)nullpointer;
        Value.m_hasValue = false;
        Value.m_value = T();
        return *this;
    }

    template<class T, class U>
    Nullable<T> CastAsNullable(const Nullable<U>& source)
    {
        if (source.HasValue())
            return static_cast<T>(source.GetValueOrDefault());

        return nullptr;
    }

    template<class T>
    std::string ToString(const Nullable<T>& value)
    {
        if (value.HasValue())
            return ToString(value.GetValueOrDefault());

        return {};
    }

    inline std::string ToString(const Nullable<std::string>& value)
    {
        if (value.HasValue())
            return value.GetValueOrDefault();

        return {};
    }

    inline std::string ToString(const Nullable<bool>& value)
    {
        if (value.HasValue())
            return value.GetValueOrDefault() ? "true" : "false";

        return {};
    }

    inline std::string ToString(const Nullable<decimal_t>& value)
    {
        if (value.HasValue())
            return value.GetValueOrDefault().str();

        return {};
    }

    inline std::string ToString(const Nullable<Guid>& value)
    {
        if (value.HasValue())
            return ToString(value.GetValueOrDefault());

        return {};
    }

    inline std::string ToString(const Nullable<DateTime>& value, const char* fmt = "%Y-%m-%d %H:%M:%S%F")
    {
        if (value.HasValue())
            return ToString(value.GetValueOrDefault(), fmt);

        return {};
    }

    template<class T>
    static int32_t CompareValues(Nullable<T> leftNullable, Nullable<T> rightNullable)
    {
        const bool leftHasValue = leftNullable.HasValue();
        const bool rightHasValue = rightNullable.HasValue();

        if (leftHasValue && rightHasValue)
        {
            const T& leftValue = leftNullable.GetValueOrDefault();
            const T& rightValue = rightNullable.GetValueOrDefault();
            return leftValue < rightValue ? -1 : (leftValue > rightValue ? 1 : 0);
        }

        if (!leftHasValue && !rightHasValue)
            return 0;

        return leftHasValue ? 1 : -1;
    }
}

#endif