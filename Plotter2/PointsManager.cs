using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Plotter2
{
    class PointsManager
    {
        List<PointF> points;
        public List<List<PointF>> layers;
        public float xRange, yRange;
        public float leftLimit, rightLimit;
        public float minPointsCount = 2000;
        public int ActiveLayerIndex = 0;
        public float myzoom, mymaxZoom;
        public int leftindexP, rightindexP, leftindexL, rightindexL;
        public int outpointsCount;

        public PointF[] averagePoints;        
        public PointsManager(List<PointF> ps)
        {
            points = ps;
            xRange = points[points.Count - 1].X - points[0].X;

            leftLimit = points[0].X;
            rightLimit = points[points.Count - 1].X;

            List<PointF> list = new List<PointF>();
            int step = points.Count / 3400;
            float buffer = 0;
            int c = 0;
            foreach (PointF p in points)
            {
                buffer += p.Y;
                if (c >= step)
                {
                    list.Add(new PointF(p.X, buffer / c));
                    buffer = 0;
                    c = 0;
                }
                c++;
            }
            list = list.GetRange(0, 3301);
            averagePoints = list.ToArray();
        }

        public void createLayers()
        {
            layers = new List<List<PointF>>();
            layers.Add(points);
            float count = points.Count/2;
            int step = 1;            
            do
            {
                count /= 2;
                step = (int)(points.Count / count);
                layers.Add(MakeConvolution(layers.Last(), step));                
            } while (count > minPointsCount);
        }

        private List<PointF> MakeConvolution(List<PointF> src, int step)
        {
            List<PointF> layer = new List<PointF>();            
            for (int i = 0; i < points.Count - step; i += step)
            {
                PointF[] minmax = getMinAndMax(points.GetRange(i, step));
                if (minmax[0].X > minmax[1].X)
                {
                    layer.Add(minmax[1]);
                    layer.Add(minmax[0]);
                }
                else
                {
                    layer.Add(minmax[0]);
                    layer.Add(minmax[1]);
                }
            }
            return layer;
        }

        public List<PointF> getPointsInRange(float leftx, float rightx)
        {
            int[] bord1 = getBorders(points, leftx, rightx);
            int i1 = bord1[1];
            int i2 = bord1[0];

            /*if (i1 == -1) i1 = points.Count;
            if (i2 == -1) i2 = 0;*/

            float psCount = i1 - i2;
            if (psCount == 0) return new List<PointF>();

            float zoom = psCount / 1920f;
            myzoom = zoom;
            float topZoom = points.Count / 1920f; // minPointsCount;
            mymaxZoom = topZoom;
            float step = topZoom / layers.Count;

            int layerIndex = 0;
            do
            {
                int[] bord = getBorders(layers[layerIndex], leftx, rightx);
                int ps = bord[1] - bord[0];
                if (ps <= 3000) break;
                layerIndex++;
            } while (layerIndex < layers.Count-1);

            /**/
            //int layerIndex = (int)(zoom / step);
            /**/

            if (layerIndex < 0) layerIndex = 0;
            ActiveLayerIndex = layerIndex;

            List<PointF> currLayer = layers[layerIndex];
            int[] bord2 = getBorders(currLayer, leftx, rightx);
            int startI = bord2[0];// currLayer.FindIndex(p => p.X >= leftx);
            int endI = bord2[1];//currLayer.FindIndex(p => p.X >= rightx);

            if (startI < 0) startI = 0;

            List<PointF> outPoints = currLayer.GetRange(startI, endI - startI);
            outpointsCount = outPoints.Count;

            leftindexP = i2;
            rightindexP = i1;
            leftindexL = startI;
            rightindexL = endI;

            return outPoints;
        }

        private int[] getBorders(List<PointF> ps, float lx, float rx)
        {
            int si = 0;
            int ei = ps.Count-1;
            foreach (PointF p in ps)
            {
                if (p.X >= lx) break;
                si++;
            }
            for (int i = ps.Count-1; i > 0; i--)
            {
                if (ps[i].X <= rx) break;
                ei--;
            }
            return new int[] { si, ei };
        }
                
        private PointF[] getMinAndMax(List<PointF> ps)
        {
            PointF[] minmax = new PointF[] {new PointF(ps[0].X, ps[0].Y), new PointF(0,0)}; //[0] - min, [1] - max            
            foreach (PointF p in ps)
            {
                if (minmax[0].Y > p.Y) minmax[0] = p;
                if (minmax[1].Y < p.Y) minmax[1] = p;
                //minmax[1].X = minmax[0].X;
            }

            return minmax;
        }
    }
}
