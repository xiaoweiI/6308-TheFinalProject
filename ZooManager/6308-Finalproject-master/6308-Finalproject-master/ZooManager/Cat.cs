namespace ZooManager
{
    public class Cat : Animal
    {
        public new int X { get; set; } 
        public new int Y { get; set; }
        public int Life { get; set; } = 1; // Initial life is 1
        public Cat(string name)
        {
            
            emoji = "🐱";
            Name = name;
        }
        

        public void IncreaseLife()
        {
            Life++;
        }
        
        public string GetEmojiWithLife()
        {
            return $"{emoji}{Life}";
        }

        //public override void Activate(Game game)
        //{            // Cat activation logic        }
    }
}
