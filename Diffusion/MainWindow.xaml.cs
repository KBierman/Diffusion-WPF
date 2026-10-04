using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Diffusion
{
    public partial class MainWindow : Window
    {

        private int width = 100;
        private int height = 50;
        private double tickCounterValue = 0;
        private double tickCounterInterval = .25;
        private double[,] gridA, gridB;
        private double[,] nextA, nextB;
        private readonly DispatcherTimer _timer;
        public MainWindow()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(tickCounterInterval)
            };
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
            try
            {
                double.Parse(feedBox.Text);
                double.Parse(killBox.Text);
                Update();
                ColorGrid();
            }
            catch
            {
            }
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
                    ScreenGrid.Children[(height * i + j)].SetValue(Border.BackgroundProperty, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, 200, 200)));
                }
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
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
            tickCounterValue = 0;
            tickCounter.Dispatcher.Invoke(() => tickCounter.Content = "Tick: " + tickCounterValue.ToString());

            ColorGrid();
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                pauseButton.Content = "Start";
            }
            else
            {
                _timer.Start();
                pauseButton.Content = "Stop";
            }
        }
        private async Task LoadingAnimation(CancellationToken token)
        {
            string[] states =
            {
                "Loading",
                "Loading.",
                "Loading..",
                "Loading..."
            };

            int i = 0;

            while (true)
            {
                loadLabel.Content = states[i++ % states.Length];

                await Task.Delay(250, token);
            }
        }

        private async void sandButton_Click(object sender, RoutedEventArgs e)
        {
            bool timeContinue = _timer.IsEnabled;
            disableEnableButton();
            _timer.Stop();
            using var cts = new CancellationTokenSource();

            Task loadingTask = LoadingAnimation(cts.Token);
            double grains = double.Parse(sandBox.Text);
            double feed = double.Parse(feedBox.Text);
            double kill = double.Parse(killBox.Text);
            double DA = double.Parse(DABox.Text);
            double DB = double.Parse(DBBox.Text);
            double[,] tempA = gridA;
            double[,] tempB = gridB;
            double[,] nextA = new double[width, height];
            double[,] nextB = new double[width, height];
            Task sandTask = Task.Run(() =>
            {
                for (int i = 0; i < grains; i++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            double a = tempA[x, y];
                            double b = tempB[x, y];

                            double abb = a * b * b;

                            nextA[x, y] = a + (DA * Laplacian(x, y, tempA) - abb + feed * (1.0 - a)) * 1;
                            nextB[x, y] = b + (DB * Laplacian(x, y, tempB) + abb - (kill + feed) * b) * 1;
                        }
                    }

                    double[,] extraTempA = tempA; tempA = nextA; nextA = extraTempA;
                    double[,] extraTempB = tempB; tempB = nextB; nextB = extraTempB;
                    tickCounterValue += tickCounterInterval;
                }
            });
            await sandTask;
            cts.Cancel();
            try
            {
                await loadingTask;

            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                gridA = tempA;
                gridB = tempB;
                loadLabel.Content = "";
                tickCounter.Dispatcher.Invoke(() => tickCounter.Content = "Tick: " + tickCounterValue.ToString());
                ColorGrid();
                if (timeContinue)
                {
                    _timer.Start();
                }
                disableEnableButton();
            }
        }

        private void disableEnableButton()
        {
            pauseButton.IsEnabled = !pauseButton.IsEnabled;
            resetButton.IsEnabled = !resetButton.IsEnabled;
            sandButton.IsEnabled = !sandButton.IsEnabled;
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

                    nextA[x, y] = a + (double.Parse(DABox.Text) * Laplacian(x, y, gridA) - abb + double.Parse(feedBox.Text) * (1.0 - a)) * 1;
                    nextB[x, y] = b + (double.Parse(DBBox.Text) * Laplacian(x, y, gridB) + abb - (double.Parse(killBox.Text) + double.Parse(feedBox.Text)) * b) * 1;
                }
            }

            double[,] tempA = gridA; gridA = nextA; nextA = tempA;
            double[,] tempB = gridB; gridB = nextB; nextB = tempB;
            tickCounterValue += tickCounterInterval;
            tickCounter.Dispatcher.Invoke(() => tickCounter.Content = "Tick: " + tickCounterValue.ToString());
        }
        public double Laplacian(int x, int y, double[,] grid)
        {
            double sum = 0;
            sum += grid[x, y] * -1.0;
            sum += grid[(x - 1 + width) % width, y] * 0.2;
            sum += grid[(x + 1) % width, y] * 0.2;
            sum += grid[x, (y - 1 + height) % height] * 0.2;
            sum += grid[x, (y + 1) % height] * 0.2;
            sum += grid[(x - 1 + width) % width, (y - 1 + height) % height] * 0.05;
            sum += grid[(x + 1) % width, (y - 1 + height) % height] * 0.05;
            sum += grid[(x - 1 + width) % width, (y + 1) % height] * 0.05;
            sum += grid[(x + 1) % width, (y + 1) % height] * 0.05;
            return sum;
        }

    }
}