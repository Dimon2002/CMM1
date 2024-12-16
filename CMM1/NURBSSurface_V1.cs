namespace CMM1;

public class NURBSSurface_V1
{
    private int degreeU;
    private int degreeV;
    private double[] knotVectorU;
    private double[] knotVectorV;
    private double[,] controlPointsX;
    private double[,] controlPointsY;
    private double[,] controlPointsZ;
    private double[,] weights;

    public NURBSSurface_V1(int degreeU, int degreeV,
                        double[] knotVectorU, double[] knotVectorV,
                        double[,] controlPointsX, double[,] controlPointsY, double[,] controlPointsZ,
                        double[,] weights)
    {
        this.degreeU = degreeU;
        this.degreeV = degreeV;
        this.knotVectorU = knotVectorU;
        this.knotVectorV = knotVectorV;
        this.controlPointsX = controlPointsX;
        this.controlPointsY = controlPointsY;
        this.controlPointsZ = controlPointsZ;
        this.weights = weights;
    }

    private double BasisFunction(int i, int degree, double u, double[] knotVector)
    {
        if (degree == 0)
        {
            if (knotVector[i] <= u && u < knotVector[i + 1])
                return 1.0;
            else
                return 0.0;
        }
        else
        {
            double left = 0.0;
            double right = 0.0;

            double denom1 = knotVector[i + degree] - knotVector[i];
            double denom2 = knotVector[i + degree + 1] - knotVector[i + 1];

            if (denom1 != 0)
                left = (u - knotVector[i]) / denom1 * BasisFunction(i, degree - 1, u, knotVector);

            if (denom2 != 0)
                right = (knotVector[i + degree + 1] - u) / denom2 * BasisFunction(i + 1, degree - 1, u, knotVector);

            return left + right;
        }
    }

    public double Evaluate(double u, double v, out double x, out double y, out double z)
    {
        int n = controlPointsX.GetLength(0);
        int m = controlPointsX.GetLength(1);

        double numeratorX = 0.0;
        double numeratorY = 0.0;
        double numeratorZ = 0.0;
        double denominator = 0.0;

        for (int i = 0; i < n; i++)
        {
            double Nu = BasisFunction(i, degreeU, u, knotVectorU);
            for (int j = 0; j < m; j++)
            {
                double Nv = BasisFunction(j, degreeV, v, knotVectorV);
                double weight = weights[i, j];
                double B = Nu * Nv * weight;

                numeratorX += B * controlPointsX[i, j];
                numeratorY += B * controlPointsY[i, j];
                numeratorZ += B * controlPointsZ[i, j];
                denominator += B;
            }
        }

        x = numeratorX / denominator;
        y = numeratorY / denominator;
        z = numeratorZ / denominator;

        return z;
    }
}