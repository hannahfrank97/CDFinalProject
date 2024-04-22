namespace libs;

public sealed class InputHandler
{

    private static InputHandler? _instance;
    private GameEngine engine;

    public static InputHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InputHandler();
            }
            return _instance;
        }
    }

    private InputHandler()
    {
        //INIT PROPS HERE IF NEEDED
        engine = GameEngine.Instance;
    }

    public bool Handle(ConsoleKeyInfo keyInfo)
    {
        bool skipCollitionCheck = false;
        GameObject focusedObject = engine.GetFocusedObject();

        if (focusedObject != null)
        {
            // Handle keyboard input to move the player
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    focusedObject.Move(0, -1);
                    break;
                case ConsoleKey.DownArrow:
                    focusedObject.Move(0, 1);
                    break;
                case ConsoleKey.LeftArrow:
                    focusedObject.Move(-1, 0);
                    break;
                case ConsoleKey.RightArrow:
                    focusedObject.Move(1, 0);
                    break;
                case ConsoleKey.Z:
                    engine.Undo();
                    skipCollitionCheck = true;
                    break;
                case ConsoleKey.R:
                    engine.Setup();
                    break;
                case ConsoleKey.S:
                    engine.SaveToFile();
                    skipCollitionCheck = true;
                    break;
                default:
                    break;
            }
        }
        return skipCollitionCheck;
    }

}