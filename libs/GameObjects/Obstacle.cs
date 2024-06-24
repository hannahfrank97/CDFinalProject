namespace libs
{
    public class Obstacle : GameObject
    {
        public int TimeEffect { get; set; }
        public string Message { get; set; }

        public Obstacle() : base()
        {
            Type = GameObjectType.Obstacle;
            CharRepresentation = "🪴"; // Make sure this is a string
            Color = ConsoleColor.DarkRed;
            TimeEffect = 0; // Default value
            Message = "Hello I am an obstacle!"; // Default value
        }

        public Obstacle(int posX, int posY, ConsoleColor color, string charRepresentation, GameObjectType type)
            : base(posX, posY, color, charRepresentation, type)
        {
            this.setPosX(posX);
            this.setPosY(posY);
            this.setColor(color);
            CharRepresentation = charRepresentation;
            Type = type;
            Color = color;
            this.TimeEffect = TimeEffect;
            this.Message = Message;
        }
    }
}
