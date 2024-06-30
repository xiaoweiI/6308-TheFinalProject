using ZooManager;

public class Mouse : Animal
{
    public string name { get; set; }
    public int Life { get; set; } = 1;

    public void DecreaseLife()
    {
        Life--;
    }

    
    public Mouse(string name)
    {
        Name = name;
        emoji = "🐭";
    }

    public void IncreaseLife()
    {
        Life++;
    }

    public string GetEmojiWithLife()
    {
        return $"{emoji}{Life}";
    }
}
