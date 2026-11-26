using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 对比「不限流」和「信号量限流」的并发行为。
/// 场景：100 台设备要采集，但设备端最多只能同时处理 5 个请求。
/// </summary>
class Program
{
    const int TASKS = 100;      // 100 个采集任务
    const int WORK_MS = 50;     // 每个任务模拟耗时 50ms

    static int _current;        // 当前正在执行的任务数
    static int _peak;           // 历史峰值（今天的核心指标）

    // 只用来保护 _peak 的更新，昨天学的 lock，这里只改一个 int，开销可忽略
    static readonly object _gate = new object();

    static async Task Main()
    {
        Console.WriteLine($"任务 {TASKS} 个，每个耗时 {WORK_MS}ms\n");

        await Run("不限流  ", null);

        // SemaphoreSlim(5)：同时最多 5 个任务能拿到许可
        using var sem = new SemaphoreSlim(5);
        await Run("限流 5  ", sem);
    }

    static async Task Run(string name, SemaphoreSlim? sem)
    {
        _current = 0;
        _peak = 0;
        var sw = Stopwatch.StartNew();

        var tasks = new Task[TASKS];

        for (int i = 0; i < TASKS; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                // ① 拿许可：拿不到就在这里异步等待（不占线程）
                //    这是 SemaphoreSlim 比 lock 强的地方——可以 await
                if (sem != null) await sem.WaitAsync();

                try
                {
                    // ② 进入临界区，当前并发数 +1，并返回加完之后的值
                    int now = Interlocked.Increment(ref _current);

                    // ③ 更新峰值（用 lock 保护，因为比较和赋值是两步）
                    lock (_gate)
                    {
                        if (now > _peak) _peak = now;
                    }

                    // ④ 模拟设备通讯：用 await 而不是 Thread.Sleep
                    //    如果用 Sleep，100 个任务会被线程池卡成 8 并发，
                    //    测不出「不限流」的真实峰值
                    await Task.Delay(WORK_MS);
                }
                finally
                {
                    // ⑤ 无论如何都要减计数、还许可
                    //    如果这里漏了 Release，许可会被永久占用，
                    //    可用许可越来越少，最后所有任务都卡死
                    Interlocked.Decrement(ref _current);
                    sem?.Release();
                }
            });
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"[{name}] 耗时 {sw.ElapsedMilliseconds,5} ms   峰值并发 {_peak,3}");
    }
}