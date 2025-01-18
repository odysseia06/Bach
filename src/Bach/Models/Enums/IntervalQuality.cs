namespace Bach.Models.Enums
{
    /// <summary>
    /// Defines the quality (modifier) of an interval.
    /// Common values: Perfect (P), Major (M), Minor (m), Augmented (A), Diminished (d)
    /// Doubly augmented/diminished exist, but are rare.
    /// </summary>
    public enum IntervalQuality
    {
        Perfect,
        Major,
        Minor,
        Augmented,
        Diminished,
        DoublyAugmented,
        DoublyDiminished
    }
}
