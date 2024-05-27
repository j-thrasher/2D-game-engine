using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Renderer {
    public class Polygon2D {

        private List<PointF> Points = new List<PointF>();


        public Polygon2D(Bitmap bmp, int XScale, int YScale) {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            byte[] r = new byte[bytes / 3];
            byte[] g = new byte[bytes / 3];
            byte[] b = new byte[bytes / 3];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int count = 0;
            int stride = bmpData.Stride;

            Boolean[,] pixels = new Boolean[bmpData.Height,bmpData.Width];
            Boolean[,] optimizedPixels = new Boolean[bmpData.Height, bmpData.Width];
            for (int column = 0; column < bmpData.Height; column++) {
                for (int row = 0; row < bmpData.Width; row++) {
                    b[count] = rgbValues[(column * stride) + (row * 3)];
                    g[count] = rgbValues[(column * stride) + (row * 3) + 1];
                    r[count] = rgbValues[(column * stride) + (row * 3) + 2];
                    Boolean NotEmpty = (r[count] + b[count] + g[count]) > 0;
                    pixels[column, row] = NotEmpty;
                    optimizedPixels[column, row] = false;
                    count++;
                }
            }

            int previousFirstL = 0;
            int previousLastL = 0;
            for (int k = 0; k < pixels.GetLength(0); k++) {
                Boolean AlreadyAdded = false;
                int lastK = 0;
                int lastL = 0;
                int firstK = 0;
                int firstL = 0;
                for (int l = 0; l < pixels.GetLength(1); l++) {
                    Boolean val = pixels[k, l];
                    if(val) {
                        if (!AlreadyAdded) {
                            optimizedPixels[k, l] = true;
                            AlreadyAdded = true;
                            firstK = k;
                            firstL = l;
                        }
                        lastK = k;
                        lastL = l;
                    }
                }
                if(lastL != 0) {
                    optimizedPixels[k, lastL] = true;
                }/*
                if(k > 0) {
                    if(firstL == previousFirstL) {
                        optimizedPixels[firstK, firstL] = false;
                    }
                    if (lastL == previousLastL) {
                        optimizedPixels[lastK, lastL] = false;
                    }
                }
                previousFirstL = firstL;
                previousLastL = lastL;*/
            }

            List<Vector2D> firstHalfPoints = new List<Vector2D>();
            List<Vector2D> lastHalfPoints = new List<Vector2D>();

            for (int k = 0; k < optimizedPixels.GetLength(0); k++) {
                for (int l = 0; l < optimizedPixels.GetLength(1); l++) {
                    Boolean val = optimizedPixels[k, l];
                    if (val) {
                        firstHalfPoints.Add(new Vector2D(k, l));
                        break;
                    }
                }

                for (int l = optimizedPixels.GetLength(1) - 1; l >= 0; l--) {
                    Boolean val = optimizedPixels[k, l];
                    if (val) {
                        lastHalfPoints.Insert(0, new Vector2D(k, l));
                        break;
                    }
                }
            }

            foreach (Vector2D v in firstHalfPoints) {
                this.Points.Add(new PointF(v.Y * YScale, v.X * XScale));
            }

            foreach (Vector2D v in lastHalfPoints) {
                this.Points.Add(new PointF(v.Y * YScale, v.X * XScale));
            }
        }

        public Polygon2D() { }

        public PointF[] GetPoints() {
            PointF[] floats = new PointF[Points.Count];
            for (int i = 0; i < Points.Count; i++) {
                floats[i] = Points[i];
            }
            return floats;
        }
        public PointF[] GetPoints(float XOffset, float YOffset) {
            PointF[] floats = new PointF[Points.Count];
            for (int i = 0; i < Points.Count; i++) {
                PointF current = Points[i];
                floats[i] = new PointF(current.X + XOffset, current.Y + YOffset);
            }
            return floats;
        }


        public List<PointF> GetPointList() {
            return this.Points;
        }

        public void AddPoint(PointF point) {
            this.Points.Add(point);
        }

        public void AddPoint(float x, float y) {
            this.Points.Add(new PointF(x, y));
        }

        public Rectangle getBoundBox() {
            
            int MinX = int.MaxValue;
            int MinY = int.MaxValue;
            int MaxX = 0;
            int MaxY = 0;
            foreach(PointF p in this.Points) {
                int currentY = (int) p.Y;
                int currentX = (int) p.X;

                if (currentX > MaxX) MaxX = currentX;
                if (currentY > MaxY) MaxY = currentY;

                if (currentX < MinX) MinX = currentX;
                if (currentY < MinY) MinY = currentY;
            }
            return new Rectangle(MinX, MinY, MaxX - MinX, MaxY - MinY);
        }
/*
        public Boolean Intersects(Polygon2D P) {

            //check to see if bounding boxs touch
            //optimizes itterations by simplication
            Rectangle thisRect = getBoundBox();
            Rectangle otherRect = P.getBoundBox();
            Boolean BoundingHit = Util.RectanglesIntersect(thisRect, otherRect);
            

            //if the bounding boxes arent hitting, then no way the polys themselves hit
            if(!BoundingHit) {
                return false;
            }

            foreach(PointF point in P.GetPoints()) {
                if (this.ContainsPoint(point)) return true;
            }
            return false;
        }*/

        public bool ContainsPoint(PointF Point) {
            bool result = false;
            PointF[] polygon = GetPoints();
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++) {
                if (polygon[i].Y < Point.Y && polygon[j].Y >= Point.Y || polygon[j].Y < Point.Y && polygon[i].Y >= Point.Y) {
                    if (polygon[i].X + (Point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < Point.X) {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }


        public Boolean Intersects(Polygon2D Poly) {

            GraphicsPath G1 = new GraphicsPath();
            GraphicsPath G2 = new GraphicsPath();

            PointF[] thesePoints = GetPoints();
            PointF[] otherPoints = Poly.GetPoints();

            for (int i = 0; i < thesePoints.Length - 1; i++) {
                G1.AddLine(thesePoints[i], thesePoints[i+1]);
            }
            for (int i = 0; i < otherPoints.Length - 1; i++) {
                G2.AddLine(otherPoints[i], otherPoints[i + 1]);
            }

            Region R1 = new Region(G1);
            Region R2 = new Region(G2);

            R1.Intersect(R2);

            return R1.GetRegionScans(new Matrix()).Length > 0;
        }
    }
}
