namespace libs;

public class Wall : GameObject
{
    public Wall() : base()
    {
        this.Type = GameObjectType.Wall;
        this.CharRepresentationString = "🧱";
        this.Color = ConsoleColor.Cyan;
    }
}