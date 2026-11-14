using System;

namespace PotionLab
{
    /// <summary>
    /// How an ingredient has been prepared. Each spell adds a flag, so "sliced and heated" is Sliced | Heated.
    /// (The numbers match the recipe assets that are already saved: 0 raw, 1 sliced, 2 heated, 3 both.)
    /// </summary>
    [Flags]
    public enum Prep
    {
        Raw = 0,
        Sliced = 1,
        Heated = 2,
        SlicedHeated = Sliced | Heated
    }

    public static class PrepExtensions
    {
        public static bool Includes(this Prep prep, Prep part)
        {
            return (prep & part) == part;
        }

        public static string Describe(this Prep prep)
        {
            switch (prep)
            {
                case Prep.Sliced: return "sliced";
                case Prep.Heated: return "heated";
                case Prep.SlicedHeated: return "sliced and heated";
                default: return "raw";
            }
        }
    }
}
