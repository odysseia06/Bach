using System;
using Bach.Models.Enums;

namespace Bach.Models
{
    /// <summary>
    /// Represents a musical interval based on diatonic number and quality.
    /// Integrates with Pitch and Note classes, allowing intervals to be derived from existing notes/pitches
    /// and applied to them to create new, transposed musical elements.
    /// </summary>
    public class Interval
    {
        #region Properties
        /// <summary>
        /// The diatonic number of the interval (1=Unison, 2=Second, 3=Third, ... 8=Octave, etc.).
        /// Intervals above 8 are compound intervals.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// The quality of the interval (Perfect/Major/Minor/Augmented/Diminished).
        /// This, combined with the number, defines the exact type of interval (e.g. M3, P5).
        /// </summary>
        public IntervalQuality Quality { get; }

        /// <summary>
        /// The number of semitones spanned by this interval in a 12-TET system.
        /// Useful for applying the interval to pitches (e.g. transposing by semitones).
        /// </summary>
        public int Semitones => CalculateSemitones();

        /// <summary>
        /// Indicates whether this interval is compound (i.e., larger than an octave).
        /// </summary>
        public bool IsCompound => Number > 8;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Interval given the diatonic number and quality.
        /// Example: new Interval(3, IntervalQuality.Major) -> Major Third (M3).
        /// </summary>
        public Interval(int number, IntervalQuality quality)
        {
            if (number < 1)
                throw new ArgumentException("Interval number must be at least 1.");
            Number = number;
            Quality = quality;
        }

        #endregion

        #region Static Factories

        /// <summary>
        /// Creates an interval from two pitches.
        /// For example, if pitch2 is a perfect fifth above pitch1, returns P5.
        /// </summary>
        public static Interval FromPitches(Pitch lowerPitch, Pitch higherPitch)
        {
            if (higherPitch.Frequency < lowerPitch.Frequency)
                (lowerPitch, higherPitch) = (higherPitch, lowerPitch);

            // Calculate semitone distance:
            int semitoneDistance = higherPitch.MidiNoteNumber - lowerPitch.MidiNoteNumber;

            // We need to guess the best interval number/quality from semitones and likely pitch names.
            // For a real application, we would determine the interval number by counting staff steps.
            // Here is a simplified logic:
            // 1) Determine the "generic" interval by counting letters between note names
            // For simplicity, we assume Pitch always returns a natural note name (C,D,E,F,G,A,B).
            // Counting is inclusive: C to E is a third because C,D,E => 3 letters.

            // Get the "generic interval number" by counting note letters:
            int letterDistance = GetLetterDistance(lowerPitch.NoteName, higherPitch.NoteName,
                                                   lowerPitch.Octave, higherPitch.Octave);

            // Determine quality by comparing semitone distance with expected major/perfect intervals.
            // This logic is simplified. In a full solution, you'd handle all edge cases.
            var (baseSemitones, isPerfectClass) = GetBaseSemitonesAndClass(letterDistance);
            IntervalQuality quality = DetermineQuality(isPerfectClass, baseSemitones, semitoneDistance);

            return new Interval(letterDistance, quality);
        }

