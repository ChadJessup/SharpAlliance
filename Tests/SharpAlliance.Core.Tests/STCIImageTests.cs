using SharpAlliance.Core.Managers.Image;
using Xunit;

namespace SharpAlliance.Core.Tests
{
    public class STCIImageTests
    {
        [Fact]
        public void TestEtrleDecompress_AllZeroes()
        {
            var imageLoader = new STCIImageFileLoader();
            byte COMPRESSED_FLAG = STCIImageFileLoader.COMPRESSED_FLAG;
            byte MAX_COMPR_BYTES = STCIImageFileLoader.MAX_COMPR_BYTES;

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { (byte)(COMPRESSED_FLAG | 0x02) }), new byte[] { 0b00, 0b00 });
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { (byte)(COMPRESSED_FLAG | 0x02), (byte)(COMPRESSED_FLAG | 0x02) }), new byte[] { 0b00, 0b00, 0b00, 0b00 });

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { (byte)(COMPRESSED_FLAG | 0x0F) }), new byte[15]);
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { (byte)(COMPRESSED_FLAG | 0x0F), (byte)(COMPRESSED_FLAG | 0x0F) }), new byte[30]);

            byte max_byte = (byte)(COMPRESSED_FLAG | MAX_COMPR_BYTES);
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { max_byte }), new byte[MAX_COMPR_BYTES]);
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { max_byte, max_byte }), new byte[2 * MAX_COMPR_BYTES]);
        }

        [Fact]
        public void TestEtrleDecompress_Data()
        {
            var imageLoader = new STCIImageFileLoader();

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02, 0x03 }), new byte[] { 0x02, 0x03 });
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x05, 0x02, 0x03, 0x04, 0x05, 0x06 }), new byte[] { 0x02, 0x03, 0x04, 0x05, 0x06 });

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02, 0x03, 0x02, 0x02, 0x03 }), new byte[] { 0x02, 0x03, 0x02, 0x03 });
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02, 0x03, 0x03, 0x04, 0x05, 0x06 }), new byte[] { 0x02, 0x03, 0x04, 0x05, 0x06 });
        }

        [Fact]
        public void TestEtrleDecompress_Mixed()
        {
            var imageLoader = new STCIImageFileLoader();
            byte COMPRESSED_FLAG = STCIImageFileLoader.COMPRESSED_FLAG;

            byte two_zero_bytes = (byte)(COMPRESSED_FLAG | 0x02);

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02, 0x03, two_zero_bytes }), new byte[] { 0x02, 0x03, 0x00, 0x00 });
            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { two_zero_bytes, 0x02, 0x02, 0x03 }), new byte[] { 0x00, 0x00, 0x02, 0x03 });

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { two_zero_bytes, 0x02, 0x02, 0x03, two_zero_bytes }),
                new byte[] { 0x00, 0x00, 0x02, 0x03, 0x00, 0x00 });

            Assert.Equal(imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02, 0x03, two_zero_bytes, 0x02, 0x02, 0x03 }),
                new byte[] { 0x02, 0x03, 0x00, 0x00, 0x02, 0x03 });
        }

        [Fact]
        public void TestEtrleDecompress_NotEnoughData()
        {
            var imageLoader = new STCIImageFileLoader();
            imageLoader.DecompressETRLEBytes(new byte[] { 0x02, 0x02 });
        }
    }
}
