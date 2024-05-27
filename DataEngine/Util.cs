using GameEngine.DataEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GameEngine.Renderer {
    public class Util {

        public static String datetimeFormat = "yyyy-MM-dd HH:mm:ss:fff";


        public static Boolean RectanglesIntersect(System.Drawing.Rectangle A, System.Drawing.Rectangle B) {
            Vector2D VectorA = new Vector2D(B.X, B.Y);
            Vector2D VectorB = new Vector2D(B.X + B.Width, B.Y);
            Vector2D VectorC = new Vector2D(B.X, B.Y + B.Height);
            Vector2D VectorD = new Vector2D(B.X + B.Width, B.Y + B.Height);
            Line2D LineA = new Line2D(VectorA, VectorB);
            Line2D LineB = new Line2D(VectorB, VectorD);
            Line2D LineC = new Line2D(VectorC, VectorD);
            Line2D LineD = new Line2D(VectorA, VectorC);
            //Console.WriteLine($"LineA: {LineA.Intersects(A)} LineB: {LineB.Intersects(A)} LineC: {LineC.Intersects(A)} LineD: {LineD.Intersects(A)} ");
            return LineA.Intersects(A) || LineB.Intersects(A) || LineC.Intersects(A) || LineD.Intersects(A);
        }


        public static Vector2D WhereLinesIntersect(Vector2D lineA1, Vector2D lineA2, Vector2D lineB1, Vector2D lineB2) {
            float A1 = lineA2.Y - lineA1.Y;
            float B1 = lineA1.X - lineA2.X;
            float C1 = (A1 * lineA1.X) + (B1 * lineA1.Y);

            float A2 = lineB2.Y - lineB1.Y;
            float B2 = lineB1.X - lineB2.X;
            float C2 = (A2 * lineB1.X) + (B2 * lineB1.Y);

            float delta = (A1 * B2) - (A2 * B1);
            if(delta == 0) {
                return null;
            } else {
                float x = ((B2 * C1) - (B1 * C2)) / delta;
                float y = ((A1 * C2) - (A2 * C1)) / delta;
                return new Vector2D(x, y);
            }
        }

        public static Boolean LineSegmentsIntersect(Vector2D lineA1, Vector2D lineA2, Vector2D lineB1, Vector2D lineB2) {
            Vector2D intersectPoint = WhereLinesIntersect(lineA1, lineA2, lineB1, lineB2);
            if (intersectPoint == null) {
                return false;
            }
            Boolean OnlineA = false;
            Boolean OnLineB = false;
            OnlineA = (lineA1.X > intersectPoint.X && intersectPoint.X > lineA2.X) || (lineA1.X < intersectPoint.X && intersectPoint.X < lineA2.X);
            OnLineB = (lineB1.X > intersectPoint.X && intersectPoint.X > lineB2.X) || (lineB1.X < intersectPoint.X && intersectPoint.X < lineB2.X);
            return OnlineA && OnLineB;
        }

        public static float YIntercept(Vector2D a, Vector2D b) {
            float slope = SlopeOf2Points(a, b);
            float intercept = 0f;
            intercept = slope * a.X;
            intercept = a.Y - intercept;
            return intercept;
        }

        public static float SlopeOf2Points(Vector2D a, Vector2D b) {
            float rise = (a.Y - b.Y);
            float run = (a.X - b.X);
            if(rise == 0f || run == 0f) {
                return 0f;
            }
            return rise / run;
        }

        public static string SHA256Hash(string rawData) {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create()) {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static long getSeconds() {
            double timestamp = Stopwatch.GetTimestamp();
            double seconds = timestamp / Stopwatch.Frequency;
            return (long)seconds;
        }

        public static DateTime addTimeToDate(DateTime date, long offset) {
            return new DateTime(date.Ticks + (offset * 10000));
        }

        public static Boolean comesBefore(DateTime before, DateTime after) {
            return before.Ticks < after.Ticks;
        }

        public static string nowString() {
            return DateTime.Now.ToString(datetimeFormat);
        }

        public static DateTime now() {
            return DateTime.Now;
        }

        public static DateTime stringToDate(String s) {
            return DateTime.Parse(s);
        }
        public static String dateToString(DateTime d) {
            return d.ToString(datetimeFormat);
        }

        public static long currentMilli() {
            string last = nowString().Substring(nowString().LastIndexOf(":") + 1);
            return long.Parse(last);
        }
        public static long nanoTime() {
            return Stopwatch.GetTimestamp();
        }

        public static Boolean PointWithinRectangle(System.Drawing.Rectangle R, int X, int Y) {
            int MinX = R.X;
            int MaxX = MinX + R.Width;
            int MinY = R.Y;
            int MaxY = MinY + R.Height;
            return (X >= MinX && X <= MaxX) && (Y >= MinY && Y <= MaxY);
        }

        public static Boolean PointWithinRectangle(System.Drawing.Rectangle R, Point P) {
            int MinX = R.X;
            int MaxX = MinX + R.Width;
            int MinY = R.Y;
            int MaxY = MinY + R.Height;
            return (P.X >= MinX && P.X <= MaxX) && (P.Y >= MinY && P.Y <= MaxY);
        }

        public static Boolean PointWithinRectangle(System.Drawing.Rectangle R, Vector2D V) {
            int MinX = R.X;
            int MaxX = MinX + R.Width;
            int MinY = R.Y;
            int MaxY = MinY + R.Height;
            return (V.X >= MinX && V.X <= MaxX) && (V.Y >= MinY && V.Y <= MaxY);
        }

        public static List<Vector2D> GetPointsOfRectangle(System.Drawing.Rectangle R) {
            List<Vector2D> points = new List<Vector2D>();
            int MinX = R.X;
            int MaxX = MinX + R.Width;
            int MinY = R.Y;
            int MaxY = MinY + R.Height;
            points.Add(new Vector2D(MinX, MinY));
            points.Add(new Vector2D(MinX, MaxY));
            points.Add(new Vector2D(MaxX, MinY));
            points.Add(new Vector2D(MaxX, MaxY));
            return points;
        }




        public static bool IsPointInPolygon4(PointF[] polygon, PointF testPoint) {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++) {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y) {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X) {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }







    }
}
