using Microsoft.Xna.Framework.Graphics;


namespace Client.Data.Texture
{
    public class OZDReader : BaseReader<TextureData>
    {
        public class DecoderSettings
        {
            public bool UseParallel { get; set; } = false;
        }

        public OZDReader() { }

        protected override TextureData Read(byte[] buffer)
        {
            buffer = ModulusCryptor.ModulusCryptor.Decrypt(buffer);
            if (buffer[0] == 'D' && buffer[1] == 'D' && buffer[2] == 'S' && buffer[3] == ' ')
                return ReadDDS(buffer);

            throw new ApplicationException($"Invalid OZD file");
        }

        private TextureData ReadDDS(byte[] buffer)
        {
            var header = buffer.AsSpan(0, 128);
            using var br = new BinaryReader(new MemoryStream(header.ToArray()));

            var signature = br.ReadString(4);
            var headerSize = br.ReadInt32();
            var flags = br.ReadInt32();
            var height = br.ReadInt32();
            var width = br.ReadInt32();
            var pitchOrLinearSize = br.ReadInt32();
            var depth = br.ReadInt32();
            var mipMapCount = br.ReadInt32();

            br.BaseStream.Seek(84, SeekOrigin.Begin);
            var pixelFormat = br.ReadString(4);

            SurfaceFormat surfaceFormat = pixelFormat switch
            {
                "DXT1" => SurfaceFormat.Dxt1,
                "DXT3" => SurfaceFormat.Dxt3,
                "DXT5" => SurfaceFormat.Dxt5,
                _ => throw new ApplicationException($"Unsupported DDS format: {pixelFormat}")
            };

            var compressedData = buffer.AsSpan(128).ToArray();

            return new TextureData
            {
                Width = width,
                Height = height,
                Format = surfaceFormat,
                IsCompressed = false,
                Data = DecompressDXT3(width, height, compressedData),
                Components = 4
            };
        }

        public byte[] DecompressDXT3(int width, int height, byte[] data)
        {
            int blockCountX = width / 4;
            int blockCountY = height / 4;

            byte[] output = new byte[width * height * 4]; // RGBA8

            int srcOffset = 0;

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    // ---- READ ALPHA BLOCK (8 bytes) ----
                    ulong alphaBlock = BitConverter.ToUInt64(data, srcOffset);
                    srcOffset += 8;

                    // ---- READ COLOR BLOCK ----
                    ushort color0 = BitConverter.ToUInt16(data, srcOffset);
                    ushort color1 = BitConverter.ToUInt16(data, srcOffset + 2);
                    uint colorBits = BitConverter.ToUInt32(data, srcOffset + 4);
                    srcOffset += 8;

                    // Decode the 2 base colors (RGB565 → RGB888)
                    uint c0 = RGB565ToRGBA(color0);
                    uint c1 = RGB565ToRGBA(color1);

                    // Build the 4-color palette
                    uint[] palette = new uint[4];
                    palette[0] = c0;
                    palette[1] = c1;
                    palette[2] = LerpRGB(c0, c1, 2, 1); // 2/3 c0 + 1/3 c1
                    palette[3] = LerpRGB(c0, c1, 1, 2); // 1/3 c0 + 2/3 c1

                    // ---- DECOMPRESS 4×4 BLOCK ----
                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIdx = py * 4 + px;

                            // Alpha: 4 bits → 8 bits
                            int alpha4 = (int)((alphaBlock >> (pixelIdx * 4)) & 0xF);
                            byte alpha8 = (byte)(alpha4 * 17); // expand 0–15 → 0–255

                            // Color: 2-bit index
                            int colorIndex = (int)((colorBits >> (pixelIdx * 2)) & 3);
                            uint rgb = palette[colorIndex];

                            int dx = bx * 4 + px;
                            int dy = by * 4 + py;

                            int dst = (dy * width + dx) * 4;

                            // RGBA output
                            output[dst + 0] = (byte)((rgb >> 16) & 0xFF); // R
                            output[dst + 1] = (byte)((rgb >> 8) & 0xFF); // G
                            output[dst + 2] = (byte)(rgb & 0xFF); // B
                            output[dst + 3] = alpha8;                    // A
                        }
                    }
                }
            }

            return output;
        }

        private uint RGB565ToRGBA(ushort c)
        {
            int r = (c >> 11) & 31;
            int g = (c >> 5) & 63;
            int b = c & 31;

            // Convert 5 or 6 bits → 8 bits
            r = (r * 527 + 23) >> 6;
            g = (g * 259 + 33) >> 6;
            b = (b * 527 + 23) >> 6;

            return (uint)(0xFF000000 | (r << 16) | (g << 8) | b);
        }

        private uint LerpRGB(uint c0, uint c1, int w0, int w1)
        {
            int r = (int)(((c0 >> 16) & 0xFF) * w0 + ((c1 >> 16) & 0xFF) * w1) / (w0 + w1);
            int g = (int)(((c0 >> 8) & 0xFF) * w0 + ((c1 >> 8) & 0xFF) * w1) / (w0 + w1);
            int b = (int)((c0 & 0xFF) * w0 + (c1 & 0xFF) * w1) / (w0 + w1);

            return (uint)(0xFF000000 | (r << 16) | (g << 8) | b);
        }

    }
}