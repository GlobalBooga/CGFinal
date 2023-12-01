
namespace SpaceShooter
{
    public static class Program
    {
        static void Main()
        {
            using (Game theGame = new Game(1280, 1000, "game"))
            {
                theGame.Run();
            }
        }
    }
}