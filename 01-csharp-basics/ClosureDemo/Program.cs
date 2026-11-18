using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== 1. 经典坑：for 循环 ===");
        var actions = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            actions.Add(() => Console.WriteLine($"  i = {i}"));
        }
        foreach (var a in actions) a();

        Console.WriteLine("\n=== 2. 修法 A：循环内建副本 ===");
        var actions2 = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            int copy = i;
            actions2.Add(() => Console.WriteLine($"  copy = {copy}"));
        }
        foreach (var a in actions2) a();

        Console.WriteLine("\n=== 3. 修法 B：用 foreach ===");
        var actions3 = new List<Action>();
        foreach (var i in Enumerable.Range(0, 3))
        {
            actions3.Add(() => Console.WriteLine($"  i = {i}"));
        }
        foreach (var a in actions3) a();

        Console.WriteLine("\n=== 4. 现实版：给每个设备注册回调（错误写法）===");
        var handlers = new List<Action>();
        for (int idx = 0; idx < 3; idx++)
        {
            handlers.Add(() => Console.WriteLine($"  [COM{idx}] 数据上报"));
        }
        foreach (var h in handlers) h();

        Console.WriteLine("\n=== 5. 修法：先固定住端口名 ===");
        var handlers2 = new List<Action>();
        for (int idx = 0; idx < 3; idx++)
        {
            string port = $"COM{idx}";
            handlers2.Add(() => Console.WriteLine($"  [{port}] 数据上报"));
        }
        foreach (var h in handlers2) h();

        Console.WriteLine("\n=== 6. 反向验证：捕获的是变量，不是值 ===");
        int x = 1;
        Action show = () => Console.WriteLine($"  x = {x}");
        show();
        x = 99;
        show();
    }
}