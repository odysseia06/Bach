using Bach.Models;
using Bach.Models.Enums;

namespace Bach.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Pitch pitch = new("C", 4);
            Note note = new(pitch);
            Chord chord = new(pitch, ChordQuality.Major);

            Console.WriteLine("Hello, World!");
        }
    }
}
