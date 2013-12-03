#if MONO

using System;
using System.Diagnostics.Tracing;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
namespace System.Threading
{

    internal static class PlatformHelper
    {
        private static volatile int s_processorCount;
        private static volatile int s_lastProcessorCountRefreshTicks;
        private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000;
        internal static int ProcessorCount
        {
            get
            {
                int tickCount = Environment.TickCount;
                int num = PlatformHelper.s_processorCount;
                if (num == 0 || tickCount - PlatformHelper.s_lastProcessorCountRefreshTicks >= 30000)
                {
                    num = (PlatformHelper.s_processorCount = Environment.ProcessorCount);
                    PlatformHelper.s_lastProcessorCountRefreshTicks = tickCount;
                }
                return num;
            }
        }
        internal static bool IsSingleProcessor
        {
            get
            {
                return PlatformHelper.ProcessorCount == 1;
            }
        }
    }

    internal static class TimeoutHelper
    {
        public static uint GetTime()
        {
            return (uint)Environment.TickCount;
        }
        public static int UpdateTimeOut(uint startTime, int originalWaitMillisecondsTimeout)
        {
            uint num = TimeoutHelper.GetTime() - startTime;
            if (num > 2147483647u)
            {
                return 0;
            }
            int num2 = originalWaitMillisecondsTimeout - (int)num;
            if (num2 <= 0)
            {
                return 0;
            }
            return num2;
        }
    }

    internal struct CancellationCallbackCoreWorkArguments
    {
        internal SparselyPopulatedArrayFragment<CancellationCallbackInfo> m_currArrayFragment;
        internal int m_currArrayIndex;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CancellationCallbackCoreWorkArguments(SparselyPopulatedArrayFragment<CancellationCallbackInfo> currArrayFragment, int currArrayIndex)
        {
            this.m_currArrayFragment = currArrayFragment;
            this.m_currArrayIndex = currArrayIndex;
        }
    }

    internal class SparselyPopulatedArray<T> where T : class
    {
        private readonly SparselyPopulatedArrayFragment<T> m_head;
        private volatile SparselyPopulatedArrayFragment<T> m_tail;
        internal SparselyPopulatedArrayFragment<T> Tail
        {
            get
            {
                return this.m_tail;
            }
        }
        internal SparselyPopulatedArray(int initialSize)
        {
            this.m_head = (this.m_tail = new SparselyPopulatedArrayFragment<T>(initialSize));
        }
        internal SparselyPopulatedArrayAddInfo<T> Add(T element)
        {
            SparselyPopulatedArrayFragment<T> sparselyPopulatedArrayFragment2;
            int num2;
            while (true)
            {
                SparselyPopulatedArrayFragment<T> sparselyPopulatedArrayFragment = this.m_tail;
                while (sparselyPopulatedArrayFragment.m_next != null)
                {
                    sparselyPopulatedArrayFragment = (this.m_tail = sparselyPopulatedArrayFragment.m_next);
                }
                for (sparselyPopulatedArrayFragment2 = sparselyPopulatedArrayFragment; sparselyPopulatedArrayFragment2 != null; sparselyPopulatedArrayFragment2 = sparselyPopulatedArrayFragment2.m_prev)
                {
                    if (sparselyPopulatedArrayFragment2.m_freeCount < 1)
                    {
                        sparselyPopulatedArrayFragment2.m_freeCount--;
                    }
                    if (sparselyPopulatedArrayFragment2.m_freeCount > 0 || sparselyPopulatedArrayFragment2.m_freeCount < -10)
                    {
                        int length = sparselyPopulatedArrayFragment2.Length;
                        int num = (length - sparselyPopulatedArrayFragment2.m_freeCount) % length;
                        if (num < 0)
                        {
                            num = 0;
                            sparselyPopulatedArrayFragment2.m_freeCount--;
                        }
                        for (int i = 0; i < length; i++)
                        {
                            num2 = (num + i) % length;
                            if (sparselyPopulatedArrayFragment2.m_elements[num2] == null && Interlocked.CompareExchange<T>(ref sparselyPopulatedArrayFragment2.m_elements[num2], element, default(T)) == null)
                            {
                                goto Block_5;
                            }
                        }
                    }
                }
                SparselyPopulatedArrayFragment<T> sparselyPopulatedArrayFragment3 = new SparselyPopulatedArrayFragment<T>((sparselyPopulatedArrayFragment.m_elements.Length == 4096) ? 4096 : (sparselyPopulatedArrayFragment.m_elements.Length * 2), sparselyPopulatedArrayFragment);
                if (Interlocked.CompareExchange<SparselyPopulatedArrayFragment<T>>(ref sparselyPopulatedArrayFragment.m_next, sparselyPopulatedArrayFragment3, null) == null)
                {
                    this.m_tail = sparselyPopulatedArrayFragment3;
                }
            }
        Block_5:
            int num3 = sparselyPopulatedArrayFragment2.m_freeCount - 1;
            sparselyPopulatedArrayFragment2.m_freeCount = ((num3 > 0) ? num3 : 0);
            return new SparselyPopulatedArrayAddInfo<T>(sparselyPopulatedArrayFragment2, num2);
        }
    }

