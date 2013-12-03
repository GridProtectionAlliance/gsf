using System;
using System.Runtime;
using System.Collections.Generic;
using System.Threading;

namespace System.ServiceModel.Channels
{
        internal class SynchronizedPool<T> where T : class
 {
     private struct Entry
     {
         public int threadID;
         public T value;
     }
     private struct PendingEntry
     {
         public int returnCount;
         public int threadID;
     }
     private static class SynchronizedPoolHelper
     {
         public static readonly int ProcessorCount = SynchronizedPool<T>.SynchronizedPoolHelper.GetProcessorCount();
         private static int GetProcessorCount()
         {
             return Environment.ProcessorCount;
         }
     }
     private class GlobalPool
     {
         private Stack<T> items;
         private int maxCount;
         public int MaxCount
         {
             [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
             get
             {
                 return this.maxCount;
             }
             set
             {
                 lock (this.ThisLock)
                 {
                     while (this.items.Count > value)
                     {
                         this.items.Pop();
                     }
                     this.maxCount = value;
                 }
             }
         }
         private object ThisLock
         {
             get
             {
                 return this;
             }
         }
         public GlobalPool(int maxCount)
         {
             this.items = new Stack<T>();
             this.maxCount = maxCount;
         }
         public void DecrementMaxCount()
         {
             lock (this.ThisLock)
             {
                 if (this.items.Count == this.maxCount)
                 {
                     this.items.Pop();
                 }
                 this.maxCount--;
             }
         }
         public T Take()
         {
             if (this.items.Count > 0)
             {
                 lock (this.ThisLock)
                 {
                     if (this.items.Count > 0)
                     {
                         return this.items.Pop();
                     }
                 }
             }
             return default(T);
         }
         public bool Return(T value)
         {
             if (this.items.Count < this.MaxCount)
             {
                 lock (this.ThisLock)
                 {
                     if (this.items.Count < this.MaxCount)
                     {
                         this.items.Push(value);
                         return true;
                     }
                 }
                 return false;
             }
             return false;
         }
         public void Clear()
         {
             lock (this.ThisLock)
             {
                 this.items.Clear();
             }
         }
     }
     private SynchronizedPool<T>.Entry[] entries;
     private SynchronizedPool<T>.GlobalPool globalPool;
     private int maxCount;
     private SynchronizedPool<T>.PendingEntry[] pending;
     private int promotionFailures;
     private const int maxPendingEntries = 128;
     private const int maxPromotionFailures = 64;
     private const int maxReturnsBeforePromotion = 64;
     private const int maxThreadItemsPerProcessor = 16;
     private object ThisLock
     {
         get
         {
             return this;
         }
     }
     public SynchronizedPool(int maxCount)
     {
         int num = maxCount;
         int num2 = 16 + SynchronizedPool<T>.SynchronizedPoolHelper.ProcessorCount;
         if (num > num2)
         {
             num = num2;
         }
         this.maxCount = maxCount;
         this.entries = new SynchronizedPool<T>.Entry[num];
         this.pending = new SynchronizedPool<T>.PendingEntry[4];
         this.globalPool = new SynchronizedPool<T>.GlobalPool(maxCount);
     }
     public void Clear()
     {
         SynchronizedPool<T>.Entry[] array = this.entries;
         for (int i = 0; i < array.Length; i++)
         {
             array[i].value = default(T);
         }
         this.globalPool.Clear();
     }
     private void HandlePromotionFailure(int thisThreadID)
     {
         int num = this.promotionFailures + 1;
         if (num >= 64)
         {
             lock (this.ThisLock)
             {
                 this.entries = new SynchronizedPool<T>.Entry[this.entries.Length];
                 this.globalPool.MaxCount = this.maxCount;
             }
             this.PromoteThread(thisThreadID);
             return;
         }
         this.promotionFailures = num;
     }
     private bool PromoteThread(int thisThreadID)
     {
         lock (this.ThisLock)
         {
             for (int i = 0; i < this.entries.Length; i++)
             {
                 int threadID = this.entries[i].threadID;
                 if (threadID == thisThreadID)
                 {
                     bool result = true;
                     return result;
                 }
                 if (threadID == 0)
                 {
                     this.globalPool.DecrementMaxCount();
                     this.entries[i].threadID = thisThreadID;
                     bool result = true;
                     return result;
                 }
             }
         }
         return false;
     }
     private void RecordReturnToGlobalPool(int thisThreadID)
     {
         SynchronizedPool<T>.PendingEntry[] array = this.pending;
         int i = 0;
         while (i < array.Length)
         {
             int threadID = array[i].threadID;
             if (threadID == thisThreadID)
             {
                 int num = array[i].returnCount + 1;
                 if (num < 64)
                 {
                     array[i].returnCount = num;
                     return;
                 }
                 array[i].returnCount = 0;
                 if (!this.PromoteThread(thisThreadID))
                 {
                     this.HandlePromotionFailure(thisThreadID);
                     return;
                 }
                 break;
             }
             else
             {
                 if (threadID == 0)
                 {
                     return;
                 }
                 i++;
             }
         }
     }
     private void RecordTakeFromGlobalPool(int thisThreadID)
     {
         SynchronizedPool<T>.PendingEntry[] array = this.pending;
         for (int i = 0; i < array.Length; i++)
         {
             int threadID = array[i].threadID;
             if (threadID == thisThreadID)
             {
                 return;
             }
             if (threadID == 0)
             {
                 lock (array)
                 {
                     if (array[i].threadID == 0)
                     {
                         array[i].threadID = thisThreadID;
                         return;
                     }
                 }
             }
         }
         if (array.Length >= 128)
         {
             this.pending = new SynchronizedPool<T>.PendingEntry[array.Length];
             return;
         }
         SynchronizedPool<T>.PendingEntry[] destinationArray = new SynchronizedPool<T>.PendingEntry[array.Length * 2];
         Array.Copy(array, destinationArray, array.Length);
         this.pending = destinationArray;
     }
     public bool Return(T value)
     {
         int managedThreadId = Thread.CurrentThread.ManagedThreadId;
         return managedThreadId != 0 && (this.ReturnToPerThreadPool(managedThreadId, value) || this.ReturnToGlobalPool(managedThreadId, value));
     }
     private bool ReturnToPerThreadPool(int thisThreadID, T value)
     {
         SynchronizedPool<T>.Entry[] array = this.entries;
         int i = 0;
         while (i < array.Length)
         {
             int threadID = array[i].threadID;
             if (threadID == thisThreadID)
             {
                 if (array[i].value == null)
                 {
                     array[i].value = value;
                     return true;
                 }
                 return false;
             }
             else
             {
                 if (threadID == 0)
                 {
                     break;
                 }
                 i++;
             }
         }
         return false;
     }
     private bool ReturnToGlobalPool(int thisThreadID, T value)
     {
         this.RecordReturnToGlobalPool(thisThreadID);
         return this.globalPool.Return(value);
     }
     public T Take()
     {
         int managedThreadId = Thread.CurrentThread.ManagedThreadId;
         if (managedThreadId == 0)
         {
             return default(T);
         }
         T t = this.TakeFromPerThreadPool(managedThreadId);
         if (t != null)
         {
             return t;
         }
         return this.TakeFromGlobalPool(managedThreadId);
     }
     private T TakeFromPerThreadPool(int thisThreadID)
     {
         SynchronizedPool<T>.Entry[] array = this.entries;
         int i = 0;
         while (i < array.Length)
         {
             int threadID = array[i].threadID;
             if (threadID == thisThreadID)
             {
                 T value = array[i].value;
                 if (value != null)
                 {
                     array[i].value = default(T);
                     return value;
                 }
                 return default(T);
             }
             else
             {
                 if (threadID == 0)
                 {
                     break;
                 }
                 i++;
             }
         }
         return default(T);
     }
     private T TakeFromGlobalPool(int thisThreadID)
     {
         this.RecordTakeFromGlobalPool(thisThreadID);
         return this.globalPool.Take();
     }
 }
 internal abstract class InternalBufferManager
 {
     private class PooledBufferManager : InternalBufferManager
     {
         private abstract class BufferPool
         {
             private class SynchronizedBufferPool : InternalBufferManager.PooledBufferManager.BufferPool
             {
                 private SynchronizedPool<byte[]> innerPool;
                 internal SynchronizedBufferPool(int bufferSize, int limit) : base(bufferSize, limit)
                 {
                     this.innerPool = new SynchronizedPool<byte[]>(limit);
                 }
                 internal override void OnClear()
                 {
                     this.innerPool.Clear();
                 }
                 internal override byte[] Take()
                 {
                     return this.innerPool.Take();
                 }
                 internal override bool Return(byte[] buffer)
                 {
                     return this.innerPool.Return(buffer);
                 }
             }
             private class LargeBufferPool : InternalBufferManager.PooledBufferManager.BufferPool
             {
                 private Stack<byte[]> items;
                 private object ThisLock
                 {
                     get
                     {
                         return this.items;
                     }
                 }
                 internal LargeBufferPool(int bufferSize, int limit) : base(bufferSize, limit)
                 {
                     this.items = new Stack<byte[]>(limit);
                 }
                 internal override void OnClear()
                 {
                     lock (this.ThisLock)
                     {
                         this.items.Clear();
                     }
                 }
                 internal override byte[] Take()
                 {
                     lock (this.ThisLock)
                     {
                         if (this.items.Count > 0)
                         {
                             return this.items.Pop();
                         }
                     }
                     return null;
                 }
                 internal override bool Return(byte[] buffer)
                 {
                     lock (this.ThisLock)
                     {
                         if (this.items.Count < base.Limit)
                         {
                             this.items.Push(buffer);
                             return true;
                         }
                     }
                     return false;
                 }
             }
             private int bufferSize;
             private int count;
             private int limit;
             private int misses;
             private int peak;
             public int BufferSize
             {
                 get
                 {
                     return this.bufferSize;
                 }
             }
             public int Limit
             {
                 get
                 {
                     return this.limit;
                 }
             }
             public int Misses
             {
                 get
                 {
                     return this.misses;
                 }
                 set
                 {
                     this.misses = value;
                 }
             }
             public int Peak
             {
                 get
                 {
                     return this.peak;
                 }
             }
             public BufferPool(int bufferSize, int limit)
             {
                 this.bufferSize = bufferSize;
                 this.limit = limit;
             }
             public void Clear()
             {
                 this.OnClear();
                 this.count = 0;
             }
             public void DecrementCount()
             {
                 int num = this.count - 1;
                 if (num >= 0)
                 {
                     this.count = num;
                 }
             }
             public void IncrementCount()
             {
                 int num = this.count + 1;
                 if (num <= this.limit)
                 {
                     this.count = num;
                     if (num > this.peak)
                     {
                         this.peak = num;
                     }
                 }
             }
             internal abstract byte[] Take();
             internal abstract bool Return(byte[] buffer);
             internal abstract void OnClear();
             internal static InternalBufferManager.PooledBufferManager.BufferPool CreatePool(int bufferSize, int limit)
             {
                 if (bufferSize < 85000)
                 {
                     return new InternalBufferManager.PooledBufferManager.BufferPool.SynchronizedBufferPool(bufferSize, limit);
                 }
                 return new InternalBufferManager.PooledBufferManager.BufferPool.LargeBufferPool(bufferSize, limit);
             }
         }
         private const int minBufferSize = 128;
         private const int maxMissesBeforeTuning = 8;
         private const int initialBufferCount = 1;
         private readonly object tuningLock;
         private int[] bufferSizes;
         private InternalBufferManager.PooledBufferManager.BufferPool[] bufferPools;
         private long memoryLimit;
         private long remainingMemory;
         private bool areQuotasBeingTuned;
         private int totalMisses;
         public PooledBufferManager(long maxMemoryToPool, int maxBufferSize)
         {
             this.tuningLock = new object();
             this.memoryLimit = maxMemoryToPool;
             this.remainingMemory = maxMemoryToPool;
             List<InternalBufferManager.PooledBufferManager.BufferPool> list = new List<InternalBufferManager.PooledBufferManager.BufferPool>();
             int num = 128;
             while (true)
             {
                 long num2 = this.remainingMemory / (long)num;
                 int num3 = (num2 > 2147483647L) ? 2147483647 : ((int)num2);
                 if (num3 > 1)
                 {
                     num3 = 1;
                 }
                 list.Add(InternalBufferManager.PooledBufferManager.BufferPool.CreatePool(num, num3));
                 this.remainingMemory -= (long)num3 * (long)num;
                 if (num >= maxBufferSize)
                 {
                     break;
                 }
                 long num4 = (long)num * 2L;
                 if (num4 > (long)maxBufferSize)
                 {
                     num = maxBufferSize;
                 }
                 else
                 {
                     num = (int)num4;
                 }
             }
             this.bufferPools = list.ToArray();
             this.bufferSizes = new int[this.bufferPools.Length];
             for (int i = 0; i < this.bufferPools.Length; i++)
             {
                 this.bufferSizes[i] = this.bufferPools[i].BufferSize;
             }
         }
         public override void Clear()
         {
             for (int i = 0; i < this.bufferPools.Length; i++)
             {
                 InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
                 bufferPool.Clear();
             }
         }
         private void ChangeQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool, int delta)
         {
             if (TraceCore.BufferPoolChangeQuotaIsEnabled(Fx.Trace))
             {
                 TraceCore.BufferPoolChangeQuota(Fx.Trace, bufferPool.BufferSize, delta);
             }
             InternalBufferManager.PooledBufferManager.BufferPool bufferPool2 = bufferPool;
             int num = bufferPool2.Limit + delta;
             InternalBufferManager.PooledBufferManager.BufferPool bufferPool3 = InternalBufferManager.PooledBufferManager.BufferPool.CreatePool(bufferPool2.BufferSize, num);
             for (int i = 0; i < num; i++)
             {
                 byte[] array = bufferPool2.Take();
                 if (array == null)
                 {
                     break;
                 }
                 bufferPool3.Return(array);
                 bufferPool3.IncrementCount();
             }
             this.remainingMemory -= (long)(bufferPool2.BufferSize * delta);
             bufferPool = bufferPool3;
         }
         private void DecreaseQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool)
         {
             this.ChangeQuota(ref bufferPool, -1);
         }
         private int FindMostExcessivePool()
         {
             long num = 0L;
             int result = -1;
             for (int i = 0; i < this.bufferPools.Length; i++)
             {
                 InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
                 if (bufferPool.Peak < bufferPool.Limit)
                 {
                     long num2 = (long)(bufferPool.Limit - bufferPool.Peak) * (long)bufferPool.BufferSize;
                     if (num2 > num)
                     {
                         result = i;
                         num = num2;
                     }
                 }
             }
             return result;
         }
         private int FindMostStarvedPool()
         {
             long num = 0L;
             int result = -1;
             for (int i = 0; i < this.bufferPools.Length; i++)
             {
                 InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[i];
                 if (bufferPool.Peak == bufferPool.Limit)
                 {
                     long num2 = (long)bufferPool.Misses * (long)bufferPool.BufferSize;
                     if (num2 > num)
                     {
                         result = i;
                         num = num2;
                     }
                 }
             }
             return result;
         }
         private InternalBufferManager.PooledBufferManager.BufferPool FindPool(int desiredBufferSize)
         {
             for (int i = 0; i < this.bufferSizes.Length; i++)
             {
                 if (desiredBufferSize <= this.bufferSizes[i])
                 {
                     return this.bufferPools[i];
                 }
             }
             return null;
         }
         private void IncreaseQuota(ref InternalBufferManager.PooledBufferManager.BufferPool bufferPool)
         {
             this.ChangeQuota(ref bufferPool, 1);
         }
         public override void ReturnBuffer(byte[] buffer)
         {
             InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.FindPool(buffer.Length);
             if (bufferPool != null)
             {
                 if (buffer.Length != bufferPool.BufferSize)
                 {
                     throw Fx.Exception.Argument("buffer", InternalSR.BufferIsNotRightSizeForBufferManager);
                 }
                 if (bufferPool.Return(buffer))
                 {
                     bufferPool.IncrementCount();
                 }
             }
         }
         public override byte[] TakeBuffer(int bufferSize)
         {
             InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.FindPool(bufferSize);
             if (bufferPool == null)
             {
                 if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
                 {
                     TraceCore.BufferPoolAllocation(Fx.Trace, bufferSize);
                 }
                 return Fx.AllocateByteArray(bufferSize);
             }
             byte[] array = bufferPool.Take();
             if (array != null)
             {
                 bufferPool.DecrementCount();
                 return array;
             }
             if (bufferPool.Peak == bufferPool.Limit)
             {
                 bufferPool.Misses++;
                 if (++this.totalMisses >= 8)
                 {
                     this.TuneQuotas();
                 }
             }
             if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
             {
                 TraceCore.BufferPoolAllocation(Fx.Trace, bufferPool.BufferSize);
             }
             return Fx.AllocateByteArray(bufferPool.BufferSize);
         }
         private void TuneQuotas()
         {
             if (this.areQuotasBeingTuned)
             {
                 return;
             }
             bool flag = false;
             try
             {
                 Monitor.TryEnter(this.tuningLock, ref flag);
                 if (!flag || this.areQuotasBeingTuned)
                 {
                     return;
                 }
                 this.areQuotasBeingTuned = true;
             }
             finally
             {
                 if (flag)
                 {
                     Monitor.Exit(this.tuningLock);
                 }
             }
             int num = this.FindMostStarvedPool();
             if (num >= 0)
             {
                 InternalBufferManager.PooledBufferManager.BufferPool bufferPool = this.bufferPools[num];
                 if (this.remainingMemory < (long)bufferPool.BufferSize)
                 {
                     int num2 = this.FindMostExcessivePool();
                     if (num2 >= 0)
                     {
                         this.DecreaseQuota(ref this.bufferPools[num2]);
                     }
                 }
                 if (this.remainingMemory >= (long)bufferPool.BufferSize)
                 {
                     this.IncreaseQuota(ref this.bufferPools[num]);
                 }
             }
             for (int i = 0; i < this.bufferPools.Length; i++)
             {
                 InternalBufferManager.PooledBufferManager.BufferPool bufferPool2 = this.bufferPools[i];
                 bufferPool2.Misses = 0;
             }
             this.totalMisses = 0;
             this.areQuotasBeingTuned = false;
         }
     }
     private class GCBufferManager : InternalBufferManager
     {
         private static InternalBufferManager.GCBufferManager value = new InternalBufferManager.GCBufferManager();
         public static InternalBufferManager.GCBufferManager Value
         {
             get
             {
                 return InternalBufferManager.GCBufferManager.value;
             }
         }
         private GCBufferManager()
         {
         }
         public override void Clear()
         {
         }
         public override byte[] TakeBuffer(int bufferSize)
         {
             return Fx.AllocateByteArray(bufferSize);
         }
         public override void ReturnBuffer(byte[] buffer)
         {
         }
     }
     [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
     protected InternalBufferManager()
     {
     }
     public abstract byte[] TakeBuffer(int bufferSize);
     public abstract void ReturnBuffer(byte[] buffer);
     public abstract void Clear();
     public static InternalBufferManager Create(long maxBufferPoolSize, int maxBufferSize)
     {
         if (maxBufferPoolSize == 0L)
         {
             return InternalBufferManager.GCBufferManager.Value;
         }
         return new InternalBufferManager.PooledBufferManager(maxBufferPoolSize, maxBufferSize);
     }
 }
 /// <summary>Many  features require the use of buffers, which are expensive to create and destroy. You can use the <see cref="T:System.ServiceModel.Channels.BufferManager" /> class to manage a buffer pool. The pool and its buffers are created when you instantiate this class and destroyed when the buffer pool is reclaimed by garbage collection. Every time you need to use a buffer, you take one from the pool, use it, and return it to the pool when done. This process is much faster than creating and destroying a buffer every time you need to use one.</summary>
 [__DynamicallyInvokable]
 public abstract class BufferManager
 {
     private class WrappingBufferManager : BufferManager
     {
         private InternalBufferManager innerBufferManager;
         public InternalBufferManager InternalBufferManager
         {
             get
             {
                 return this.innerBufferManager;
             }
         }
         public WrappingBufferManager(InternalBufferManager innerBufferManager)
         {
             this.innerBufferManager = innerBufferManager;
         }
         public override byte[] TakeBuffer(int bufferSize)
         {
             if (bufferSize < 0)
             {
                 throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bufferSize", bufferSize, SR.GetString("ValueMustBeNonNegative")));
             }
             return this.innerBufferManager.TakeBuffer(bufferSize);
         }
         public override void ReturnBuffer(byte[] buffer)
         {
             if (buffer == null)
             {
                 throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
             }
             this.innerBufferManager.ReturnBuffer(buffer);
         }
         public override void Clear()
         {
             this.innerBufferManager.Clear();
         }
     }
     private class WrappingInternalBufferManager : InternalBufferManager
     {
         private BufferManager innerBufferManager;
         public WrappingInternalBufferManager(BufferManager innerBufferManager)
         {
             this.innerBufferManager = innerBufferManager;
         }
         public override void Clear()
         {
             this.innerBufferManager.Clear();
         }
         public override void ReturnBuffer(byte[] buffer)
         {
             this.innerBufferManager.ReturnBuffer(buffer);
         }
         public override byte[] TakeBuffer(int bufferSize)
         {
             return this.innerBufferManager.TakeBuffer(bufferSize);
         }
     }
     /// <summary>Gets a buffer of at least the specified size from the pool. </summary>
     /// <returns>A byte array that is the requested size of the buffer.</returns>
     /// <param name="bufferSize">The size, in bytes, of the requested buffer.</param>
     /// <exception cref="T:System.ArgumentOutOfRangeException">
     ///   <paramref name="bufferSize" /> cannot be less than zero.</exception>
     [__DynamicallyInvokable]
     public abstract byte[] TakeBuffer(int bufferSize);
     /// <summary>Returns a buffer to the pool.</summary>
     /// <param name="buffer">A reference to the buffer being returned.</param>
     /// <exception cref="T:System.ArgumentNullException">
     ///   <paramref name="buffer" /> reference cannot be null.</exception>
     /// <exception cref="T:System.ArgumentException">Length of <paramref name="buffer" /> does not match the pool's buffer length property.</exception>
     [__DynamicallyInvokable]
     public abstract void ReturnBuffer(byte[] buffer);
     /// <summary>Releases the buffers currently cached in the manager.</summary>
     [__DynamicallyInvokable]
     public abstract void Clear();
     /// <summary>Creates a new BufferManager with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
     /// <returns>Returns a <see cref="T:System.ServiceModel.Channels.BufferManager" /> object with the specified parameters.</returns>
     /// <param name="maxBufferPoolSize">The maximum size of the pool.</param>
     /// <param name="maxBufferSize">The maximum size of an individual buffer.</param>
     /// <exception cref="T:System.InsufficientMemoryException">There was insufficient memory to create the requested buffer pool.</exception>
     /// <exception cref="T:System.ArgumentOutOfRangeException">
     ///   <paramref name="maxBufferPoolSize" /> or <paramref name="maxBufferSize" /> was less than zero.</exception>
     [__DynamicallyInvokable]
     public static BufferManager CreateBufferManager(long maxBufferPoolSize, int maxBufferSize)
     {
         if (maxBufferPoolSize < 0L)
         {
             throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferPoolSize", maxBufferPoolSize, SR.GetString("ValueMustBeNonNegative")));
         }
         if (maxBufferSize < 0)
         {
             throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, SR.GetString("ValueMustBeNonNegative")));
         }
         return new BufferManager.WrappingBufferManager(InternalBufferManager.Create(maxBufferPoolSize, maxBufferSize));
     }
     internal static InternalBufferManager GetInternalBufferManager(BufferManager bufferManager)
     {
         if (bufferManager is BufferManager.WrappingBufferManager)
         {
             return ((BufferManager.WrappingBufferManager)bufferManager).InternalBufferManager;
         }
         return new BufferManager.WrappingInternalBufferManager(bufferManager);
     }
     /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.Channels.BufferManager" /> class. </summary>
     [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
     protected BufferManager()
     {
     }
 }
}
