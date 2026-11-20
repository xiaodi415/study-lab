using System;
using System.Collections.Generic;
using System.Linq;

// 所有实体都要有 Id，这个约束让我们能访问
interface IEntity
{
    int Id { get; set; }
}

class Device : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => $"#{Id} {Name}";
}

class WorkOrder : IEntity
{
    public int Id { get; set; }
    public string No { get; set; } = "";
    public override string ToString() => $"#{Id} {No}";
}

// 版本 A：只有 class + new()
class RepoA<T> where T : class, new()
{
    private readonly List<T> _items = new();

    public T Create() => new T();          // ✅ new() 约束让这行合法

    public void Add(T item) => _items.Add(item);

    public int Count => _items.Count;

    // ❌ 下面这行编译不过：T 上没有 Id 成员
    // public T? FindById(int id) => _items.FirstOrDefault(x => x.Id == id);
}

// 版本 B：加上 IEntity 约束
class Repository<T> where T : class, IEntity, new()
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public T Create()
    {
        var item = new T();
        item.Id = _nextId++;               // ✅ 有 IEntity 约束才能访问 Id
        _items.Add(item);
        return item;
    }

    public T? FindById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public IReadOnlyList<T> All() => _items;

    public bool Remove(int id)
    {
        var item = FindById(id);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }
}

class Program
{
    static void Main()
    {
        var repo = new Repository<Device>();

        var d1 = repo.Create();
        d1.Name = "COM3 采集器";
        var d2 = repo.Create();
        d2.Name = "COM4 采集器";

        Console.WriteLine($"设备总数：{repo.All().Count}");
        foreach (var d in repo.All()) Console.WriteLine($"  {d}");

        Console.WriteLine($"查 #2：{repo.FindById(2)}");
        Console.WriteLine($"删除 #1：{repo.Remove(1)}");
        Console.WriteLine($"剩余：{repo.All().Count}");

        // 同一套 Repository 直接服务另一种实体，一行没改
        var orders = new Repository<WorkOrder>();
        var wo = orders.Create();
        wo.No = "WO20261120";
        Console.WriteLine($"工单：{orders.FindById(1)}");

    

    }
}