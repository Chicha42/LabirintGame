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

        public GameModel(int width, int height, int keysDoorsCount, int branchesCount)
        {
            Maze = new Maze(width, height, keysDoorsCount, branchesCount);
            Player = new Player(Maze, 1, 0, new List<Key>());
            Enemies = new List<Enemy>();
            Keys = new List<Key>();
            Doors = new List<Door>();
        }
        
        public GameModel(int[,] customGrid)
        {
            Maze = new Maze(customGrid);
            Player = new Player(Maze, 1, 0, new List<Key>());
            Enemies = new List<Enemy>();
            Keys = new List<Key>();
            Doors = new List<Door>();
        }
    }
}