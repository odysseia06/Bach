using System;
using System.Collections.Generic;
using System.Linq;
using Bach.Models.Enums;

namespace Bach.Models
{
    public class Chord
    {
        public Pitch Root { get; private set; }
        public ChordQuality Quality { get; private set; }

        private List<Interval> Intervals { get; set; }

        #region Constructors

        public Chord(Pitch root, ChordQuality quality)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            Quality = quality;
            Intervals = GetIntervalsForQuality(quality);

            if (Intervals.Count < 3)
                throw new ArgumentException("A chord must have at least three notes.");
            if (Intervals[0].Number != 1 || Intervals[0].Quality != IntervalQuality.Perfect)
                throw new ArgumentException("First interval must be a Perfect Unison (P1).");
        }

        #endregion

        #region Public Methods

        public List<Pitch> GetPitches()
        {
            return Intervals.Select(interval => interval.ApplyToPitch(Root)).ToList();
        }

        public List<Note> GetNotes(NoteValue duration = NoteValue.Quarter,
                                   Accidental accidental = Accidental.Natural,
                                   Dynamics dynamics = Dynamics.MezzoForte,
                                   Articulation articulation = Articulation.Normal)
        {
            return GetPitches().Select(p => new Note(p, duration, accidental, dynamics, articulation)).ToList();
        }

        public Chord Transpose(Pitch newRoot)
        {
            return new Chord(newRoot, Quality);
        }

        public override string ToString()
        {
            var pitches = GetPitches();
            string pitchList = string.Join(" ", pitches.Select(p => p.NoteName + p.Octave));
            return $"{Root.NoteName}{Root.Octave} {Quality} Chord: {pitchList}";
        }

        #endregion

        #region Static Factory Methods

        // These methods create chords by calling the constructor with the appropriate ChordQuality.
        // They rely on GetIntervalsForQuality to define intervals.

        public static Chord CreateMajorChord(Pitch root) => new Chord(root, ChordQuality.Major);
        public static Chord CreateMinorChord(Pitch root) => new Chord(root, ChordQuality.Minor);
        public static Chord CreateDiminishedChord(Pitch root) => new Chord(root, ChordQuality.Diminished);
        public static Chord CreateAugmentedChord(Pitch root) => new Chord(root, ChordQuality.Augmented);
        public static Chord CreateDominantChord(Pitch root) => new Chord(root, ChordQuality.Dominant);
        public static Chord CreateMajor7Chord(Pitch root) => new Chord(root, ChordQuality.Major7);
        public static Chord CreateMinor7Chord(Pitch root) => new Chord(root, ChordQuality.Minor7);
        public static Chord CreateDominant7Chord(Pitch root) => new Chord(root, ChordQuality.Dominant7);
        public static Chord CreateDiminished7Chord(Pitch root) => new Chord(root, ChordQuality.Diminished7);
        public static Chord CreateHalfDiminished7Chord(Pitch root) => new Chord(root, ChordQuality.HalfDiminished7);
        public static Chord CreateAugmented7Chord(Pitch root) => new Chord(root, ChordQuality.Augmented7);
        public static Chord CreateSuspended2Chord(Pitch root) => new Chord(root, ChordQuality.Suspended2);
        public static Chord CreateSuspended4Chord(Pitch root) => new Chord(root, ChordQuality.Suspended4);
        public static Chord CreateMajor6Chord(Pitch root) => new Chord(root, ChordQuality.Major6);
        public static Chord CreateMinor6Chord(Pitch root) => new Chord(root, ChordQuality.Minor6);
        public static Chord CreateDominant9Chord(Pitch root) => new Chord(root, ChordQuality.Dominant9);
        public static Chord CreateMajor9Chord(Pitch root) => new Chord(root, ChordQuality.Major9);
        public static Chord CreateMinor9Chord(Pitch root) => new Chord(root, ChordQuality.Minor9);

        #endregion

        #region Interval Definitions

        private static List<Interval> GetIntervalsForQuality(ChordQuality quality)
        {
            switch (quality)
            {
                case ChordQuality.Major:
                    return new List<Interval> { P1(), M3(), P5() };
                case ChordQuality.Minor:
                    return new List<Interval> { P1(), m3(), P5() };
                case ChordQuality.Diminished:
                    return new List<Interval> { P1(), m3(), d5() };
                case ChordQuality.Augmented:
                    return new List<Interval> { P1(), M3(), A5() };
                case ChordQuality.Dominant:
                    return new List<Interval> { P1(), M3(), P5() }; // Just a major triad
                case ChordQuality.Major7:
                    return new List<Interval> { P1(), M3(), P5(), M7() };
                case ChordQuality.Minor7:
                    return new List<Interval> { P1(), m3(), P5(), m7() };
                case ChordQuality.Dominant7:
                    return new List<Interval> { P1(), M3(), P5(), m7() };
                case ChordQuality.Diminished7:
                    return new List<Interval> { P1(), m3(), d5(), d7() };
                case ChordQuality.HalfDiminished7:
                    return new List<Interval> { P1(), m3(), d5(), m7() };
                case ChordQuality.Augmented7:
                    return new List<Interval> { P1(), M3(), A5(), m7() };
                case ChordQuality.Suspended2:
                    return new List<Interval> { P1(), M2(), P5() };
                case ChordQuality.Suspended4:
                    return new List<Interval> { P1(), P4(), P5() };
                case ChordQuality.Major6:
                    return new List<Interval> { P1(), M3(), P5(), M6() };
                case ChordQuality.Minor6:
                    return new List<Interval> { P1(), m3(), P5(), M6() };
                case ChordQuality.Dominant9:
                    return new List<Interval> { P1(), M3(), P5(), m7(), M9() };
                case ChordQuality.Major9:
                    return new List<Interval> { P1(), M3(), P5(), M7(), M9() };
                case ChordQuality.Minor9:
                    return new List<Interval> { P1(), m3(), P5(), m7(), M9() };
                default:
                    // Default to a major triad if unknown
                    return new List<Interval> { P1(), M3(), P5() };
            }
        }

        #endregion

        #region Interval Shortcuts

        private static Interval P1() => new Interval(1, IntervalQuality.Perfect);
        private static Interval M2() => new Interval(2, IntervalQuality.Major);
        private static Interval m3() => new Interval(3, IntervalQuality.Minor);
        private static Interval M3() => new Interval(3, IntervalQuality.Major);
        private static Interval P4() => new Interval(4, IntervalQuality.Perfect);
        private static Interval P5() => new Interval(5, IntervalQuality.Perfect);
        private static Interval d5() => new Interval(5, IntervalQuality.Diminished);
        private static Interval A5() => new Interval(5, IntervalQuality.Augmented);
        private static Interval M6() => new Interval(6, IntervalQuality.Major);
        private static Interval m7() => new Interval(7, IntervalQuality.Minor);
        private static Interval M7() => new Interval(7, IntervalQuality.Major);
        private static Interval d7() => new Interval(7, IntervalQuality.Diminished);
        private static Interval M9() => new Interval(9, IntervalQuality.Major);

        #endregion
    }
}
