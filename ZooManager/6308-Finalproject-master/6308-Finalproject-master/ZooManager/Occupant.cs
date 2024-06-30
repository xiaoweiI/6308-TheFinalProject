using System;
namespace ZooManager
{
    public class Occupant
    {
        public string emoji;
        public string species;
        public int X { get; set; }
        public int Y { get; set; }
        public Point location;

        public void ReportLocation()
        {
            Console.WriteLine($"I am at {location.x},{location.y}");
        }
    }
}
