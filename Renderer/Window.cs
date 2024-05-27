using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Drawing.Drawing2D;
using GameEngine.DataEngine;



//WRONG
//MPST is much faster if simply done with a timespan and delta calculation

/*
Minimum MPST is around 12-15ms due to making the update threads sleep for a millisecond
https://stackoverflow.com/questions/6254703/thread-sleep-for-less-than-1-millisecond
could probably solve it via a nanosecond Delta counter, but would kill CPU so here we are
 */




namespace GameEngine.Renderer {

    public class Canvas : Form {

        public Boolean IsNull = true;
        public Canvas() {
            this.DoubleBuffered = true;
            this.IsNull = false;
        }
    }


    public abstract class Window {



        //form vars
        private String Title;
        private Vector2D ScreenSize;
        private Canvas Canvas;
        public Color Bg = Color.FromArgb(86, 147, 64);
        public Camera Cam;
        public Boolean Debug = true;
        public Boolean FullScreen = true;
        public static int TicksPerSecond = 50;
        public static int FramesPerSecond = 100;


        //runtime logic
        private Thread RenderThread = null;
        private Thread UpdateThread = null;
        private Thread DebugThread = null;
        private Dictionary<String, Boolean> Keyboard = new Dictionary<String, Boolean>();
        private Series FrameRateSeries = null;
        private Player PlayerRef;
        public static float SimulationDistance = 300f;
        public static float RenderDistance = 500f;



        //render layers
        //private Dictionary<int, Dictionary<String, Player>> Players = new Dictionary<int, Dictionary<String, Player>>();
        private Dictionary<int, Dictionary<String, Entity>> Entities = new Dictionary<int, Dictionary<String, Entity>>();
        private Dictionary<int, Dictionary<String, Sprite2D>> culledLayers = new Dictionary<int, Dictionary<String, Sprite2D>>();
        public static int PREGROUND = 0;
        public static int GROUND = 1;
        public static int POSTGROUND = 2;

        public static int PREOBJECT = 3;
        public static int OBJECT = 4;
        public static int POSTOBJECT = 5;

        public static int PREPROJECTILE = 6;
        public static int PROJECTILE = 7;
        public static int POSTPROJECTILE = 8;

        public static int PREMOB = 9;
        public static int MOB = 10;
        public static int POSTMOB = 11;

        public static int PREPLAYER = 12;
        public static int PLAYER = 13;
        public static int POSTPLAYER = 14;

        public static int layerCount = 15;



        //debug vars
        public Dictionary<String, Series> DebugSeries = new Dictionary<String, Series>();
        public static Color DebugColor = Color.FromArgb(255, 0, 255);
        public static Color DebugBackGroundColor = Color.FromArgb(28, 0, 47);
        public static Font debugFont = new Font("Consolas", 24, FontStyle.Regular, GraphicsUnit.Pixel);

        public static Pen DebugPen = new Pen(DebugColor);
        public static SolidBrush DebugBrush = new SolidBrush(DebugColor);

        public Window(Vector2D ss, string t) {

            //make the canvas
            this.Title = t;
            this.ScreenSize = ss;
            if (this.ScreenSize.LessThan(new Vector2D(400, 300))) {
                ScreenSize = new Vector2D(400, 300);
            }

            this.Canvas = new Canvas();

            Canvas.Size = new Size((int)this.ScreenSize.X, (int)this.ScreenSize.Y);
            Canvas.Text = this.Title;
            Canvas.Paint += Render;
            Canvas.KeyDown += HandleKeyDown;
            Canvas.KeyUp += HandleKeyUp;
            Canvas.FormClosed += new FormClosedEventHandler(this.Exit);
            Canvas.MinimumSize = new Size(400, 300);
            Canvas.StartPosition = FormStartPosition.CenterScreen;
            Canvas.Text = Title;
            if (FullScreen) Canvas.FormBorderStyle = FormBorderStyle.None;
            this.Cam = new Camera(new Vector2D(0, 0), ScreenSize);

            Screen s = Screen.FromControl(getScreenControl());
            Console.WriteLine(s.ToString());


            //add the debug info to the series counter
            this.DebugSeries.Add("Render", new Series("Render"));
            this.FrameRateSeries = new Series("Frames");
            this.DebugSeries.Add("Update", new Series("Update"));
            this.DebugSeries.Add("MSPT", new Series("MSPT"));


            //run onload stuff
            Onload();

            //create the threads, and start them
            UpdateThread = new Thread(UpdateTick);
            DebugThread = new Thread(DebugUpdate);
            RenderThread = new Thread(RenderUpdate);
            UpdateThread.Start();
            DebugThread.Start();
            RenderThread.Start();

            //finally, start running the screen
            Application.Run(Canvas);
        }

