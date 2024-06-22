namespace libs;

public class Obstacle : GameObject
{
    public int TimeEffect { get; set; }
    public string Message { get; set; }

    public Obstacle() : base()
    {
        Type = GameObjectType.Obstacle;
        CharRepresentationString = "#"; // Use string
        Color = ConsoleColor.DarkRed;
        TimeEffect = 0; // Default value
        Message = "Hello I am an obstacle!"; //Default value
    }

    public Obstacle(int posX, int posY, ConsoleColor color, string charRepresentationString, GameObjectType type) : base(posX, posY, color, charRepresentationString, type)
    {
        this.setPosX(posX);
        this.setPosY(posY);
        this.setColor(color);
        this.CharRepresentationString = charRepresentationString;
        Type = type;
        Color = color;
        this.TimeEffect = TimeEffect;
        this.Message = Message;

    
    }
}