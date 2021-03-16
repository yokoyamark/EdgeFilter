using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EdgeFilter
{
    static class Prewitt
    {
        static readonly int[,] H = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
        static readonly int[,] V = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

        public static void FilterPrewitt(Bitmap img)
        {

            PixelFormat pixelFormat = PixelFormat.Format8bppIndexed;

            BitmapData prewittBmpData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, pixelFormat);

            // prewittBmpData のピクセルデータの先頭位置
            IntPtr ptr = prewittBmpData.Scan0;
            // ピクセルデータの大きさの配列
            byte[] pixels = new byte[prewittBmpData.Stride * prewittBmpData.Height];
            // エッジフィルタ実行後のピクセルデータのbyte配列
            byte[] resultPixels = new byte[prewittBmpData.Stride * prewittBmpData.Height];

            // prewittBmpData -> pixels にコピー
            // Marshal.Copy(コピー元のメモリポインタ, コピー先の配列, 
            //              Copyを開始する配列内の０から始まるインデックス, コピーする配列要素の数);
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);


            for (int y = 0; y < prewittBmpData.Height; y++)
            {
                for (int x = 0; x < prewittBmpData.Width; x++)
                {
                    int sumH = 0;
                    int sumV = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int i_y = y + i - 1;
                            int j_x = x + j - 1;
                            if (i_y <= 0) i_y = 0;
                            if (i_y >= prewittBmpData.Height) i_y = prewittBmpData.Height - 1;
                            if (j_x <= 0) j_x = 0;
                            if (j_x >= prewittBmpData.Width) j_x = prewittBmpData.Width - 1;

                            sumH += H[i, j] * pixels[prewittBmpData.Stride * i_y + j_x];

                            sumV += V[i, j] * pixels[prewittBmpData.Stride * i_y + j_x];

                        }
                    }
                    //if (x == 8 && y == 8) { MessageBox.Show(pixels[prewittBmpData.Stride * y + x].ToString()); }
                    resultPixels[prewittBmpData.Stride * y + x] = (byte)Math.Sqrt(Math.Pow(sumH, 2) + Math.Pow(sumV, 2));
                }
            }
            // resultPixels -> prewittBmpData にコピー
            System.Runtime.InteropServices.Marshal.Copy(resultPixels, 0, ptr, pixels.Length);

            img.UnlockBits(prewittBmpData);
        }
    }
}
