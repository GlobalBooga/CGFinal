
namespace SpaceShooter
{
    public class BoxCollider
    {
        public GameObject box;
        public Actor owner;
        public bool enabled = true;

        public BoxCollider(GameObject b, Actor o)
        {
            owner = o;
            box = b;
        }
    }
}
