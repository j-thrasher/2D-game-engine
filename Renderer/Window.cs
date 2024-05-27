﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Drawing.Drawing2D;
using GameEngine.DataEngine;
using System.CodeDom;
using System.Windows.Controls;



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
        public readonly int FramesPerSecond = 100;
        public int DisplayRefreshRate = 0;

        //runtime logic
        private Thread RenderThread = null;
        private Thread UpdateThread = null;
        private Thread DebugThread = null;
        private Dictionary<String, Boolean> Keyboard = new Dictionary<String, Boolean>();
        private Series FrameRateSeries = null;
        private Player PlayerRef;
        public static float SimulationDistance = 300f;
        public static float RenderDistance = 500f;
        public readonly float MAX_SERIES_UPDATE_TIME = 500f;

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
        private Dictionary<String, Series> DebugSeries = new Dictionary<String, Series> ();
        public static Color DebugColor = Color.FromArgb(255, 0, 255);
        public static Color DebugBackGroundColor = Color.FromArgb(0, 0, 0);
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
            //this.DisplayRefreshRate = DisplayMode.RefreshRate;

            this.Canvas = new Canvas();

            Canvas.Size = new Size((int)this.ScreenSize.X, (int)this.ScreenSize.Y);
            Canvas.Text = this.Title;
            Canvas.Paint += Render;
            Canvas.KeyDown += HandleKeyDown;
            Canvas.KeyUp += HandleKeyUp;
            Canvas.FormClosed += new FormClosedEventHandler(this.Exit);
            Canvas.Text = Title;
            Canvas.Resize += HandleResize;
            HandleGoFullScreen();
           



            //add the debug info to the series counter
            AddDebugSeries("Render");
            this.FrameRateSeries = new Series("Frames");
            AddDebugSeries("Update");
            AddDebugSeries("MSPT");


            //run onload stuff
            Onload();

            //create the threads, and start them
            UpdateThread = new Thread(UpdateTick);
            Thread.Sleep(new TimeSpan(1000L));
            DebugThread = new Thread(DebugUpdate);
            Thread.Sleep(new TimeSpan(1000L));
            RenderThread = new Thread(RenderUpdate);
            Thread.Sleep(new TimeSpan(1000L));
            UpdateThread.Start();
            DebugThread.Start();
            RenderThread.Start();

            //finally, start running the screen
            Application.Run(Canvas);
        }

        public abstract void Onload();
        public abstract void OnUpdate();

        public void AddDebugSeries(String SeriesName) {
            Console.WriteLine($"Added debug series: {SeriesName}");
            this.DebugSeries.Add(SeriesName, new Series(SeriesName));
        }

        public void AddDebugSeriesTiming(String SeriesName, long Before, long After) {
            if (!this.DebugSeries.ContainsKey(SeriesName)) {
                AddDebugSeries(SeriesName);
            }

            if(Before > After) {
                long temp = Before;
                Before = After;
                After = temp;
            }

            float DeltaTime = ((After - Before) / 10000f);

            if(DeltaTime > MAX_SERIES_UPDATE_TIME) {
                return;
            }

            this.DebugSeries[SeriesName].Add(DeltaTime);
        }

        public Vector2D GetScreenSize() {
            return this.ScreenSize;
        }

        public Canvas GetScreenControl() {
            return this.Canvas;
        }

        private void Exit(object sender, EventArgs e) {
            this.RenderThread.Abort();
            Environment.Exit(Environment.ExitCode);
        }

        public Boolean IsKeyPushed(String keyCode) {
            return (this.Keyboard.ContainsKey(keyCode) ? Keyboard[keyCode] : false);
        }

        private long LastResize = Util.nanoTime();
        private long ResizeCoolMillis = 100;
        public void HandleResize(object sender, EventArgs e) {
            long now = Util.nanoTime();
            int MillisSinceLastResize = (int) ((now - LastResize) / 10000f);
            if(MillisSinceLastResize >= ResizeCoolMillis) {
                LastResize = now;

                HandleCameraMove();
                Console.WriteLine($"Resizing");
            }
        }

        public void HandleCameraMove() {

            this.Cam = new Camera(new Vector2D(0, 0), new Vector2D(this.Canvas.Size.Width, this.Canvas.Size.Height));
        }
 

        public void HandleGoFullScreen() {
            Screen s = Screen.FromControl(GetScreenControl());
            if (FullScreen) {
                Canvas.FormBorderStyle = FormBorderStyle.None;
                Canvas.ClientSize = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height + 1000);
                Canvas.StartPosition = FormStartPosition.Manual;
                Canvas.Location = new Point(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top);
                Canvas.SetDesktopLocation(0, 0);
            } else {
                Canvas.ClientSize = new Size(400, 300);
                Canvas.StartPosition = FormStartPosition.CenterScreen;
                Canvas.SetDesktopLocation(0, 0);
                Canvas.FormBorderStyle = FormBorderStyle.Sizable;
            }
            HandleCameraMove();
            Console.WriteLine(s.ToString());
        }

        public void HandleKeyDown(object sender, KeyEventArgs e) {
            String key = e.KeyCode + "";
            if(key.Equals("F11")) {
                this.FullScreen = !this.FullScreen;
                HandleGoFullScreen();
            }
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
                    if (now > minNextNanoTime) {

                        beforeUpdate = now;
                        minNextNanoTime = now + millisPerTick;

                        OnUpdate();

                        long afterCall = Util.nanoTime();
                        this.AddDebugSeriesTiming("Update", beforeUpdate, afterCall);
                    }
                    afterUpdate = Util.nanoTime();

                    this.AddDebugSeriesTiming("MSPT", beforeUpdate, afterUpdate);


                    //this is to limit loops on the cpu
                    //if it didnt sleep this would run as fast as it can and kill cpu
                    //this isnt added to the MSPT/UPDATE timing in debug mode
                    Thread.Sleep(new TimeSpan(1000L));


                } catch(Exception e) { Console.WriteLine($"Error in UpdateTick Loop: {e.Message}"); }
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
                        this.AddDebugSeriesTiming("Render", beforeUpdate, afterUpdate);
                    }
                    afterUpdate = Util.nanoTime();
                    this.AddDebugSeriesTiming("MSPT", beforeUpdate, afterUpdate);


                    //this is to limit loops on the cpu
                    //if it didnt sleep this would run as fast as it can and kill cpu
                    //this isnt added to the MSPT/UPDATE timing in debug mode
                    Thread.Sleep(new TimeSpan(1000L));

                } catch (Exception e) { Console.WriteLine($"Error in RenderUpdate Loop: {e.Message}"); }

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
                    float EntitiesRelativeX = ee.Position.X - ee.SpriteXOffset();
                    float EntitiesRelativeY = ee.Position.Y - ee.SpriteYOffset();
                    g.DrawRectangle(DebugPen, new Rectangle((int)(EntitiesRelativeX - 10 - this.PlayerRef.SpriteYOffset()), (int)(EntitiesRelativeY - 10 - this.PlayerRef.SpriteXOffset()), 20, 20));


                    if (this.Cam.Position.Distance(new Vector2D(EntitiesRelativeX - this.PlayerRef.SpriteXOffset(), EntitiesRelativeY - this.PlayerRef.SpriteYOffset())) < Window.RenderDistance) {
                        
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

                float XOffset = this.Cam.XOffset() - this.PlayerRef.SpriteXOffset();
                float YOffset = this.Cam.YOffset() - this.PlayerRef.SpriteYOffset();

                //g.DrawImage(s.Sprite, MiddleScreenX - SpriteOffsetX, MiddleScreenY - SpriteOffsetY, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale);
                g.DrawImage(s.Sprite, XOffset + p.Position.X, YOffset + p.Position.Y, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale);
                if (Debug) {

                    PointF[] hitBox = p.GetGlobalHitbox(XOffset, YOffset).GetPoints();
                    if (hitBox.Length > 0) {
                        g.DrawPolygon(DebugPen, hitBox); 
                        //g.DrawRectangle(DebugPen, s.GetGlobalBoundingBox());
                    }



                    Polygon2D polygon = Polygon2D.GenerateVariableResolutionCirclePoly(12, Window.RenderDistance);
                    PointF[] RenderDistanceDebugPoints = polygon.GetRelativePolygon(XOffset + p.Position.X, YOffset + p.Position.Y).GetPoints();
                    g.DrawPolygon(DebugPen, RenderDistanceDebugPoints);



                    //g.DrawEllipse(DebugPen, new Rectangle((int) (MiddleScreenX - SpriteOffsetX - (Window.SimulationDistance/2)), (int) (MiddleScreenY - SpriteOffsetY - (Window.SimulationDistance / 2)), (int) Window.SimulationDistance, (int) Window.SimulationDistance));
                    //g.DrawRectangle(DebugPen, new Rectangle(MiddleScreenX - SpriteOffsetX, MiddleScreenY - SpriteOffsetY, s.Sprite.Width * s.XScale, s.Sprite.Height * s.YScale));
                }
            }
        }
    }
}


/*

}*/