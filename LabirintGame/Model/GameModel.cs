using System.Collections.Generic;

namespace LabirintGame.Model
{
    public class GameModel
    {
        public Maze Maze { get; }
        public Player Player { get; }
        public List<Enemy> Enemies { get; }
        public List<Key> Keys { get; }
        public List<Door> Doors { get; }

        public GameModel(int width, int height, int keysDoorsCount)
        {
            Maze = new Maze(width, height, keysDoorsCount);
            Player = new Player(Maze, 1, 0, new List<Key>());
            Enemies = new List<Enemy>();
            Keys = new List<Key>();
            Doors = new List<Door>();
        }
    }
}