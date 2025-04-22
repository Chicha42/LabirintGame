namespace LabirintGame.Model
{
    public class Enemy : Pos
    {
        public int Health { get; private set; }
        public int Damage { get; private set; }

        public Enemy(int health, int damage, int x, int y) : base(x, y)
        {
            Health = health;
            Damage = damage;
        }
    }
}