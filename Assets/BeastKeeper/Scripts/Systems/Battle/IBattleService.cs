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
        bool IsBattleActive { get; }
        
        void TriggerBattle(MonsterData enemyData);
        void EndBattle(bool playerWon);
    }
}
