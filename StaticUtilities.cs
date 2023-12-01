using StbImageSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Assimp;

namespace SpaceShooter
{
    public static class StaticUtilities
    {
        public static Texture ExplosionTexture;

        public static Player player;
        public static readonly Vector3 WorldForward = Vector3.UnitY;
        public static readonly Vector3 WorldRight = Vector3.UnitX;
        public static readonly Vector3 WorldUp = Vector3.UnitZ;

        public static readonly Vector3 MaxXYPos = WorldForward * 3 + WorldRight * 13;
        public static readonly Vector3 MinXYPos = WorldForward * -10 + WorldRight * -13;

        public static readonly string RootDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent + "\\";
        public static readonly string ShaderDirectory = RootDirectory + "Shaders\\";
        public static readonly string TextureDirectory = RootDirectory + "Textures\\";
        public static readonly string ObjectDirectory = RootDirectory + "Objects\\";
        public static readonly string[] pointLightDefinition =
        {
            "pointLights[",
            "INDEX",
            "]."
        };

        public static readonly List<PointLight> Lights = new List<PointLight>();

        public const uint meshVertStride = 8;
        
        static StaticUtilities()
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
        }

        public static void CheckError(string stage)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                Console.WriteLine($"OpenGL Error ({stage}): {errorCode}");
            }
        }

        public static float[] MergeMeshData(this Mesh mymesh)
        {
            int n = mymesh.Vertices.Count;
            float[] export = new float[n * 8];

            for (int i = 0; i < n; i++)
            {
                int index = i * 8;

                // verts
                export[index] = mymesh.Vertices[i].X;
                export[index+1] = mymesh.Vertices[i].Y;
                export[index+2] = mymesh.Vertices[i].Z;

                // normals
                export[index+3] = mymesh.Normals[i].X;
                export[index+4] = mymesh.Normals[i].Y;
                export[index+5] = mymesh.Normals[i].Z;

                // uvs
                export[index+6] = mymesh.TextureCoordinateChannels[0][i].X;
                export[index+7] = mymesh.TextureCoordinateChannels[0][i].Y;
            }

            return export;
        }

        public static GameObject FBXToGameObject(string file, Shader shader)
        {
            AssimpContext importer = new AssimpContext();
            PostProcessSteps pps = PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs;
            Scene scene = importer.ImportFile(ObjectDirectory + file, pps);
            return new GameObject(scene.Meshes[0].MergeMeshData(), scene.Meshes[0].GetUnsignedIndices(), shader);
        }

        public static void RenderLitObject(GameObject litObject)
        {
            int id;
            litObject.shader.Use();
            for (int i = 0; i < Lights.Count; i++)
            {
                PointLight currentLight = Lights[i];
                pointLightDefinition[1] = i.ToString();
                string merged = string.Concat(pointLightDefinition);

                id = litObject.shader.GetUniformLocation(merged + "lightColor");
                GL.Uniform3(id, currentLight.Color);
                id = litObject.shader.GetUniformLocation(merged + "lightPos");
                GL.Uniform3(id, currentLight.Transform.Position);
                id = litObject.shader.GetUniformLocation(merged + "lightIntensity");
                GL.Uniform1(id, currentLight.Intensity);
            }

            id = litObject.shader.GetUniformLocation("numPointLights");
            GL.Uniform1(id, Lights.Count);

            litObject.Render();
        }


        public static readonly float[] QuadVertices =
    {
        -1.0f,  1.0f, 0.0f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Top Left (X, Y, Z, Nx, Ny, Nz, U, V)
        -1.0f, -1.0f, 0.0f,  0.0f, 1.0f, 0.0f, 0.0f, 0.0f, // Bottom Left (X, Y, Z, Nx, Ny, Nz, U, V)
        1.0f, -1.0f, 0.0f,  0.0f, 1.0f, 0.0f, 1.0f, 0.0f, // Bottom Right (X, Y, Z, Nx, Ny, Nz, U, V)
        1.0f,  1.0f, 0.0f,  0.0f, 1.0f, 0.0f, 1.0f, 1.0f  // Top Right (X, Y, Z, Nx, Ny, Nz, U, V)
    };

        public static readonly uint[] QuadIndices =
        {
        0, 1, 2,
        0, 2, 3
    };

        public static readonly float[] BoxVertices =
    {
    // Front face
    -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,  // Bottom-left (Position), Front Normal, UV
    0.5f, -0.5f, 0.5f,  0.0f, 0.0f, 1.0f, 1.0f, 0.0f,  // Bottom-right (Position), Front Normal, UV
    0.5f,  0.5f, 0.5f,  0.0f, 0.0f, 1.0f, 1.0f, 1.0f,  // Top-right (Position), Front Normal, UV
    -0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,  // Top-left (Position), Front Normal, UV
    
    // Right face
    0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f,  0.0f, 0.0f,  // Bottom-left (Position), Right Normal, UV
    0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,  // Bottom-right (Position), Right Normal, UV
    0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,  // Top-right (Position), Right Normal, UV
    0.5f,  0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,  // Top-left (Position), Right Normal, UV
    
    // Back face
    -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f , 0.0f, 0.0f,  // Bottom-left (Position), Back Normal, UV
    0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f,  // Bottom-right (Position), Back Normal, UV
    0.5f,  0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,  // Top-right (Position), Back Normal, UV
    -0.5f,  0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f,  // Top-left (Position), Back Normal, UV
    
    // Left face
    -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,  // Bottom-left (Position), Left Normal, UV
    -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,  // Bottom-right (Position), Left Normal, UV
    -0.5f,  0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f,  // Top-right (Position), Left Normal, UV
    -0.5f,  0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,  // Top-left (Position), Left Normal, UV
    
    // Top face
    -0.5f,  0.5f, 0.5f,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,  // Bottom-left (Position), Top Normal, UV
    0.5f,  0.5f, 0.5f,  0.0f, 1.0f, 0.0f, 1.0f, 0.0f,  // Bottom-right (Position), Top Normal, UV
    0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f, 1.0f,  // Top-right (Position), Top Normal, UV
    -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,  // Top-left (Position), Top Normal, UV
    
    // Bottom face
    -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f,  // Bottom-left (Position), Bottom Normal, UV
    0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,  // Bottom-right (Position), Bottom Normal, UV
    0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,  // Top-right (Position), Bottom Normal, UV
    -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f   // Top-left (Position), Bottom Normal, UV
    };

        public static readonly uint[] BoxIndices =
        {
        0, 1, 2,
        2, 3, 0,

        // Right face
        4, 5, 6,
        6, 7, 4,

        // Back face
        8, 10, 9,
        10, 8, 11,

        // Left face
        12, 13, 14,
        14, 15, 12,

        // Top face
        16, 17, 18,
        18, 19, 16,

        // Bottom face
        20, 21, 22,
        22, 23, 20
    };
    }
}
