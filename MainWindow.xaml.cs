using Microsoft.Win32;
using System.IO;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;


namespace EdgeFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string file;
        private string fileName;
        private string fileParentName;

        private Bitmap sourcePrewitt;
        private Bitmap sourceSobel;
        public Bitmap resultPrewitt;
        public Bitmap resultSobel;

        Boolean donePre = false;
        Boolean doneSob = false;

        // 画像を選びなおすときの初期化
        public void Init()
        {
            donePre = false;
            doneSob = false;

            sourcePrewitt.Dispose();
            sourceSobel.Dispose();
            resultPrewitt.Dispose();
            resultSobel.Dispose();

            this.Prewitt_Button.IsChecked = false;
            this.Sobel_Button.IsChecked = false;

            this.Save_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/baseline_save_alt_white_36dp.png"));
        }

        public void Button_Click_Open_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "general picture files|*.jpg;*.jpeg;*.png;*.bmp|other picture files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (openFileDialog1.ShowDialog() == true)
            {
                // 画像を選びなおすとき
                if (file != null) Init();
                try
                {
                    // 選択したファイルをメモリにコピーする
                    MemoryStream ms = new MemoryStream();
                    using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open))
                    {
                        fs.CopyTo(ms);      // FileStreamの内容をメモリストリームにコピー
                        file = openFileDialog1.FileName;
                        fileName = Path.GetFileNameWithoutExtension(file);
                        // 親ディレクトリ名 (フォルダ名) を取得する
                        fileParentName = Path.GetDirectoryName(@file);

                    }

                    // ストリームの位置をリセット
                    ms.Seek(0, SeekOrigin.Begin);

                    // ストリームをもとにBitmapImageを作成
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();            // BitmapImage の初期化の開始を通知
                    bmp.StreamSource = ms;      // BitmapImage のストリーム ソースを設定
                    bmp.EndInit();              // BitmapImage の初期化の終了を通知

                    // BitmapImageをSourceに指定して画面に表示する
                    this.Main_Image.Source = bmp;
                    ReadImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public void Button_Click_Prewitt(object sender, EventArgs e)
        {
            if (sourcePrewitt != null)  // 画像を選択済み
            {
                if (this.Prewitt_Button.IsChecked == true)
                {
                    this.Sobel_Button.IsChecked = false;
                    if (!donePre)  // Prewittフィルタ未実行
                    {
                        donePre = true;
                        // CreateGrayscaleImage(sourcePrewitt, resultPrewitt);  // グレイスケール化
                        Prewitt.FilterPrewitt(resultPrewitt);  // Prewittを実行
                        DrawImage(resultPrewitt);
                    }
                    else  // 実行済み
                    {
                        DrawImage(resultPrewitt);
                    }
                }
                else
                {
                    DrawImage(sourcePrewitt);
                }
            }
            else
            {
                MessageBox.Show("Please Select picture.");
            }


        }

        public void Button_Click_Sobel(object sender, EventArgs e)
        {
            if (sourceSobel != null)
            {
                if (this.Sobel_Button.IsChecked == true)
                {
                    this.Prewitt_Button.IsChecked = false;
                    if (!doneSob) // Sobelフィルタ未実行
                    {
                        doneSob = true;
                        // CreateGrayscaleImage(sourceSobel, resultSobel);  // グレイスケール化
                        Sobel.FilterSobel(resultSobel);  // Sobelを実行
                        DrawImage(resultSobel);
                    }
                    else  // 実行済み
                    {
                        DrawImage(resultSobel);
                    }
                }
                else
                {
                    DrawImage(sourceSobel);
                }

            }
            else
            {
                MessageBox.Show("Please Select picture.");
            }
        }

        public void Button_Click_Save(object sender, EventArgs e)
        {
            if (donePre && doneSob)
            {
                try
                {
                    resultPrewitt.Save(fileParentName + "\\" + fileName + "_prewitt.bmp",
                        System.Drawing.Imaging.ImageFormat.Bmp);
                    resultSobel.Save(fileParentName + "\\" + fileName + "_sobel.bmp",
                        System.Drawing.Imaging.ImageFormat.Bmp);
                    this.Save_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/baseline_download_done_white_36dp.png"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please Push Prewitt Filter and Sobel Filter.");
            }
        }

        // 画像の読み込み
        private void ReadImage()
        {

            // 画像ファイルをBitmap型として読み込む
            sourcePrewitt = (Bitmap)System.Drawing.Image.FromFile(@file);
            sourceSobel = (Bitmap)System.Drawing.Image.FromFile(@file);

            // 8bit Bitmap を用意
            resultPrewitt = new Bitmap(sourcePrewitt.Width, sourcePrewitt.Height, PixelFormat.Format8bppIndexed);
            CreateGrayscaleImage(sourcePrewitt, resultPrewitt);  // グレイスケール化
            resultSobel = new Bitmap(sourceSobel.Width, sourceSobel.Height, PixelFormat.Format8bppIndexed);
            CreateGrayscaleImage(sourceSobel, resultSobel);  // グレイスケール化

        }

        // 8bitグレイスケールへ変換する　(元のimage, 変換先image)
        public void CreateGrayscaleImage(Bitmap img, Bitmap grayBmp)
        {
            PixelFormat pixelFormat = PixelFormat.Format8bppIndexed;
            //int pixelSize = 1;

            // グレースケールになるようにカラーパレットをセット
            ColorPalette pal = grayBmp.Palette;
            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            grayBmp.Palette = pal;

            // BMPデータをとる(grayBmp固定)
            BitmapData grayBmpData = grayBmp.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, pixelFormat);

            // grayBmpData のピクセルデータの先頭位置
            IntPtr ptr = grayBmpData.Scan0;
            // ピクセルデータの大きさの配列
            byte[] pixels = new byte[grayBmpData.Stride * grayBmpData.Height];

            // grayBmpData -> pixels にコピー
            // Marshal.Copy(コピー元のメモリポインタ, コピー先の配列, 
            //              Copyを開始する配列内の０から始まるインデックス, コピーする配列要素の数);
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);

            for (int y = 0; y < grayBmpData.Height; y++)
            {
                for (int x = 0; x < grayBmpData.Width; x++)
                {
                    // 元imageからピクセルのカラー情報を取り出す
                    Color imgColor = img.GetPixel(x, y);
                    int grayscale = (imgColor.R + imgColor.G + imgColor.B) / 3;
                    // グレイスケールのカラーパレットからデータをセット
                    pixels[grayBmpData.Stride * y + x] = (byte)grayscale;
                }
            }
            // pixels -> grayBmpData にコピー
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);

            grayBmp.UnlockBits(grayBmpData);

            // await Task.Delay(10);
            // DrawImage(grayBmp);
        }

        // 画面に描写
        private void DrawImage(Bitmap bitmap)
        {
            this.Main_Image.Source = ToBitmapImage(bitmap);
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
