using System.Xml.Serialization;

namespace PokeAByte.Domain.Models;

public record MemoryContract
{
    public long MemoryAddressStart { get; set; } = 0x0L;
    public int DataLength { get; set; } = 0;
    public string DataType => typeof(byte[]).Name;
    public byte[]? Data { get; set; }
    public string BizHawkIdentifier { get; set; } = "";

    public byte[] Serialize()
    {
        var xmlSerializer = new XmlSerializer(typeof(MemoryContract));
        using var memoryStream = new MemoryStream();
        xmlSerializer.Serialize(memoryStream, this);
        return memoryStream.ToArray();
    }

    public static MemoryContract? Deserialize(byte[] data)
    {
        var xmlSerializer = new XmlSerializer(typeof(MemoryContract));
        var memoryStream = new MemoryStream(data);
        return (MemoryContract?)xmlSerializer.Deserialize(memoryStream);
    }
}
