using UnityEngine;

namespace PotionLab
{
    /// <summary>An ingredient on the shelf was clicked.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Shelf Item Event")]
    public class ShelfItemEvent : EventChannel<ShelfItem>
    {
    }
}
