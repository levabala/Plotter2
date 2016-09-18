﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plotter2
{
    public partial class Form1 : Form
    {
        Camera cam;
        Matrix m;
        float max, min, coeff;
        Pen myPen = new Pen(Color.Green, 1);
        PointF lastmousepos = new PointF(0f, 0f);
        Timer wheelEndTimer = new Timer();
        PointF[] toDrawArr;
        List<PointF> points = new List<PointF>();
        int ActiveLayerIndex = 0;

        string msg1, msg2;

        float leftX, rightX;

        PointsManager pm;


        /*{
            new PointF(0,0),
            new PointF(10,20),
            new PointF(30,20),
            new PointF(50,20),
            new PointF(66,43),
            new PointF(67,48),
            new PointF(69,2),
            new PointF(110,20),
            new PointF(130,20),
            new PointF(150,20),
            new PointF(166,43),
            new PointF(167,48),
            new PointF(169,2),
            new PointF(210,20),
            new PointF(230,20),
            new PointF(250,20),
            new PointF(266,43),
            new PointF(267,48),
            new PointF(269,2),
            new PointF(661,200)
        };   */
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            coeff = 1.5f;

            FileToPoints(@"D:\work\test_6000_fast.raw");
            
            leftX = points[0].X;
            rightX = points[points.Count - 1].X;

            pm = new PointsManager(points);
            pm.createLayers();            

            restartMatrix();
            getPointsToDraw();
            Invalidate();
            
            wheelEndTimer.Interval = 200;
            wheelEndTimer.Tick += WheelEndTimer_Tick;

            Paint += Form1_Paint1;
            MouseWheel += Form1_MouseWheel;
            MouseUp += Form1_MouseUp;
            MouseMove += Form1_MouseMove;

            /*
            fillLayers();

            restartMatrix();            
            getPointsToDraw();

            wheelEndTimer.Interval = 200;
            wheelEndTimer.Tick += WheelEndTimer_Tick;

            Paint += Form1_Paint;
            MouseWheel += Form1_MouseWheel;
            MouseUp += Form1_MouseUp;
            MouseMove += Form1_MouseMove;*/

            Invalidate();
        }

        private void Form1_Paint1(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            g.DrawString("Layer borders: " + pm.leftindexL.ToString() + ":" + pm.rightindexL.ToString() + " -> " + (pm.rightindexL-pm.leftindexL).ToString(), Font, Brushes.Red, new PointF(10, 10));
            g.DrawString("Points borders: " + pm.leftindexP.ToString() + ":" + pm.rightindexP.ToString() + " -> " + (pm.rightindexP - pm.leftindexP).ToString(), Font, Brushes.Red, new PointF(10, 25));            
            g.DrawString("Zoom: " + pm.myzoom.ToString(), Font, Brushes.Red, new PointF(10, 40));
            g.DrawString("MaxZoom: " + pm.mymaxZoom.ToString() + ":" + pm.rightindexL.ToString(), Font, Brushes.Red, new PointF(10, 55));
            g.DrawString("Active layer: " + pm.ActiveLayerIndex.ToString(), Font, Brushes.Red, new PointF(10, 70));
            g.DrawString("Points drawed: " + toDrawArr.Length.ToString(), Font, Brushes.Red, new PointF(10, 85));

            g.Transform = m;

            if (toDrawArr.Length <= 1) return;

            g.DrawLines(Pens.Green, toDrawArr);  
        }

        private void FileToPoints(string path)
        {            
            Int64[] data = Parser.parseLM(path, 242);
            max = 0;
            min = data[1] - data[0];            

            for (int i = 1; i < data.Length; i += 1)
            {                
                double tt = data[i] - data[i - 1];
                double rpm = 60.0 / (tt * 1024 * 16e-9);
                float rpmf = (float)rpm;
                points.Add(new PointF((i), rpmf));
                //if (cr > 10000)continue;
                if (rpm > max) max = rpmf;
                else if (rpm < min) min = rpmf;
            }
        }

        /*private void fillLayers()
        {
            float count = points.Count;
            float step = 1;
            int index = 0;
            do
            {
                List<PointF> layer = new List<PointF>();
                step = points.Count / count;
                for (float i = 0; i < points.Count; i += step)
                {
                    index = (int)i;
                    layer.Add(points[index]);
                }
                layer.Add(points[points.Count - 1]);
                layers.Add(layer);
                count /= coeff;
            } while (count > ClientSize.Width);
            if (layers.Count == 0) layers.Add(points);
            GetLayerIndex();
        }*/

        /*float lastW = 0f;
        private void GetLayerIndex()
        {            
            int index = 0;
            int pointsCount = cam.getPointsCount();
            float zoom = pointsCount / ClientSize.Width;
            float topZoom = points.Count / ClientSize.Width;
            float step = topZoom / layers.Count;
            bool aa = false;

            if (zoom <= 1)
                index = layers.Count - 1;
            else
            {
                index = (int)(zoom / step);
                aa = true;
            }

            Text = "zoom: " + zoom.ToString() + " topZoom:" + topZoom.ToString() + " step: " + step.ToString() + " || " + (zoom / step).ToString() + " -> " + index.ToString() + " " + aa.ToString();
            lastW = cam.width;
            ActiveLayerIndex = index;            
        }*/        

        private void restartMatrix()
        {
            m = new Matrix();
            m.Scale(1, -1);
            m.Translate(0, -ClientSize.Height);
            m.Scale(ClientSize.Width / pm.xRange, ClientSize.Height / (max - min));

            for (int i = 0; i < 10; i++)
                Form1_MouseWheel(this, new MouseEventArgs(MouseButtons.None, 0, ClientSize.Width / 2, ClientSize.Height / 2, -1));
        }

        private PointF DataPoint(PointF scr)
        {
            Matrix mr = m.Clone();
            mr.Invert();
            PointF[] po = new PointF[] { new PointF(scr.X, scr.Y) };
            mr.TransformPoints(po);
            return po[0];
        }

        private PointF GetMatrixScale()
        {
            PointF[] ps = new PointF[] { new PointF(1f, 1f)};
            m.TransformPoints(ps);
            return ps[0];
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            PointF pos = DataPoint(e.Location);
            bool inXscale = e.Location.Y < 50;
            bool inYscale = e.Location.X < 50;
            float z = e.Delta > 0 ? 1.1f : 1.0f / 1.1f;
            float kx = z;
            float ky = z;
            if (ModifierKeys.HasFlag(Keys.Control) || inXscale) ky = 1; //!(m.Elements[1] > -1e-5 && m.Elements[1] < -0.05) 
            if (ModifierKeys.HasFlag(Keys.Shift) || inYscale) kx = 1; //!(m.Elements[0] > 0.05 && m.Elements[0] < 1000)
            PointF po = DataPoint(e.Location);
            m.Translate(po.X, po.Y);
            m.Scale(kx, ky);
            m.Translate(-po.X, -po.Y);

            Text += " |" + kx.ToString() + "| ";
            /*cam.clientW = DataPoint(cam.rightP).X - DataPoint(cam.leftP).X;
            cam.clientH = ClientSize.Height * m.Elements[4];*/
                        
            Invalidate();

            wheelEndTimer.Stop();
            wheelEndTimer.Start();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                updateCameraPos();
        }
        
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            Text = DataPoint(e.Location).ToString();
            if (e.Button == MouseButtons.Left)
            {
                float dx = DataPoint(lastmousepos).X - DataPoint(e.Location).X;
                float dy = DataPoint(lastmousepos).Y - DataPoint(e.Location).Y;

                //cam.Move(dx, dy);
                leftX += dx;
                rightX += dx;
                m.Translate(-dx, -dy);
                Invalidate();
            }
            lastmousepos = e.Location;
        }

        private void updateCameraPos()
        {

            PointF newlp = DataPoint(new PointF(0f, 0f));
            PointF newrp = DataPoint(new PointF(ClientSize.Width, ClientSize.Height));

            /*cam.MoveTo(newlp, newrp);
            cam.detectPointsToDraw();*/

            leftX = newlp.X;
            rightX = newrp.X;

            getPointsToDraw();
            Invalidate();
        }

        private void WheelEndTimer_Tick(object sender, EventArgs e)
        {
            wheelEndTimer.Stop();

            PointF newlp = DataPoint(new PointF(0f, 0f));
            PointF newrp = DataPoint(new PointF(ClientSize.Width, ClientSize.Height));

            leftX = newlp.X;
            rightX = newrp.X;
            /*cam.detectStep = points.Count / ClientSize.Width;
            cam.MoveTo(newlp, newrp);
            cam.detectPointsToDraw();*/

            getPointsToDraw();
            Invalidate();
        }

        private void getPointsToDraw()
        {
            /*GetLayerIndex();
            cam.points = layers[ActiveLayerIndex];

            List<PointF> toDraw = new List<PointF>();

            cam.detectPointsToDraw();
            toDraw = cam.toDraw;

            toDrawArr = toDraw.ToArray();*/

            toDrawArr = pm.getPointsInRange(leftX, rightX).ToArray();
            PointF scale = GetMatrixScale();
            //Text = pm.ActiveLayerIndex.ToString() + " zoom: " + pm.myzoom.ToString() + " topZoom: " + pm.mymaxZoom.ToString() + "  scaleX: " + scale.X + " scaleY: " + scale.Y;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            myPen.LineJoin = LineJoin.Bevel;
            Graphics g = e.Graphics;

            g.Transform = m;

            if (toDrawArr.Length <= 1) return;           
            g.DrawLines(myPen, toDrawArr);            
        }
    }
}