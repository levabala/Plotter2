using System;
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
        Matrix m;
        float max, min;
        Pen myPen = new Pen(Color.Green, 1);
        PointF lastmousepos = new PointF(0f, 0f);
        Timer wheelEndTimer = new Timer();
        PointF[] toDrawArr;
        List<PointF> points = new List<PointF>(); 
        List<PointF> pointsDrawedLog = new List<PointF>();
        float leftX, rightX;
        bool averageLine = true;
        PointsManager pm;

        Font smallFont = new Font("Arial Narrow", 8);
        Pen xGridPen = new Pen(Brushes.Blue, 1);
        Pen yGridPen = new Pen(Brushes.Green, 1);
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
            FileToPoints(@"D:\work\test_6000_fast.raw");
            
            leftX = points[0].X;
            rightX = points[points.Count - 1].X;

            xGridPen.DashPattern = new float[] { 1, 5, 1 };
            yGridPen.DashPattern = new float[] { 1, 5, 1 };

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

            Invalidate();
        }

        Pen avLinePen = new Pen(Color.DarkBlue, 3);
        private void Form1_Paint1(object sender, PaintEventArgs e)
        {
            myPen.LineJoin = LineJoin.Round;            
            Graphics g = e.Graphics;
            
            g.DrawString("Layer borders: " + pm.leftindexL.ToString() + ":" + pm.rightindexL.ToString() + " -> " + (pm.rightindexL-pm.leftindexL).ToString(), Font, Brushes.Red, new PointF(10, 10));
            g.DrawString("Points borders: " + pm.leftindexP.ToString() + ":" + pm.rightindexP.ToString() + " -> " + (pm.rightindexP - pm.leftindexP).ToString(), Font, Brushes.Red, new PointF(10, 25));            
            g.DrawString("Zoom: " + pm.myzoom.ToString(), Font, Brushes.Red, new PointF(10, 40));
            g.DrawString("MaxZoom: " + pm.mymaxZoom.ToString() + ":" + pm.rightindexL.ToString(), Font, Brushes.Red, new PointF(10, 55));
            g.DrawString("Active layer: " + pm.ActiveLayerIndex.ToString(), Font, Brushes.Red, new PointF(10, 70));
            g.DrawString("Points drawed: " + toDrawArr.Length.ToString(), Font, Brushes.Red, new PointF(10, 85));

            drawAxises(g);

            g.Transform = m;                        

            if (toDrawArr.Length <= 1) return;

            if (averageLine) g.DrawBeziers(avLinePen, pm.averagePoints);
            g.DrawLines(myPen, toDrawArr);  
        }

        public PointF ScreenPoint(PointF scr)
        {
            Matrix mr = m.Clone();
            //mr.Invert();
            PointF[] po = new PointF[] { new PointF(scr.X, scr.Y) };
            mr.TransformPoints(po);
            return po[0];
        }

        private void drawAxises(Graphics g)
        {
            /*PointF axes_step_pt = new PointF(0, 0);
            PointF axes_step_pt2 = new PointF(100, 50);
            PointF axes_step = DataPoint(axes_step_pt);
            PointF axes_step2 = DataPoint(axes_step_pt2);

            float dx = axes_step2.X - axes_step.X;
            float dy = axes_step2.Y - axes_step.Y;

            int power = 10;

            double lg_x = Math.Log(Math.Abs(dx), power);
            double lg_y = Math.Log(Math.Abs(dy), power);

            double pow_x = Math.Pow(power, Math.Floor(lg_x) + 1);
            double pow_y = Math.Pow(power, Math.Floor(lg_y) + 1);

            int step_x = (int)(pow_x / 2);
            if (step_x < 1) step_x = 1;
            int step_y = (int)(pow_y / 2);
            if (step_y < 1) step_y = 1;

            int x = 0;
            PointF xp = ScreenPoint(new PointF(x, 0));
            if (xp.X < 0) xp.X = 0;
            do
            {
                xp = ScreenPoint(new PointF(x, 0));
                //g.DrawLine(Pens.Blue, xp.X, xp.Y, xp.X, xp.Y + 20);
                g.DrawLine(Pens.Blue, xp.X, 0, xp.X, 10);
                g.DrawLine(xGridPen, xp.X, 10, xp.X, ClientRectangle.Height);
                g.DrawString(x.ToString(), smallFont, Brushes.Blue, xp.X, +10);
                x += step_x;
            } while (xp.X < ClientRectangle.Width);

            int y = 0;
            PointF yp = ScreenPoint(new PointF(0, y));
            do
            {
                yp = ScreenPoint(new PointF(0, y));
                //g.DrawLine(Pens.Red, yp.X, yp.Y, yp.X-20, yp.Y);
                g.DrawLine(Pens.Red, 0, yp.Y, 10, yp.Y);
                g.DrawLine(yGridPen, 10, yp.Y, ClientRectangle.Width, yp.Y);
                g.DrawString(y.ToString(), smallFont, Brushes.Red, 10, yp.Y);
                y += step_y;
            } while (yp.Y > 0);*/
            PointF axes_step_pt = new PointF(0, 0);
            PointF axes_step_pt2 = new PointF(100, 50);
            PointF axes_step = DataPoint(axes_step_pt);
            PointF axes_step2 = DataPoint(axes_step_pt2);

            float dx = axes_step2.X - axes_step.X;
            float dy = axes_step2.Y - axes_step.Y;

            int power = 10;

            double lg_x = Math.Log(Math.Abs(dx), power);
            double lg_y = Math.Log(Math.Abs(dy), power);

            double pow_x = Math.Pow(power, Math.Floor(lg_x) + 1);
            double pow_y = Math.Pow(power, Math.Floor(lg_y) + 1);

            int step_x = (int)(pow_x / 2);

            if (step_x < 1) step_x = 1;
            int step_y = (int)(pow_y / 2);
            if (step_y < 1) step_y = 1;

            int kx = 0;
            PointF origin = DataPoint(new PointF(0, ClientRectangle.Height));
            int x = ((int)(origin.X / step_x) + 1) * step_x;
            PointF xp = ScreenPoint(new PointF(x, 0));
            do
            {
                xp = ScreenPoint(new PointF(x, 0));
                //g.DrawLine(Pens.Blue, xp.X, xp.Y, xp.X, xp.Y + 20);
                g.DrawLine(Pens.Blue, xp.X, 0, xp.X, 10);
                g.DrawLine(xGridPen, xp.X, 10, xp.X, ClientRectangle.Height);
                g.DrawString(x.ToString(), smallFont, Brushes.Blue, xp.X + 3, -2);
                x += step_x;
                kx += 1;
            } while (xp.X < ClientRectangle.Width && kx < 22);

            int ky = 0;
            int y = 0; // (int)origin.Y;
            PointF yp = ScreenPoint(new PointF(0, y));
            do
            {
                yp = ScreenPoint(new PointF(0, y));
                //g.DrawLine(Pens.Red, yp.X, yp.Y, yp.X-20, yp.Y);
                g.DrawLine(Pens.Red, 0, yp.Y, 10, yp.Y);
                g.DrawLine(yGridPen, 10, yp.Y, ClientRectangle.Width, yp.Y);
                g.DrawString(y.ToString(), smallFont, Brushes.Red, 0, yp.Y);
                y += step_y;
                ky += 1;
            } while (yp.Y > 0 && ky < 22);

            g.DrawString(
             String.Format("sx: {0}\nkx: {1}\nsy:{2}\nky:{3}",
             step_x, kx, step_y, ky),
             this.Font, Brushes.Green, 50, 150
             );
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
                points.Add(new PointF((float)(data[i]*16e-3), rpmf));
                //if (cr > 10000)continue;
                if (rpm > max) max = rpmf;
                else if (rpm < min) min = rpmf;
            }
        }      

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
            if (ModifierKeys.HasFlag(Keys.Control) || inXscale) ky = 1;
            if (ModifierKeys.HasFlag(Keys.Shift) || inYscale) kx = 1;
            PointF po = DataPoint(e.Location);
            m.Translate(po.X, po.Y);
            m.Scale(kx, ky);
            m.Translate(-po.X, -po.Y);

            Text += " |" + kx.ToString() + "| ";
                        
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

            getPointsToDraw();
            Invalidate();
        }

        private void getPointsToDraw()
        {
            toDrawArr = pm.getPointsInRange(leftX, rightX).ToArray();
            PointF scale = GetMatrixScale();
        }
    }
}
