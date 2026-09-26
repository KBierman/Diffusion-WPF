using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Diffusion
{
    public partial class MainWindow : Window
    {

        private int width = 50;
        private int height = 50;
        private double tickCounterValue = 0;
        private double tickCounterInterval = .25;
        private double[,] gridA, gridB;
        private double[,] nextA, nextB;

        // Gray-Scott parameters
        private double dA = 1.0;
        private double dB = 0.5;
        private double feed = 0.055;
        private double kill = 0.062;
        private double dt = 1.0;
        private readonly DispatcherTimer _timer;

        DiffusionBasics diffusion = new DiffusionBasics();
        public MainWindow()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(tickCounterInterval);
            _timer.Tick += Timer_Tick;
            gridA = new double[width, height];
            gridB = new double[width, height];
            nextA = new double[width, height];
            nextB = new double[width, height];
            InitializeComponent();
            for (int i = 0; i < width; i++)
            {
                ScreenGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ScreenGrid.Width / width) });
            }
            for (int i = 0; i < height; i++)
            {
                ScreenGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ScreenGrid.Height / height) });
            }
            ResetGrid();
            _timer.Start();
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        public void ResetGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridA[x, y] = 1.0;
                    gridB[x, y] = 0.0;
                }
            }

            int cx = width / 2;
            int cy = height / 2;
            for (int x = cx - 5; x < cx + 5; x++)
            {
                for (int y = cy - 5; y < cy + 5; y++)
                {
                    gridB[x, y] = 1.0;
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double a = gridA[i, j];
                    double b = gridB[i, j];
                    byte r = (byte)(Math.Clamp((a - b) * 255, 0, 255));
                    Border border = new Border()
                    {
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, 200, 200))
                    };
                    int tempI = i;
                    int tempJ = j;
                    border.MouseLeftButtonDown += (sender, e) => addReactant(tempI, tempJ);
                    ScreenGrid.Children.Add(border);

                    Grid.SetColumn(ScreenGrid.Children[ScreenGrid.Children.Count - 1], i);
                    Grid.SetRow(ScreenGrid.Children[ScreenGrid.Children.Count - 1], j);
                }
            }
        }

        public void addReactant(int x, int y)
        {
            gridB[x, y] = 1.0;
            ColorGrid();
        }

        public void ColorGrid()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double a = gridA[i, j];
                    double b = gridB[i, j];
                    byte r = (byte)(Math.Clamp((a - b) * 255, 0, 255));
                    ScreenGrid.Children[(width * i + j)].SetValue(Border.BackgroundProperty, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, 200, 200)));
                }
            }
        }

        public void Update()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double a = gridA[x, y];
                    double b = gridB[x, y];

                    double abb = a * b * b;

                    nextA[x, y] = a + (dA * diffusion.Laplacian(x, y, gridA, width, height) - abb + feed * (1.0 - a)) * dt;
                    nextB[x, y] = b + (dB * diffusion.Laplacian(x, y, gridB, width, height) + abb - (kill + feed) * b) * dt;
                }
            }

            double[,] tempA = gridA; gridA = nextA; nextA = tempA;
            double[,] tempB = gridB; gridB = nextB; nextB = tempB;
            tickCounterValue += tickCounterInterval;
            tickCounter.Dispatcher.Invoke(() => tickCounter.Content = "Tick: " + tickCounterValue.ToString());
            ColorGrid();
        }

    }
}