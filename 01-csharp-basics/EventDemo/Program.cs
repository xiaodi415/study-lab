using System;

// ① 自定义事件参数：继承 EventArgs
class TemperatureAlarmEventArgs : EventArgs
{
    public double Temperature { get; }
    public double Threshold { get; }

    public TemperatureAlarmEventArgs(double temperature, double threshold)
    {
        Temperature = temperature;
        Threshold = threshold;
    }
}

class Sensor
{
    private readonly double _threshold;

    // ② 声明事件：外部只能 += / -=，不能赋值、不能调用
    public event EventHandler<TemperatureAlarmEventArgs>? Alarm;

    public Sensor(double threshold) => _threshold = threshold;

    public void Feed(double temperature)
    {
        Console.WriteLine($"  采集：{temperature}℃（阈值 {_threshold}℃）");
        if (temperature > _threshold)
        {
            OnAlarm(temperature);
        }
    }

    // ③ 触发事件的标准写法
    protected virtual void OnAlarm(double temperature)
    {
        var handler = Alarm;          // 先拷到局部变量：多线程下防止判断后被置空
        handler?.Invoke(this, new TemperatureAlarmEventArgs(temperature, _threshold));
    }
}

class Program
{
    static void Main()
    {
        var sensor = new Sensor(80);

        // 订阅者 1：普通方法
        sensor.Alarm += OnAlarm_Console;

        // 订阅者 2：lambda。必须存到变量里，否则退订不了
        EventHandler<TemperatureAlarmEventArgs> logHandler =
            (s, e) => Console.WriteLine($"  [日志] 报警已记录：{e.Temperature}℃");
        sensor.Alarm += logHandler;

        Console.WriteLine("=== 1. 正常温度（不触发）===");
        sensor.Feed(75);

        Console.WriteLine("\n=== 2. 超阈值（两个订阅者都收到）===");
        sensor.Feed(95);

        Console.WriteLine("\n=== 3. 退订控制台，只留日志 ===");
        sensor.Alarm -= OnAlarm_Console;
        sensor.Feed(99);

        Console.WriteLine("\n=== 4. 全部退订（事件为 null）===");
        sensor.Alarm -= logHandler;
        sensor.Feed(100);

        Console.WriteLine("\n=== 5. 订阅者抛异常会中断后面所有订阅者 ===");
        sensor.Alarm += (s, e) => throw new Exception("订阅者 1 炸了");
        sensor.Alarm += OnAlarm_Console;
        try { sensor.Feed(101); }
        catch (Exception ex) { Console.WriteLine($"  捕获：{ex.Message}"); }
    }

    static void OnAlarm_Console(object? sender, TemperatureAlarmEventArgs e)
    {
        Console.WriteLine($"  [控制台] 报警！{e.Temperature}℃ 超过阈值 {e.Threshold}℃");
    }
}