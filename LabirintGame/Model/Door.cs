using System.Collections.Generic;
using System.Linq;

namespace LabirintGame.Model
{
    public class Door : Pos
    {
        public Color RequiredColor;
        public int Id;

        public Door(int id, Color requiredColor, int x, int y) : base(x, y)
        {
            RequiredColor = requiredColor;
            Id = id;
        }
    }
}