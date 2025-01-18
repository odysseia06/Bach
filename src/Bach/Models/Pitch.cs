using System;

namespace Bach.Models
{
    public class Pitch
    {
        /// <summary>
        /// The frequency of the pitch in Hertz (Hz).
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// The note name (e.g. "A", "Bb", "C#") of the pitch.
        /// </summary>
        public string NoteName { get; private set; } = string.Empty;

        /// <summary>
        /// The octave number of the pitch.
        /// </summary>
        public int Octave { get; private set; }

        /// <summary>
        /// The MIDI note number corresponding to the pitch.
        /// </summary>
        public int MidiNoteNumber { get; private set; }

        /// <summary>
        /// The pitch class (0-11), where 0 is C, 1 is C#/Db, 2 is D, etc.
        /// </summary>
        public int PitchClass { get; private set; }

        /// <summary>
        /// The scientific pitch notation of the pitch (e.g. "A4", "Bb3", "C#5").
        /// </summary>
        public string ScientificPitchNotation => $"{NoteName}{Octave}";

        /// <summary>
        /// The standard tuning frequency for A4. Default is 440 Hz.
        /// </summary>
        public static double TuningStandard { get; set; } = 440.0;

        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Pitch"/> class from a frequency.
        /// </summary>
        /// <param name="frequency">The frequency in Hertz (Hz).</param>
        public Pitch(double frequency)
        {
            Frequency = frequency;
            UpdatePropertiesFromFrequency();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pitch"/> class from a note name and octave.
        /// </summary>
        /// <param name="noteName">The note name (e.g., C, C#, D).</param>
        /// <param name="octave">The octave number.</param>
        public Pitch(string noteName, int octave)
        {
            NoteName = noteName;
            Octave = octave;
            UpdateFrequencyFromNote();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pitch"/> class from a MIDI note number.
        /// </summary>
        /// <param name="midiNoteNumber"></param>
        public Pitch(int midiNoteNumber)
        {
            MidiNoteNumber = midiNoteNumber;
            UpdatePropertiesFromMidiNote();
        }

        #endregion

        #region Private Methods

        private void UpdatePropertiesFromFrequency()
        {
            MidiNoteNumber = (int)Math.Round(69 + 12 * Math.Log2(Frequency / TuningStandard));
            UpdatePropertiesFromMidiNote();
        }

        private void UpdatePropertiesFromMidiNote()
        {
            PitchClass = MidiNoteNumber % 12;
            Octave = (MidiNoteNumber / 12) - 1;
            NoteName = NoteNames[PitchClass];
            Frequency = TuningStandard * Math.Pow(2, (MidiNoteNumber - 69) / 12.0);
        }

        private void UpdateFrequencyFromNote()
        {
            int noteIndex = Array.IndexOf(NoteNames, NoteName);
            if (noteIndex == -1)
            {
                throw new ArgumentException("Invalid note name.");
            }
            PitchClass = noteIndex;
            MidiNoteNumber = PitchClass + 12 * (Octave + 1);
            Frequency = TuningStandard * Math.Pow(2, (MidiNoteNumber - 69) / 12.0);
        }
        #endregion

        #region Public Methods

        public void Transpose(int semitones)
        {
            MidiNoteNumber += semitones;
            UpdatePropertiesFromMidiNote();
        }

        public override string ToString()
        {
            return $"{ScientificPitchNotation} ({Frequency:F2} Hz)";
        }

        #endregion

        #region Operators

        public static Pitch operator +(Pitch pitch, int semitones)
        {
            return new Pitch(pitch.MidiNoteNumber + semitones);
        }

        public static Pitch operator -(Pitch pitch, int semitones)
        {
            return new Pitch(pitch.MidiNoteNumber - semitones);
        }

        public static int operator -(Pitch pitch1, Pitch pitch2)
        {
            return pitch1.MidiNoteNumber - pitch2.MidiNoteNumber;
        }

        #endregion

        #region Equality Members

        public override bool Equals(object? obj)
        {
            return obj is Pitch pitch &&
                   MidiNoteNumber == pitch.MidiNoteNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MidiNoteNumber);
        }

        #endregion
    }
}
