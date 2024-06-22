namespace libs;

using Newtonsoft.Json;

public class GameObject : IGameObject, IMovement
{
    public string CharRepresentationString { get; set; } // Change to string for deserialization

    [JsonIgnore]
    public char CharRepresentation => CharRepresentationString[0]; // Convert string to char
    private ConsoleColor _color;

    // direction of the last movement
    private int dX;
    private int dY;

    private int _posX;
    private int _posY;

    private int _prevPosX;
    private int _prevPosY;

    public GameObjectType Type;

    public GameObject()
    {
        this._posX = 5;
        this._posY = 5;
        this._color = ConsoleColor.Gray;
        CharRepresentationString = " "; // Default value
    }

    public GameObject(int posX, int posY)
    {
        this._posX = posX;
        this._posY = posY;
        CharRepresentationString = " "; // Default value
    }

    public GameObject(int posX, int posY, ConsoleColor color)
    {
        this._posX = posX;
        this._posY = posY;
        this._color = color;
        CharRepresentationString = " "; // Default value
    }

    public GameObject(
        int posX,
        int posY,
        ConsoleColor color,
        string charRepresentationString,
        GameObjectType type
    )
    {
        this._posX = posX;
        this._posY = posY;
        this._color = color;
        CharRepresentationString = charRepresentationString;
        this.Type = type;
    }

    public ConsoleColor Color
    {
        get { return _color; }
        set { _color = value; }
    }

    public int PosX
    {
        get { return _posX; }
        set { _posX = value; }
    }

    public int PosY
    {
        get { return _posY; }
        set { _posY = value; }
    }

    public int GetPrevPosY()
    {
        if (_prevPosY == null || _prevPosY == 0)
            return _posY;

        return _prevPosY;
    }

    public int GetPrevPosX()
    {
        if (_prevPosX == null || _prevPosX == 0)
            return _posX;

        return _prevPosX;
    }

    public int getDy()
    {
        return dY;
    }

    public int getDx()
    {
        return dX;
    }

    public void setPosX(int posX)
    {
        this._posX = posX;
    }

    public void setPosY(int posY)
    {
        this._posY = posY;
    }

    public void setColor(ConsoleColor color)
    {
        this._color = color;
    }

    public virtual void Move(int dx, int dy)
    {
        this.dY = dy;
        this.dX = dx;
        _prevPosX = _posX;
        _prevPosY = _posY;
        _posX += dx;
        _posY += dy;
    }

    protected void doMove(int dx, int dy)
    {
        this.dY = dy;
        this.dX = dx;
        _prevPosX = _posX;
        _prevPosY = _posY;
        _posX += dx;
        _posY += dy;
    }

    public void UndoMove()
    {
        _posX = _prevPosX;
        _posY = _prevPosY;
    }

    public string toJSON()
    {
        return JsonConvert.SerializeObject(this);
    }
}
