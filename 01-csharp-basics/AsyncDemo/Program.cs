using AsyncDemo;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== 场景 1：UI 线程上用 .Result（会死锁）===");
        RunOnUiThread(async ctx =>
        {
            Console.WriteLine("  [UI] 调用 DoAsync().Result");
            int r = DoAsync().Result;                  // ← 阻塞 UI 线程
            Console.WriteLine($"  [UI] 结果 {r}");
            ctx.Stop();
        }, 2000);

        Console.WriteLine("=== 场景 2：async 到底，用 await（正确）===");
        RunOnUiThread(async ctx =>
        {
            Console.WriteLine("  [UI] await DoAsync()");
            int r = await DoAsync();                   // ← 不阻塞
            Console.WriteLine($"  [UI] 结果 {r}");
            ctx.Stop();
        }, 3000);

        Console.WriteLine("=== 场景 3：ConfigureAwait(false) 绕过 ===");
        RunOnUiThread(async ctx =>
        {
            Console.WriteLine("  [UI] 调用 DoAsyncNoCapture().Result");
            int r = DoAsyncNoCapture().Result;         // ← 阻塞，但续体不回来
            Console.WriteLine($"  [UI] 结果 {r}");
            ctx.Stop();
        }, 3000);
    }

    // 普通异步方法：await 后会回到原来的上下文
    static async Task<int> DoAsync()
    {
        Console.WriteLine("  [DoAsync] 开始，await Task.Delay(300)");
        await Task.Delay(300);
        Console.WriteLine("  [DoAsync] 续体执行");
        return 42;
    }

    // 加了 ConfigureAwait(false)：续体不回 UI 线程
    static async Task<int> DoAsyncNoCapture()
    {
        Console.WriteLine("  [DoAsyncNoCapture] 开始");
        await Task.Delay(300).ConfigureAwait(false);
        Console.WriteLine("  [DoAsyncNoCapture] 续体在线程池执行（不在 UI 线程）");
        return 42;
    }

    // 在"UI 线程"上跑一段代码，超时就判定死锁
    static void RunOnUiThread(Func<UiContext, Task> body, int timeoutMs)
    {
        var ctx = new UiContext();

        var uiThread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(ctx);
            ctx.Post(async _ => await body(ctx), null);
            ctx.Pump();
        })
        { IsBackground = true };

        uiThread.Start();

        if (uiThread.Join(timeoutMs))
            Console.WriteLine("  ✅ 正常结束\n");
        else
            Console.WriteLine($"  ❌ 超时 {timeoutMs}ms —— 死锁，UI 线程已放弃\n");
    }
}