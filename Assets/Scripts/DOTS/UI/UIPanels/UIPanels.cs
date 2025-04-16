using UnityEngine.UIElements;
using Unity.Entities;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct ShowPanelContext
    {
        public Entity spaceEntity;
        public EntityManager entityManager;
        public int playerID;
    }
    
    // This panel is for the controller to know what kind of events to send
    // This class might be obsolete
    public class OnLandPanel : Panel
    {
        public SpaceType PanelType { get; protected set; }
        public OnLandPanel(VisualElement parent) : base(parent) { }
        public virtual void Show(ShowPanelContext context) { }
    }
}
