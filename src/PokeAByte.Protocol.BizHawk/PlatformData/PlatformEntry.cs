namespace PokeAByte.Protocol.BizHawk.PlatformData;

internal class PlatformEntry
{
    internal string SystemId { get; }

    /// <summary>
    /// The default number of frames to skip for the platform when updating the memory mapped file.
    /// This is used when the setup instruction from the client does not specify a frameskip.
    /// </summary>
    internal int FrameSkipDefault { get; }
    internal DomainLayout[] Domains { get; }

    public PlatformEntry(string systemId, DomainLayout[] domains, int frameSkipDefault = 0)
    {
        SystemId = systemId;
        FrameSkipDefault = frameSkipDefault;
        Domains = domains;
    }
}