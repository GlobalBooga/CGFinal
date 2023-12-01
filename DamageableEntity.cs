using OpenTK.Mathematics;

namespace SpaceShooter
{
    public abstract class DamageableEntity : Actor, IDamageable
    {
        public GameObject mesh;
        public BoxCollider collider;
        internal Shader shader;

        internal int health;
        internal int maxHealth;

        public DamageableEntity()
        {
        }

        public virtual void ApplyDamage(int damage)
        {
            health = MathHelper.Clamp(health - damage, 0, maxHealth);
            if (health == 0) Dead();
        }

        public virtual void Dead()
        {

        }
    }
}
