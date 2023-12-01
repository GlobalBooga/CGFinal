using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace SpaceShooter
{
    public class Texture
    {
        public readonly int Handle;

        public Texture(string path)
        {
            Handle = GL.GenTexture();

            Use();

            // load image
            using (Stream fileStream = File.OpenRead(StaticUtilities.TextureDirectory + path))
            {
                ImageResult img = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
            }

            // set filter mode
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest); // downscale
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); // upscale

            // set wrapping mode
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // x repeats
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); // y repeats

            // genreate mip maps
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
