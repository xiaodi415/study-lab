using System;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// 对比四种并发计数方式的正确性与性能：
/// 无锁（错误）、lock、Interlocked、Monitor 手动写法。
/// 目的：亲眼看到「非原子操作会丢更新」，以及「原子指令比锁快多少」。
/// </summary>
class Program
{
    // 线程数：8 个。太少（如 2 个）冲突概率低，竞态不容易复现
    const int THREADS = 8;

    // 每线程累加次数：50 万次。次数越多，交错的机会越多，丢得越明显
    const int PER_THREAD = 500_000;

    // 期望的最终结果：8 × 50 万 = 400 万
    const long EXPECTED = (long)THREADS * PER_THREAD;

    static void Main()
    {
        Console.WriteLine($"线程 {THREADS} 个，每线程 {PER_THREAD:N0} 次，期望 {EXPECTED:N0}\n");

        Show("① 无锁（错误）  ", Naive());
        Show("② lock          ", WithLock());
        Show("③ Interlocked   ", WithInterlocked());
        Show("④ Monitor 手动   ", WithMonitor());
    }

    /// <summary>
    /// 统一打印：耗时、结果、丢失数。
    /// 「丢失数」是关键指标——它直接量化了竞态造成的错误。
    /// </summary>
    static void Show(string name, (double ms, long value) r)
    {
        long lost = EXPECTED - r.value;
        Console.WriteLine($"  {name} {r.ms,9:F1} ms   结果 {r.value,10:N0}   丢失 {lost,9:N0}");
    }

    /// <summary>
    /// ① 无锁版本：结果一定是错的。
    /// 用来证明 counter++ 不是原子操作。
    /// </summary>
    static (double ms, long value) Naive()
    {
        long counter = 0;                       // 被 8 个线程共享的变量
        var sw = Stopwatch.StartNew();

        RunThreads(() =>
        {
            for (int i = 0; i < PER_THREAD; i++)
            {
                counter++;                      // 读-改-写三步，中途可能被其他线程插队
                                                // 两个线程同时读到 100，各自写回 101 → 丢 1 次
            }
        });

        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, counter);
    }

    /// <summary>
    /// ② lock 版本：结果是正确的，但最慢。
    /// lock 会阻塞抢锁失败的线程，涉及内核同步和上下文切换。
    /// </summary>
    static (double ms, long value) WithLock()
    {
        long counter = 0;

        // 锁对象三原则：私有、引用类型、readonly
        // 不能是 string（字符串驻留会让全程序共用一把锁）
        // 不能是 this（外部也能 lock 同一对象，容易死锁）
        var gate = new object();

        var sw = Stopwatch.StartNew();

        RunThreads(() =>
        {
            for (int i = 0; i < PER_THREAD; i++)
            {
                lock (gate)                 // 同一时刻只允许一个线程进入临界区
                {
                    counter++;              // 现在这三步是原子的了
                }
            }
        });

        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, counter);
    }

    /// <summary>
    /// ③ Interlocked 版本：结果正确，且比 lock 快 5 倍以上。
    /// 底层是 CPU 的单条原子指令（x86 上是 lock xadd），不阻塞、不切换线程。
    /// </summary>
    static (double ms, long value) WithInterlocked()
    {
        long counter = 0;
        var sw = Stopwatch.StartNew();

        RunThreads(() =>
        {
            for (int i = 0; i < PER_THREAD; i++)
            {
                // ref 传的是变量本身（内存地址），不是副本
                // 这是 Interlocked 能直接改原变量的原因
                Interlocked.Increment(ref counter);
            }
        });

        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, counter);
    }

    /// <summary>
    /// ④ Monitor 手动版本：和 lock 完全等价。
    /// 写出来是为了证明：lock 就是 Monitor.Enter + try/finally + Monitor.Exit 的语法糖。
    /// 为什么必须 try/finally？——临界区里抛异常也要保证释放锁，否则永久死锁。
    /// </summary>
    static (double ms, long value) WithMonitor()
    {
        long counter = 0;
        var gate = new object();
        var sw = Stopwatch.StartNew();

        RunThreads(() =>
        {
            for (int i = 0; i < PER_THREAD; i++)
            {
                Monitor.Enter(gate);        // 拿锁（拿不到就阻塞在这里）
                try
                {
                    counter++;
                }
                finally
                {
                    Monitor.Exit(gate);     // 无论如何都要放锁
                }
            }
        });

        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, counter);
    }

    /// <summary>
    /// 统一启动 8 个线程，跑同一段工作，然后等全部结束。
    /// 注意：这里没有闭包陷阱——lambda 捕获的是 work 参数，不是循环变量 t。
    /// </summary>
    static void RunThreads(Action work)
    {
        var threads = new Thread[THREADS];

        for (int t = 0; t < THREADS; t++)
            threads[t] = new Thread(() => work());

        foreach (var th in threads) th.Start();   // 先全部启动
        foreach (var th in threads) th.Join();    // 再全部等待（分开写才是真并发）
    }
}