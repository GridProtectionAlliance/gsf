using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace System
{
    internal interface ITuple
    {
        int Size
        {
            get;
        }
        string ToString(StringBuilder sb);
        int GetHashCode(IEqualityComparer comparer);
    }
    /// <summary>Defines methods to support the comparison of objects for structural equality. </summary>
    [__DynamicallyInvokable]
    public interface IStructuralEquatable
    {
        /// <summary>Determines whether an object is structurally equal to the current instance.</summary>
        /// <returns>true if the two objects are equal; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <param name="comparer">An object that determines whether the current instance and <paramref name="other" /> are equal. </param>
        [__DynamicallyInvokable]
        bool Equals(object other, IEqualityComparer comparer);
        /// <summary>Returns a hash code for the current instance.</summary>
        /// <returns>The hash code for the current instance.</returns>
        /// <param name="comparer">An object that computes the hash code of the current object.</param>
        [__DynamicallyInvokable]
        int GetHashCode(IEqualityComparer comparer);
    }
    /// <summary>Supports the structural comparison of collection objects.</summary>
    [__DynamicallyInvokable]
    public interface IStructuralComparable
    {
        /// <summary>Determines whether the current collection object precedes, occurs in the same position as, or follows another object in the sort order.</summary>
        /// <returns>An integer that indicates the relationship of the current collection object to <paramref name="other" />, as shown in the following table.Return valueDescription-1The current instance precedes <paramref name="other" />.0The current instance and <paramref name="other" /> are equal.1The current instance follows <paramref name="other" />.</returns>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <param name="comparer">An object that compares members of the current collection object with the corresponding members of <paramref name="other" />.</param>
        /// <exception cref="T:System.ArgumentException">This instance and <paramref name="other" /> are not the same type.</exception>
        [__DynamicallyInvokable]
        int CompareTo(object other, IComparer comparer);
    }
    /// <summary>Provides static methods for creating tuple objects. </summary>
    [__DynamicallyInvokable]
    public static class Tuple
    {
        /// <summary>Creates a new 1-tuple, or singleton.</summary>
        /// <returns>A tuple whose value is (<paramref name="item1" />).</returns>
        /// <param name="item1">The value of the only component of the tuple.</param>
        /// <typeparam name="T1">The type of the only component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }
        /// <summary>Creates a new 2-tuple, or pair.</summary>
        /// <returns>A 2-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
        /// <summary>Creates a new 3-tuple, or triple.</summary>
        /// <returns>A 3-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
        /// <summary>Creates a new 4-tuple, or quadruple.</summary>
        /// <returns>A 4-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.  </typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
        /// <summary>Creates a new 5-tuple, or quintuple.</summary>
        /// <returns>A 5-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
        /// <summary>Creates a new 6-tuple, or sextuple.</summary>
        /// <returns>A 6-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
        /// <summary>Creates a new 7-tuple, or septuple.</summary>
        /// <returns>A 7-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />, <paramref name="item7" />).</returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <param name="item7">The value of the seventh component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
        /// <summary>Creates a new 8-tuple, or octuple.</summary>
        /// <returns>An 8-tuple (octuple) whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />, <paramref name="item7" />, <paramref name="item8" />). </returns>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <param name="item5">The value of the fifth component of the tuple.</param>
        /// <param name="item6">The value of the sixth component of the tuple.</param>
        /// <param name="item7">The value of the seventh component of the tuple.</param>
        /// <param name="item8">The value of the eighth component of the tuple.</param>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
        /// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
        [__DynamicallyInvokable]
        public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(item1, item2, item3, item4, item5, item6, item7, new Tuple<T8>(item8));
        }
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2), h3);
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2), Tuple.CombineHashCodes(h3, h4));
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2, h3, h4), h5);
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2, h3, h4), Tuple.CombineHashCodes(h5, h6));
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2, h3, h4), Tuple.CombineHashCodes(h5, h6, h7));
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
        {
            return Tuple.CombineHashCodes(Tuple.CombineHashCodes(h1, h2, h3, h4), Tuple.CombineHashCodes(h5, h6, h7, h8));
        }
    }
    /// <summary>Represents a 1-tuple, or singleton. </summary>
    /// <typeparam name="T1">The type of the tuple's only component.</typeparam>
    /// <filterpriority>1</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        /// <summary>Gets the value of the <see cref="T:System.Tuple`1" /> object's single component. </summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`1" /> object's single component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 1;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`1" /> class.</summary>
        /// <param name="item1">The value of the tuple's only component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1)
        {
            this.m_Item1 = item1;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`1" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`1" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1> tuple = other as Tuple<T1>;
            return tuple != null && comparer.Equals(this.m_Item1, tuple.m_Item1);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`1" /> object to a specified object, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`1" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`1" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`1" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1> tuple = other as Tuple<T1>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            return comparer.Compare(this.m_Item1, tuple.m_Item1);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`1" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`1" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`1" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(this.m_Item1);
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`1" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`1" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 2-tuple, or pair. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`2" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`2" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`2" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`2" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 2;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`2" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`2" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`2" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2> tuple = other as Tuple<T1, T2>;
            return tuple != null && comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`2" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`2" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`2" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`2" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2> tuple = other as Tuple<T1, T2>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item2, tuple.m_Item2);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`2" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`2" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`2" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`2" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`2" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 3-tuple, or triple. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 3;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`3" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`3" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`3" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3> tuple = other as Tuple<T1, T2, T3>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2)) && comparer.Equals(this.m_Item3, tuple.m_Item3);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`3" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`3" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`3" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`3" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3> tuple = other as Tuple<T1, T2, T3>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item3, tuple.m_Item3);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`3" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`3" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`3" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`3" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`3" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 4-tuple, or quadruple. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's fourth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's fourth component.</returns>
        [__DynamicallyInvokable]
        public T4 Item4
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item4;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 4;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`4" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
            this.m_Item4 = item4;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`4" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`4" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false. </returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3, T4> tuple = other as Tuple<T1, T2, T3, T4>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3)) && comparer.Equals(this.m_Item4, tuple.m_Item4);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`4" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`4" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`4" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`4" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3, T4> tuple = other as Tuple<T1, T2, T3, T4>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item3, tuple.m_Item3);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item4, tuple.m_Item4);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`4" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`4" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`4" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`4" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`4" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(", ");
            sb.Append(this.m_Item4);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 5-tuple, or quintuple. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's fourth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's fourth component.</returns>
        [__DynamicallyInvokable]
        public T4 Item4
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item4;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's fifth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's fifth component.</returns>
        [__DynamicallyInvokable]
        public T5 Item5
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item5;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 5;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`5" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
            this.m_Item4 = item4;
            this.m_Item5 = item5;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`5" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`5" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3, T4, T5> tuple = other as Tuple<T1, T2, T3, T4, T5>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4)) && comparer.Equals(this.m_Item5, tuple.m_Item5);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`5" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`5" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`5" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`5" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3, T4, T5> tuple = other as Tuple<T1, T2, T3, T4, T5>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item3, tuple.m_Item3);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item4, tuple.m_Item4);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item5, tuple.m_Item5);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`5" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`5" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`5" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`5" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`5" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(", ");
            sb.Append(this.m_Item4);
            sb.Append(", ");
            sb.Append(this.m_Item5);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 6-tuple, or sextuple. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        private readonly T6 m_Item6;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's fourth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's fourth component.</returns>
        [__DynamicallyInvokable]
        public T4 Item4
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item4;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's fifth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's fifth  component.</returns>
        [__DynamicallyInvokable]
        public T5 Item5
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item5;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's sixth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's sixth component.</returns>
        [__DynamicallyInvokable]
        public T6 Item6
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item6;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 6;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`6" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
            this.m_Item4 = item4;
            this.m_Item5 = item5;
            this.m_Item6 = item6;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`6" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`6" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3, T4, T5, T6> tuple = other as Tuple<T1, T2, T3, T4, T5, T6>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4) && comparer.Equals(this.m_Item5, tuple.m_Item5)) && comparer.Equals(this.m_Item6, tuple.m_Item6);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`6" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`6" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`6" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`6" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3, T4, T5, T6> tuple = other as Tuple<T1, T2, T3, T4, T5, T6>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item3, tuple.m_Item3);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item4, tuple.m_Item4);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item5, tuple.m_Item5);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item6, tuple.m_Item6);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`6" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`6" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`6" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`6" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`6" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(", ");
            sb.Append(this.m_Item4);
            sb.Append(", ");
            sb.Append(this.m_Item5);
            sb.Append(", ");
            sb.Append(this.m_Item6);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents a 7-tuple, or septuple. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        private readonly T6 m_Item6;
        private readonly T7 m_Item7;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's fourth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's fourth component.</returns>
        [__DynamicallyInvokable]
        public T4 Item4
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item4;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's fifth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's fifth component.</returns>
        [__DynamicallyInvokable]
        public T5 Item5
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item5;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's sixth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's sixth component.</returns>
        [__DynamicallyInvokable]
        public T6 Item6
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item6;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's seventh component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's seventh component.</returns>
        [__DynamicallyInvokable]
        public T7 Item7
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item7;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 7;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`7" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        /// <param name="item7">The value of the tuple's seventh component.</param>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
            this.m_Item4 = item4;
            this.m_Item5 = item5;
            this.m_Item6 = item6;
            this.m_Item7 = item7;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`7" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`7" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3, T4, T5, T6, T7> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4) && comparer.Equals(this.m_Item5, tuple.m_Item5) && comparer.Equals(this.m_Item6, tuple.m_Item6)) && comparer.Equals(this.m_Item7, tuple.m_Item7);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`7" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`7" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`7" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`7" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3, T4, T5, T6, T7> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item3, tuple.m_Item3);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item4, tuple.m_Item4);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item5, tuple.m_Item5);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item6, tuple.m_Item6);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Item7, tuple.m_Item7);
        }
        /// <summary>Returns the hash code for the current <see cref="T:System.Tuple`7" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`7" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" /> method calculates the hash code of the current <see cref="T:System.Tuple`7" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7));
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`7" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`7" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(", ");
            sb.Append(this.m_Item4);
            sb.Append(", ");
            sb.Append(this.m_Item5);
            sb.Append(", ");
            sb.Append(this.m_Item6);
            sb.Append(", ");
            sb.Append(this.m_Item7);
            sb.Append(")");
            return sb.ToString();
        }
    }
    /// <summary>Represents an n-tuple, where n is 8 or greater.</summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    /// <typeparam name="TRest">Any generic Tuple object that defines the types of the tuple's remaining components.</typeparam>
    /// <filterpriority>2</filterpriority>
    [__DynamicallyInvokable]
    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        private readonly T6 m_Item6;
        private readonly T7 m_Item7;
        private readonly TRest m_Rest;
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's first component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's first component.</returns>
        [__DynamicallyInvokable]
        public T1 Item1
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item1;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's second component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's second component.</returns>
        [__DynamicallyInvokable]
        public T2 Item2
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item2;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's third component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's third component.</returns>
        [__DynamicallyInvokable]
        public T3 Item3
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item3;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's fourth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's fourth component.</returns>
        [__DynamicallyInvokable]
        public T4 Item4
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item4;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's fifth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's fifth component.</returns>
        [__DynamicallyInvokable]
        public T5 Item5
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item5;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's sixth component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's sixth component.</returns>
        [__DynamicallyInvokable]
        public T6 Item6
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item6;
            }
        }
        /// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's seventh component.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's seventh component.</returns>
        [__DynamicallyInvokable]
        public T7 Item7
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Item7;
            }
        }
        /// <summary>Gets the current <see cref="T:System.Tuple`8" /> object's remaining components.</summary>
        /// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's remaining components.</returns>
        [__DynamicallyInvokable]
        public TRest Rest
        {
            [__DynamicallyInvokable]
            get
            {
                return this.m_Rest;
            }
        }
        int ITuple.Size
        {
            get
            {
                return 7 + ((ITuple)((object)this.m_Rest)).Size;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`8" /> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param>
        /// <param name="item2">The value of the tuple's second component.</param>
        /// <param name="item3">The value of the tuple's third component.</param>
        /// <param name="item4">The value of the tuple's fourth component</param>
        /// <param name="item5">The value of the tuple's fifth component.</param>
        /// <param name="item6">The value of the tuple's sixth component.</param>
        /// <param name="item7">The value of the tuple's seventh component.</param>
        /// <param name="rest">Any generic Tuple object that contains the values of the tuple's remaining components.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="rest" /> is not a generic Tuple object.</exception>
        [__DynamicallyInvokable]
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            if (!(rest is ITuple))
            {
                throw new ArgumentException("ArgumentException_TupleLastArgumentNotATuple");
            }
            this.m_Item1 = item1;
            this.m_Item2 = item2;
            this.m_Item3 = item3;
            this.m_Item4 = item4;
            this.m_Item5 = item5;
            this.m_Item6 = item6;
            this.m_Item7 = item7;
            this.m_Rest = rest;
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`8" /> object is equal to a specified object.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }
        /// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`8" /> object is equal to a specified object based on a specified comparison method.</summary>
        /// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
        /// <param name="other">The object to compare with this instance.</param>
        /// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
        [__DynamicallyInvokable]
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;
            return tuple != null && (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4) && comparer.Equals(this.m_Item5, tuple.m_Item5) && comparer.Equals(this.m_Item6, tuple.m_Item6) && comparer.Equals(this.m_Item7, tuple.m_Item7)) && comparer.Equals(this.m_Rest, tuple.m_Rest);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`8" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`8" /> object.</exception>
        [__DynamicallyInvokable]
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        /// <summary>Compares the current <see cref="T:System.Tuple`8" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
        /// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
        /// <param name="other">An object to compare with the current instance.</param>
        /// <param name="comparer">An object that provides custom rules for comparison.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="other" /> is not a <see cref="T:System.Tuple`8" /> object.</exception>
        [__DynamicallyInvokable]
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType");
            }
            int num = comparer.Compare(this.m_Item1, tuple.m_Item1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item2, tuple.m_Item2);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item3, tuple.m_Item3);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item4, tuple.m_Item4);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item5, tuple.m_Item5);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item6, tuple.m_Item6);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(this.m_Item7, tuple.m_Item7);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(this.m_Rest, tuple.m_Rest);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`8" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        /// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`8" /> object by using a specified computation method.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`8" /> object.</param>
        [__DynamicallyInvokable]
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            ITuple tuple = (ITuple)((object)this.m_Rest);
            if (tuple.Size >= 8)
            {
                return tuple.GetHashCode(comparer);
            }
            switch (8 - tuple.Size)
            {
                case 1:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 2:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 3:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 4:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 5:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 6:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                case 7:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), tuple.GetHashCode(comparer));
                default:
                    return -1;
            }
        }
        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }
        /// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`8" /> instance.</summary>
        /// <returns>The string representation of this <see cref="T:System.Tuple`8" /> object.</returns>
        [__DynamicallyInvokable]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            return ((ITuple)this).ToString(stringBuilder);
        }
        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(", ");
            sb.Append(this.m_Item2);
            sb.Append(", ");
            sb.Append(this.m_Item3);
            sb.Append(", ");
            sb.Append(this.m_Item4);
            sb.Append(", ");
            sb.Append(this.m_Item5);
            sb.Append(", ");
            sb.Append(this.m_Item6);
            sb.Append(", ");
            sb.Append(this.m_Item7);
            sb.Append(", ");
            return ((ITuple)((object)this.m_Rest)).ToString(sb);
        }
    }
}
