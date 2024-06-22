namespace libs;

public sealed class PlayerSingelton : GameObject
{
    private static PlayerSingelton? _instance;

    public static PlayerSingelton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayerSingelton();
            }
            return _instance;
        }
    }

    private PlayerSingelton()
        : base()
    {
        Type = GameObjectType.Player;
        CharRepresentationString = "⬆️"; // up: ↑, down: ↓, left: ←, right: →
        Color = ConsoleColor.DarkYellow;
    }

    public override void Move(int dx, int dy)
    {
        if (dx == 0 && dy == -1)
        {
            if (CharRepresentationString != "⬆️")
                CharRepresentationString = "⬆️";
            else
                doMove(dx, dy);
        }
        else if (dx == 0 && dy == 1)
        {
            if (CharRepresentationString != "⬇️")
                CharRepresentationString = "⬇️";
            else
                doMove(dx, dy);
        }
        else if (dx == -1 && dy == 0)
        {
            if (CharRepresentationString != "⬅️")
                CharRepresentationString = "⬅️";
            else
                doMove(dx, dy);
        }
        else if (dx == 1 && dy == 0)
        {
            if (CharRepresentationString != "➡️")
                CharRepresentationString = "➡️";
            else
                doMove(dx, dy);
        }
    }
}