        /// <summary>
        /// Creates an interval from two notes.
        /// Since Note contains a Pitch, we just delegate to FromPitches.
        /// </summary>
        public static Interval FromNotes(Note lowerNote, Note higherNote)
        {
            return FromPitches(lowerNote.Pitch, higherNote.Pitch);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts the interval's size to cents. In 12-TET, one semitone = 100 cents.
        /// Useful for comparing intervals or working with frequency ratios.
        /// </summary>
        public double ToCents() => Semitones * 100.0;

        /// <summary>
        /// Inverts the interval according to standard rules:
        ///  - The sum of the interval numbers of an interval and its inversion is 9 (for simple intervals).
        ///  - Perfect inverts to perfect, major inverts to minor, augmented inverts to diminished, etc.
        /// </summary>
        public Interval Invert()
        {
            int baseNumber = (Number - 1) % 7 + 1;
            int inversionNumber = 9 - baseNumber;
            if (inversionNumber <= 0) inversionNumber += 7;

            // For compound intervals, add the same number of octaves:
            int octaveShifts = (Number - 1) / 7;
            inversionNumber += octaveShifts * 7;

            IntervalQuality invertedQuality = InvertQuality(Quality, baseNumber);
            return new Interval(inversionNumber, invertedQuality);
        }

        /// <summary>
        /// Applies this interval to a given Pitch, returning a new Pitch that is transposed by this interval.
        /// This simulates, for example, moving a note up a minor third, or down a perfect fifth.
        /// </summary>
        public Pitch ApplyToPitch(Pitch basePitch)
        {
            // Transpose the pitch by the number of semitones defined by this interval.
            int newMidi = basePitch.MidiNoteNumber + Semitones;
            return new Pitch(newMidi);
        }

        /// <summary>
        /// Applies this interval to a Note, returning a new Note transposed by this interval.
        /// This allows you to move a melody or a single note by a certain interval.
        /// </summary>
        public Note ApplyToNote(Note baseNote)
        {
            // We apply to the pitch inside the Note:
            var newPitch = ApplyToPitch(baseNote.Pitch);
            // Create a new Note with the same duration, articulation, dynamics, etc., but the transposed pitch
            return new Note(newPitch, baseNote.Duration, baseNote.Accidental, baseNote.Dynamics, baseNote.Articulation);
        }

        /// <summary>
        /// Human-readable shorthand: M3, P5, m2, A4, etc.
        /// </summary>
        public override string ToString()
        {
            return $"{QualityToString(Quality)}{Number}";
        }

        #endregion

        #region Private Methods

        private int CalculateSemitones()
        {
            int baseNumber = (Number - 1) % 7 + 1;
            int octaveFactor = (Number - 1) / 7;

            // Base semitones for perfect/major intervals:
            // 1 (Unison): P=0 semitones
            // 2 (Second): M=2 semitones
            // 3 (Third): M=4 semitones
            // 4 (Fourth): P=5 semitones
            // 5 (Fifth): P=7 semitones
            // 6 (Sixth): M=9 semitones
            // 7 (Seventh): M=11 semitones
            // 8 (Octave): P=12 semitones
            int baseSemitones = baseNumber switch
            {
                1 => 0,
                2 => 2,
                3 => 4,
                4 => 5,
                5 => 7,
                6 => 9,
                7 => 11,
                _ => 0
            };

            baseSemitones += 12 * octaveFactor;

            bool isPerfectClass = IsPerfectClassInterval(baseNumber);
            // Apply quality adjustments
            int semitoneAdjustment = Quality switch
            {
                IntervalQuality.Perfect when isPerfectClass => 0,
                IntervalQuality.Major when !isPerfectClass => 0,
                IntervalQuality.Minor when !isPerfectClass => -1,
                IntervalQuality.Augmented when isPerfectClass => 1,
                IntervalQuality.Augmented when !isPerfectClass => 1,
                IntervalQuality.Diminished when isPerfectClass => -1,
                IntervalQuality.Diminished when !isPerfectClass => -2,
                IntervalQuality.DoublyAugmented when isPerfectClass => 2,
                IntervalQuality.DoublyAugmented when !isPerfectClass => 2,
                IntervalQuality.DoublyDiminished when isPerfectClass => -2,
                IntervalQuality.DoublyDiminished when !isPerfectClass => -3,
                _ => 0
            };

            return baseSemitones + semitoneAdjustment;
        }

        private static bool IsPerfectClassInterval(int baseNumber) => baseNumber == 1 || baseNumber == 4 || baseNumber == 5;

        private static string QualityToString(IntervalQuality quality) => quality switch
        {
            IntervalQuality.Perfect => "P",
            IntervalQuality.Major => "M",
            IntervalQuality.Minor => "m",
            IntervalQuality.Augmented => "A",
            IntervalQuality.Diminished => "d",
            IntervalQuality.DoublyAugmented => "AA",
            IntervalQuality.DoublyDiminished => "dd",
            _ => ""
        };

        private static IntervalQuality InvertQuality(IntervalQuality originalQuality, int baseNumber)
        {
            bool perfectClass = IsPerfectClassInterval(baseNumber);
            return originalQuality switch
            {
                IntervalQuality.Perfect when perfectClass => IntervalQuality.Perfect,
                IntervalQuality.Major when !perfectClass => IntervalQuality.Minor,
                IntervalQuality.Minor when !perfectClass => IntervalQuality.Major,
                IntervalQuality.Augmented => IntervalQuality.Diminished,
                IntervalQuality.Diminished => IntervalQuality.Augmented,
                IntervalQuality.DoublyAugmented => IntervalQuality.DoublyDiminished,
                IntervalQuality.DoublyDiminished => IntervalQuality.DoublyAugmented,
                _ => IntervalQuality.Perfect
            };
        }

        private static int GetLetterDistance(string lowerNoteName, string higherNoteName, int lowerOctave, int higherOctave)
        {
            // Map note letters to scale degrees: C(1), D(2), E(3), F(4), G(5), A(6), B(7)
            // For counting letters: C->D->E->F->G->A->B->C...
            // The number of letters between C and E (C,D,E) = 3
            int lowerDegree = LetterToDegree(lowerNoteName[0]);
            int higherDegree = LetterToDegree(higherNoteName[0]);

            // Add 7 for each octave difference
            int octaveSteps = (higherOctave - lowerOctave) * 7;

            // Letter distance (in diatonic steps)
            int letterDist = (higherDegree + octaveSteps) - lowerDegree + 1; // inclusive count
            return letterDist;
        }

        private static int LetterToDegree(char c) => c switch
        {
            'C' => 1,
            'D' => 2,
            'E' => 3,
            'F' => 4,
            'G' => 5,
            'A' => 6,
            'B' => 7,
            _ => 1
        };

        private static (int baseSemitones, bool isPerfectClass) GetBaseSemitonesAndClass(int letterDistance)
        {
            // Reduce letterDistance to 1-7 for the purpose of determining base interval:
            int baseNumber = ((letterDistance - 1) % 7) + 1;
            bool pc = IsPerfectClassInterval(baseNumber);
            int bs = baseNumber switch
            {
                1 => 0,  // Unison
                2 => 2,  // Major second
                3 => 4,  // Major third
                4 => 5,  // Perfect fourth
                5 => 7,  // Perfect fifth
                6 => 9,  // Major sixth
                7 => 11, // Major seventh
                _ => 0
            };
            // Add octaves if > 7
            int octaves = (letterDistance - 1) / 7;
            bs += 12 * octaves;

            return (bs, pc);
        }

        private static IntervalQuality DetermineQuality(bool isPerfectClass, int baseSemitones, int actualSemitones)
        {
            int diff = actualSemitones - baseSemitones;
            // Perfect class intervals: Perfect=0 diff, Aug=+1, dim=-1, etc.
            // Major class intervals: Major=0 diff, Minor=-1, Aug=+1, dim=-2
            if (isPerfectClass)
            {
                return diff switch
                {
                    0 => IntervalQuality.Perfect,
                    1 => IntervalQuality.Augmented,
                    -1 => IntervalQuality.Diminished,
                    2 => IntervalQuality.DoublyAugmented,
                    -2 => IntervalQuality.DoublyDiminished,
                    _ => IntervalQuality.Perfect // fallback
                };
            }
            else
            {
                return diff switch
                {
                    0 => IntervalQuality.Major,
                    -1 => IntervalQuality.Minor,
                    1 => IntervalQuality.Augmented,
                    -2 => IntervalQuality.Diminished,
                    2 => IntervalQuality.DoublyAugmented,
                    -3 => IntervalQuality.DoublyDiminished,
                    _ => IntervalQuality.Major // fallback
                };
            }
        }

        #endregion
    }
}
