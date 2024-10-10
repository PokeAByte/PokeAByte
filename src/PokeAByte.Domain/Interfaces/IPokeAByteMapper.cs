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
    /// Potential triggers for variable changes. These are evaluated when the calculated new value of the variable
    /// is different to the currently stored one, or on first evaluation of the variable.
    /// </summary>
    public enum VariableTrigger {
        /// <summary>
        /// Nothing happens on variable value change. Default.
        /// </summary>
        None,
        /// <summary>
        /// Sets the semi-static "reload_addresses" variable to <see langword="true"/>, causing PokeAByte to re-solved
        /// all addresses. For instance a property may have an address of <c>{base_ptr} + 0xAC</c>. 
        /// If the <c>base_ptr</c> variable changes, then the property address needs to be updated.
        /// </summary>
        ReloadAddresses
    }

    /// <summary>
    /// A mapper defined variable. These may be referenced in property addresses (<c>addresss="{dma_a} + 0xAC"</c>) and
    /// are exposed to the mapper JavaScript via the <c>__variables</c> global object. <br/>
    /// Variables are evaluated in order of their definition in the XML and may reference a previously defined variable
    /// as well: 
    /// <code> 
    /// &lt;variable name="dma_a" type="uint" size="4" address="0x3005D8C" trigger="reload_address"/&gt;
    /// &lt;variable name="player_id" type="uint" size="2" address="{dma_b} + 10" /&gt;
    /// </code>
    /// Variables are processed before the <c>preprocessor</c> script.
    /// </summary>
    /// <param name="Name"> The name of the variable. </param>
    /// <param name="Type"> 
    /// The type of the variable. <br/>
    /// Valid types are: <c>int</c>, <c>uint</c>, <c>bool</c>.
    /// </param>
    /// <param name="Size"> The size of the variable in bytes. E.g. how many bytes make of an <c>int</c>, etc. </param>
    /// <param name="Address"> The address expression for the variable. </param>
    /// <param name="Trigger"> Optional action trigger for when the value of the variable changes. </param>
    public record MapperVariable(
        string Name, 
        string Type, 
        int Size,
        string Address, 
        VariableTrigger Trigger
    );

    public interface IPokeAByteMapper : IDisposable
    {
        MetadataSection Metadata { get; }
        MemorySection Memory { get; }
        Dictionary<string, IPokeAByteProperty> Properties { get; }
        Dictionary<string, ReferenceItems> References { get; }
        public IList<MapperVariable> Variables { get; }

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
