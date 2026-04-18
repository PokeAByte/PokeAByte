public static class PathExtensions
{
    extension(Path)
    {
        /// <summary>
        /// Normalize path to use '/' as the directory separator.
        /// </summary>
        /// <param name="path"> The path to modify.</param>
        /// <returns> The normalized path. </returns>
        public static string Normalize(string path) => path.Replace(Path.DirectorySeparatorChar, '/');

        /// <summary>
        /// Removes leading "/" from path.
        /// </summary>
        /// <param name="path"> The path to modify.</param>
        /// <returns> The path without leading "/". </returns>
        public static string AsRelative(string path) => path.StartsWith("/")
            ? path[1..]
            : path;

        public static string SetJsExtension(string path) => path[0..path.LastIndexOf(".")] + ".js";
    }
}