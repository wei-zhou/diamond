namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class RandomGenerator : IDisposable
    {
        private readonly RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        public byte NextByte()
        {
            throw new NotImplementedException();
        }

        public byte NextByte(byte min, byte max)
        {
            throw new NotImplementedException();
        }

        public int NextInt32()
        {
            throw new NotImplementedException();
        }

        public int NextInt32(int min, int max)
        {
            throw new NotImplementedException();
        }

        public decimal NextDecimal()
        {
            throw new NotImplementedException();
        }

        public decimal NextDecimal(decimal min, decimal max)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.provider.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
