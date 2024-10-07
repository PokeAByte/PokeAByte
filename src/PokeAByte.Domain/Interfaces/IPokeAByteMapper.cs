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

    /// <summary>
    /// Holds all the data relevant to a given mapper.
    /// </summary>
    /// <remarks>
    /// Exposed to the mapper JavaScript via the <c>__mapper</c> object.
    /// </remarks>
    public interface IPokeAByteMapper : IDisposable
    {
        /// <summary>
        /// Metadata identifiying the mapper.
        /// </summary>
        MetadataSection Metadata { get; }

        /// <summary>
        /// The raw memory sections that the mapper specified needing from the emulator. <br/>
        /// Defaults to an empty <see cref="MemorySection"/> with no read ranges. <br/>
        /// PokeAByte will default to the read ranges specified in the relevant <see cref="IPlatformOptions"/> 
        /// implementation in that case.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Not all driver implementations respect these read ranges.
        /// For this reason, this property should not be relied on in mapper JavaScript.
        /// </remarks>
        MemorySection Memory { get; }

        /// <summary>
        /// A dictionary holding the properties specified by the mapper. <br/>
        /// The dictionary key is the <see cref="IPokeAByteProperty.Path"/>.
        /// </summary>
        Dictionary<string, IPokeAByteProperty> Properties { get; }

        /// <summary>
        /// Provides a glossary to translate game specific IDs and dataytypes into more useful values. <br/>
        /// Each entry in the glossary is a collection of key-value pairs.
        /// </summary>
        /// <remarks>
        /// Mappers should make sure to always include a <c>defaultCharacterMap</c> glossary entry or specify
        /// a valid reference for every single <c>string</c> property. <br/>
        /// See also <see cref="IPokeAByteProperty.Reference"/>.
        /// </remarks>
        Dictionary<string, ReferenceItems> References { get; }

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
                if (property == null) {
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
