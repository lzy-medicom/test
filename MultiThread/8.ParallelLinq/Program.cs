using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using medicom;

namespace _8.ParallelLinq
{
    public class Custom
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
    }

    class Program
    {
        #region TestName
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        private static void TestName()
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            StringBuilder testName = new StringBuilder("****************************************\n");
            testName.Append(frames[1].GetMethod().Name);
            testName.Replace('_', ':');
            for (Int32 index = 1; index < testName.Length; index++)
            {
                // If this is an uppercase character, insert a space before it
                if (Char.IsUpper(testName[index]))
                {
                    testName.Insert(index, ' ');
                    index++;
                }
            }
            testName.Append("\n****************************************");
            TestName(testName.ToString());
        }

        private static void TestName(String testName)
        {
            Console.WriteLine();
            Console.WriteLine(testName);
        }
        #endregion
        static void Main(string[] args)
        {
            // CodeTimer.Initialize方法应该在测试开始前调用
            CodeTimer.Initialize();

            //*****************************************************************
            // 
            TestPLinq();

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey(true);
        }

        #region PLINQ Tests
        public static void TestPLinq()
        {
            Stopwatch sw = new Stopwatch();
            List<Custom> customs = new List<Custom>();
            for (int i = 0; i < 2000000; i++)
            {
                customs.Add(new Custom() { Name = "Jack", Age = 21, Address = "NewYork" });
                customs.Add(new Custom() { Name = "Jime", Age = 26, Address = "China" });
                customs.Add(new Custom() { Name = "Tina", Age = 29, Address = "ShangHai" });
                customs.Add(new Custom() { Name = "Luo", Age = 30, Address = "Beijing" });
                customs.Add(new Custom() { Name = "Wang", Age = 60, Address = "Guangdong" });
                customs.Add(new Custom() { Name = "Feng", Age = 25, Address = "YunNan" });
            }

            sw.Start();
            var result = customs.Where<Custom>(c => c.Age > 26).ToList();
            sw.Stop();
            Console.WriteLine("Linq time is {0}.", sw.ElapsedMilliseconds);

            sw.Restart();
            sw.Start();
            var result2 = customs.AsParallel().Where<Custom>(c => c.Age > 26).ToList();
            sw.Stop();
            Console.WriteLine("Parallel Linq time is {0}.", sw.ElapsedMilliseconds);
        }
        #endregion
    }


}
