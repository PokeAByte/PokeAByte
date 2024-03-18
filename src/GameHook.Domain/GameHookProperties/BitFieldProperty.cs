using GameHook.Domain.Interfaces;
using System.Collections;

namespace GameHook.Domain.GameHookProperties
{
    public class BitFieldProperty : GameHookProperty, IGameHookProperty
    {
        public BitFieldProperty(IGameHookInstance instance, PropertyAttributes variables) : base(instance, variables)
        {
        }

        protected override byte[] FromValue(string value)
        {
            throw new NotImplementedException();
        }

        protected override object? ToValue(byte[] data)
        {
            var bitArray = new BitArray(data);

            var boolArray = new bool[bitArray.Length];
            bitArray.CopyTo(boolArray, 0);

            return boolArray;
        }
    }
}
