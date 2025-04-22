using System.Diagnostics;

namespace LabirintGame.Model
{
    public class Player : Pos
    {
        public int Health { get; set; }
        private Maze _maze;
        public List<Key> CollectedKeys = new List<Key>();

        public Player(Maze maze, int startX, int startY, List<Key> collectedKeys) : base(startX, startY)
        {
            CollectedKeys = collectedKeys;
            _maze = maze;
            Health = 100;
        }

        public void Move(int deltaX, int deltaY)
        {
            int newX = X + deltaX;
            int newY = Y + deltaY;

            if (newX >= 0 && newY >= 0 &&
                newX < _maze.Width && newY < _maze.Height &&
                _maze.Grid[newY, newX] != 0 &&
                _maze.Grid[newY, newX] != 20 &&
                _maze.Grid[newY, newX] != 19 &&
                _maze.Grid[newY, newX] != 18)
            {
                X = newX;
                Y = newY;
            }
            
            if (_maze.Grid[newY, newX] == 10 ||
                _maze.Grid[newY, newX] == 9 ||
                _maze.Grid[newY, newX] == 8)
            {
                CollectedKeys.Add(new Key(_maze.Grid[newY, newX], 
                    _maze.KeyColors[_maze.Grid[newY, newX]-8], newX, newY));
                _maze.Grid[newY, newX] = 1;
            }
            
            if (_maze.Grid[newY, newX] == 20 ||
                _maze.Grid[newY, newX] == 19 ||
                _maze.Grid[newY, newX] == 18)
            {
                for (var i = 0;i < CollectedKeys.Count; i++)
                    if (CollectedKeys[i].Id +10 ==  _maze.Grid[newY, newX])
                        _maze.Grid[newY, newX] = 1;
            }
        }
    }
}