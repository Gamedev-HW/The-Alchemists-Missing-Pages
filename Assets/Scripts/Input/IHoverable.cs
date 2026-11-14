namespace PotionLab
{
    /// <summary>Something that reacts when a mouse moves over it (phones have no hover).</summary>
    public interface IHoverable
    {
        void OnHoverStart();
        void OnHoverEnd();
    }
}