    internal class SparselyPopulatedArrayFragment<T> where T : class
    {
        internal readonly T[] m_elements;
        internal volatile int m_freeCount;
        internal volatile SparselyPopulatedArrayFragment<T> m_next;
        internal volatile SparselyPopulatedArrayFragment<T> m_prev;
        internal T this[int index]
        {
            get
            {
                return Volatile.Read<T>(ref this.m_elements[index]);
            }
        }
        internal int Length
        {
            get
            {
                return this.m_elements.Length;
            }
        }
        internal SparselyPopulatedArrayFragment<T> Prev
        {
            get
            {
                return this.m_prev;
            }
        }

        internal SparselyPopulatedArrayFragment(int size)
            : this(size, null)
        {
        }
        internal SparselyPopulatedArrayFragment(int size, SparselyPopulatedArrayFragment<T> prev)
        {
            this.m_elements = new T[size];
            this.m_freeCount = size;
            this.m_prev = prev;
        }
        internal T SafeAtomicRemove(int index, T expectedElement)
        {
            T t = Interlocked.CompareExchange<T>(ref this.m_elements[index], default(T), expectedElement);
            if (t != null)
            {
                this.m_freeCount++;
            }
            return t;
        }
    }

    internal struct SparselyPopulatedArrayAddInfo<T> where T : class
    {
        private SparselyPopulatedArrayFragment<T> m_source;
        private int m_index;
        internal SparselyPopulatedArrayFragment<T> Source
        {
            get
            {
                return this.m_source;
            }
        }
        internal int Index
        {
            get
            {
                return this.m_index;
            }
        }

        internal SparselyPopulatedArrayAddInfo(SparselyPopulatedArrayFragment<T> source, int index)
        {
            this.m_source = source;
            this.m_index = index;
        }
    }

    /// <summary>Represents a callback delegate that has been registered with a <see cref="T:System.Threading.CancellationToken" />. </summary>
    [__DynamicallyInvokable]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable
    {
        private readonly CancellationCallbackInfo m_callbackInfo;
        private readonly SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> m_registrationInfo;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CancellationTokenRegistration(CancellationCallbackInfo callbackInfo, SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo)
        {
            this.m_callbackInfo = callbackInfo;
            this.m_registrationInfo = registrationInfo;
        }
        [FriendAccessAllowed]
        internal bool TryDeregister()
        {
            if (this.m_registrationInfo.Source == null)
            {
                return false;
            }
            CancellationCallbackInfo cancellationCallbackInfo = this.m_registrationInfo.Source.SafeAtomicRemove(this.m_registrationInfo.Index, this.m_callbackInfo);
            return cancellationCallbackInfo == this.m_callbackInfo;
        }
        /// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenRegistration" /> class.</summary>
        /// <exception cref="T:System.ObjectDisposedException">The token source that created this token registration instance has already been disposed.</exception>
        [__DynamicallyInvokable]
        public void Dispose()
        {
            bool flag = this.TryDeregister();
            CancellationCallbackInfo callbackInfo = this.m_callbackInfo;
            if (callbackInfo != null)
            {
                CancellationTokenSource cancellationTokenSource = callbackInfo.CancellationTokenSource;
                if (cancellationTokenSource.IsCancellationRequested && !cancellationTokenSource.IsCancellationCompleted && !flag && cancellationTokenSource.ThreadIDExecutingCallbacks != Thread.CurrentThread.ManagedThreadId)
                {
                    cancellationTokenSource.WaitForCallbackToComplete(this.m_callbackInfo);
                }
            }
        }
        /// <summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal.</summary>
        /// <returns>True if the instances are equal; otherwise, false.</returns>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        [__DynamicallyInvokable]
        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }
        /// <summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are not equal.</summary>
        /// <returns>True if the instances are not equal; otherwise, false.</returns>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        [__DynamicallyInvokable]
        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }
        /// <summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
        /// <returns>True, if both this and <paramref name="obj" /> are equal. False, otherwise.Two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal if they both refer to the output of a single call to the same Register method of a <see cref="T:System.Threading.CancellationToken" />.</returns>
        /// <param name="obj">The other object to which to compare this instance.</param>
        [__DynamicallyInvokable]
        public override bool Equals(object obj)
        {
            return obj is CancellationTokenRegistration && this.Equals((CancellationTokenRegistration)obj);
        }
        /// <summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
        /// <returns>True, if both this and <paramref name="other" /> are equal. False, otherwise. Two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal if they both refer to the output of a single call to the same Register method of a <see cref="T:System.Threading.CancellationToken" />.</returns>
        /// <param name="other">The other <see cref="T:System.Threading.CancellationTokenRegistration" /> to which to compare this instance.</param>
        [__DynamicallyInvokable]
        public bool Equals(CancellationTokenRegistration other)
        {
            return this.m_callbackInfo == other.m_callbackInfo && this.m_registrationInfo.Source == other.m_registrationInfo.Source && this.m_registrationInfo.Index == other.m_registrationInfo.Index;
        }
        /// <summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
        /// <returns>A hash code for the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            if (this.m_registrationInfo.Source != null)
            {
                return this.m_registrationInfo.Source.GetHashCode() ^ this.m_registrationInfo.Index.GetHashCode();
            }
            return this.m_registrationInfo.Index.GetHashCode();
        }
    }
}

#endif