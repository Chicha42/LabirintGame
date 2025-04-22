using System.Data.Common;

namespace LabirintGame.Model
{
    public class Key : Pos
    {
        public Color Color { get; }
        public int Id { get; }

        public Key(int id, Color color, int x, int y) : base(x, y)
        {
            Color = color;
            Id = id;
        }
    }
}