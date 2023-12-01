using OpenTK.Graphics.OpenGL4;

namespace SpaceShooter
{
    public class Shader : IDisposable
    {
        public readonly int Handle;
        protected bool _isDisposed;

        public Shader(string vertexPath, string fragmentPath)
        {
            // initialize vertex shader
            int vertexHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexHandle, File.ReadAllText(StaticUtilities.ShaderDirectory + vertexPath));

            // initialize fragment shader
            int fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentHandle, File.ReadAllText(StaticUtilities.ShaderDirectory + fragmentPath));

            LoadShader(vertexHandle);
            LoadShader(fragmentHandle);


            // bind the shaders to the program
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexHandle);
            GL.AttachShader(Handle, fragmentHandle);

            
            int successState;


            // handle linking
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out successState);

            if (successState == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in vertex shader: " + GL.GetProgramInfoLog(Handle));
                Console.ForegroundColor = ConsoleColor.White;
            }


            // clean up VRam
            GL.DetachShader(Handle, fragmentHandle);
            GL.DetachShader(Handle, vertexHandle);

            GL.DeleteShader(fragmentHandle);
            GL.DeleteShader(vertexHandle);
        }

        ~Shader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Oops! Memory leak");
            Console.ForegroundColor = ConsoleColor.White;
        }

        int LoadShader(int handle)
        {
            int success;

            GL.CompileShader(handle);
            GL.GetShader(handle, ShaderParameter.CompileStatus, out success);

            if (success == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in vertex shader: " + GL.GetShaderInfoLog(handle));
                Console.ForegroundColor = ConsoleColor.White;
            }

            return success;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attributeName)
        {
            return GL.GetAttribLocation(Handle, attributeName);
        }

        public int GetUniformLocation(string attributeName)
        {
            return GL.GetUniformLocation(Handle, attributeName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // DO NOT CALL our deconstructor
        }

        protected virtual void Dispose(bool state)
        {
            if (state)
            {
                GL.DeleteProgram(Handle);
                _isDisposed = true;
            }
        }
    }
}
