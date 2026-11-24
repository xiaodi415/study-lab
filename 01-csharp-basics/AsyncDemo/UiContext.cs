using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace AsyncDemo
{
    /// <summary>
    /// 模拟 UI 线程的同步上下文（脚手架，不用改也不用深究）
    /// 真实场景里这个角色由 WinForms/WPF 的消息循环扮演
    /// </summary>
    class UiContext : SynchronizationContext
    {
        private readonly BlockingCollection<Action> _queue = new();

        public override void Post(SendOrPostCallback d, object? state)
            => _queue.Add(() => d(state));

        public void Pump()                       // 消息循环
        {
            foreach (var action in _queue.GetConsumingEnumerable())
                action();
        }

        public void Stop() => _queue.CompleteAdding();
    }
}
