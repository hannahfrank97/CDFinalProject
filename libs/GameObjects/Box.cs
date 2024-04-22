namespace libs;

public class Box : GameObject
{

    public Box() : base()
    {
        Type = GameObjectType.Box;
        CharRepresentation = '■';
        Color = ConsoleColor.DarkGreen;
    }

    public Box(int posX, int posY, ConsoleColor color, char charRepresentation, GameObjectType type) : base(posX, posY, color, charRepresentation, type)
    {
        this.setPosX(posX);
        this.setPosY(posY);
        this.setColor(color);
        CharRepresentation = charRepresentation;
        Type = type;
        Color = color;
    }
}