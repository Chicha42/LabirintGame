namespace LabirintGame.Model
{
    public class Maze
    {
        public int[,] Grid { get; private set; }
        public int Width { get; }
        public int Height { get; }
        private readonly Random _random = new();

        private readonly List<Key> _keys = [];
        private readonly List<Door> _doors = [];
        
        public readonly Dictionary<int, Color> KeyColors = new()
        {
            {0, Color.Red },    
            {1, Color.Green},  
            {2, Color.Blue},  
            {3, Color.Purple}
        };

        private static readonly Dictionary<int, Color> DoorColors = new()
        {
            {0, Color.DarkRed},
            {1, Color.DarkGreen},
            {2, Color.DarkBlue},
            {3, Color.DarkViolet}
        };

        public Maze(int width, int height, int numKeys, int branchesCount)
        {
            Width = width;
            Height = height;
            Grid = new int[height, width];
            for (var i = 0; i < numKeys; i++)
            {
                _keys.Add(new Key(i, KeyColors[i], 0, 0));
                _doors.Add(new Door(i,DoorColors[i], 0, 0));
            }
            GenerateMazeWithKeysAndDoors(branchesCount);
        }
        
        public Maze(int[,] customGrid)
        {
            Grid = customGrid;
            Width = customGrid.GetLength(1);
            Height = customGrid.GetLength(0);
        }

        private void GenerateMazeWithKeysAndDoors(int branchesCount)
        {
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    Grid[y, x] = 0;

            var path = new List<(int x, int y)>();
            var visited = new bool[Height, Width];
            Dfs(Width - 2, Height - 2, path, visited);

            foreach (var (x, y) in path)
                Grid[y, x] = 1;

            AddBranches(path, branchesCount);

            PlaceKeysAndDoors(_keys, _doors);

            Grid[0, 1] = 1;
            Grid[Height - 2, Width - 2] = 3;
            Grid[Height - 2, Width - 1] = 0;
            Grid[Height - 1, Width - 2] = 0;
        }
        
        private void PlaceKeysAndDoors(List<Key> keys, List<Door> doors)
        {
            var emptyCells = new List<(int x, int y)>();

            for (var y = 5; y < Height - 5; y++)
            {
                for (var x = 5; x < Width - 5; x++)
                {
                    if (Grid[y, x] == 1)
                        emptyCells.Add((x, y));
                }
            }

            emptyCells = emptyCells.OrderBy(_ => _random.Next()).ToList();
            var cells = new List<(int x, int y)>();
            
            foreach (var (x,y) in emptyCells)
            {
                if ((CountWallsAround(x, y) == 2 && cells.Count < 3) || (CountWallsAround(x, y) >= 2 && cells.Count >= 3))
                    cells.Add((x, y));

                if (cells.Count == keys.Count + doors.Count) break;
            }
            
            var cellsWithDistances = cells
                .Select(cell => new
                {
                    cell.x,
                    cell.y,
                    distance = GetDistanceBfs(1, 0, cell.x, cell.y)
                })
                .OrderBy(cell => cell.distance)
                .ToList();
            
            var keysDoorsCells = cellsWithDistances.Select(cell => (cell.x, cell.y)).ToList();

            foreach (var (x, y) in keysDoorsCells.ToList())
            {
                if (doors.Count > 0 && CountWallsAround(x, y) == 2 && doors.Count > keys.Count)
                {
                    var door = doors[0];
                    door.X = x;
                    door.Y = y;
                    Grid[y, x] = 20- door.Id;
                    doors.RemoveAt(0);
                    keysDoorsCells.Remove((x, y));
                }
                else if (keys.Count > 0 && CountWallsAround(x, y) >= 2 && keys.Count==doors.Count)
                {
                    var key = keys[0];
                    key.X = x;
                    key.Y = y;
                    Grid[y, x] = 10-key.Id;
                    keys.RemoveAt(0);
                    keysDoorsCells.Remove((x, y));
                }

                if (keys.Count == 0 && doors.Count == 0)
                    break;
            }
        }

        private int CountWallsAround(int x, int y)
        {
            var count = 0;
            var dirs = new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

            foreach (var (dx, dy) in dirs)
            {
                var nx = x + dx;
                var ny = y + dy;
                if (!InBounds(nx, ny) || Grid[ny, nx] == 0)
                    count++;
            }

            return count;
        }

        private void Dfs(int x, int y, List<(int x, int y)> path, bool[,] visited)
        {
            visited[y, x] = true;
            path.Add((x, y));

            var dirs = new (int dx, int dy)[] { (0, -2), (0, 2), (-2, 0), (2, 0) };
            dirs = dirs.OrderBy(_ => _random.Next()).ToArray();

            foreach (var (dx, dy) in dirs)
            {
                var nx = x + dx;
                var ny = y + dy;

                if (InBounds(nx, ny) && !visited[ny, nx])
                {
                    Grid[y + dy / 2, x + dx / 2] = 1;
                    Dfs(nx, ny, path, visited);
                }
            }
        }
        
        private int GetDistanceBfs(int startX, int startY, int targetX, int targetY)
        {
            var distances = new int[Height, Width];
            var visited = new bool[Height, Width];
            Queue<(int x, int y)> queue = new();

            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    distances[y, x] = -1;

            queue.Enqueue((startX, startY));
            distances[startY, startX] = 0;

            var dirs = new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                foreach (var (dx, dy) in dirs)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (InBounds(nx, ny) && !visited[ny, nx] && Grid[ny, nx] != 0)
                    {
                        visited[ny, nx] = true;
                        distances[ny, nx] = distances[y, x] + 1;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            return distances[targetY, targetX];
        }

        private void AddBranches(List<(int x, int y)> mainPath, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var (x, y) = mainPath[_random.Next(5, mainPath.Count - 5)];

                var dirs = new (int dx, int dy)[] { (0, -2), (0, 2), (-2, 0), (2, 0) };
                dirs = dirs.OrderBy(_ => _random.Next()).ToArray();

                foreach (var (dx, dy) in dirs)
                {
                    var nx = x + dx;
                    var ny = y + dy;
                    if (InBounds(nx, ny) && Grid[ny, nx] == 0)
                    {
                        Grid[y + dy / 2, x + dx / 2] = 1;
                        Grid[ny, nx] = 1;
                        break;
                    }
                }
            }
        }

        private bool InBounds(int x, int y)
            => x > 0 && y > 0 && x < Width - 1 && y < Height - 1;
        
        public readonly Random Random = new();

        public List<(int x, int y)> GetEmptyCells()
        {
            var cells = new List<(int x, int y)>();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Grid[y, x] == 1 && !(x == 1 && y == 0))
                        cells.Add((x, y));
                }
            }
            return cells;
        }
    }
}