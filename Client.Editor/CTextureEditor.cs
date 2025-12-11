using Client.Data.OZB;
using Client.Data.Texture;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;

namespace Client.Editor
{
    public partial class CTextureEditor : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextureData Data { get; private set; }

        public CTextureEditor()
        {
            InitializeComponent();
            pictureBox1.BackColor = Color.White;
        }

        public async void Init(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            switch (ext)
            {
                case ".ozj":
                    {
                        var reader = new OZJReader();
                        Data = await reader.Load(filePath);
                        SetData();
                    }
                    break;
                case ".ozt":
                    {
                        var reader = new OZTReader();
                        Data = await reader.Load(filePath);
                        SetData();
                    }
                    break;
                case ".ozb":
                    {
                        var reader = new OZBReader();
                        var texture = await reader.Load(filePath);
                        Data = new TextureData
                        {
                            Components = 4,
                            Width = texture.Width,
                            Height = texture.Height,
                            Data = texture.Data.SelectMany(x => new byte[] { x.R, x.G, x.B, x.A }).ToArray()
                        };
                    }
                    break;
                case ".ozd":
                    {
                        var reader = new OZDReader();
                        Data = await reader.Load(filePath);
                        SetData();
                    }
                    break;
                case ".ozp":
                    {
                        var reader = new OZPReader();
                        Data = await reader.Load(filePath);
                        SetData();
                    }
                    break;
                case ".ozg":
                    {
                        var reader = new OZGReader();
                        var texture = await reader.Load(filePath);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Extension {ext} not supported");
            }
        }

        public void SetData()
        {
            var textureData = Data;

            if (textureData.IsCompressed)
            {
                textureData.Data = DecompressDXT3(textureData.Width, textureData.Height, textureData.Data);
                textureData.IsCompressed = false;
                textureData.Components = 4;
            }

            var bitmap = new Bitmap((int)textureData.Width, (int)textureData.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < textureData.Height; y++)
            {
                for (int x = 0; x < textureData.Width; x++)
                {
                    int index = (y * (int)textureData.Width + x) * textureData.Components;

                    byte r = textureData.Data[index];
                    byte g = textureData.Data[index + 1];
                    byte b = textureData.Data[index + 2];
                    byte a = (textureData.Components == 4) ? textureData.Data[index + 3] : (byte)255; // Si son RGB, se asume A = 255

                    Color color = Color.FromArgb(a, r, g, b);

                    bitmap.SetPixel(x, y, color);
                }
            }

            pictureBox1.Image = bitmap;
        }

        public void Export()
        {
            var bitmap = (Bitmap)pictureBox1.Image;
            using (var sfd = new SaveFileDialog())
            {
                var isPng = Data.Components == 4;
                sfd.Filter = isPng ? "PNG|*.png" : "JPG|*.jpg";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    bitmap.Save(sfd.FileName, isPng ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
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
