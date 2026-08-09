using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for managing screen switching, popup panels, and general UI flow.
    /// </summary>
    public interface IUISystem : IGameService
    {
        void ShowScreen(string screenId);
        void HideScreen(string screenId);
        void PushPopup(string popupId, object data = null);
        void PopPopup();
    }
}
