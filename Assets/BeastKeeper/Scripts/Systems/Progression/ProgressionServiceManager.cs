using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Utility helper to lazily retrieve or register the IProgressionSystem in the ServiceLocator,
    /// and to keep the XP award bridge subscribed to battle events.
    /// </summary>
    public static class ProgressionServiceManager
    {
        private static ProgressionEventBridge eventBridge;

        public static IProgressionSystem Get()
        {
            if (!ServiceLocator.TryGet<IProgressionSystem>(out var service))
            {
                service = new PlayerProgression();
                ServiceLocator.Register<IProgressionSystem>(service);
            }

            if (eventBridge == null)
            {
                eventBridge = new ProgressionEventBridge(service);
            }

            return service;
        }
    }
}
