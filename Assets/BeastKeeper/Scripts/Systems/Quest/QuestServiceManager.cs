using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Utility helper to lazily retrieve or register the IQuestSystem in the ServiceLocator.
    /// </summary>
    public static class QuestServiceManager
    {
        public static IQuestSystem Get()
        {
            if (!ServiceLocator.TryGet<IQuestSystem>(out var service))
            {
                service = new QuestSystem();
                ServiceLocator.Register<IQuestSystem>(service);
            }
            return service;
        }
    }
}
