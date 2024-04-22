namespace libs;

public class Wall : GameObject
{
    public Wall() : base()
    {
        this.Type = GameObjectType.Wall;
        this.CharRepresentation = '█';
        this.Color = ConsoleColor.Cyan;
    }
}