using EDA.Events;

namespace EDA.Bus
{
    /// <summary>EventBus in-memory gốc của bài thực hành (đồng bộ, cùng process).</summary>
    public class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<IEvent>>> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Action<IEvent>>();
            _handlers[type].Add(e => handler((T)e));
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            var type = @event.GetType();
            if (_handlers.TryGetValue(type, out var list))
            {
                foreach (var handler in list)
                    handler(@event);
            }
        }

        public void Dispose() { }
    }
}
