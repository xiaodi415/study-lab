using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== 1. 定义查询，什么都不执行 ===");
        // 仅定义查询，不会立即执行 Numbers()。
        // 只有遍历 query，或调用 ToList()、Count() 等终结操作时才会执行。
        var query = Numbers().Where(n => n % 2 == 0);
        Console.WriteLine("  查询已定义（上面不该有任何 [Source] 输出）");

        Console.WriteLine("\n=== 2. 第一次遍历，才真正开始 ===");
        foreach (var n in query) Console.WriteLine($"  结果 {n}");

        Console.WriteLine("\n=== 3. 第二次遍历，又执行一遍 ===");
        foreach (var n in query) Console.WriteLine($"  结果 {n}");

        Console.WriteLine("\n=== 4. 加 ToList() 立即物化 ===");
        var list = Numbers().Where(n => n % 2 == 0).ToList();
        Console.WriteLine("  ToList 已完成，源已跑完");
        Console.WriteLine($"  遍历 list 第一次：{string.Join(",", list)}");
        Console.WriteLine($"  遍历 list 第二次：{string.Join(",", list)}");
        Console.WriteLine("  ↑ 两次都没有 [Source]，说明只查了一次");

        Console.WriteLine("\n=== 5. 捕获变量：定义后改条件，结果跟着变 ===");
        int min = 2;
        var q2 = new[] { 1, 2, 3, 4, 5 }.Where(x => x > min);
        Console.WriteLine($"  定义 q2 时 min = {min}");
        min = 4;
        Console.WriteLine($"  遍历前把 min 改成 {min}");
        Console.WriteLine($"  q2 的结果：{string.Join(",", q2)}");
    }

    // 每次「遍历」都会完整跑一遍这个方法
    static IEnumerable<int> Numbers()
    {
        Console.WriteLine("  [Source] 开始");
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine($"  [Source] 产出 {i}");
            // 暂停当前迭代器的执行，将当前值 i 返回给调用方。
            // 下次继续遍历时，会从这里继续执行。
            yield return i;
        }
        Console.WriteLine("  [Source] 结束");
    }
}