namespace GameHook.Domain.Interfaces
{
    public record PropertyAttributes
    {
        public required string Path { get; init; }

        public required string Type { get; init; }
        public string? MemoryContainer { get; init; }
        public string? Address { get; init; }
        public int? Length { get; init; } = 1;
        public int? Size { get; init; }
        public string? Bits { get; set; }
        public string? Reference { get; set; }
        public string? Description { get; set; }

        public string? Value { get; set; }

        public string? ReadFunction { get; set; }
        public string? WriteFunction { get; set; }

        public string? AfterReadValueExpression { get; set; }
        public string? AfterReadValueFunction { get; set; }

        public string? BeforeWriteValueFunction { get; set; }
        //20240514 : From my understanding, GBA pokemon games use IWRAM to store
        //a pointer to a block of data held within EWRAM. It seems EWRAM is volatile 
        //and gets changed during transitions. Because of this we have to work with 
        //two different addresses. I do not want to cause unintended side effects 
        //by using one of the already created properties so I am making my own called 'PointerAddress'
        //this keeps track of the IWRAM address without modifying the base address which should
        //be overwritten in the .js file -Andrew
        public string? PointerAddress { get; set; }
        public int? PointerAddressOffset { get; set; }
    }

    public interface IGameHookProperty
    {
        string Path { get; }

        string Type { get; }
        string? MemoryContainer { get; }
        uint? Address { get; }
        //Please refer to PropertyAttributes.PointerAddress comment -Andrew
        uint? PointerAddress { get; set; }
        int? PointerAddressOffset { get; set; }
        int? Length { get; }
        int? Size { get; }
        string? Reference { get; }
        string? Bits { get; }
        string? Description { get; }
        object? Value { get; set; }
        byte[]? Bytes { get; }
        byte[]? BytesFrozen { get; }

        bool IsFrozen { get; }
        bool IsReadOnly { get; }

        HashSet<string> FieldsChanged { get; }

        void ProcessLoop(IMemoryManager container);

        Task WriteValue(string value, bool? freeze);
        Task WriteBytes(byte[] bytes, bool? freeze);

        Task FreezeProperty(byte[] bytesFrozen);
        Task UnfreezeProperty();
    }
}
