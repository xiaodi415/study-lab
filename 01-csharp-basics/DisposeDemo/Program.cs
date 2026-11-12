using System;

class FakePort : IDisposable
{
    public string Name { get; }
    private bool _disposed;

    public FakePort(string name)
    {
        Name = name;
        Console.WriteLine($"  打开 {Name}");
    }

    public void Write(string s) => Console.WriteLine($"  {Name} 发送: {s}");

    public void Dispose()
    {
        if (_disposed) return;      // 幂等：重复调用不会出错
        _disposed = true;
        Console.WriteLine($"  关闭 {Name}");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== 1. 不用 using（忘了关）===");
        var p1 = new FakePort("COM3");
        p1.Write("hello");
        // 没调用 Dispose —— 端口一直占着

        Console.WriteLine("\n=== 2. 用 using ===");
        using (var p2 = new FakePort("COM4"))
        {
            p2.Write("hello");
        }

        Console.WriteLine("\n=== 3. using 里抛异常 ===");
        try
        {
            using var p3 = new FakePort("COM5");
            p3.Write("hello");
            throw new Exception("模拟设备异常");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  捕获: {ex.Message}");
        }

        Console.WriteLine("\n=== 4. 手写 try/finally ===");
        var p4 = new FakePort("COM6");
        try
        {
            p4.Write("hello");
        }
        finally
        {
            p4.Dispose();
        }
        using (var p = new FakePort("COM7"))
        {
            p.Write("hello");
            return;              // ← 提前退出
        }
    }
}