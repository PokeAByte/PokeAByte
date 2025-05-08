using PokeAByte.Domain.PokeAByteProperties;

namespace PokeAByte.Domain.Interfaces
{
    public class MetadataSection
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string GameName { get; init; } = string.Empty;
        public string GamePlatform { get; init; } = string.Empty;
    }

    public class MemorySection
    {
        public ReadRange[] ReadRanges { get; init; } = [];
    }

    public class ReadRange
    {
        public uint Start { get; init; }
        public uint End { get; init; }
    }

    public interface IPokeAByteMapper : IDisposable
    {
        MetadataSection Metadata { get; }
        MemorySection Memory { get; }
        Dictionary<string, IPokeAByteProperty> Properties { get; }
        Dictionary<string, ReferenceItems> References { get; }
        public IPlatformOptions PlatformOptions { get; }

        public IPokeAByteProperty? get_property(string path) {
            if (Properties.TryGetValue(path, out var property)) {
                return property;
            }
            return null;
        }

        public object? get_property_value(string path) {
            if (Properties.TryGetValue(path, out var property)) {
                return property.Value;
            }
            throw new Exception($"{path} is not defined in properties.");
        }

        public void set_property_value(string path, object? value) {
            if (Properties.TryGetValue(path, out var property)) {
                property.Value = value;
            }
            throw new Exception($"{path} is not defined in properties.");
        }

        /// <summary>
        /// Copies all properties under the source path into the appopriate desitnation path properties. <br/>
        /// The source and destination are identified by their partial paths.
        /// If sourcepath is "gameTime" and destinationpath is "time", the properties are copied like this: <br/>
        /// gameTime.seconds -> time.seconds <br/>
        /// gameTime.minutes -> time.minutes <br/>
        /// gameTime.hours -> time.hours <br/>
        /// Assuming that "time.seconds", "time.minutes" and "time.hours" are defined properties. <br/>
        /// If the calculated destination path does not exist, it is skipped. In the above example "gameTime.frames" 
        /// will not be copied.
        /// </summary>
        /// <param name="sourcePath">
        /// The partial path of the source properties to copy. 
        /// </param>
        /// <param name="destinationPath">
        /// The partial path that the source properties will be copied to. 
        /// </param>
        /// <remarks>
        /// This API is not used internally by PokeAByte, but may be invoked by the javascript of some mappers. <br/>
        /// </remarks>
        public void copy_properties(string sourcePath, string destinationPath)
        {
            var pathSpan = destinationPath.AsSpan();
            int destPathLength = destinationPath.Length;

            foreach (var key in Properties.Keys.Where(key => key.AsSpan().StartsWith(destinationPath)))
            {
                var property = Properties[key] as PokeAByteProperty;
                if (property == null)
                {
                    continue;
                }
                var sourceKey = string.Concat(sourcePath, key.Substring(destPathLength));

                if (Properties.TryGetValue(sourceKey, out var source))
                {
                    property.MemoryContainer = source.MemoryContainer;
                    property.Address = source.Address;
                    property.Length = source.Length;
                    property.Size = source.Size;
                    property.Bits = source.Bits;
                    property.Reference = source.Reference;
                    property.Bytes = source.Bytes;
                    property.Value = source.Value;
                }
            }
        }
    }
}
