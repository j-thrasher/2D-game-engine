
using GameEngine.DataEngine;
using System;
using System.Drawing;
using System.Windows.Media;

namespace GameEngine.Renderer.Noise {
    public class NoiseMap2D : Entity {

        public int Width = 0;
        public int Height = 0;
        public float[,] Points = new float[0, 0];
        public float Scale = 0.0f;

        public override void HandleUpdate() { }

        public override void HandleStateUpdate() { }

        public override bool HandleCollision(CollisionEventArgs Collision) {
            return false;
        }


        public NoiseMap2D(Vector2D Pos, int Width, int Height, float Scale = 0.01f) : base (Pos, "NoiseMap", null) {
            this.Width = Width;
            this.Height = Height;
            this.Scale = Scale;
            this.Points = new float[Width, Height];
            this.Position = Pos;
            GenerateMap();
            GenerateSprite();
        }

        public void GenerateSprite() {

            Bitmap bmp = new Bitmap(Width, Height);

            using (Graphics graph = Graphics.FromImage(bmp)) {
                Rectangle ImageSize = new Rectangle(0, 0, Width, Height);
                graph.FillRectangle(System.Drawing.Brushes.White, ImageSize);



                for (var i = 0; i < this.Points.GetLength(0); i++) {
                    for (var j = 0; j < this.Points.GetLength(1); j++) {
                        int MaxColorValue = 255;
                        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(((int)Points[i, j]), ((int)Points[i, j]), ((int)Points[i, j])));


                        graph.FillRectangle(myBrush, new Rectangle(i, j, 1, 1));
                    }
                }

            }

            SetSprite(new Sprite2D(bmp, Window.GROUND));

        }

        private void GenerateMap() {
            Noise s = new Noise();
            s.Seed = (int) (Util.nanoTime() / 100000);
            s.Calc2D(ref this.Points, this.Scale);
        }

        public Boolean ContainsPoint(float X, float Y) {
            Boolean ContainsX = this.Points.GetLength(0) > 0 && this.Points.GetLength(0) > X;
            Boolean ContainsY = this.Points.GetLength(1) > 0 && this.Points.GetLength(1) > Y;
            return ContainsX && ContainsY;
        }

        public static NoiseMap2D BlendMaps(NoiseMap2D MapA, NoiseMap2D MapB, float WeightA, float WeightB) {
            NoiseMap2D resultant = new NoiseMap2D(MapA.Position, MapA.Width, MapA.Height);

            float TotalWeight = WeightA + WeightB;
            float BlendedWeightA = WeightA / TotalWeight;
            float BlendedWeightB = WeightB / TotalWeight;


            for (var i = 0; i < MapA.Points.GetLength(0); i++) {
                for (var j = 0; j < MapA.Points.GetLength(1); j++) {
                    if(MapB.ContainsPoint(i, j)) {
                        float ValueA = MapA.Points[i, j] * BlendedWeightA;
                        float ValueB = MapB.Points[i, j] * BlendedWeightB;
                        resultant.Points[i, j] = ValueA + ValueB;
                    }
                    else {
                        resultant.Points[i, j] = MapA.Points[i, j];
                    }
                }
            }
            resultant.GenerateSprite();
            return resultant;
        }

        public void ClampValues(int clamp) {
            for (var i = 0; i < this.Points.GetLength(0); i++) {
                for (var j = 0; j < this.Points.GetLength(1); j++) {
                    int ScaledDown = (int)Points[i, j] / clamp;
                    Points[i, j] = ScaledDown * clamp;
                }
            }
            GenerateSprite();
        }
    }
}
