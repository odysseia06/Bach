using System;
using Bach.Models.Enums;

namespace Bach.Models
{
    /// <summary>
    /// Represents a musical note with pitch, duration, accidental, dynamics, and articulation
    /// </summary>
    public class Note
    {
        /// <summary>
        /// The pitch of the note
        /// </summary>
        public Pitch Pitch { get; private set; }

        /// <summary>
        /// The duration (note value) of the note
        /// </summary>
        public NoteValue Duration { get; set; }

        /// <summary>
        /// The accidental applied to the note
        /// </summary>
        public Accidental Accidental { get; private set; }

        public Dynamics Dynamics { get; set; }

        /// <summary>
        /// The articulation of the note (e.g., staccato, legato)
        /// </summary>
        public Articulation Articulation { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Note class.
        /// </summary>
        /// <param name="pitch">The pitch of the note.</param>
        /// <param name="duration">The duration (note value) of the note.</param>
        /// <param name="accidental">The accidental applied to the note.</param>
        /// <param name="dynamics">The dynamic level of the note.</param>
        /// <param name="articulation">The articulation of the note.</param>
        public Note(Pitch pitch, 
            NoteValue duration = NoteValue.Quarter, 
            Accidental accidental = Accidental.Natural, 
            Dynamics dynamics = Dynamics.MezzoForte, 
            Articulation articulation = Articulation.Normal)
        {
            Pitch = pitch;
            Duration = duration;
            Accidental = accidental;
            Dynamics = dynamics;
            Articulation = articulation;

            ApplyAccidental();
        }

        #endregion

        #region Private Methods

        private void ApplyAccidental()
        {
            // Adjust the pitch according to the accidental
            int semitoneAdjustment = Accidental switch
            {
                Accidental.Natural => 0,
                Accidental.Sharp => 1,
                Accidental.Flat => -1,
                Accidental.DoubleSharp => 2,
                Accidental.DoubleFlat => -2,
                Accidental.HalfSharp => 0, // Placeholder for microtonal adjustments
                Accidental.HalfFlat => 0,  // Placeholder for microtonal adjustments
                _ => 0,
            };

            if (semitoneAdjustment != 0)
            {
                Pitch = new Pitch(Pitch.MidiNoteNumber + semitoneAdjustment);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Transposes the note by a number of semitones.
        /// </summary>
        /// <param name="semitones">The number of semitones to transpose.</param>
        public void Transpose(int semitones)
        {
            Pitch = new Pitch(Pitch.MidiNoteNumber + semitones);
        }

        public override string ToString()
        {
            // TODO: For now remove accidental symbol from Pitch.NoteName to avoid double accidentals
            string baseName = Pitch.NoteName.Replace("#", "");

            string accidentalSymbol = Accidental switch
            {
                Accidental.Natural => "",
                Accidental.Sharp => "♯",
                Accidental.Flat => "♭",
                Accidental.DoubleSharp => "𝄪",
                Accidental.DoubleFlat => "𝄫",
                Accidental.HalfSharp => "𝄲",
                Accidental.HalfFlat => "𝄳",
                _ => ""
            };

            return $"{baseName}{accidentalSymbol}{Pitch.Octave} " +
                   $"{Duration} {Dynamics} {Articulation}";
        }

        #endregion
    }
}
