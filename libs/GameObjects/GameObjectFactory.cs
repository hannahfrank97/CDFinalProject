namespace libs;

public class GameObjectFactory : IGameObjectFactory
{
    public GameObject CreateGameObject(dynamic obj)
    {

        GameObject newObj = new GameObject();
        string type = obj.Type;

        switch (type)
        {
            case "Player":
                newObj = PlayerSingelton.Instance;
                newObj.PosX = obj.PosX;
                newObj.PosY = obj.PosY;
                break;
            case "Wall":
                newObj = obj.ToObject<Wall>();
                break;
            case "Box":
                newObj = obj.ToObject<Box>();
                break;
            case "Goal":
                newObj = obj.ToObject<Goal>();
                break;
            case "Obstacle":
                newObj = obj.ToObject<Obstacle>();
                break;
        }

        return newObj;
    }
}