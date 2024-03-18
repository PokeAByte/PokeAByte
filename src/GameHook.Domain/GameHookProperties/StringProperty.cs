using GameHook.Domain.Interfaces;

namespace GameHook.Domain.GameHookProperties
{
    public class StringProperty : GameHookProperty, IGameHookProperty
    {
        public StringProperty(IGameHookInstance instance, PropertyAttributes variables) : base(instance, variables)
        {
            if (Reference == null)
            {
                Reference = "defaultCharacterMap";
            }
        }

        protected override byte[] FromValue(string value)
        {
            if (ComputedReference == null) throw new Exception("ReferenceObject is NULL.");
            if (Length == null) throw new Exception("Length is NULL.");

            var uints = value
                .Select(x => ComputedReference.Values.FirstOrDefault(y => x.ToString() == y?.Value?.ToString()))
                .ToList();

            if (uints.Count + 1 > Length)
            {
                uints = uints.Take(Length ?? 0 - 1).ToList();
            }

            var nullTerminationKey = ComputedReference.Values.First(x => x.Value == null);
            uints.Add(nullTerminationKey);

            return uints
                .Select(x =>
                {
                    if (x?.Value == null)
                    {
                        return nullTerminationKey.Key;
                    }

                    return x.Key;
                })
                .Select(x => (byte)x)
                .ToArray();
        }

        protected override object? ToValue(byte[] data)
        {
            if (Instance == null) throw new Exception("Instance is NULL.");
            if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");
            if (ComputedReference == null) { throw new Exception("ReferenceObject is NULL."); }

            string?[] results = Array.Empty<string>();

            if (Size > 1)
            {
                // For strings that have characters mapper to more than a single byte.
                results = data.Chunk(Size ?? 1).Select(b =>
                {
                    var value = b.ReverseBytesIfBE(Instance.PlatformOptions.EndianType).get_ulong_be();

                    var referenceItem = ComputedReference.Values.SingleOrDefault(x => x.Key == value);
                    return referenceItem?.Value?.ToString() ?? null;
                }).ToArray();
            }
            else
            {
                // For strings where one character equals one byte.
                results = data.Select(b =>
                {
                    var referenceItem = ComputedReference.Values.SingleOrDefault(x => x.Key == b);
                    return referenceItem?.Value?.ToString() ?? null;
                }).ToArray();
            }

            // Return the completed string buffer.
            return string.Join(string.Empty, results.TakeWhile(s => s != null));
        }
    }
}
