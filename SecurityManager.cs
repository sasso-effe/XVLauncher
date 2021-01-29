using System;

namespace XVLauncher
{
    /// <summary>
    /// Singleton class to generate php files path, just to avoid to put it as plain text.
    /// It is a really basic security system, do not use it to protect sensible data.
    /// </summary>
    class SecurityManager
    {
        private readonly string PATH;
        private static SecurityManager instance = null;


        private SecurityManager(string path)
        {
            this.PATH = path;
        }

        /// <summary>
        /// Singleton pattern's getter method.
        /// </summary>
        /// <returns>Singleton instance of <see cref="SecurityManager"/>.</returns>
        public static SecurityManager GetInstance()
        {
            if (instance is null)
            {
                instance = new SecurityManager(GeneratePath());
            }
            return instance;
        }

        public string GetPath()
        {
            return PATH;
        }

        private static string GeneratePath()
        {
            // TODO: implement a method that return php files path (ex: https://www.example.com/my-path/)
            throw new NotImplementedException("SecurityManager.GeneratePath() is not implemented!");
        }

    }
}
