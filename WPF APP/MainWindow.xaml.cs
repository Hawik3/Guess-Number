using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
namespace GuessNumber
{
    public partial class MainWindow : Window
    {
        string apiAdress = "your api";




        WriteableBitmap image = new(28, 28, 96, 96, PixelFormats.Gray8, null);
        readonly Dictionary<string, Label> labelsName;
        byte[,] pixelsCollor = new byte[28, 28];
        readonly Label[,] pixels = new Label[28, 28];
        const byte width = 28;
        const byte height = 28;
        readonly SolidColorBrush BlackCollor = new(Color.FromRgb(0, 0, 0));
        readonly SolidColorBrush GrayCollor = new(Color.FromRgb(170, 170, 170));
        readonly SolidColorBrush WhiteCollor = new(Color.FromRgb(255, 255, 255));
        public MainWindow()
        {
            InitializeComponent();
            InitializeDrawingCanvas();
            labelsName = new()
            {
                {"num0", Value0 },
                {"num1", Value1 },
                {"num2", Value2 },
                {"num3", Value3 },
                {"num4", Value4 },
                {"num5", Value5 },
                {"num6", Value6 },
                {"num7", Value7 },
                {"num8", Value8 },
                {"num9", Value9 },


            };
        }
        private void InitializeDrawingCanvas()
        {
            #region Grid
            for (int i = 0; i < 28; i++)
            {
                RowDefinition row = new();
                CanvasGrid.RowDefinitions.Add(row);
            }
            for (int i = 0; i < 28; i++)
            {
                ColumnDefinition column = new();
                CanvasGrid.ColumnDefinitions.Add(column);
            }
            #endregion
            #region Labels
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    Label label = new()
                    {
                        Background = Brushes.Black
                    };
                    Grid.SetColumn(label, i);
                    Grid.SetRow(label, j);
                    label.MouseMove += DrawWithMouse;
                    CanvasGrid.Children.Add(label);
                    pixels[i, j] = label;
                    pixelsCollor[i, j] = 0;

                }
            }
            #endregion
        }
        private void UpdateGrid()
        {
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    switch (pixelsCollor[i, j])
                    {
                        case 0:
                            pixels[i, j].Background = BlackCollor;
                            break;
                        case 175:
                            pixels[i, j].Background = GrayCollor;
                            break;
                        case 255:
                            pixels[i, j].Background = WhiteCollor;
                            break;
                    }
                    pixels[i, j].Background = new SolidColorBrush(Color.FromRgb(pixelsCollor[i, j], pixelsCollor[i, j], pixelsCollor[i, j]));
                }
            }
            
        }
        private void DrawWithMouse(object sender, MouseEventArgs e)
        {
            #region Left Mouse  Button
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Label label = (Label)sender;
                int row = Grid.GetRow(label);
                int column = Grid.GetColumn(label);
                if (pixelsCollor[column, row] < 171)
                {
                    pixelsCollor[column, row] += 85;
                }
                else if ((column != 0) && (pixelsCollor[column - 1, row] < 171))
                {
                    pixelsCollor[column - 1, row] += 85;
                }
                else if ((column != 27) && (pixelsCollor[column + 1, row] < 171))
                {
                    pixelsCollor[column + 1, row] += 85;
                }
                else if ((row != 0) && (pixelsCollor[column, row - 1] < 171))
                {
                    pixelsCollor[column, row - 1] += 85;
                }
                else if ((row != 27) && (pixelsCollor[column, row + 1] < 171))
                {
                    pixelsCollor[column, row + 1] += 85;
                }

                UpdateGrid();
            }
            #endregion
            #region Right Mouse  Button
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Label label = (Label)sender;
                int row = Grid.GetRow(label);
                int column = Grid.GetColumn(label);
                if (pixelsCollor[column, row] > 84)
                {
                    pixelsCollor[column, row] -= 85;
                }
                else if ((column != 0) && (pixelsCollor[column - 1, row] > 84))
                {
                    pixelsCollor[column - 1, row] -= 85;
                }
                else if ((column != 27) && (pixelsCollor[column + 1, row] > 84))
                {
                    pixelsCollor[column + 1, row] -= 85;
                }
                else if ((row != 0) && (pixelsCollor[column, row - 1] > 84))
                {
                    pixelsCollor[column, row - 1] -= 85;
                }
                else if ((row != 27) && (pixelsCollor[column, row + 1] > 84))
                {
                    pixelsCollor[column, row + 1] -= 85;
                }

                UpdateGrid();
            }
            #endregion
        }
        private void ConvertToImage()
        {
            int stride = width * (image.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = pixelsCollor[x, y];
                    pixels[y * stride + x] = value;
                }
            }
            image.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

        }

        private async void GuessNumber()
        {
          
            HttpClient client = new HttpClient();
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(File.OpenRead("image.png"));
            formData.Add(fileContent, "picture", "image");
            var response = await client.PostAsync(apiAdress, formData);
            if (response.IsSuccessStatusCode)
            {
                List<KeyValuePair<string, float>> result = await response.Content.ReadAsAsync<List<KeyValuePair<string, float>>>();
              
                
                foreach (KeyValuePair<string, float> score in result)
                {
                    labelsName[score.Key].Width = score.Value * 200;
                }
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString(), "Error", MessageBoxButton.OK);
            }

        }

        private void GuessNumber_Click(object sender, RoutedEventArgs e)
        {
            ConvertToImage();
            BitmapEncoder encoder = new PngBitmapEncoder();
            BitmapFrame frame = BitmapFrame.Create(image);
            encoder.Frames.Add(frame);
            using (FileStream stream = new("image.png", FileMode.Create))
            {
                encoder.Save(stream);
            }

            GuessNumber();
        }

    }
}
