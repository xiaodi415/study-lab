using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// 三个 demo：BlockingCollection、Channel、背压对比。
/// 场景：采集快、入库慢，中间需要一个缓冲队列。
/// </summary>
class Program
{
    static async Task Main()
    {
        await Demo1_BlockingCollection();
        await Demo2_Channel();
        await Demo3_Backpressure();
    }

    /// <summary>
    /// Demo1：BlockingCollection —— 同步阻塞版。
    /// 队列满了 Add 会阻塞线程，队列空了 Take 会阻塞线程。
    /// </summary>
    static async Task Demo1_BlockingCollection()
    {
        Console.WriteLine("=== 1. BlockingCollection（容量 5，生产 10ms/个，消费 30ms/个）===");

        // boundedCapacity: 5 —— 队列最多存 5 个，满了生产者就被卡住
        using var queue = new BlockingCollection<int>(boundedCapacity: 5);

        // 生产者：速度快
        var producer = Task.Run(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                queue.Add(i);                      // 队列满时，这行会阻塞
                Console.WriteLine($"  [生产] {i}");
                Thread.Sleep(10);                  // 生产快：10ms 一个
            }
            queue.CompleteAdding();                // 明确告诉消费者：不会再有数据了
        });

        // 消费者：速度慢
        var consumer = Task.Run(() =>
        {
            // GetConsumingEnumerable 在队列空时等待，CompleteAdding 之后自动结束
            foreach (var item in queue.GetConsumingEnumerable())
            {
                Thread.Sleep(30);                  // 消费慢：30ms 一个
                Console.WriteLine($"    [消费] {item}");
            }
        });

        await Task.WhenAll(producer, consumer);
        Console.WriteLine("  完成\n");
    }

    /// <summary>
    /// Demo2：Channel —— 异步版，同样的逻辑，但等待时不占线程。
    /// </summary>
    static async Task Demo2_Channel()
    {
        Console.WriteLine("=== 2. Channel（容量 5，异步）===");

        // 有界 channel：容量 5
        var channel = Channel.CreateBounded<int>(5);

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= 10; i++)
            {
                await channel.Writer.WriteAsync(i);   // 满了就异步等待，不占线程
                Console.WriteLine($"  [生产] {i}");
                await Task.Delay(10);
            }
            channel.Writer.Complete();                 // 等价于 CompleteAdding
        });

        var consumer = Task.Run(async () =>
        {
            // ReadAllAsync 是异步遍历，空时 await，Complete 之后自动结束
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                await Task.Delay(30);
                Console.WriteLine($"    [消费] {item}");
            }
        });

        await Task.WhenAll(producer, consumer);
        Console.WriteLine("  完成\n");
    }

    /// <summary>
    /// Demo3：背压对比 —— 这是今天最重要的一课。
    /// 无界队列：生产者一路狂奔，200 个数据全堆在内存里。
    /// 有界队列：生产者被限速，内存占用被钉死在 10 个。
    /// </summary>
    static async Task Demo3_Backpressure()
    {
        Console.WriteLine("=== 3. 背压对比（生产 200 个，消费 5ms/个）===");

        await Run("无界   ", Channel.CreateUnbounded<int>());
        await Run("有界 10", Channel.CreateBounded<int>(10));

        static async Task Run(string name, Channel<int> ch)
        {
            int consumed = 0;
            var total = Stopwatch.StartNew();

            var consumer = Task.Run(async () =>
            {
                await foreach (var _ in ch.Reader.ReadAllAsync())
                {
                    consumed++;
                    await Task.Delay(5);           // 消费慢
                }
            });

            // 单独记录生产者耗时 —— 这是背压是否生效的指标
            var producerSw = Stopwatch.StartNew();
            for (int i = 0; i < 200; i++)
                await ch.Writer.WriteAsync(i);
            producerSw.Stop();
            ch.Writer.Complete();

            await consumer;
            total.Stop();

            Console.WriteLine($"  [{name}] 生产者耗时 {producerSw.ElapsedMilliseconds,4} ms   "
                            + $"总耗时 {total.ElapsedMilliseconds,4} ms   消费 {consumed}");
        }
    }
}