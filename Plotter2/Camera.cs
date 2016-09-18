using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Plotter2
{
    class Camera
    {
        public PointF leftP, rightP;
        public List<PointF> allPoints;
        public List<PointF> points, toDraw;
        public float width, height, detectStep;
        public float clientW, clientH;
        public float detectBuffer = 20f;
        public Camera(PointF lp, PointF rp, List<PointF> ps, List<PointF> allPs)
        {
            leftP = lp;
            rightP = rp;
            points = ps;
            allPoints = allPs;

            width = rightP.X - leftP.X;
            height = rightP.Y - leftP.Y;
        }

        public int getPointsCount()
        {
            return allPoints.FindIndex(p => p.X > rightP.X) - allPoints.FindIndex(p => p.X > leftP.X);
        }

        public void detectPointsToDraw()
        {           
            int startIndex = 0;
            int endIndex = points.Count;
            foreach (PointF p in points)
            {
                if (p.X >= leftP.X - detectBuffer) break;
                startIndex++;
            }            
            for (int i = points.Count-1; i > 0; i--)
            {                
                if (points[i].X <= rightP.X + detectBuffer) break;
                endIndex = i;
            }

            toDraw = points.GetRange(startIndex, endIndex - startIndex);
        }

        public void MoveTo(PointF lp, PointF rp)
        {
            leftP = lp;
            rightP = rp;
            width = rightP.X - leftP.X;
            height = rightP.Y - leftP.Y;
        }

        public void Move(float dx, float dy)
        {
            leftP.X += dx;
            rightP.X += dx;
            leftP.Y += dy;
            rightP.Y += dy;
        }
    }
}
