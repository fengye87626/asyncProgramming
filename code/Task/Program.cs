using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSample {
    class Program {
        static object syncObj = new object();
        static int Total = 0;
        static ManualResetEvent beginSignal = new ManualResetEvent(false);

        static void Main() {
            DataParallel();
            //SumParallel();
            //SimpleTask();

            //var task = Task.Run(() => "Hello, Task.");
            //WriteLine(task.Result);

            //var task = Task.Factory.StartNew((id) => string.Format("id: {0}", id), 100);
            //task.ContinueWith((t) => Console.WriteLine(task.Result)).ContinueWith((t) =>
            //    WriteLine("Finished.")).Wait();

            //Task<int>[] tasks = new Task<int>[2] {
            //    Task.Run(() => { return 34; }),
            //    Task.Run(() => { return 8; })
            //};

            //var continuation = Task.Factory.ContinueWhenAll(tasks,
            //        (antecedents) => {
            //            int answer = antecedents[0].Result + antecedents[1].Result;
            //            Console.WriteLine("The answer is {0}", answer);
            //        });
            //continuation.Wait();

            //SimpleNestedTask();

            //TaskCancellation.main();
            //ParallelInvoke.main();

            //DownloadTaskSample();
        }

        static void SimpleNestedTask() {
            var parent = Task.Factory.StartNew(() => {
                Console.WriteLine("Outer task executing.");

                var child = Task.Factory.StartNew(() => {
                    Console.WriteLine("Nested task starting.");
                    Thread.SpinWait(500000);
                    Console.WriteLine("Nested task completing.");
                } , TaskCreationOptions.AttachedToParent);
            });

            parent.Wait();
            Console.WriteLine("Outer has completed.");
        }



        static void DataParallel() {
            string[] messages = new string[] { "one", "two", "three" };

            Parallel.ForEach(messages, msg => WriteLine(msg));
            Parallel.For(0, messages.Length, index => WriteLine(messages[index]));
        }


        static Action sumAction = () => {
            for (int j = 0; j < 1000; j++) {
                Interlocked.Increment(ref Total);
            }

            WriteLine("Task {0} finished on thread {1} .", Task.CurrentId, Thread.CurrentThread.ManagedThreadId);
        };

        static void SumParallel() {            
            Console.WriteLine("Sum job start...");

            Parallel.Invoke(sumAction, sumAction, sumAction);

            Task.Delay(100);

            Console.WriteLine("Sum total: {0}", Total);
        }

        //static void Sum() {
        //    int threadCount = 10;

        //    Console.WriteLine("Sum job start...");

        //    Task[] tasks = new Task[threadCount];
        //    for (int i = 0; i < threadCount; i++) {
        //        tasks[i] = Task.Factory.StartNew(sumAction);
        //    }

        //    beginSignal.Set();
        //    Task.WaitAll(tasks);

        //    Console.WriteLine("Sum total: {0}, should be {1}", Total, threadCount * 1000);
        //}

        // Demonstrated features: 
        //		Task ctor() 
        // 		Task.Factory 
        //		Task.Wait() 
        //		Task.RunSynchronously() 
        // Expected results: 
        // 		Task t1 (alpha) is created unstarted. 
        //		Task t2 (beta) is created started. 
        //		Task t1's (alpha) start is held until after t2 (beta) is started. 
        //		Both tasks t1 (alpha) and t2 (beta) are potentially executed on threads other than the main thread on multi-core machines. 
        //		Task t3 (gamma) is executed synchronously on the main thread. 
        // Documentation: 
        //		http://msdn.microsoft.com/en-us/library/system.threading.tasks.task_members(VS.100).aspx 
        static void SimpleTask() {
            Console.WriteLine("main thread id: {0}", Thread.CurrentThread.ManagedThreadId);

            Action<object> action = (object obj) => {
                Console.WriteLine("Task={0}, obj={1}, Thread={2}", Task.CurrentId, obj.ToString(), Thread.CurrentThread.ManagedThreadId);
            };

            // Construct an unstarted task
            Task t1 = new Task(action, "alpha");

            // Cosntruct a started task
            Task t2 = Task.Factory.StartNew(action, "beta");
            
            // Block the main thread to demonstate that t2 is executing
            t2.Wait();

            // Launch t1 
            t1.Start();

            Console.WriteLine("t1 has been launched. (Main Thread={0})", Thread.CurrentThread.ManagedThreadId);

            // Wait for the task to finish. 
            // You may optionally provide a timeout interval or a cancellation token 
            // to mitigate situations when the task takes too long to finish.
            t1.Wait();

            // Construct an unstarted task
            Task t3 = new Task(action, "gamma");

            // Run it synchronously
            t3.RunSynchronously();

            // Although the task was run synchrounously, it is a good practice to wait for it which observes for  
            // exceptions potentially thrown by that task.
            Console.WriteLine("begin to t3.wait...");
            t3.Wait();
        }

        static void DownloadTaskSample() {
            // NOTE: Synchronous .Wait() calls added only for demo purposes

            // Single async request
            Download("http://www.microsoft.com").ContinueWith(CompletedDownloadData).Wait();

            // Single async request with timeout
            Download("http://www.microsoft.com").WithTimeout(new TimeSpan(0, 0, 0, 0, 1)).ContinueWith(CompletedDownloadData).Wait();

            // Serial async requests
            Task.Factory.TrackedSequence(
                () => Download("http://blogs.msdn.com/pfxteam"),
                () => Download("http://blogs.msdn.com/nativeconcurrency"),
                () => Download("http://exampleexampleexample.com"), // will fail
                () => Download("http://msdn.com/concurrency"),
                () => Download("http://bing.com")
            ).ContinueWith(SerialTasksCompleted).Wait();

            // Concurrent async requests
            Task.Factory.ContinueWhenAll(new[]
        {
            Download("http://blogs.msdn.com/pfxteam"),
            Download("http://blogs.msdn.com/nativeconcurrency"),
            Download("http://exampleexampleexample.com"), // will fail
            Download("http://msdn.com/concurrency"),
            Download("http://bing.com")
        }, ConcurrentTasksCompleted).Wait();

            // Done
            Console.WriteLine();
            Console.WriteLine("Press <enter> to exit.");
            Console.ReadLine();
        }

        static Task<byte[]> Download(string url) {
            return new WebClient().DownloadDataTask(url);
        }

        static void CompletedDownloadData(Task<byte[]> task) {
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    Console.WriteLine("Request succeeded: {0}", task.Result.Length);
                    break;
                case TaskStatus.Faulted:
                    Console.WriteLine("Request failed: {0}", task.Exception.InnerException);
                    break;
                case TaskStatus.Canceled:
                    Console.WriteLine("Request was canceled");
                    break;
            }
        }

        static void SerialTasksCompleted(Task<IList<Task>> tasks) {
            int failures = tasks.Result.Where(t => t.Exception != null).Count();
            Console.WriteLine("Serial result: {0} successes and {1} failures", tasks.Result.Count() - failures, failures);
        }

        static void ConcurrentTasksCompleted(Task<byte[]>[] tasks) {
            int failures = tasks.Where(t => t.Exception != null).Count();
            Console.WriteLine("Concurrent result: {0} successes and {1} failures", tasks.Length - failures, failures);
        }

        static int Calculate(int n) {
            if (n <= 1) {
                return n;
            }
            else {
                return Calculate(n - 1) + Calculate(n - 2);
            }
        }

        static void WriteLine(string message, params object[] args) {
            lock (syncObj)
                Console.WriteLine(message, args);
        }

    }
}
