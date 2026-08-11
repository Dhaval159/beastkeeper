using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Utility helper to lazily retrieve or register the IInventorySystem in the ServiceLocator.
    /// </summary>
    public static class InventoryServiceManager
    {
        public static IInventorySystem Get()
        {
            if (!ServiceLocator.TryGet<IInventorySystem>(out var service))
            {
                service = new InventorySystem();
                ServiceLocator.Register<IInventorySystem>(service);
            }
            return service;
        }
    }
}
