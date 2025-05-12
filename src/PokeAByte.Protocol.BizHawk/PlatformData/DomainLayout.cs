namespace PokeAByte.Protocol.BizHawk.PlatformData;

internal readonly struct DomainLayout
{
    public readonly string DomainId;
    public readonly long Start;
    public readonly int Length;
    public readonly long End;

    public DomainLayout(string domain, long start, int length)
    {
        DomainId = domain;
        Start = start;
        Length = length;
        End = start + length;
    }
}