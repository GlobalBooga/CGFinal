using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace SpaceShooter
{
    public class LaserBolt
    {
        private Transform? parentTransform;
        private GameObject? parentGameObject;
        private DamageableEntity? parentEntity;

        public readonly GameObject mesh;
        public readonly PointLight light;
        private Shader laserShader;

        public float lightIntensity = 0.5f;
        private int damage = 1;

        public float lifeTime = 1f;
        public float speed = 20f;

        private float aliveTime;
        private float shaderTime;

        public Vector3 direction = Vector3.Zero;

        public bool IsDead { get; private set; } = false;
        private bool isExploding;

        public LaserBolt(object? parent = null)
        {
            laserShader = new Shader("LaserBolt.vert", "LaserBolt.frag");
            mesh = new GameObject(StaticUtilities.BoxVertices, StaticUtilities.BoxIndices, laserShader);

            laserShader.Use();
            int id = laserShader.GetUniformLocation("explosionTex");
            GL.Uniform1(id, 2);

            light = new PointLight(Vector3.One, lightIntensity);

            if (parent is Transform)
            {
                parentTransform = parent as Transform;
                parentGameObject = parentTransform?.parent;
            }
            else if (parent is GameObject)
            {
                parentGameObject = parent as GameObject;
                parentTransform = parentGameObject?.transform;
            }
            else if (parent is Actor)
            {
                parentEntity = parent as DamageableEntity;
                parentGameObject = parentEntity?.mesh;
                parentTransform = parentGameObject?.transform;
            }
        }

        public void Render(float deltaTime)
        {
            if (IsDead) return;

            laserShader.Use();
            
            if (isExploding)
            {
                StaticUtilities.ExplosionTexture.Use(TextureUnit.Texture2);
                if (shaderTime > (1.0f / 24.0f * 25.0f)) SetActive(false);
            }

            GL.Uniform1(laserShader.GetUniformLocation("time"), shaderTime += deltaTime);
            if (!isExploding) mesh.Render();
        }

        public void Update(float deltaTime)
        {
            if (IsDead || Game.IsPaused) return;

            if (aliveTime < lifeTime)
            {
                aliveTime += deltaTime;
                mesh.transform.Position += direction * speed * deltaTime;
                light.Transform.Position = mesh.transform.Position;
                CheckCollisions();
            }
            else if (!isExploding)
            {
                SetActive(false);
            }
        }

        public void Destroy()
        {
            mesh.Dispose();
            laserShader.Dispose();
        }

        public void SetActive(bool isActive)
        {
            light.Intensity = isActive == false? 0 : lightIntensity;
            IsDead = !isActive;
            Game.TransparentUnlitObjects.Remove(mesh);

            if (isActive)
            {
                mesh.transform.Position = parentTransform != null ? parentTransform.Position : Vector3.Zero;
                light.Transform.Position = mesh.transform.Position;
                aliveTime = 0;
                shaderTime = 0;
                isExploding = false;

                laserShader.Use();
                int id = laserShader.GetUniformLocation("state");
                GL.Uniform1(id, 0);
            }
        }

        private void CheckCollisions()
        {
            Vector3 origin = mesh.transform.Position;
            const float collisionRadius = 1f;

            foreach (var collider in Game.Colliders)
            {
                if (!collider.enabled) continue;

                if (parentEntity != null)
                {
                    if (collider.owner == parentEntity) continue;
                    if (collider.owner.GetType() == parentEntity.GetType()) continue;
                }
                
                float[] colliderVerts = collider.box.GetVerts();

                // foreach face
                for (uint faceIndex = 0; faceIndex < colliderVerts.Length; faceIndex += StaticUtilities.meshVertStride)
                {
                    bool outsideAllVerts = false;
                    bool outsideAllEdges = false;

                    Vector3[] face = new Vector3[4]; // bl, br, tr, tl
                    Vector3 faceNormal = Vector3.UnitX * colliderVerts[faceIndex + 3] + Vector3.UnitY * colliderVerts[faceIndex + 4] + Vector3.UnitZ * colliderVerts[faceIndex + 5];
                    // skip top and bottom faces
                    if (MathF.Abs(Vector3.Dot(faceNormal, StaticUtilities.WorldUp)) > 0.9f) continue;

                    // get face
                    face[0] = Vector3.UnitX * colliderVerts[faceIndex] + Vector3.UnitY * colliderVerts[faceIndex + 1] + Vector3.UnitZ * colliderVerts[faceIndex + 2] + collider.box.transform.Position;
                    faceIndex += StaticUtilities.meshVertStride;
                    face[1] = Vector3.UnitX * colliderVerts[faceIndex] + Vector3.UnitY * colliderVerts[faceIndex + 1] + Vector3.UnitZ * colliderVerts[faceIndex + 2] + collider.box.transform.Position;
                    faceIndex += StaticUtilities.meshVertStride;
                    face[2] = Vector3.UnitX * colliderVerts[faceIndex] + Vector3.UnitY * colliderVerts[faceIndex + 1] + Vector3.UnitZ * colliderVerts[faceIndex + 2] + collider.box.transform.Position;
                    faceIndex += StaticUtilities.meshVertStride;
                    face[3] = Vector3.UnitX * colliderVerts[faceIndex] + Vector3.UnitY * colliderVerts[faceIndex + 1] + Vector3.UnitZ * colliderVerts[faceIndex + 2] + collider.box.transform.Position;


                    float d = Vector3.Dot((face[0] + face[1] + face[2]) / -3f, faceNormal);
                    float ppd = Vector3.Dot(faceNormal, origin) + d;

                    if (ppd > collisionRadius)
                    {
                        continue;
                    }

                    bool outsideV1 = (face[0] - origin).LengthSquared > collisionRadius;
                    bool outsideV2 = (face[1] - origin).LengthSquared > collisionRadius;
                    bool outsideV3 = (face[2] - origin).LengthSquared > collisionRadius;
                    bool outsideV4 = (face[3] - origin).LengthSquared > collisionRadius;

                    if (outsideV1 && outsideV2 && outsideV3 && outsideV4)
                    {
                        outsideAllVerts = true;
                    }

                    Vector3 a = face[0] - face[1];
                    Vector3 b = face[1] - face[2];
                    Vector3 c = face[2] - face[3];
                    Vector3 e = face[3] - face[0];

                    Vector3 ip;
                    if (!IntersectRaySegment(face[0], a, origin, collisionRadius, out ip) &&
                        !IntersectRaySegment(face[1], b, origin, collisionRadius, out ip) &&
                        !IntersectRaySegment(face[2], c, origin, collisionRadius, out ip) &&
                        !IntersectRaySegment(face[3], e, origin, collisionRadius, out ip)) 
                    {
                        outsideAllEdges = true;
                    }

                    if (outsideAllVerts && outsideAllEdges)
                    {
                        continue;
                    }

                    OnCollisionEnter(collider);
                }
            }
        }

        bool IntersectRaySegment(Vector3 o, Vector3 d, Vector3 so, float radius2, out Vector3 ip)
        {
            ip = Vector3.Zero;

            float l = d.Length;
            d /= l;

            Vector3 m = o - so;
            float b = Vector3.Dot(m,d);
            float c = Vector3.Dot(m,m) - radius2;

            if (c > 0.0f && b > 0.0f) return false;

            float discr = b * b - c;

            if (discr < 0.0f) return false;

            float t = -b - MathF.Sqrt(discr);

            if (t < 0.0f) t = 0.0f;

            ip = o + (d * t);

            if (t > 1) return false;

            return true;
        }

        void OnCollisionEnter(BoxCollider other)
        {
            Game.TransparentUnlitObjects.Add(mesh);
            light.Intensity = 0;

            isExploding = true;
            shaderTime = 0;
            aliveTime = lifeTime;

            laserShader.Use();
            int id = laserShader.GetUniformLocation("state");
            GL.Uniform1(id, 1);

            //Console.WriteLine("gjhghjghjgjg");

            if (other.owner is IDamageable) (other.owner as IDamageable)?.ApplyDamage(damage);
        }
    }
}
