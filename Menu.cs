using OpenTK.Graphics.OpenGL4;

namespace SpaceShooter
{
    public class Menu: Actor
    {
        private Texture menu1;
        private Texture menu2;
        private Texture menu3;
        private Texture menu4;

        private Shader shader;
        private GameObject mesh;

        private int menuIndex;
        public Action onQuit;
        public Action onRestart;
        public Action onResume;

        public Menu(float aspectRatio)
        {
            menu1 = new Texture("menu.png");
            menu2 = new Texture("menu2.png");
            menu3 = new Texture("menu3.png");
            menu4 = new Texture("menu4.png");
            shader = new Shader("menu.vert", "menu.frag");
            mesh = new GameObject(StaticUtilities.QuadVertices, StaticUtilities.QuadIndices, shader);

            shader.Use();
            int id = shader.GetUniformLocation("menu1");
            GL.Uniform1(id, 0);
            id = shader.GetUniformLocation("menu2");
            GL.Uniform1(id, 1);
            id = shader.GetUniformLocation("menu3");
            GL.Uniform1(id, 0);
            id = shader.GetUniformLocation("menu4");
            GL.Uniform1(id, 1);
            id = shader.GetUniformLocation("menu");
            GL.Uniform1(id, 0);
            id = shader.GetUniformLocation("aspectRatio");
            GL.Uniform1(id, aspectRatio);
        }

        public override void Destroy()
        {
            shader.Dispose();
            mesh.Dispose();
        }

        public override void Render(float deltaTime)
        {
            if (Game.GameOver)
            {
                menu3.Use(TextureUnit.Texture0);
                menu4.Use(TextureUnit.Texture1);
                mesh.Render();
            }
            else if (Game.IsPaused)
            {
                menu1.Use(TextureUnit.Texture0);
                menu2.Use(TextureUnit.Texture1);
                mesh.Render();
            }
        }

        public void SelectUp()
        {
            if (!Game.IsPaused) return;
            shader.Use();
            int id = shader.GetUniformLocation("menu");
            GL.Uniform1(id, menuIndex = Game.GameOver ? 2 : 0);
        }

        public void SelectDown()
        {
            if (!Game.IsPaused) return;
            shader.Use();
            int id = shader.GetUniformLocation("menu");
            GL.Uniform1(id, menuIndex = Game.GameOver? 3 : 1);
        }

        public void Confirm()
        {
            if (!Game.IsPaused) return;
            if (menuIndex == 0) onResume?.Invoke();
            else if (menuIndex == 2) onRestart?.Invoke();
            else onQuit?.Invoke();
        }

        public void Pause()
        {
            menuIndex = 0;
        }

        public void End()
        {
            menuIndex = 2;
        }
    }
}
