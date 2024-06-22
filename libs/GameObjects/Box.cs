namespace libs;

public class Box : GameObject
{

    public Box() : base()
    {
        Type = GameObjectType.Box;
        CharRepresentationString = "■";
        Color = ConsoleColor.DarkGreen;
    }

    public Box(int posX, int posY, ConsoleColor color,  string charRepresentationString, GameObjectType type) : base(posX, posY, color, charRepresentationString, type)
    {
        this.setPosX(posX);
        this.setPosY(posY);
        this.setColor(color);
        Type = type;
        Color = color;
    }
}