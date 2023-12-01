using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;

namespace SpaceShooter
{
    public class Game : GameWindow
    {
        public static readonly List<Actor> actors = new();
        public static readonly List<GameObject> LitObjects = new();
        public static readonly List<GameObject> UnLitObjects = new();
        public static readonly List<GameObject> TransparentUnlitObjects = new();
        public static readonly List<BoxCollider> Colliders = new();

        public static readonly Shader DefaultLitShader = new("Default\\DefaultLit.vert", "Default\\DefaultLit.frag");
        public static readonly Shader DefaultUnlitShader = new("Default\\DefaultUnlit.vert", "Default\\DefaultUnlit.frag");

        public static Matrix4 view = Matrix4.Identity;
        public static Matrix4 projection = Matrix4.Identity;

        public static Camera gameCam;
        private Vector2 previousMousePos;

        private int maxEnemyCount = 1;
        private int enemiesDowned;
        private int enemyPool = 10;

        public static GameObject hud;
        private Menu menu;
        public static bool IsPaused;
        public static bool GameOver;

        public Game(int width, int height, string title) :
           base(GameWindowSettings.Default, new NativeWindowSettings { Title = title, Size = (width, height) })
        {
            view = Matrix4.CreateTranslation(0, 0, 0);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), (float)width / height, 0.1f, 100f);
            gameCam = new Camera(StaticUtilities.WorldUp * 12, (float)Size.X / Size.Y);
            menu = new((float)width / height);
            menu.onQuit += QuitGame;
            menu.onResume += ResumeGame;
            menu.onRestart += RestartGame;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0, 0, 0, 1);
            previousMousePos = new Vector2(MouseState.X, MouseState.Y);
            CursorState = CursorState.Grabbed;

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            StaticUtilities.ExplosionTexture = new Texture("Explosion.png");
            hud = new GameObject(StaticUtilities.QuadVertices, StaticUtilities.QuadIndices, new Shader("hud.vert","hud.frag"));
            hud.shader.Use();
            GL.Uniform1(hud.shader.GetUniformLocation("aspectRatio"), (float)Size.X / Size.Y);
            GL.Uniform1(hud.shader.GetUniformLocation("health"), 5);


            // player
            actors.Add(StaticUtilities.player = new Player());
            StaticUtilities.player.onPlayerDied += GameEnded;

            // pool enemies
            for (int i = 0; i < enemyPool; i++)
            {
                Enemy e = new Enemy();
                e.onEnemyDied += ProgressGame;
                if (i >= maxEnemyCount) e.SetActive(false);
                actors.Add(e);
            }

            // lights
            StaticUtilities.Lights.Add(new PointLight(new Vector3(1, 1, 1), 2));
            StaticUtilities.Lights[0].Transform.Position = StaticUtilities.WorldUp * 3;

            // menu
            actors.Add(menu);
        }

        protected override void OnUnload()
        {
            // free vram
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.UseProgram(0);

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].Destroy();
            }
            for (int i = 0; i < UnLitObjects.Count; i++)
            {
                UnLitObjects[i].Dispose();
            }
            for (int i = 0; i < LitObjects.Count; i++)
            {
                LitObjects[i].Dispose();
            }
            for (int i = 0; i < TransparentUnlitObjects.Count; i++)
            {
                TransparentUnlitObjects[i].Dispose();
            }
            hud.shader.Dispose();


            base.OnUnload();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            HandleKeyboardInputs();
            HandleMouseInputs(args);

            UpdateActors(args);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            gameCam.Fov -= e.OffsetY;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            //first
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            view = gameCam.GetViewMatrix();
            projection = gameCam.GetProjectionMatrix();

            RenderLitObjects();
            RenderUnlitObjects();
            RenderActors(args);
            RenderTransparentObjects();
            hud.Render();

            //last
            SwapBuffers();
        }

        private void RenderLitObjects()
        {
            foreach (var lit in LitObjects)
            {
                StaticUtilities.RenderLitObject(lit);
            }
        }

        private void RenderUnlitObjects()
        {
            foreach (GameObject unlit in UnLitObjects)
            {
                unlit.Render();
            }
        }
        
        private void RenderTransparentObjects()
        {
            for (int i = TransparentUnlitObjects.Count - 1; i >= 0; i--)
            {
                TransparentUnlitObjects[i].Render();
            }
        }

        private void RenderActors(FrameEventArgs args)
        {
            foreach (var actor in actors)
            {
                actor.Render((float)args.Time);
            }
        }

        private void UpdateActors(FrameEventArgs args)
        {
            foreach (var actor in actors)
            {
                actor.Update((float)args.Time);
            }
        }

        private void HandleKeyboardInputs()
        {
            if (KeyboardState.IsKeyPressed(Keys.Escape) && !GameOver)
            {
                IsPaused = !IsPaused;
                if (IsPaused) { CursorState = CursorState.Normal; menu.Pause(); }
                else CursorState = CursorState.Grabbed;
                return;
            }

            Player player = StaticUtilities.player;

            // PRESSED
            if (KeyboardState.IsKeyPressed(Keys.W))
            {
                player.SetDirection(StaticUtilities.WorldForward);
                menu.SelectUp();
            }
            if (KeyboardState.IsKeyPressed(Keys.S))
            {
                player.SetDirection(-StaticUtilities.WorldForward);
                menu.SelectDown();
            }
            if (KeyboardState.IsKeyPressed(Keys.A))
            {
                player.SetDirection(-StaticUtilities.WorldRight);
            }
            if (KeyboardState.IsKeyPressed(Keys.D))
            {
                player.SetDirection(StaticUtilities.WorldRight);
            }
            if (KeyboardState.IsKeyPressed(Keys.Space))
            {
                menu.Confirm();
            }

            
            // RELEASED
            if (KeyboardState.IsKeyReleased(Keys.W))
            {
                player.SetDirection(-StaticUtilities.WorldForward);
            }
            if (KeyboardState.IsKeyReleased(Keys.S))
            {
                player.SetDirection(StaticUtilities.WorldForward);
            }
            if (KeyboardState.IsKeyReleased(Keys.A))
            {
                player.SetDirection(StaticUtilities.WorldRight);
            }
            if (KeyboardState.IsKeyReleased(Keys.D))
            {
                player.SetDirection(-StaticUtilities.WorldRight);
            }
        }

        private void HandleMouseInputs(FrameEventArgs args)
        {
            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                StaticUtilities.player.Shoot();
            }

            // Get the mouse state
            const float sensitivity = 0f;

            // Calculate the offset of the mouse position
            var deltaX = MouseState.X - previousMousePos.X;
            var deltaY = MouseState.Y - previousMousePos.Y;
            previousMousePos = new Vector2(MouseState.X, MouseState.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            gameCam.Yaw += deltaX * sensitivity;
            gameCam.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
        }

        private void QuitGame()
        {
            Close();
        }

        private void ResumeGame()
        {
            IsPaused = false; 
            CursorState = CursorState.Grabbed;
        }

        private void RestartGame()
        {
            IsPaused = false;
            GameOver = false;
            maxEnemyCount = 1;

            foreach (var item in actors)
            {
                item.SetActive(false);

                (item as Player)?.ResetLasers();
                (item as Enemy)?.ResetLasers();
            }

            // enable player again
            actors[0].SetActive(true);
            actors[1].SetActive(true);
        }

        private void GameEnded()
        {
            GameOver = true;
            IsPaused = true;

            menu.End();
        }

        private void ProgressGame()
        {
            if (++enemiesDowned >= maxEnemyCount)
            {
                enemiesDowned = 0;
                maxEnemyCount = MathHelper.Clamp(maxEnemyCount + 1, 0, enemyPool);
                for (int i = 0; i < maxEnemyCount; i++)
                {
                    //find the next deactivated enemy
                    foreach (Actor item in actors)
                    {
                        if (item is not Enemy) continue;
                        if ((item as Enemy)!.IsDead)
                        {
                            item.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }
    }
}
