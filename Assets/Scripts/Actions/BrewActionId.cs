namespace PotionLab
{
    /// <summary>
    /// The actions on the bar buttons. The order matches the plaque images
    /// (slice, heat, to the pot, pour, stop, mix, serve), so keep it when adding new ones at the end.
    /// </summary>
    public enum BrewActionId
    {
        Slice,
        Heat,
        DropInPot,
        Pour,
        StopPour,
        Mix,
        Serve
    }
}