        public abstract void Onload();
        public abstract void OnUpdate();

        public Vector2D GetScreenSize() {
            return this.ScreenSize;
        }

        public Control getScreenControl() {
            return this.Canvas;
        }

        private void Exit(object sender, EventArgs e) {
            this.RenderThread.Abort();
            Environment.Exit(Environment.ExitCode);
        }

        public Boolean IsKeyPushed(String keyCode) {
            return (this.Keyboard.ContainsKey(keyCode) ? Keyboard[keyCode] : false);
        }

        public void HandleKeyDown(object sender, KeyEventArgs e) {
            String key = e.KeyCode + "";
            if (this.Keyboard.ContainsKey(key)) {
                this.Keyboard[key] = true;
            } else {
                this.Keyboard.Add(key, true);
            }
        }

        public void HandleKeyUp(object sender, KeyEventArgs e) {
            String key = e.KeyCode + "";
            if (this.Keyboard.ContainsKey(key)) {
                this.Keyboard[key] = false;
            } else {
                this.Keyboard.Add(key, false);
            }
        }

        public void UpdateTick() {

            long millisPerTick = (1000 / TicksPerSecond) * 10000;
            long lastNanoTime = Util.nanoTime();
            long minNextNanoTime = lastNanoTime + millisPerTick;
            long afterUpdate = 0L, beforeUpdate = 0L;
            while (RenderThread.IsAlive) {
                try {

                    long now = Util.nanoTime();
                    beforeUpdate = now;
                    if (now > minNextNanoTime) {
                        minNextNanoTime = now + millisPerTick;

                        OnUpdate();

                        long afterCall = Util.nanoTime();

                        this.DebugSeries["Update"].Add((afterCall - beforeUpdate) / 10000f);
                    }
                    afterUpdate = Util.nanoTime();
                    this.DebugSeries["MSPT"].Add((afterUpdate - beforeUpdate) / 10000f);


                    //this is to limit loops on the cpu
                    //if it didnt sleep this would run as fast as it can and kill cpu
                    //this isnt added to the MSPT/UPDATE timing in debug mode
                    Thread.Sleep(new TimeSpan(1000L));


                } catch(Exception e) { Console.WriteLine($"Error in Logic Loop: {e.Message}"); }
            }
        }

        public void RenderUpdate() {


            long millisPerTick = (1000 / FramesPerSecond) * 10000;
            long lastNanoTime = Util.nanoTime();
            long minNextNanoTime = lastNanoTime + millisPerTick;
            long afterUpdate = 0L, beforeUpdate = 0L;

            while (RenderThread.IsAlive) {
                try {
                    long now = Util.nanoTime();
                    beforeUpdate = now;
                    if (now > minNextNanoTime) {
                        minNextNanoTime = now + millisPerTick;

                        //here we have to check if the form is ready to handle the BeginInvoke call
                        //if it is, simply refresh the screen (Render method)
                        if (Canvas.IsHandleCreated) Canvas.BeginInvoke((MethodInvoker)delegate { Canvas.Refresh(); });

                        this.FrameRateSeries.Add();
                        long afterCall = Util.nanoTime();
                        this.DebugSeries["Render"].Add((afterCall - beforeUpdate) / 10000f);
                    }
                    afterUpdate = Util.nanoTime();
                    this.DebugSeries["MSPT"].Add((afterUpdate - beforeUpdate) / 10000f);


                    //this is to limit loops on the cpu
                    //if it didnt sleep this would run as fast as it can and kill cpu
                    //this isnt added to the MSPT/UPDATE timing in debug mode
                    Thread.Sleep(new TimeSpan(1000L));

                } catch (Exception e) { Console.WriteLine($"Error in Logic Loop: {e.Message}"); }

            }
        }

        public void DebugUpdate() {

            while(DebugThread.IsAlive) {
                foreach (String s in DebugSeries.Keys.ToList()) {
                    Series Current = DebugSeries[s];
                    this.DebugSeries[s].Truncate();
                }
                Thread.Sleep(5000);
            }
        }


        public void Register(Object obj) {
            //Console.WriteLine("Trying to register: " + obj);
            if(((Entity)obj).Tag.Equals("Mob")) {
                Mob c = (Mob)obj;
                Sprite2D s = c.GetSprite();
                if (this.Entities.ContainsKey(s.Layer)) {
                    this.Entities[s.Layer][c.ID] = c;
                } else {
                    this.Entities[s.Layer] = new Dictionary<string, Entity>();
                    this.Entities[s.Layer][c.ID] = c;
                }
            }

            if(((Entity)obj).Tag.Equals("Player")) {
                Player c = ((Player)obj);
                this.PlayerRef = c;
            }
        }

