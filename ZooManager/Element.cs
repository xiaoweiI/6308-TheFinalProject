namespace ZooManager
{
    public abstract class Element
    {
        public string Name { get; set; }
        public string Emoji { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public abstract void Activate(Game game);
    }
}
