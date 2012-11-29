#if MONO
using System;
using System.Runtime;
using System.Security.Permissions;

namespace System.Threading
{
	/// <summary>Provides support for spin-based waiting.</summary>
	[__DynamicallyInvokable]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public struct SpinWait
	{
		internal const int YIELD_THRESHOLD = 10;
		internal const int SLEEP_0_EVERY_HOW_MANY_TIMES = 5;
		internal const int SLEEP_1_EVERY_HOW_MANY_TIMES = 20;
		private int m_count;
		/// <summary>Gets the number of times <see cref="M:System.Threading.SpinWait.SpinOnce" /> has been called on this instance.</summary>
		/// <returns>Returns an integer that represents the number of times <see cref="M:System.Threading.SpinWait.SpinOnce" /> has been called on this instance.</returns>
		[__DynamicallyInvokable]
		public int Count
		{
			[__DynamicallyInvokable]
			get
			{
				return this.m_count;
			}
		}
		/// <summary>Gets whether the next call to <see cref="M:System.Threading.SpinWait.SpinOnce" /> will yield the processor, triggering a forced context switch.</summary>
		/// <returns>Whether the next call to <see cref="M:System.Threading.SpinWait.SpinOnce" /> will yield the processor, triggering a forced context switch.</returns>
		[__DynamicallyInvokable]
		public bool NextSpinWillYield
		{
			[__DynamicallyInvokable]
			get
			{
				return this.m_count > 10 || PlatformHelper.IsSingleProcessor;
			}
		}
		/// <summary>Performs a single spin.</summary>
		[__DynamicallyInvokable]
		public void SpinOnce()
		{
			if (this.NextSpinWillYield)
			{
				//CdsSyncEtwBCLProvider.Log.SpinWait_NextSpinWillYield();
				int num = (this.m_count >= 10) ? (this.m_count - 10) : this.m_count;
				if (num % 20 == 19)
				{
					Thread.Sleep(1);
				}
				else
				{
                    Thread.Sleep(0);
                    //if (num % 5 == 4)
                    //{
                    //    Thread.Sleep(0);
                    //}
                    //else
                    //{
                    //    Thread.Sleep(0);
                    //}
				}
			}
			else
			{
				Thread.SpinWait(4 << this.m_count);
			}
			this.m_count = ((this.m_count == 2147483647) ? 10 : (this.m_count + 1));
		}
		/// <summary>Resets the spin counter.</summary>
		[__DynamicallyInvokable]
		public void Reset()
		{
			this.m_count = 0;
		}
		/// <summary>Spins until the specified condition is satisfied.</summary>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition" /> argument is null.</exception>
		[__DynamicallyInvokable]
		public static void SpinUntil(Func<bool> condition)
		{
			SpinWait.SpinUntil(condition, -1);
		}
		/// <summary>Spins until the specified condition is satisfied or until the specified timeout is expired.</summary>
		/// <returns>True if the condition is satisfied within the timeout; otherwise, false</returns>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition" /> argument is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
		[__DynamicallyInvokable]
		public static bool SpinUntil(Func<bool> condition, TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > 2147483647L)
			{
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			}
			return SpinWait.SpinUntil(condition, (int)timeout.TotalMilliseconds);
		}
		/// <summary>Spins until the specified condition is satisfied or until the specified timeout is expired.</summary>
		/// <returns>True if the condition is satisfied within the timeout; otherwise, false</returns>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition" /> argument is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
		[__DynamicallyInvokable]
		public static bool SpinUntil(Func<bool> condition, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "SpinWait_SpinUntil_TimeoutWrong");
			}
			if (condition == null)
			{
				throw new ArgumentNullException("condition", "SpinWait_SpinUntil_ArgumentNull");
			}
			uint num = 0u;
			if (millisecondsTimeout != 0 && millisecondsTimeout != -1)
			{
				num = TimeoutHelper.GetTime();
			}
			SpinWait spinWait = default(SpinWait);
			while (!condition())
			{
				if (millisecondsTimeout == 0)
				{
					return false;
				}
				spinWait.SpinOnce();
				if (millisecondsTimeout != -1 && spinWait.NextSpinWillYield && (long)millisecondsTimeout <= (long)((ulong)(TimeoutHelper.GetTime() - num)))
				{
					return false;
				}
			}
			return true;
		}
	}
}

#endif