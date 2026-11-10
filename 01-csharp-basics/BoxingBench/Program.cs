
using System.Collections;
using System.Diagnostics;
//0 8 变大
class Program
{
    const int N = 1_000_000;

    static void Main()
    {
        Console.WriteLine($".NET {Environment.Version}  |  64位进程: {Environment.Is64BitProcess}\n");

        // 预热：先跑小规模，让 JIT 编译好代码。
        // 否则第一次运行会把「编译时间」算进去，结论全错。
        _ = RunBoxing(1000);
        _ = RunGeneric(1000);

        long bestBox = long.MaxValue, bestGen = long.MaxValue;
        long boxAlloc = 0, genAlloc = 0;

        // 跑 3 轮取最小值：最小值最接近真实性能，受系统干扰最少
        for (int round = 0; round < 3; round++)
        {
            var a = RunBoxing(N);
            var b = RunGeneric(N);
            bestBox = Math.Min(bestBox, a.ms);
            bestGen = Math.Min(bestGen, b.ms);
            boxAlloc = a.alloc;
            genAlloc = b.alloc;
        }

        Console.WriteLine($"装箱  ArrayList : {bestBox,5} ms   分配 {boxAlloc / 1024.0 / 1024.0,6:F1} MB");
        Console.WriteLine($"泛型  List<int> : {bestGen,5} ms   分配 {genAlloc / 1024.0 / 1024.0,6:F1} MB");
        Console.WriteLine();
        Console.WriteLine($"时间倍数: {(double)bestBox / bestGen:F1} 倍");
        Console.WriteLine($"内存倍数: {(double)boxAlloc / genAlloc:F1} 倍");
    }

    static (long ms, long alloc) RunBoxing(int n)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        var list = new ArrayList(n);            // 预分配容量，排除扩容带来的干扰
        for (int i = 0; i < n; i++)
        {
            list.Add(i);                        // ← 每次都在装箱：int → object
        }

        sw.Stop();
        long alloc = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(list);                     // 防止编译器把整个 list 优化掉
        return (sw.ElapsedMilliseconds, alloc);
    }

    static (long ms, long alloc) RunGeneric(int n)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        var list = new List<int>(n);
        for (int i = 0; i < n; i++)
        {
            list.Add(i);                        // ← 直接存 int，零装箱
        }

        sw.Stop();
        long alloc = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(list);
        return (sw.ElapsedMilliseconds, alloc);
    }
}