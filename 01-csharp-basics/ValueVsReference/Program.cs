struct PointStruct { public int X; }
class PointClass { public int X; }

class Program
{
    static void Main()
    {
        // ① struct 赋值：改 s2，s1 会变吗？
        PointStruct s1 = new PointStruct { X = 1 };
        PointStruct s2 = s1;
        s2.X = 2;
        Console.WriteLine($"① struct  s1.X={s1.X}  s2.X={s2.X}");   // 猜：?1 2

        // ② class 赋值：改 c2，c1 会变吗？
        PointClass c1 = new PointClass { X = 1 };
        PointClass c2 = c1;
        c2.X = 2;
        Console.WriteLine($"② class   c1.X={c1.X}  c2.X={c2.X}");   // 猜：?1 2

        // ③ 传参：值类型，方法里改了，外面变吗？
        int n = 1;
        ChangeInt(n);
        Console.WriteLine($"③ 传 int  n={n}");                    // 猜：? 变 1

        // ④ 传参：方法改对象字段，外面变吗？
        ChangeField(c1);
        Console.WriteLine($"④ 改字段  c1.X={c1.X}");              // 猜：?没变 1

        // ⑤ 传参：方法里 new 一个新对象，外面变吗？
        Reassign(c1);
        Console.WriteLine($"⑤ 重新赋值 c1.X={c1.X}");             // 猜：? 不变

        // ⑥ 装箱后比较
        object o1 = 1;
        object o2 = 1;
        Console.WriteLine($"⑥ 装箱  ==:{o1 == o2}  Equals:{o1.Equals(o2)}");  // 猜：?
    }

    static void ChangeInt(int x) { x = 99; }

    static void ChangeField(PointClass p) { p.X = 50; }

    static void Reassign(PointClass p) { p = new PointClass { X = 99 }; }
}