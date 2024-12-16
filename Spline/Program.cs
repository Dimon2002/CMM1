using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spline.Utils;

namespace Spline
{
    class Program
    {
        static void Main(string[] args)
        {
            double[][][] ptGrid = null;
            double[][] weights = null;
            double[] r, a;
            double u, v, e;
            NURBSSurface surface = null;
            int uDegree, vDegree;
            KnotSet uKnot, vKnot;

            uDegree = 3;
            vDegree = 3;
            uKnot = KnotSet.CreateToUniform(uDegree, 4);
            vKnot = KnotSet.CreateToUniform(vDegree, 5);
            weights = new double[][]
            {
                new double[]{ 1, 1, 1, 1, 1, },
                new double[]{ 1, 1, 1, 1, 1, },
                new double[]{ 1, 1, 1, 1, 1, },
                new double[]{ 1, 1, 1, 1, 1, },
            };

            ptGrid = new double[][][]
            {
                new double[][]{
                    new double[] {0.0, 0.0, 1.0 },
                    new double[] {0.0, 1.0, 2.0 },
                    new double[] {0.0, 2.0, 0.0 },
                    new double[] {0.0, 3.0, 1.0 },
                    new double[] {0.0, 4.0, 1.0 },
                },
                new double[][]{
                    new double[] {1.0, 0.0, 2.0 },
                    new double[] {1.0, 1.0, 2.0 },
                    new double[] {1.0, 2.0, 1.0 },
                    new double[] {1.0, 3.0, 1.0 },
                    new double[] {1.0, 4.0, 1.0 },
                },
                new double[][]{
                    new double[] {2.0, 0.0, 2.0 },
                    new double[] {2.0, 1.0, 1.0 },
                    new double[] {2.0, 2.0, 1.0 },
                    new double[] {2.0, 3.0, 1.0 },
                    new double[] {2.0, 4.0, 2.0 },
                },
                new double[][]{
                    new double[] {3.0, 0.0, 2.0 },
                    new double[] {3.0, 1.0, 1.0 },
                    new double[] {3.0, 2.0, 1.0 },
                    new double[] {3.0, 3.0, 1.0 },
                    new double[] {3.0, 4.0, 2.0 },
                },
            };
            
            u = 0.5;
            v = 0.5;
            surface = new NURBSSurface(ptGrid, uKnot, vKnot, uDegree, vDegree, weights);
            r = surface.ParameterAt(u, v);
        }
    }
}
