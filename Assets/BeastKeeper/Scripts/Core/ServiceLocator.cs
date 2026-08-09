using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Core
{
    /// <summary>
    /// A simple runtime Service Locator to manage dependencies and system access.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IGameService> Services = new Dictionary<Type, IGameService>();

        /// <summary>
        /// Registers a service implementation of type T.
        /// </summary>
        public static void Register<T>(T service) where T : IGameService
        {
            Type type = typeof(T);
            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting.");
                Services[type] = service;
            }
            else
            {
                Services.Add(type, service);
            }
        }

        /// <summary>
        /// Unregisters a service of type T.
        /// </summary>
        public static void Unregister<T>() where T : IGameService
        {
            Type type = typeof(T);
            if (Services.ContainsKey(type))
            {
                Services.Remove(type);
            }
            else
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is not registered.");
            }
        }

        /// <summary>
        /// Retrieves a service of type T. Throws an exception if not found.
        /// </summary>
        public static T Get<T>() where T : IGameService
        {
            Type type = typeof(T);
            if (Services.TryGetValue(type, out IGameService service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"[ServiceLocator] Service of type {type.Name} is not registered.");
        }

        /// <summary>
        /// Tries to retrieve a service of type T. Returns true if successful.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : IGameService
        {
            Type type = typeof(T);
            if (Services.TryGetValue(type, out IGameService foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = default;
            return false;
        }
    }
}
