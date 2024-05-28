using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace GameEngine.Renderer {
    public class Sprite2D {


        public Bitmap Sprite = null;
        private Polygon2D HitBox;
        public int Layer = Window.PREGROUND;
        public int XScale = 4;
        public int YScale = 4;
        public string Dir = "";

        public Sprite2D() { }

        public Sprite2D(String Dir, int Layer) {
            this.Dir = Dir;
            this.Layer = Layer;
            Image i = Image.FromFile($"Res/Sprites/{Dir}.png");
            this.Sprite = new Bitmap(i);
            this.HitBox = new Polygon2D(new Bitmap(i), this.XScale, this.YScale);
        }

        public Sprite2D(Image image, int Layer) {
            this.Layer = Layer;
            this.Sprite = new Bitmap(image);
            this.HitBox = new Polygon2D(new Bitmap(image), this.XScale, this.YScale);
        }
        
        public Sprite2D(Bitmap bmp, int Layer, Boolean b) {
            this.Layer = Layer;
            this.Sprite = bmp;
            this.HitBox = new Polygon2D(bmp, this.XScale, this.YScale);
        }

        public Polygon2D GeneratedHitBox() {
            return this.HitBox;
        }
    }
}
