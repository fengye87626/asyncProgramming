using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MultiThreading {
    public class Program {
        static object syncObj = new object();
        public static int Total = 0;

        static void Main(string[] args) {
            Console.WriteLine("Program begin...");
            #region lab1
            //Sum();
            //Sum2();
            #endregion

            #region lab2
            int number = 22;
            int maxCount = 100;

            Stopwatch watch = Stopwatch.StartNew();
            CalcByMultiThread(number, maxCount);
            Console.WriteLine("CalcByMultiThread cost {0} ms.", watch.ElapsedMilliseconds);

            //let thread cooldown
            Thread.Sleep(100);

            watch.Restart();
            CalcByThreadPool(number, maxCount);
            Console.WriteLine("CalcByThreadPool cost {0} ms.", watch.ElapsedMilliseconds);
            #endregion

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void Sum() {
            int threadCount = 10;
            int maxCount = 1000;
            bool beginSignal = false;
            Console.WriteLine("Sum job start...");

            for (int i = 0; i < threadCount; i++) {
                Thread t = new Thread((id) => {
                    while (beginSignal) Thread.Sleep(0);

                    for (int j = 0; j < maxCount; j++) {
                        Total++;
                    }

                    WriteLine("Thread {0} finished.", (int)id);
                });

                t.Start(i + 1);
            }

            beginSignal = true;
            // sleep 1 second to let thread finish the job.
            Thread.Sleep(1000);
            Console.WriteLine("Sum total: {0}, should be {1}", Total, threadCount * maxCount);
        }


        static void Sum2() {
            int threadCount = 10;
            int maxCount = 1000;
            ManualResetEvent beginSignal = new ManualResetEvent(false);
            Console.WriteLine("Sum job start...");

            for (int i = 0; i < threadCount; i++) {
                Thread t = new Thread((id) => {
                    beginSignal.WaitOne();
                    for (int j = 0; j < maxCount; j++) {
                        Interlocked.Increment(ref Total);
                        //Add();
                    }

                    WriteLine("Thread {0} finished.", (int)id);
                });

                t.Start(i + 1);
            }

            beginSignal.Set();
            // sleep 1 second to let thread finish the job.
            Thread.Sleep(1000);
            Console.WriteLine("Sum total: {0}, should be {1}", Total, threadCount * maxCount);
        }        

        static void CalcByMultiThread(int Number, int MaxCount) {
            int threadCount = MaxCount;
            ManualResetEvent doneEvent = new ManualResetEvent(false);
            for (int i = 0; i < MaxCount; i++) {
                Thread t = new Thread(() => {
                    Calculate(Number);
                    if (Interlocked.Decrement(ref threadCount) <= 0)
                        doneEvent.Set();
                });

                t.Start();
            }
            doneEvent.WaitOne();
        }

        static void CalcByThreadPool(int Number, int MaxCount) {
            int threadCount = MaxCount;
            ManualResetEvent doneEvent = new ManualResetEvent(false);
            for (int i = 0; i < MaxCount; i++) {
                ThreadPool.QueueUserWorkItem(
                    (o) => {
                        Calculate(Number);
                        if (Interlocked.Decrement(ref threadCount) <= 0) doneEvent.Set();
                    });
            }
            doneEvent.WaitOne();
        }

        static int Calculate(int n) {
            if (n <= 1) {
                return n;
            }
            else {
                return Calculate(n - 1) + Calculate(n - 2);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void Add() {
            Total++;
        }

        static void WriteLine(string message, params object[] args) {
            lock (syncObj)
                Console.WriteLine(message, args);
        }
    }
}
