using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Renderer {
    public class Series {

        public String Title = "";
        public int SeriesLimit = 10;
        public float Min = int.MaxValue;
        public float Max = int.MinValue;

        private long InitTime = 0L;

        public float Total = 0f;
        public int Count = 0;

        public Series(String Title) {
            this.Title = Title;
            this.InitTime = Util.nanoTime();
        }

        public float AveragePerSecond() {
            long now = Util.nanoTime();
            float diffSeconds = (now - InitTime) / 10000000f;
            return Count / diffSeconds;
        }

        public float Average() {
            return Total / Count;
        }

        public void Truncate() {
            Total = Total * 0.5f;
            Count = (int) (Count * 0.5);
        }


        public void Add() {
            Count++;
        }

        public void Add(float val) {
            Total += val;
            Count++;
        }


    }
}
