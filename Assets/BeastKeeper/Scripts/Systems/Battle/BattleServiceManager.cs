using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Utility helper to lazily retrieve or register the IBattleService in the ServiceLocator.
    /// </summary>
    public static class BattleServiceManager
    {
        public static IBattleService Get()
        {
            if (!ServiceLocator.TryGet<IBattleService>(out var service))
            {
                service = new BattleService();
                ServiceLocator.Register<IBattleService>(service);
            }
            return service;
        }
    }
}
