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

    private PlayerSingelton() : base()
    {
        Type = GameObjectType.Player;
        CharRepresentation = '☻';
        Color = ConsoleColor.DarkYellow;
    }
}