using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Paint
{
    enum ToolBox
    {
        pen, paintBucket
    }

    public partial class Form1 : Form
    {
        private ToolBox toolBox = ToolBox.pen;

        private Color colorA = Color.Black;

        private int prevX;
        private int prevY;
        private bool canDraw       = false;
        private bool canUseToolBox = true;

        private Thread thread = null;


        public Form1()
        {
            InitializeComponent();
            saveFileDialog1.Filter = "Image files (*.gif, *.bmp, *.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.gif; *bmp; *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            openFileDialog1.Filter = "Image files | *.gif; *bmp; *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            pictureBox1.Image = CreateEmptyBmp(Color.White, panel1.Width, panel1.Height);
            pictureBox2.Image = CreateEmptyBmp(Color.Black, 50, 50);
            timer1.Start();
        }

        private Bitmap CreateEmptyBmp(Color c, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(c);
            }

            return bmp;
        }

        private Bitmap Pen(Bitmap bmp, MouseEventArgs e)
        {
            using (var pen = new Pen(colorA, 1))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawLine(pen, prevX, prevY, e.X, e.Y);
                }
            }
            return bmp;
        }

        private Bitmap PaintBucket(Bitmap bmp, Color pC, Point point)
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(point);

            while (stack.Count > 0)
            {
                Point p = stack.Pop();

                if (p.X > 0 && p.X < bmp.Width && p.Y > 0 && p.Y < bmp.Height)
                {
                    if (bmp.GetPixel(p.X, p.Y) != pC)
                        continue;

                    bmp.SetPixel(p.X, p.Y, colorA);
                    stack.Push(new Point(p.X + 1, p.Y));
                    stack.Push(new Point(p.X - 1, p.Y));
                    stack.Push(new Point(p.X, p.Y + 1));
                    stack.Push(new Point(p.X, p.Y - 1));
                }
            }
            return bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            prevX = e.X;
            prevY = e.Y;
            canDraw = true;

            switch (toolBox)
            {
                case ToolBox.paintBucket:
                    canUseToolBox = false;
                    thread = new Thread(() =>
                    {
                        Bitmap b = pictureBox1.Image as Bitmap;
                        Color c = b.GetPixel(e.X, e.Y);
                        pictureBox1.Image = PaintBucket(b, c, new Point(e.X, e.Y));
                    });
                    thread.Start();
                    break;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            switch (toolBox)
            {
                case ToolBox.pen:
                    if (!canDraw)
                        break;

                    Bitmap b = pictureBox1.Image as Bitmap;
                    pictureBox1.Image = Pen(b, e);
                    break;
            }

            prevX = e.X;
            prevY = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            canDraw = false;          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.OK)
                return;

            colorA = colorDialog1.Color;      
            pictureBox2.Image = CreateEmptyBmp(colorA, 50, 50);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            toolBox = ToolBox.pen;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            toolBox = ToolBox.paintBucket;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = CreateEmptyBmp(Color.White, panel1.Width, panel1.Height);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (thread != null && thread.ThreadState != ThreadState.Running)
                canUseToolBox = true;

            flowLayoutPanel1.Enabled = canUseToolBox;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            pictureBox1.Image.Save(saveFileDialog1.FileName);
            saveFileDialog1 = new SaveFileDialog();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            Bitmap b = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
            pictureBox1.Image = b;
            pictureBox1.Width = b.Width;
            pictureBox1.Height = b.Height;
            panel1.AutoScrollMinSize = new Size(b.Width, b.Height);
            openFileDialog1 = new OpenFileDialog();
        }
    }
}
