using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Renderer {
    public class Animation {


        private int FrameRate = 0;
        private List<Sprite2D> Sprites = new List<Sprite2D>();

        public Animation(int FrameRate, int SpriteCount) {
            this.FrameRate = FrameRate;
            for(int i = 0; i < SpriteCount; i++) {
                this.Sprites.Add(new Sprite2D());
            }
        }
        public Animation(int FrameRate) {
            this.FrameRate = FrameRate;
        }

        public int FrameCount() {
            return this.Sprites.Count();
        }

        public void AddSprite(Sprite2D Sprite) {
            this.Sprites.Add(Sprite);
        }

        public int RelativeTick(int tick) {
            return tick % (this.Sprites.Count * this.FrameRate);
        }

        public int GetRelativeFrame(int tick) {
            int rTick = RelativeTick(tick);
            return rTick / FrameRate;
        }

        public Sprite2D GetSprite(int tick) {
            if (this.Sprites.Count <= 0 || this.Sprites.Count < tick) {
                return null;
            }
            return this.Sprites[tick];
        }


        public void AddSpritesViaSheet(String Dir, int Layer, int Rows, int Columns) {
            Image i = Image.FromFile($"Res/Sprites/{Dir}.png");
            Bitmap bmp = new Bitmap(i);

            int Width = bmp.Width / Columns;
            int Height = bmp.Height / Rows;


            for (int Column = 0; Column < Columns; Column++) {
                for(int Row = 0; Row < Rows; Row++) {
                    Bitmap Cropped = bmp.Clone(new Rectangle(Column * Width, Row * Height, Width, Height), bmp.PixelFormat);
                    this.Sprites.Add(new Sprite2D(Cropped, Layer));
                }
            }
        }
    }
}
