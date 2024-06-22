namespace libs
{
    public sealed class PlayerSingelton : GameObject
    {
        private static PlayerSingelton? _instance;
        private static readonly object _lock = new object();

        public static PlayerSingelton Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PlayerSingelton();
                    }
                    return _instance;
                }
            }
        }

        private PlayerSingelton()
            : base()
        {
            Type = GameObjectType.Player;
            CharRepresentation = '↑'; // up: ↑, down: ↓, left: ←, right: →
            Color = ConsoleColor.DarkYellow;
        }

        public override void Move(int dx, int dy)
        {
            lock (_lock)
            {
                if (dx == 0 && dy == -1)
                {
                    if (CharRepresentation != '↑')
                        CharRepresentation = '↑';
                    else
                        doMove(dx, dy);
                }
                else if (dx == 0 && dy == 1)
                {
                    if (CharRepresentation != '↓')
                        CharRepresentation = '↓';
                    else
                        doMove(dx, dy);
                }
                else if (dx == -1 && dy == 0)
                {
                    if (CharRepresentation != '←')
                        CharRepresentation = '←';
                    else
                        doMove(dx, dy);
                }
                else if (dx == 1 && dy == 0)
                {
                    if (CharRepresentation != '→')
                        CharRepresentation = '→';
                    else
                        doMove(dx, dy);
                }
            }
        }
    }
}
