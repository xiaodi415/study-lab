using System;

// ① 自定义委托：int 进，int 出
delegate int Calc(int x);

class Program
{
    static void Main()
    {
        Console.WriteLine("=== 1. 三种写法做同一件事 ===");

        Calc c1 = Square;                        // 自定义委托
        Func<int, int> c2 = Square;              // 内置泛型委托
        Action<string> log = Console.WriteLine;  // 无返回值

        Console.WriteLine($"  自定义 delegate : {c1(5)}");
        Console.WriteLine($"  Func<int,int>   : {c2(5)}");
        log("  Action<string>  : 没有返回值，只执行动作");

        Console.WriteLine("\n=== 2. 匿名方法与 lambda ===");
        Calc c3 = delegate (int x) { return x * x; };   // C# 2.0 匿名方法
        Calc c4 = x => x * x;                            // C# 3.0 lambda
        Console.WriteLine($"  匿名方法 : {c3(5)}");
        Console.WriteLine($"  lambda   : {c4(5)}");

        Console.WriteLine("\n=== 3. 多播：一个委托挂多个方法 ===");
        Action<string> pipeline = Step1;
        pipeline += Step2;
        pipeline += Step3;
        pipeline("hello");

        Console.WriteLine("\n=== 4. 用 -= 摘掉一个 ===");
        pipeline -= Step2;
        pipeline("hello");

        Console.WriteLine("\n=== 5. 多播的坑：返回值只保留最后一个 ===");
        Func<int> multi = () => 1;
        multi += () => 2;
        multi += () => 3;
        Console.WriteLine($"  multi() = {multi()}   （不是 6，也不是 1）");

        Console.WriteLine("\n=== 6. 委托作为参数（回调）===");
        Process(10, Square);
        Process(10, x => x + 100);
    }

    static int Square(int x) => x * x;

    static void Step1(string s) => Console.WriteLine($"  Step1: {s}");
    static void Step2(string s) => Console.WriteLine($"  Step2: {s}");
    static void Step3(string s) => Console.WriteLine($"  Step3: {s}");

    static void Process(int value, Func<int, int> handler)
        => Console.WriteLine($"  Process({value}) → {handler(value)}");
}