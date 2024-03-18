using GameHook.Domain.Interfaces;

namespace GameHook.Domain.GameHookProperties
{
    public class BinaryCodedDecimalProperty : GameHookProperty, IGameHookProperty
    {
        public BinaryCodedDecimalProperty(IGameHookInstance instance, PropertyAttributes variables) : base(instance, variables)
        {
        }

        protected override byte[] FromValue(string value)
        {
            throw new NotImplementedException();
        }

        protected override object? ToValue(byte[] data)
        {
            int result = 0;

            foreach (byte bcd in data)
            {
                result *= 100;
                result += 10 * (bcd >> 4);
                result += bcd & 0xf;
            }

            return result;
        }
    }
}
