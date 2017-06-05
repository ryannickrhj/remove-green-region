using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemovingGreenRegion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class ImageAnalyzer
    {
        public ImageAnalyzer()
        {
        }

        public static Bitmap getRemoveGreenBitmap(Bitmap inputBitmap)
        {
            Bitmap bitmap = new Bitmap(inputBitmap);
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride;
            int pixcelDepth = 4;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int r = ptr[(x * pixcelDepth) + y * stride + 0];
                        int g = ptr[(x * pixcelDepth) + y * stride + 1];
                        int b = ptr[(x * pixcelDepth) + y * stride + 2];
                        int max = Math.Max(Math.Max(r, g), b);
                        int min = Math.Min(Math.Min(r, g), b);
                        if (g != min && (g == max || max - g < 8) && (max - min) > 96)
                        {
                            ptr[(x * pixcelDepth) + y * stride + 3] = 0;
                        }
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
    }

    public partial class MainWindow : Window
    {
        Bitmap beforeBitmap, afterBitmap;
        ImageAnalyzer imageAnalyzer = new ImageAnalyzer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Windows.Point windowPosition = PointToScreen(new System.Windows.Point(0, 0));
            System.Windows.Point beforeImageBorderPosition = beforeImageBorder.PointToScreen(new System.Windows.Point(0, 0));
            double margin = 20, leftMargin = 20, rightMargin = 30, topMargin = 50, bottonMargin = 60;
            double width = (e.NewSize.Width - leftMargin - margin - rightMargin) / 2;
            double height = (e.NewSize.Height - topMargin - bottonMargin);

            beforeImageBorder.Margin = new Thickness(leftMargin, topMargin, 0, 0);
            beforeImageBorder.Width = width;
            beforeImageBorder.Height = height;

            afterImageBorder.Margin = new Thickness(leftMargin + width + margin, topMargin, 0, 0);
            afterImageBorder.Width = width;
            afterImageBorder.Height = height;
        }

        private void openImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.tif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.tif";
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                beforeBitmap = null;
                afterBitmap = null;
                beforeImage.Source = null;
                afterImage.Source = null;
                GC.Collect();
                string imagePath = openFileDialog.FileName;
                beforeBitmap = new Bitmap(System.Drawing.Image.FromFile(imagePath));
                afterBitmap = ImageAnalyzer.getRemoveGreenBitmap(beforeBitmap);
                beforeImage.Source = getBitmapSourceFromBitmap(beforeBitmap);
                afterImage.Source = getBitmapSourceFromBitmap(afterBitmap);
                saveImageButton.IsEnabled = true;
            }
        }

        private void saveImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Image files (*.png) | *.png";
            saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (saveFileDialog.ShowDialog() == true)
            {
                afterBitmap.Save(saveFileDialog.FileName);
            }
        }

        BitmapSource getBitmapSourceFromBitmap(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
        }
    }
}
