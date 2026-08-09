using System;
using System.Collections.Generic;

namespace BeastKeeper.Core
{
    /// <summary>
    /// A simple, type-safe global message broker/event bus.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Delegates = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a listener function to a specific event type.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (Delegates.TryGetValue(eventType, out Delegate del))
            {
                Delegates[eventType] = Delegate.Combine(del, listener);
            }
            else
            {
                Delegates[eventType] = listener;
            }
        }

        /// <summary>
        /// Unsubscribes a listener function from a specific event type.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (Delegates.TryGetValue(eventType, out Delegate del))
            {
                Delegate currentDel = Delegate.Remove(del, listener);
                if (currentDel == null)
                {
                    Delegates.Remove(eventType);
                }
                else
                {
                    Delegates[eventType] = currentDel;
                }
            }
        }

        /// <summary>
        /// Publishes/raises an event to all subscribers.
        /// </summary>
        public static void Raise<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (Delegates.TryGetValue(eventType, out Delegate del))
            {
                (del as Action<T>)?.Invoke(gameEvent);
            }
        }
    }
}
