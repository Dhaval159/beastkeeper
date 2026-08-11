using System;
using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Bridges battle events to the progression service so XP is awarded through the EventBus
    /// rather than via direct BattleSession-to-Progression coupling. Subscribe/Dispose is
    /// idempotent, so duplicate subscription attempts never double-award XP.
    /// </summary>
    public sealed class ProgressionEventBridge : IDisposable
    {
        private readonly IProgressionSystem progression;
        private bool subscribed;

        public ProgressionEventBridge(IProgressionSystem progression)
        {
            this.progression = progression;
            Subscribe();
        }

        public void Subscribe()
        {
            if (subscribed) return;
            EventBus.Subscribe<BattleVictoryEvent>(OnBattleVictory);
            subscribed = true;
        }

        public void Dispose()
        {
            if (!subscribed) return;
            EventBus.Unsubscribe<BattleVictoryEvent>(OnBattleVictory);
            subscribed = false;
        }

        private void OnBattleVictory(BattleVictoryEvent e)
        {
            if (progression == null) return;
            progression.AddExperience(Math.Max(1, e.ExperienceAwarded));
            progression.CheckLevelUp();
        }
    }
}
