namespace libs;
using Newtonsoft.Json;

public class Map
{
    private string[,] RepresentationalLayer;
    private List<GameObject?[,]> history = new List<GameObject?[,]>();
    private GameObject?[,] GameObjectLayer;
    private int _mapWidth;
    private int _mapHeight;
    private string _levelName;

    public string LevelName
    {
        get { return _levelName; }
        set { _levelName = value; }
    }

    public GameObject?[,] GetGameObjectLayer()
    {
        return GameObjectLayer;
    }

    public Map()
    {
        _mapWidth = 30;
        _mapHeight = 8;
        RepresentationalLayer = new string[_mapHeight, _mapWidth];
        GameObjectLayer = new GameObject?[_mapHeight, _mapWidth];
    }

    public Map(int width, int height)
    {
        _mapWidth = width;
        _mapHeight = height;
        RepresentationalLayer = new string[_mapHeight, _mapWidth];
        GameObjectLayer = new GameObject?[_mapHeight, _mapWidth];
    }

    public void Initialize()
    {
        history.Clear();
        RepresentationalLayer = new string[_mapHeight, _mapWidth];
        GameObjectLayer = new GameObject?[_mapHeight, _mapWidth];

        // Initialize the map with null values or some default values
        for (int i = 0; i < GameObjectLayer.GetLength(0); i++)
        {
            for (int j = 0; j < GameObjectLayer.GetLength(1); j++)
            {
                GameObjectLayer[i, j] = null; // or new Floor() if needed
            }
        }
    }

    public int MapWidth
    {
        get { return _mapWidth; }
        set { _mapWidth = value; Initialize(); }
    }

    public int MapHeight
    {
        get { return _mapHeight; }
        set { _mapHeight = value; Initialize(); }
    }

    public GameObject Get(int x, int y)
    {
        if (x < 0 || x >= _mapHeight || y < 0 || y >= _mapWidth)
        {
            return null;
        }

        return GameObjectLayer[x, y];
    }

    public void Set(GameObject gameObject)
    {
        if (gameObject == null)
        {
            throw new ArgumentNullException(nameof(gameObject));
        }

        int posY = gameObject.PosY;
        int posX = gameObject.PosX;
        int prevPosY = gameObject.GetPrevPosY();
        int prevPosX = gameObject.GetPrevPosX();

        if (posY < 0 || posY >= _mapHeight || posX < 0 || posX >= _mapWidth)
        {
            throw new ArgumentOutOfRangeException("Invalid game object position.");
        }

        if (GameObjectLayer[posY, posX] != null && (GameObjectLayer[posY, posX].Type == GameObjectType.Player || (GameObjectLayer[posY, posX].Type == GameObjectType.Box && gameObject.Type == GameObjectType.Goal)))
        {
            return;
        }

        if (prevPosX >= 0 && prevPosX < _mapWidth &&
            prevPosY >= 0 && prevPosY < _mapHeight)
        {
            GameObjectLayer[prevPosY, prevPosX] = null; // or new Floor() if needed
        }

        GameObjectLayer[posY, posX] = gameObject;
        RepresentationalLayer[gameObject.PosY, gameObject.PosX] = gameObject.CharRepresentation;
    }

    public void Save()
    {
        var cloneLayer = new GameObject?[_mapHeight, _mapWidth];
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                cloneLayer[i, j] = GameObjectLayer[i, j];
            }
        }
        history.Add(cloneLayer);
    }

    public void Undo()
    {
        if (history.Count > 0)
        {
            GameObjectLayer = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
        }
    }

    public void Reset()
    {
        Initialize();
    }
}
