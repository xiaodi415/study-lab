#nullable enable
using System;


class TraceRecord
{
    public string Sn { get; set; } = "";
    public string? Operator { get; set; }        // 可能没有操作员
    public DeviceInfo? Device { get; set; }      // 可能没有设备信息
}

class DeviceInfo
{
    public string Model { get; set; } = "";
}

class Program
{
    static void Main()
    {
        TraceRecord? rec = Find("SN001");   // 模拟：查不到，返回 null

        Console.WriteLine("=== 1. ?. 空条件 ===");
        Console.WriteLine($"  rec?.Sn            = {rec?.Sn ?? "(null)"}");

        Console.WriteLine("\n=== 2. 链式 ?. ===");
        Console.WriteLine($"  rec?.Device?.Model = {rec?.Device?.Model ?? "(null)"}");
        Console.WriteLine($"  rec?.Operator      = {rec?.Operator ?? "(null)"}");

        Console.WriteLine("\n=== 3. ?? 提供默认值 ===");
        Console.WriteLine($"  rec?.Operator ?? anonymous = {rec?.Operator ?? "anonymous"}");

        Console.WriteLine("\n=== 4. ??= 只在 null 时赋值 ===");
        rec ??= new TraceRecord { Sn = "DEFAULT" };
        Console.WriteLine($"  rec?.Sn            = {rec?.Sn}");

        Console.WriteLine("\n=== 5. 不用 ?. 会怎样 ===");
        TraceRecord? missing = Find("SN999");
        try
        {
            Console.WriteLine("  " + missing.Sn);   // 编译器会警告 CS8602
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("  NullReferenceException —— 运行时炸了");
        }
    }

    static TraceRecord? Find(string sn) => null;
}