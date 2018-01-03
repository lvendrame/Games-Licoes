using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test01
{
    public static class Timer
    {
        public static DateTime OldTime { get; private set; }
        public static DateTime CurrentTime { get; private set; }
        public static int ElapsedTime { get; private set; }
        public static int LagTime { get; private set; }

        public static void Start()
        {
            OldTime = CurrentTime = DateTime.Now;
            ElapsedTime = 0;
        }

        public static void Update()
        {
            OldTime = CurrentTime;
            CurrentTime = DateTime.Now;
            ElapsedTime = (int)CurrentTime.Subtract(OldTime).TotalMilliseconds;

            LagTime += ElapsedTime;
        }

        public static bool VerifyLag()
        {
            if(LagTime >= 16)
            {
                LagTime = 0;
                return false;
            }
            return true;
        }


        

    }
}
