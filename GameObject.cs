using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace SpaceShooter
{
    public class GameObject
    {
        public Transform transform;
        
        private int vertexArrayObject;
        private int vertexBufferObject;
        private int elementBufferObject;

        private readonly float[] vertices;
        private readonly uint[] indices;
        public readonly Shader shader;

        public GameObject(float[] vertices, uint[] indices, Shader shader)
        {
            transform = new Transform(this);

            this.vertices = vertices;
            this.indices = indices;
            this.shader = shader;


            // init vbo
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticRead);


            // init vao
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            int id = 0;//shader.GetAttribLocation("vertPosition");
            GL.VertexAttribPointer(id, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(id);
            StaticUtilities.CheckError("vertPosition");


            id = 1;//shader.GetAttribLocation("aNormals");
            GL.VertexAttribPointer(id, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(id);
            StaticUtilities.CheckError("aNormals");


            id = 2;//shader.GetAttribLocation("UV0");
            GL.VertexAttribPointer(id, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(id);
            StaticUtilities.CheckError("bind UVs");

                
            // init ebo
            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            StaticUtilities.CheckError("elementBuffer");
        }

        public void Render()
        {
            shader.Use();

            int id = shader.GetUniformLocation("model");
            GL.UniformMatrix4(id, true, ref transform.GetMatrix);

            id = shader.GetUniformLocation("view");
            GL.UniformMatrix4(id, true, ref Game.view);

            id = shader.GetUniformLocation("projection");
            GL.UniformMatrix4(id, true, ref Game.projection);

            id = shader.GetUniformLocation("viewPos");
            GL.Uniform3(id, Game.gameCam.Position);


            GL.BindVertexArray(vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);


            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteBuffer(elementBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);
        }

        public float[] GetVerts() => vertices;
        public uint[] GetIndices() => indices;
    }
}
