using System.Diagnostics;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine($".NET {Environment.Version}\n");

        // 预热，排除 JIT
        _ = Concat(100);
        _ = Build(100);

        Console.WriteLine($"{"规模",8} {"+= 拼接",11} {"StringBuilder",15} {"倍数",8} {"拼接分配",11}");
        Console.WriteLine(new string('-', 62));

        foreach (int n in new[] { 1_000, 4_000, 16_000 })
        {
            double t1 = double.MaxValue, t2 = double.MaxValue;
            long a1 = 0, a2 = 0;

            for (int r = 0; r < 3; r++)          // 3 轮取最小值
            {
                var x = Concat(n);
                var y = Build(n);
                t1 = Math.Min(t1, x.ms);
                t2 = Math.Min(t2, y.ms);
                a1 = x.alloc;
                a2 = y.alloc;
            }

            Console.WriteLine($"{n,8:N0} {t1,9:F2} ms {t2,13:F2} ms {t1 / t2,7:F1}x {a1 / 1048576.0,9:F1} MB");
        }
    }

    static (double ms, long alloc) Concat(int n)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        string s = string.Empty;
        for (int i = 0; i < n; i++)
        {
            s += "x";            // ← 每次循环都创建一个新字符串
        }

        sw.Stop();
        GC.KeepAlive(s);
        return (sw.Elapsed.TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    static (double ms, long alloc) Build(int n)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        var sb = new StringBuilder(n);   // 预分配容量，昨天学的
        for (int i = 0; i < n; i++)
        {
            sb.Append('x');
        }
        string s = sb.ToString();        // 全程只有这一次最终分配

        sw.Stop();
        GC.KeepAlive(s);
        return (sw.Elapsed.TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}