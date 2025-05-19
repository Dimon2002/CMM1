using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spline.Utils
{
    public class KnotSet : ICloneable
    {
        private bool isStrict = true;
        private double[] knotVector;

        public int Length
        {
            get
            {
                return knotVector.Length;
            }
        }

        public KnotSet(IEnumerable<double> knots)
        {
            knotVector = knots.ToArray();
        }
        public KnotSet(IEnumerable<double> knots, bool isStrict)
        {
            knotVector = knots.ToArray();
            this.isStrict = isStrict;
        }

        public double GetIndexOf(int i)
        {
            if (i < 0)
            {
                if (isStrict)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return knotVector.First();
            }
            else if (knotVector.Length <= i)
            {
                if (isStrict)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return knotVector.Last();
            }
            return knotVector[i];
        }

        /// <summary>
        /// Check the non-decreasing knot vector and minimum count 
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            // Check whether non-decreasing
            for (int i = 0; i < knotVector.Length - 1; i++)
            {
                if (knotVector[i] > knotVector[i + 1])
                {
                    return false;
                }
            }

            return true;
        }

        public double[] GetKnotArray()
        {
            return knotVector.ToArray();
        }

        public double GetMaxKnot()
        {
            return this.knotVector.Max();
        }

        public object Clone()
        {
            KnotSet clone = (KnotSet)MemberwiseClone();
            clone.knotVector = knotVector.ToArray();

            return clone;
        }

        /// <summary>
        /// Uniform B-spline
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="numControlPoints"></param>
        /// <returns></returns>
        public static KnotSet CreateToUniform(int degree, int numControlPoints)
        {
            // knot length = degree + 1 + control points
            // knot length = degree + order 
            var knots = new List<double>();
            double[] normalizedKnots = null;
            int order = degree + 1;
            int knotCount = order + numControlPoints;

            for (int i = 0; i < knotCount; i++)
            {
                if (i < order)
                {
                    knots.Add(0);
                }
                else if (order <= i && i <= knotCount - order)
                {
                    knots.Add(knots.Last() + 1);
                }
                else
                {
                    knots.Add(knots.Last());
                }
            }
            double max = knots.Last();

            normalizedKnots = knots.Select(n => n / max).ToArray();

            KnotSet knotSet = new KnotSet(normalizedKnots);

            return knotSet;
        }

        public static KnotSet CreateClampedKnots(int degree, int numControlPoints)
        {
            if (numControlPoints < degree + 1)
                throw new ArgumentException("Number of control points must be ≥ degree + 1");

            int totalKnots = numControlPoints + degree + 1;
            double[] knots = new double[totalKnots];

            // Заполнение начальных узлов (0)
            for (int i = 0; i <= degree; i++)
                knots[i] = 0.0;

            // Заполнение внутренних узлов (равномерно распределённых)
            int numInternalSegments = numControlPoints - degree;
            double internalStep = 1.0 / numInternalSegments;

            for (int i = 1; i < numInternalSegments; i++)
                knots[degree + i] = i * internalStep;

            // Заполнение конечных узлов (1)
            for (int i = numControlPoints; i < totalKnots; i++)
                knots[i] = 1.0;

            return new KnotSet(knots);
        }
    }
}
