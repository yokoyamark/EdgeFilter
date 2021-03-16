using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EdgeFilter
{
    static class Sobel
    {
        static readonly int[,] H = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        static readonly int[,] V = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        public static void FilterSobel(Bitmap img)
        {

            PixelFormat pixelFormat = PixelFormat.Format8bppIndexed;

            BitmapData sobelBmpData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, pixelFormat);

            // sobelBmpData のピクセルデータの先頭位置
            IntPtr ptr = sobelBmpData.Scan0;
            // ピクセルデータの大きさの配列
            byte[] pixels = new byte[sobelBmpData.Stride * sobelBmpData.Height];
            // エッジフィルタ実行後のピクセルデータのbyte配列
            byte[] resultPixels = new byte[sobelBmpData.Stride * sobelBmpData.Height];

            // sobelBmpData -> pixels にコピー
            // Marshal.Copy(コピー元のメモリポインタ, コピー先の配列, 
            //              Copyを開始する配列内の０から始まるインデックス, コピーする配列要素の数);
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);


            for (int y = 0; y < sobelBmpData.Height; y++)
            {
                for (int x = 0; x < sobelBmpData.Width; x++)
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
                            if (i_y >= sobelBmpData.Height) i_y = sobelBmpData.Height - 1;
                            if (j_x <= 0) j_x = 0;
                            if (j_x >= sobelBmpData.Width) j_x = sobelBmpData.Width - 1;

                            sumH += H[i, j] * pixels[sobelBmpData.Stride * i_y + j_x];

                            sumV += V[i, j] * pixels[sobelBmpData.Stride * i_y + j_x];

                        }
                    }
                    //if (x == 8 && y == 8) { MessageBox.Show(pixels[sobelBmpData.Stride * y + x].ToString()); }
                    resultPixels[sobelBmpData.Stride * y + x] = (byte)Math.Sqrt(Math.Pow(sumH, 2) + Math.Pow(sumV, 2));
                }
            }
            // resultPixels -> sobelBmpData にコピー
            System.Runtime.InteropServices.Marshal.Copy(resultPixels, 0, ptr, pixels.Length);

            img.UnlockBits(sobelBmpData);
        }
    }
}
