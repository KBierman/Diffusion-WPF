namespace Diffusion
{
    internal class DiffusionBasics
    {

        public double Laplacian(int x, int y, double[,] gridA, int width, int height)
        {
            double sum = 0;
            sum += gridA[x, y] * -1.0;
            sum += gridA[(x - 1 + width) % width, y] * 0.2;
            sum += gridA[(x + 1) % width, y] * 0.2;
            sum += gridA[x, (y - 1 + height) % height] * 0.2;
            sum += gridA[x, (y + 1) % height] * 0.2;
            sum += gridA[(x - 1 + width) % width, (y - 1 + height) % height] * 0.05;
            sum += gridA[(x + 1) % width, (y - 1 + height) % height] * 0.05;
            sum += gridA[(x - 1 + width) % width, (y + 1) % height] * 0.05;
            sum += gridA[(x + 1) % width, (y + 1) % height] * 0.05;
            return sum;
        }
    }
}
