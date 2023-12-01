using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;


namespace SpaceShooter
{
    public class Enemy : DamageableEntity
    {
        private Vector3 cannon;
        
        // SHOOTING
        private List<LaserBolt> pooledLasers = new();
        private int amountOfPooledLasers = 2;
        private bool enableShooting = true;
        private float shootDelay = 1f;
        private float shootTimer = -0.5f;

        // MOVEMENT
        private float thrust = 3;
        private Vector3 moveDirection = Vector3.Zero;
        private Vector3 velocity = Vector3.Zero;
        private float ypos = 10f;

        // TILTING
        private Vector3 startTilt = Vector3.Zero;
        private Vector3 targetTilt = Vector3.Zero;
        private float maxTilt = MathHelper.DegreesToRadians(15f);
        private float tiltSpeed = 1f;
        private float tiltTime;

        public bool IsDead { get; private set; } = false;
        public Action onEnemyDied;

        public Enemy()
        {
            shader = new Shader("Spaceship.vert", "Spaceship.frag");
            mesh = StaticUtilities.FBXToGameObject("smallspaceship.fbx", shader);
            mesh.transform.Position = StaticUtilities.WorldForward* ypos;
            mesh.transform.Scale = Vector3.One * 0.35f;
            mesh.transform.Rotation = StaticUtilities.WorldUp * MathF.PI;
            collider = new BoxCollider(new GameObject(StaticUtilities.BoxVertices, StaticUtilities.BoxIndices, Game.DefaultUnlitShader), this);

            Game.Colliders.Add(collider);

            for (int i = 0; i < amountOfPooledLasers; i++)
            {
                LaserBolt laser = new LaserBolt(this);
                laser.mesh.shader.Use();
                GL.Uniform3(laser.mesh.shader.GetUniformLocation("usercolor"), Vector3.UnitX);
                laser.direction = -StaticUtilities.WorldForward;
                pooledLasers.Add(laser);
                pooledLasers[i].SetActive(false);

                laser.light.Color = Vector3.UnitX;
                StaticUtilities.Lights.Add(laser.light);
            }

            maxHealth = 2;
            health = maxHealth;

            Random rnd = new Random();
            thrust = MathHelper.Clamp(rnd.NextSingle() * 5f, 3f, 5f);
        }

        public void Shoot()
        {
            if (!enableShooting || shootTimer < shootDelay) return;
            shootTimer = 0;
            
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
            // lasers
            foreach (LaserBolt laser in pooledLasers)
            {
                laser.Update(deltaTime);
            }

            if (IsDead || Game.IsPaused) return;

            cannon = mesh.transform.Position - StaticUtilities.WorldForward * 2.2f;
            shootTimer += deltaTime;
            Shoot();

            // are we switching sides
            Vector3 newPlayerDirection = GetPlayerDirection();
            if (newPlayerDirection.X > 0 && moveDirection.X < 0 ||
                newPlayerDirection.X < 0 && moveDirection.X > 0 || 
                moveDirection.X == 0)
            {
                moveDirection = newPlayerDirection;
                startTilt = mesh.transform.Rotation;
                targetTilt = StaticUtilities.WorldForward * maxTilt * (moveDirection.X > 0 ? -1 : 1) + StaticUtilities.WorldUp * MathF.PI;
                tiltTime = 0;
            }

            // follow player
            Move(deltaTime, newPlayerDirection);
            Tilt(tiltTime = MathHelper.Clamp(tiltTime + deltaTime * tiltSpeed, 0, 1));
            collider.box.transform.Position = mesh.transform.Position;
        }

        private void Move(float deltaTime, Vector3 direction)
        {
            if (direction != Vector3.Zero) // if we are moving
            {
                velocity += direction * thrust * deltaTime;
            }

            Vector3 min = StaticUtilities.MinXYPos;
            Vector3 max = StaticUtilities.MaxXYPos;
            max.Y = ypos;
            Transform meshTrans = mesh.transform;

            meshTrans.Position = Vector3.Clamp(meshTrans.Position + velocity * deltaTime, min, max);
            if (meshTrans.Position.X == max.X || meshTrans.Position.X == min.X) velocity.X = 0;
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
        }

        public override void SetActive(bool isActive)
        {
            IsDead = !isActive;
            collider.enabled = isActive;

            if (isActive)
            {
                Random rnd = new Random();

                // start on random x off screen
                mesh.transform.Position = StaticUtilities.WorldForward * (ypos + 0)  + StaticUtilities.WorldRight * rnd.Next((int)StaticUtilities.MinXYPos.X, (int)StaticUtilities.MaxXYPos.X);
                mesh.transform.Rotation = StaticUtilities.WorldUp * MathF.PI;
                health = maxHealth;
                shootTimer = -0.1f;
            }
        }

        private Vector3 GetPlayerDirection()
        {
            return Vector3.UnitX * MathHelper.Clamp(StaticUtilities.player.mesh.transform.Position.X - mesh.transform.Position.X, -1, 1);
        }

        public override void Dead()
        {
            onEnemyDied?.Invoke();   
            SetActive(false);
        }

        public void ResetLasers()
        {
            foreach (var laser in pooledLasers) laser.SetActive(false);
        }
    }
}
