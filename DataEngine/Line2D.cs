using GameEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DataEngine {
    public class Line2D {

        public Vector2D A;
        public Vector2D B;

        public Line2D(Vector2D A, Vector2D B) {
            this.A = A;
            this.B = B;
        }

        public Boolean Intersects(Line2D Line) {
            Console.WriteLine($"A1{this.A.X}-{this.A.Y}, B1{this.B.X}-{this.B.Y}, A2{Line.A.X}-{Line.A.Y}, B2{Line.B.X}-{Line.B.Y}");
            return Util.LineSegmentsIntersect(this.A, this.B, Line.A, Line.B);
        }
        public Boolean Intersects(System.Drawing.Rectangle R) {
            Vector2D VectorA = new Vector2D(R.X, R.Y);
            Vector2D VectorB = new Vector2D(R.X + R.Width, R.Y);
            Vector2D VectorC = new Vector2D(R.X, R.Y + R.Height);
            Vector2D VectorD = new Vector2D(R.X + R.Width, R.Y + R.Height);
            Line2D LineA = new Line2D(VectorA, VectorB);
            Line2D LineB = new Line2D(VectorB, VectorD);
            Line2D LineC = new Line2D(VectorD, VectorC);
            Line2D LineD = new Line2D(VectorA, VectorC);

            Console.WriteLine($"LineA: {Intersects(LineA)} LineB: {Intersects(LineB)} LineC: {Intersects(LineC)} LineD: {Intersects(LineD)}");
            return Intersects(LineA) || Intersects(LineB) || Intersects(LineC) || Intersects(LineD);
        }
    }
}
