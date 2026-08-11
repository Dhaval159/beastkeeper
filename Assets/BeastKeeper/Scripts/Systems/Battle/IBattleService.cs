using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Service interface to handle initiating battles and managing transitions between exploration and battle states.
    /// </summary>
    public interface IBattleService : IGameService
    {
        MonsterData ActiveEnemyData { get; }
        int ActiveEnemyLevel { get; }
        bool IsBattleActive { get; }
        
        void TriggerBattle(MonsterData enemyData, int level = 1);
        void EndBattle(bool playerWon);
    }
}
