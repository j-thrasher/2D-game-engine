using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Renderer {
    public class Vector2D {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2D() {
            Zero();
        }

        public Vector2D(float X, float Y) {
            this.X = X;
            this.Y = Y;

        }

        public void Zero() {
            this.X = 0f;
            this.Y = 0f;
        }

        public Point ToPoint() {
            return new Point((int) this.X, (int)this.Y);
        }

        public void Add(Vector2D Vector) {
            this.X += Vector.X;
            this.Y += Vector.Y;
        }

        public void Subtract(Vector2D Vector) {
            this.X -= Vector.X;
            this.Y -= Vector.Y;
        }

        public static Vector2D StaticAdd(Vector2D A, Vector2D B) {
            float NewX = A.X + B.X;
            float NewY = A.Y + B.Y;
            return new Vector2D(NewX, NewY);
        }
        public static Vector2D StaticSubtract(Vector2D A, Vector2D B) {

            float NewX = A.X - B.X;
            float NewY = A.Y - B.Y;
            return new Vector2D(NewX, NewY);
        }

        public static Vector2D StaticDivide(Vector2D A, int scale) {
            float NewX = A.X / scale;
            float NewY = A.Y / scale;
            return new Vector2D(NewX, NewY);
        }


        public void Scalar(int Scalar) {
            this.X *= Scalar;
            this.Y *= Scalar;
        }

        /// <summary>
        /// returns true if and only if both X and Y of passed Vector are larger
        /// </summary>
        /// <param name="Vector"></param>
        public Boolean GreaterThan(Vector2D Vector) {
            return (this.X > Vector.X) && (this.Y > Vector.Y);
        }

        /// <summary>
        /// returns true if and only if both X and Y of passed Vector are smaller
        /// </summary>
        /// <param name="Vector"></param>
        public Boolean LessThan(Vector2D Vector) {
            return (this.X < Vector.X) && (this.Y < Vector.Y);
        }

        /// <summary>
        /// returns true if and only if both X and Y of passed Vector are smaller
        /// </summary>
        /// <param name="Vector"></param>
        public Boolean LessThan(Size s) {
            return (this.X < s.Width) && (this.Y < s.Height);
        }


        public float Distance(Vector2D Vector) {
            float x1 = this.X;
            float x2 = Vector.X;
            float y1 = this.Y;
            float y2 = Vector.Y;
            return (float) Math.Sqrt((((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1))));
        }

    }
}
