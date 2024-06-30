using System;
namespace ZooManager
{
    public class Animal : Occupant
    {
        public string Name;
        
    
        virtual public void Activate()
        {
            Console.WriteLine($"Animal {Name} at {location.x},{location.y} activated");
        }
    }
}
