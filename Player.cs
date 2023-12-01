using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace SpaceShooter
{
    public class Player : DamageableEntity
    {
        private Vector3[] cannons = new Vector3[2];

        // SHOOTING
        private List<LaserBolt> pooledLasers = new();
        private int amountOfPooledLasers = 40;
        private bool enableShooting = true;
        private float shootDelay = 0.25f;
        private float shootTimer;

        // MOVEMENT
        private Vector3 moveDirection = Vector3.Zero;
        private Vector3 velocity = Vector3.Zero;
        private Vector3 refVelocity; // for deceleration
        private float thrust = 10f;
        private float decelSpeed = 1.2f;
        private float decelTime;
        private bool stopped;

        // TILTING
        private Vector3 startTilt;
        private Vector3 targetTilt;
        private float maxTilt = MathHelper.DegreesToRadians(15f);
        private float tiltSpeed = 2f;
        private float tiltTime;

        public Action onPlayerDied;

        public bool IsDead { get; private set; } = false;

        public Player()
        {
            shader = new Shader("Spaceship.vert", "Spaceship.frag");
            mesh = StaticUtilities.FBXToGameObject("spaceship.fbx", shader);
            mesh.transform.Position = StaticUtilities.WorldForward * -9;
            mesh.transform.Scale = Vector3.One * 0.5f;
            collider = new BoxCollider(new GameObject(StaticUtilities.BoxVertices, StaticUtilities.BoxIndices, Game.DefaultUnlitShader), this);

            Game.Colliders.Add(collider);

            shootTimer = shootDelay;

            for (int i = 0; i < amountOfPooledLasers; i++)
            {
                LaserBolt laser = new LaserBolt(this);
                laser.mesh.shader.Use();
                GL.Uniform3(laser.mesh.shader.GetUniformLocation("usercolor"), Vector3.UnitY);
                laser.direction = StaticUtilities.WorldForward;
                pooledLasers.Add(laser);
                pooledLasers[i].SetActive(false);

                laser.light.Color = Vector3.UnitY;
                StaticUtilities.Lights.Add(laser.light);
            }

            maxHealth = 3;
            health = maxHealth;

            Game.hud.shader.Use();
            GL.Uniform1(Game.hud.shader.GetUniformLocation("health"), (int)health);
        }

        public void Shoot()
        {
            if (Game.IsPaused || IsDead || !enableShooting || shootTimer < shootDelay) return;
            shootTimer = 0;
            foreach (var cannon in cannons)
            {
                for (int i = 0; i < pooledLasers.Count; i++)
                {
                    if (pooledLasers[i].IsDead)
                    {
                        pooledLasers[i].SetActive(true);
                        pooledLasers[i].mesh.transform.Position = cannon;
                        break;
                    }
                }
            }
        }

        public override void Render(float deltaTime)
        {
            foreach (LaserBolt laser in pooledLasers)
            {
                laser.Render(deltaTime);
            }

            if (IsDead) return;

            StaticUtilities.RenderLitObject(mesh);

        }

        public override void Update(float deltaTime)
        {
            foreach (LaserBolt laser in pooledLasers)
            {
                laser.Update(deltaTime);
            }

            if (IsDead || Game.IsPaused) return;

            shootTimer += deltaTime;
            cannons[0] = mesh.transform.Position + StaticUtilities.WorldRight * 1.55f + StaticUtilities.WorldForward * 1.5f;
            cannons[1] = mesh.transform.Position - StaticUtilities.WorldRight * 1.55f + StaticUtilities.WorldForward * 1.5f;


            Move(deltaTime);
            Tilt(tiltTime = MathHelper.Clamp(tiltTime + deltaTime * tiltSpeed, 0, 1));
            collider.box.transform.Position = mesh.transform.Position;
        }

        private void Move(float deltaTime)
        {
            if (moveDirection != Vector3.Zero) // if we are moving
            {
                velocity += moveDirection * thrust * deltaTime;
                decelTime = 0;
                stopped = false;
            }
            else if (!stopped) // as soon as we stop moving
            {
                stopped = true;
                refVelocity = velocity;
            }
            else // decelerate to stop
            {
                decelTime = MathHelper.Clamp(decelTime + deltaTime * decelSpeed, 0, 1);
                velocity = Vector3.Lerp(refVelocity, Vector3.Zero, decelTime);
            }

            Vector3 min = StaticUtilities.MinXYPos;
            Vector3 max = StaticUtilities.MaxXYPos;
            Transform meshTrans = mesh.transform;

            meshTrans.Position = Vector3.Clamp(meshTrans.Position + velocity * deltaTime, min, max);
            if (meshTrans.Position.X == max.X || meshTrans.Position.X == min.X) velocity.X = 0;
            if (meshTrans.Position.Y == max.Y || meshTrans.Position.Y == min.Y) velocity.Y = 0;
        }

        private void Tilt(float x) // add tilt (x=time)
        {
            float a = x < 0.5 ? 2 * x * x : 1 - MathF.Pow(-2 * x + 2, 2) / 2;
            mesh.transform.Rotation = Vector3.Lerp(startTilt, targetTilt, a);
        }

        public override void Destroy()
        {
            mesh.Dispose();
            shader.Dispose();
            
            for (int i = 0; i < pooledLasers.Count; i++)
            {
                pooledLasers[i].Destroy();
            }
        }

        public override void SetActive(bool isActive)
        {
            IsDead = !isActive;
            collider.enabled = isActive;

            if (isActive)
            {
                mesh.transform.Position = StaticUtilities.WorldForward * -9;
                moveDirection = Vector3.Zero;
                velocity = Vector3.Zero;
                health = maxHealth;
            }

            Game.hud.shader.Use();
            GL.Uniform1(Game.hud.shader.GetUniformLocation("health"), (int)health);
        }

        public void SetDirection(Vector3 newDirection)
        {
            if (Game.IsPaused) return;

            moveDirection += newDirection;
            startTilt = mesh.transform.Rotation;
            targetTilt = StaticUtilities.WorldForward * maxTilt * moveDirection.X +
                    StaticUtilities.WorldRight * -maxTilt * moveDirection.Y;
            tiltTime = 0;
        }


        public override void Dead()
        {
            onPlayerDied?.Invoke();
            SetActive(false);
        }

        public override void ApplyDamage(int damage)
        {
            base.ApplyDamage(damage);
            Game.hud.shader.Use();
            GL.Uniform1(Game.hud.shader.GetUniformLocation("health"), health);
            //Console.WriteLine("fht");
        }

        public void ResetLasers()
        {
            foreach (var laser in pooledLasers) laser.SetActive(false);
        }
    }
}
