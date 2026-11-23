using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    const int N = 100;

    static void Main()
    {
        Console.WriteLine($"CPU 核心数：{Environment.ProcessorCount}");
        ThreadPool.GetMinThreads(out int minW, out _);
        ThreadPool.GetMaxThreads(out int maxW, out _);
        Console.WriteLine($"线程池工作线程：最小 {minW}，最大 {maxW}");
        Console.WriteLine($"任务数：{N}\n");

        Console.WriteLine("=== 实验 1：100 个空任务（测创建开销）===");
        Show("Thread    ", Run("Thread", () => { }));
        Show("ThreadPool", Run("Pool", () => { }));
        Show("Task      ", Run("Task", () => { }));

        Console.WriteLine("\n=== 实验 2：100 个阻塞 50ms（测并行能力）===");
        Show("Thread    ", Run("Thread", () => Thread.Sleep(50)));
        Show("ThreadPool", Run("Pool", () => Thread.Sleep(50)));
        Show("Task      ", Run("Task", () => Thread.Sleep(50)));
    }

    static void Show(string name, (double ms, int threads) r)
        => Console.WriteLine($"  {name}  {r.ms,8:F2} ms   用到 {r.threads,3} 个线程");

    static (double ms, int threads) Run(string kind, Action work)
    {
        var ids = new ConcurrentDictionary<int, byte>();
        var done = new CountdownEvent(N);
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < N; i++)
        {
            void Body()
            {
                ids[Environment.CurrentManagedThreadId] = 0;
                work();
                done.Signal();
            }

            switch (kind)
            {
                case "Thread":
                    new Thread(Body) { IsBackground = true }.Start();
                    break;
                case "Pool":
                    ThreadPool.QueueUserWorkItem(_ => Body());
                    break;
                default:
                    Task.Run(Body);
                    break;
            }
        }

        done.Wait();
        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, ids.Count);
    }
}