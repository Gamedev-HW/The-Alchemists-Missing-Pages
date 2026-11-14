namespace PotionLab
{
    /// <summary>Something in the scene the player can click or tap. Found by WorldPointer through the object's collider.</summary>
    public interface IClickable
    {
        void OnClick();
    }
}
