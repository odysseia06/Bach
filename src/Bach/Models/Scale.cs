using System;
using System.Collections.Generic;
using System.Linq;
using Bach.Models.Enums;

namespace Bach.Models
{
    /// <summary>
    /// Represents a musical scale defined by a tonic, a collection of Intervals representing each scale degree,
    /// a name, and a scale type.
    /// </summary>
    public class Scale
    {
        /// <summary>
        /// The tonic (root pitch) of the scale.
        /// </summary>
        public Pitch Tonic { get; private set; }

        /// <summary>
        /// The name of the scale, e.g. "C Major", "A Minor".
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of the scale (e.g. Major, Minor, Pentatonic, etc.).
        /// </summary>
        public ScaleType ScaleType { get; private set; }

        /// <summary>
        /// The intervals from the tonic to each scale degree. The first interval is always P1 (the tonic).
        /// For a major scale, this would be: P1, M2, M3, P4, P5, M6, M7, P8.
        /// </summary>
        private List<Interval> Intervals { get; set; }

        /// <summary>
        /// Number of notes in the scale pattern (including the octave if present).
        /// For a major scale, this would be 8 (C to C).
        /// </summary>
        public int Count => Intervals.Count;

        #region Constructors

        /// <summary>
        /// Creates a scale by specifying its tonic, a series of intervals that define the scale,
        /// a name, and a scale type.
        /// The first interval should represent the tonic (usually Perfect Unison).
        /// </summary>
        public Scale(Pitch tonic, IEnumerable<Interval> intervals, string name, ScaleType scaleType)
        {
            Tonic = tonic ?? throw new ArgumentNullException(nameof(tonic));

            if (intervals == null || !intervals.Any())
                throw new ArgumentException("Intervals cannot be null or empty.");

            Intervals = intervals.ToList();
            // Validate that the first interval is unison (P1)
            if (Intervals[0].Number != 1 || Intervals[0].Quality != IntervalQuality.Perfect)
                throw new ArgumentException("The first interval of a scale must be a Perfect Unison (P1).");

            Name = name;
            ScaleType = scaleType;
        }

        /// <summary>
        /// Convenience constructor to specify the tonic by note name and octave.
        /// </summary>
        public Scale(string tonicNoteName, int tonicOctave, IEnumerable<Interval> intervals, string name, ScaleType scaleType)
            : this(new Pitch(tonicNoteName, tonicOctave), intervals, name, scaleType)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the pitch at a given scale degree.
        /// If the degree exceeds the scale length, we wrap around to the next octave.
        /// For example, degree 9 in a major scale would be the 2nd degree plus an octave.
        /// </summary>
        /// <param name="degree">The scale degree (1-based).</param>
        public Pitch GetPitchAtDegree(int degree)
        {
            if (degree < 1)
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be >= 1.");

            // For example, a major scale has 8 intervals (including the octave).
            // Degrees beyond 8 continue into the next octave.
            int baseCount = Intervals.Count; // e.g. 8 for major scale
            int octaveShifts = (degree - 1) / (baseCount - 1);
            int indexInPattern = (degree - 1) % (baseCount - 1);

            // Get the interval at this pattern index (excluding octave repeats)
            Interval interval = Intervals[indexInPattern];

            // If we are beyond the first octave, we add an octave per shift.
            // One octave = 12 semitones, we can transpose the tonic by the octave shifts.
            // Alternatively, create a compound interval by adding octaves to the base interval.
            // Since compound intervals are formed by adding 7 to the interval number for each octave and adding 12 semitones:
            // We'll just transpose by semitones for simplicity.
            Pitch transposedPitch = interval.ApplyToPitch(Tonic);

            // Apply additional octave shifts (if any)
            int additionalSemitones = octaveShifts * 12;
            int finalMidi = transposedPitch.MidiNoteNumber + additionalSemitones;
            return new Pitch(finalMidi);
        }

        /// <summary>
        /// Gets all pitches of the scale within one octave.
        /// If includeOctave is true, includes the final interval (octave).
        /// </summary>
        public List<Pitch> GetPitches(bool includeOctave = true)
        {
            int limit = includeOctave ? Intervals.Count : Intervals.Count - 1;
            var pitches = new List<Pitch>();
            foreach (var interval in Intervals.Take(limit))
            {
                pitches.Add(interval.ApplyToPitch(Tonic));
            }
            return pitches;
        }

        /// <summary>
        /// Gets a Note at a given scale degree.
        /// By default, returns a quarter note with natural accidental and mezzo-forte dynamics.
        /// </summary>
        public Note GetNoteAtDegree(int degree, NoteValue duration = NoteValue.Quarter,
                                    Accidental accidental = Accidental.Natural,
                                    Dynamics dynamics = Dynamics.MezzoForte,
                                    Articulation articulation = Articulation.Normal)
        {
            Pitch pitch = GetPitchAtDegree(degree);
            return new Note(pitch, duration, accidental, dynamics, articulation);
        }

        /// <summary>
        /// Transposes the scale to a new tonic.
        /// The intervals and scale type remain the same.
        /// </summary>
        public Scale Transpose(Pitch newTonic, string newName = null)
        {
            return new Scale(newTonic, Intervals, newName ?? Name, ScaleType);
        }

        public override string ToString()
        {
            var pitches = GetPitches(true);
            string pitchList = string.Join(" ", pitches.Select(p => p.NoteName + p.Octave));
            return $"{Name} ({ScaleType}): {pitchList}";
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Creates a Major scale using Interval objects:
        /// Major scale intervals: P1, M2, M3, P4, P5, M6, M7, P8
        /// </summary>
        public static Scale CreateMajorScale(Pitch tonic)
        {
            var intervals = new List<Interval>
            {
                new Interval(1, IntervalQuality.Perfect),  // P1 (0 semitones)
                new Interval(2, IntervalQuality.Major),     // M2 (2 semitones)
                new Interval(3, IntervalQuality.Major),     // M3 (4 semitones)
                new Interval(4, IntervalQuality.Perfect),   // P4 (5 semitones)
                new Interval(5, IntervalQuality.Perfect),   // P5 (7 semitones)
                new Interval(6, IntervalQuality.Major),     // M6 (9 semitones)
                new Interval(7, IntervalQuality.Major),     // M7 (11 semitones)
                new Interval(8, IntervalQuality.Perfect)    // P8 (12 semitones)
            };

            string name = tonic.NoteName + " Major";
            return new Scale(tonic, intervals, name, ScaleType.Major);
        }

        /// <summary>
        /// Creates a Natural Minor scale using Interval objects:
        /// Natural minor intervals: P1, M2, m3, P4, P5, m6, m7, P8
        /// </summary>
        public static Scale CreateNaturalMinorScale(Pitch tonic)
        {
            var intervals = new List<Interval>
            {
                new Interval(1, IntervalQuality.Perfect), // P1 (0 semitones)
                new Interval(2, IntervalQuality.Major),    // M2 (2 semitones)
                new Interval(3, IntervalQuality.Minor),    // m3 (3 semitones)
                new Interval(4, IntervalQuality.Perfect),  // P4 (5 semitones)
                new Interval(5, IntervalQuality.Perfect),  // P5 (7 semitones)
                new Interval(6, IntervalQuality.Minor),    // m6 (8 semitones)
                new Interval(7, IntervalQuality.Minor),    // m7 (10 semitones)
                new Interval(8, IntervalQuality.Perfect)   // P8 (12 semitones)
            };

            string name = tonic.NoteName + " Minor";
            return new Scale(tonic, intervals, name, ScaleType.Minor);
        }

        #endregion
    }
}
