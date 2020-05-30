using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ayla
{
    static class Program
    {

        static bool NearlyEqual(this float left, float right, float epsilon = 0.001f)
        {
            if (Math.Abs(left - right) <= epsilon) return true;
            return false;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading Input file {args[0]} ...");

            var lines = File.ReadLines(args[0]);

            Console.WriteLine($"{lines.Count()} Lines readed.");

            float[,] rawMatrix = new float[256, 256];

            foreach(var line in lines)
            {
                // search for the first x
                var xLocation = line.IndexOf("X");
                var yLocation = line.IndexOf("Y", xLocation+1);
                var landLocation = line.IndexOf("Land", yLocation+1);

                string X = line.Substring(xLocation + 2, yLocation - xLocation - 2);
                int ix = (int)float.Parse(X );


                string Y = line.Substring(yLocation + 2, landLocation - yLocation - 2);
                int iy = (int)float.Parse(Y);

                string land = line.Substring(landLocation + 5);

                float height = float.Parse(land);

                rawMatrix[ix,iy] = height;

            }


            // channel 1  holds the height from 0 to 255
            // channel 2 holds multiplier   which is    divided by 128 
            //  so  height = ch1 + (ch2/128)*ch1
            //  so theoritically  maximum height is 
            //    255 + (255/128)*255  =  255 +2*255 = 765 meters
            // so we need to know ch1 and ch2 from height only
            // let ch1 == X
            // based on documentation it is clear that multipliers is being caluclate by 
            //  2^n / 128  where n =0 to 8
            // so  height 
            //    h = x + (2^n/128)*x
            //    h = x * (1 + 2^n/128)
            //    x = h / (1+2^n/128)

            //    for  simplicity we will solve x until for 8 equations  where n =0..8    
            //   the nearest integer value of x will determine   the channel 2  value  of 2^n 


            var file = new List<byte>();

            
            // we right 13 channel
            for(int ix = 0; ix < 256; ix++)
            {
                for(int iy = 0; iy < 256; iy++)
                {
                    byte channel1 = 0;
                    byte channel2 = 0;
                    byte channel3 = 5;   // water level;

                    float h = rawMatrix[ix, iy];

                    if (h < 255)
                    {
                        channel1 = (byte)Math.Round(h);
                        channel2 = 1;
                    }
                    else
                    {
                        bool solutionFound = false;
                        //for(int n = 0; n < 9; n++)
                        for (int y = 0; y < 256; y++)
                        {
                            //var x = (float)(h / (1 + (Math.Pow(2, n) / 128)));
                            var x = (float)(h / (1f + (y / 128f)));
                            if (x.NearlyEqual((float)Math.Round(x), 0.05f))
                            {
                                // found a nearly integer x
                                channel1 = (byte)Math.Round(x);
                                //channel2 = (byte) Math.Pow(2, n);
                                channel2 = (byte)y;

                                solutionFound = true;
                                break;
                            }

                            //throw new Exception("Can't find sutiable value");
                        }

                        if (!solutionFound) throw new Exception("Solution not found");
                    }
                    file.Add(channel1);
                    file.Add(channel2);
                    file.Add(channel3);
                    for (int rest = 0; rest < 10; rest++)
                        file.Add(0);

                    
                    // point 13 channgels completed here

                }
            }

            File.WriteAllBytes(args[0] + ".raw", file.ToArray());

            Console.WriteLine(args[0] + ".raw generated");
        }
    }
}
