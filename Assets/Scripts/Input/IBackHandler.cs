namespace PotionLab
{
    /// <summary>Something that can react to Back (Android) / Esc (PC). Returns true if it used the press.</summary>
    public interface IBackHandler
    {
        bool TryHandleBack();
    }
}
