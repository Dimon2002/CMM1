using Spline.BasisInfos;
using Spline.Interfaces;
using Spline.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spline
{
    public class BSplineSurface : BSpline, ParametricSurface
    {
        private Point3d[][] _pointsGrid;
        private KnotSet _uKnotVector;
        private KnotSet _vKnotVector;
        private int _uDegree;
        private int _vDegree;

        public ref Point3d[][] PointsGrid => ref _pointsGrid;
        public int UCount => _pointsGrid.Length;
        public int VCount => _pointsGrid[0].Length;
        /// <summary>
        /// The points index example is as following.
        /// The reason for this setting is to look at the coordinate like points[x][y].
        /// As a result, The number of U count is 3 and that of V count is 5.
        ///
        ///  3*---7*--11*
        ///    |      |    |
        ///  2*---6*--10*
        ///    |      |    |
        ///  1*---5*---9*
        ///    |      |    |
        ///  0*---4*---8*
        ///
        /// </summary>
        /// <param name="points"></param>
        public BSplineSurface(
            IEnumerable<IEnumerable<IEnumerable<double>>> points,
            KnotSet uKnotVector,
            KnotSet vKnotVector,
            int uDegree,
            int vDegree)
        {
            List<List<Point3d>> ptss = [];

            for (int i = 0; i < points.Count(); i++)
            {
                ptss.Add([]);
                var pts = points.ElementAt(i);

                for (int j = 0; j < pts.Count(); j++)
                {
                    var pt = pts.ElementAt(j);

                    Point3d target = new(pt);
                    ptss.Last().Add(target);
                }
            }
            _pointsGrid = ptss.Select(n => n.ToArray()).ToArray();

            _uKnotVector = (KnotSet)uKnotVector.Clone();
            _vKnotVector = (KnotSet)vKnotVector.Clone();
            _uDegree = uDegree;
            _vDegree = vDegree;
        }

        public BSplineSurface(
            Point3d[][] points,
            KnotSet uKnotVector,
            KnotSet vKnotVector,
            int uDegree,
            int vDegree)
        {
            _pointsGrid = points;
            _uKnotVector = (KnotSet)uKnotVector.Clone();
            _vKnotVector = (KnotSet)vKnotVector.Clone();
            _uDegree = uDegree;
            _vDegree = vDegree;
        }

        public int GetUCount()
        {
            return _pointsGrid.Length;
        }

        public int GetVCount()
        {
            return _pointsGrid[0].Length;
        }

        public double[] ParameterAt(double u, double v)
        {
            int n = GetUCount() - 1;
            int m = GetVCount() - 1;

            Point3d accumulation = new(0, 0, 0);

            for (int i = 0; i <= n; i++)
            {
                BasisInfo info1 = new BSplineBasisInfo(i, _uDegree, _uKnotVector);
                Func<double, double> basis1 = GetBasisFunction(info1);

                // Boundary condition
                if (u == 1 && i == n)
                {
                    basis1 = (t) => 1;
                }

                for (int j = 0; j <= m; j++)
                {
                    BasisInfo info2 = new BSplineBasisInfo(j, _vDegree, _vKnotVector);
                    Func<double, double> basis2 = GetBasisFunction(info2);

                    // Boundary condition
                    if (v == 1 && j == m)
                    {
                        basis2 = (t) => 1;
                    }

                    Point3d pt = _pointsGrid[i][j];
                    accumulation += pt * basis1(u) * basis2(v);
                }
            }

            return [accumulation.X, accumulation.Y, accumulation.Z];
        }

        public BSplineSurface SetKnots(KnotSet uKnot, KnotSet vKnot)
        {
            _uKnotVector = uKnot;
            _vKnotVector = vKnot;

            return this;
        }

        public BSplineSurface CreateInterpolatingSurface()
        {
            var dataPoints = PointsGrid;

            int numU = UCount;
            int numV = VCount;

            KnotSet uKnots = _uKnotVector;
            KnotSet vKnots = _vKnotVector;

            double[,] matrixU = BuildCoefficientMatrix(_uDegree, uKnots, numU);
            double[,] matrixV = BuildCoefficientMatrix(_vDegree, vKnots, numV);

            Point3d[][] controlPoints = SolveControlPoints(dataPoints, matrixU, matrixV);

            return new BSplineSurface(controlPoints, uKnots, vKnots, _uDegree, _vDegree);
        }

        private double[,] BuildCoefficientMatrix(int degree, KnotSet knots, int numPoints)
        {
            double[,] matrix = new double[numPoints, numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                double param = (double)i / (numPoints - 1);
                for (int j = 0; j < numPoints; j++)
                {
                    BasisInfo basisInfo = new BSplineBasisInfo(j, degree, knots);
                    Func<double, double> basis = GetBasisFunction(basisInfo);
                    matrix[i, j] = basis(param);
                }
            }

            matrix[numPoints - 1, numPoints - 1] = 1; // Жду тебя братик!

            return matrix;
        }

        private Point3d[][] SolveControlPoints(Point3d[][] dataPoints, double[,] matrixU, double[,] matrixV)
        {
            int numU = UCount;
            int numV = VCount;

            // Преобразование матриц коэффициентов в MathNet-матрицы
            Matrix<double> matU = DenseMatrix.OfArray(matrixU);
            Matrix<double> matV = DenseMatrix.OfArray(matrixV);

            // Инвертирование матриц (предполагаем, что они квадратные и обратимы)
            Matrix<double> invMatU = matU.Inverse();
            Matrix<double> invMatV = matV.Inverse();

            // Инициализация контрольных точек
            Point3d[][] controlPoints = new Point3d[numU][];

            // Разделение данных на координаты X, Y, Z
            Matrix<double> dataX = DenseMatrix.Create(numU, numV, 0);
            Matrix<double> dataY = DenseMatrix.Create(numU, numV, 0);
            Matrix<double> dataZ = DenseMatrix.Create(numU, numV, 0);

            for (int i = 0; i < numU; i++)
            {
                for (int j = 0; j < numV; j++)
                {
                    dataX[i, j] = dataPoints[i][j].X;
                    dataY[i, j] = dataPoints[i][j].Y;
                    dataZ[i, j] = dataPoints[i][j].Z;
                }
            }

            // Решение для каждой координаты: controlPoints = invMatU * data * invMatV.Transpose()
            Matrix<double> controlX = invMatU.Multiply(dataX).Multiply(invMatV.Transpose());
            Matrix<double> controlY = invMatU.Multiply(dataY).Multiply(invMatV.Transpose());
            Matrix<double> controlZ = invMatU.Multiply(dataZ).Multiply(invMatV.Transpose());

            // Сборка результатов в Point3d[][]
            for (int i = 0; i < numU; i++)
            {
                controlPoints[i] = new Point3d[numV];
                for (int j = 0; j < numV; j++)
                {
                    controlPoints[i][j] = new Point3d(
                        controlX[i, j],
                        controlY[i, j],
                        controlZ[i, j]
                    );
                }
            }

            return controlPoints;
        }
    }
}
