using EDA.Events;

namespace EDA.Bus
{
    /// <summary>
    /// Abstraction cho Event Channel. Service chỉ phụ thuộc interface này,
    /// nên đổi In-Memory -> RabbitMQ mà không phải sửa code service.
    /// </summary>
    public interface IEventBus : IDisposable
    {
        void Publish<T>(T @event) where T : IEvent;
        void Subscribe<T>(Action<T> handler) where T : IEvent;
    }
}