        public void Unregister(Object obj) {
            if (obj is Entity) {
                Entity c = (Entity)obj;
                Sprite2D s = c.GetSprite();
                this.Entities[s.Layer].Remove(c.ID);
                Log("Unregistered Sprite: " + c.ID);
            }
            if (obj is Player) {
                Player c = ((Player)obj);
                
                Log("Unregistered Player: " + c.ID);
            }
        }

        public void Log(Object line) {
            Console.WriteLine($"[{Util.nowString()}] - {line}");
        }



        private void Render(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;

            g.Clear(this.Bg);




            foreach(int Layer in Entities.Keys.ToList()) {
                List<Entity> LayersEntities = Entities[Layer].Values.ToList();
                foreach (Entity ee in LayersEntities) {
                    if (this.Cam.Position.Distance(new Vector2D(ee.Position.X - this.PlayerRef.SpriteXOffset(), ee.Position.Y - this.PlayerRef.SpriteYOffset())) < Window.RenderDistance) {
                        
                        switch (ee.Tag) {
                            case ("Mob"):
                                renderMob((Mob)ee);
                                break;
                        }
                    }
                }
            }


            if (this.PlayerRef != null) {
                renderPlayer(this.PlayerRef);
            }




            if (Debug) {

                int tick = 0;

                double Avg = Math.Round(this.FrameRateSeries.AveragePerSecond(), 0);
                g.DrawString($"FPS:{Avg}", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick));
                tick++;

                foreach (String s in DebugSeries.Keys.ToList()) {
                    Series Current = DebugSeries[s];
                    g.DrawString($"{s}: {Math.Round(Current.Average(), 5)}", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick));
                    tick++;
                }

                g.DrawString($"Player", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick)); tick++;
                g.DrawString($"        -State [{this.PlayerRef.State.Form}, {this.PlayerRef.State.Count}]", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick)); tick++;
                g.DrawString($"         -Vuln [{this.PlayerRef.Vuln}]", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick)); tick++;
                g.DrawString($"         -Anim [{this.PlayerRef.CurrentAnimName()}][{this.PlayerRef.CurrentFrame()}][{this.PlayerRef.CurrentFrameCount()}]", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick)); tick++;
                g.DrawString($"-IdleSaveState [{this.PlayerRef.State.IdleSaveState()}]", Window.debugFont, DebugBrush, new Point(0, Window.debugFont.Height * tick)); tick++;
            }



            void renderMob(Mob m) {
                float XOffset = this.Cam.XOffset() - this.PlayerRef.SpriteXOffset();
                float YOffset = this.Cam.YOffset() - this.PlayerRef.SpriteYOffset();
                Sprite2D s = m.GetSprite();
                g.DrawImage(s.Sprite, m.Position.X + XOffset, m.Position.Y + YOffset, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale);
                if (Debug) {
                    PointF[] hitBox = m.GetGlobalHitbox(XOffset, YOffset).GetPoints();
                    if (hitBox.Length > 0) {
                        g.DrawPolygon(DebugPen, hitBox);
                    }
                }
            }

            void renderPlayer(Player p) {
                Sprite2D s = p.GetSprite();
                int MiddleScreenX = (int)(ScreenSize.X / 2);
                int MiddleScreenY = (int)(ScreenSize.Y / 2);
                int SpriteOffsetX = (int)(s.Sprite.Width * s.XScale) / 2;
                int SpriteOffsetY = (int)(s.Sprite.Height * s.XScale) / 2;
                g.DrawImage(s.Sprite, MiddleScreenX - SpriteOffsetX, MiddleScreenY - SpriteOffsetY, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale);
                if (Debug) {

                    float XOffset = this.Cam.XOffset() - this.PlayerRef.SpriteXOffset();
                    float YOffset = this.Cam.YOffset() - this.PlayerRef.SpriteYOffset();
                    PointF[] hitBox = p.GetGlobalHitbox(XOffset, YOffset).GetPoints();
                    if (hitBox.Length > 0) {
                        g.DrawPolygon(DebugPen, hitBox); 
                        //g.DrawRectangle(DebugPen, s.GetGlobalBoundingBox());
                    }
                    //g.DrawEllipse(DebugPen, new Rectangle((int) (MiddleScreenX - SpriteOffsetX - (Window.SimulationDistance/2)), (int) (MiddleScreenY - SpriteOffsetY - (Window.SimulationDistance / 2)), (int) Window.SimulationDistance, (int) Window.SimulationDistance));
                    //g.DrawRectangle(DebugPen, new Rectangle(MiddleScreenX - SpriteOffsetX, MiddleScreenY - SpriteOffsetY, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale));
                }
            }
        }
    }
}


/*

}*/