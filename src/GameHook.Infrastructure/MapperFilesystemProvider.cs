using GameHook.Domain;
using GameHook.Domain.Interfaces;

namespace GameHook.Infrastructure
{
    public class MapperFilesystemProvider : IMapperFilesystemProvider
    {
        private readonly AppSettings _appSettings;

        public IEnumerable<MapperFilesystemDTO> MapperFiles { get; private set; } = new List<MapperFilesystemDTO>();

        public MapperFilesystemProvider(AppSettings appSettings)
        {
            _appSettings = appSettings;

            CacheMapperFiles();
        }

        public void CacheMapperFiles()
        {
            MapperFiles = GetAllMapperFiles();
        }

        private string GetId(MapperFilesystemTypes type, string filePath)
        {
            if (filePath.Contains(".."))
            {
                throw new Exception("Invalid characters in file path.");
            }

            var pathParts = filePath.Split(Path.DirectorySeparatorChar);

            var directory = pathParts.Length > 1 ? pathParts[pathParts.Length - 2] : "";
            var filenameWithExtension = pathParts[pathParts.Length - 1];

            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithExtension);

            var formattedFilename = filenameWithoutExtension.Replace(' ', '_');

            return $"{type}_{directory}_{formattedFilename}".ToLower();
        }

        private string GetDisplayName(string filePath)
        {
            if (filePath.Contains(".."))
            {
                throw new Exception("Invalid characters in file path.");
            }

            var pathParts = filePath.Split(Path.DirectorySeparatorChar);

            var directory = pathParts.Length > 1 ? pathParts[pathParts.Length - 2] : "";
            var filenameWithExtension = pathParts[pathParts.Length - 1];

            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithExtension);

            var formattedFilename = filenameWithoutExtension.Replace('_', ' ');

            return $"({directory.ToUpper()}) {formattedFilename}";
        }

        /// <summary>
        /// We replace the base path with an empty string
        /// as to not expose the absolute path of the filesystem.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MapperFilesystemDTO> GetAllMapperFiles()
        {
            if (_appSettings.MAPPER_DIRECTORY.Contains(".."))
            {
                throw new Exception("Invalid characters in mapper folder path.");
            }

            var mappers = new DirectoryInfo(_appSettings.MAPPER_DIRECTORY)
                .GetFiles("*.xml", SearchOption.AllDirectories)
                .Select(x => new MapperFilesystemDTO()
                {
                    Id = GetId(MapperFilesystemTypes.Official, x.FullName),
                    Type = MapperFilesystemTypes.Official,
                    AbsolutePath = x.FullName,
                    DisplayName = $"{GetDisplayName(x.FullName)}"
                })
                .ToList();

            if (_appSettings.MAPPER_LOCAL_DIRECTORY != null)
            {
                if (_appSettings.MAPPER_LOCAL_DIRECTORY.Contains('.') || _appSettings.MAPPER_LOCAL_DIRECTORY.Contains(".."))
                {
                    throw new Exception("Invalid characters in mapper folder path.");
                }

                var localMappers = new DirectoryInfo(_appSettings.MAPPER_LOCAL_DIRECTORY)
                    .GetFiles("*.xml", SearchOption.AllDirectories)
                    .Select(x => new MapperFilesystemDTO()
                    {
                        Id = GetId(MapperFilesystemTypes.Local, x.FullName),
                        Type = MapperFilesystemTypes.Local,
                        AbsolutePath = x.FullName,
                        DisplayName = $"(Local) {GetDisplayName(x.FullName)}"
                    })
                    .ToList();

                mappers.AddRange(localMappers);
            }

            return mappers;
        }

        public string GetMapperRootDirectory(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                throw new Exception("Absolute path was not supplied.");
            }

            var path = absolutePath;

            while (path != null)
            {
                if (path.ToLower().EndsWith($"{Path.DirectorySeparatorChar}mappers"))
                {
                    return path;
                }

                if (path.EndsWith(":\\") || path == "/")
                {
                    throw new Exception($"Could not find a mappers directory in function GetMapperRootDirectory() with an absolute path of {absolutePath}. Reached the filesystem root.");
                }

                path = Path.GetDirectoryName(path);
            }

            throw new Exception($"Could not find a mappers directory in function GetMapperRootDirectory() with an absolute path of {absolutePath}.");
        }

        public string GetRelativePath(string absolutePath)
        {
            if (absolutePath.Contains(".."))
            {
                throw new Exception("The path provided does not appear to be an absolute path.");
            }

            if (absolutePath.StartsWith(_appSettings.MAPPER_DIRECTORY))
            {
                return string.Concat(".", absolutePath.AsSpan(_appSettings.MAPPER_DIRECTORY.Length));
            }
            else if (_appSettings.MAPPER_LOCAL_DIRECTORY != null && absolutePath.StartsWith(_appSettings.MAPPER_LOCAL_DIRECTORY))
            {
                return string.Concat(".", absolutePath.AsSpan(_appSettings.MAPPER_LOCAL_DIRECTORY.Length));
            }
            else
            {
                throw new Exception($"The absolute path {absolutePath} does not appear to be a mapper directory.");
            }
        }
    }
}
