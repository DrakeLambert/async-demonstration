using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DrakeLambert.AsyncDemonstration
{
    class Program
    {

        static Stopwatch _stopWatch = new Stopwatch();

        static void Main()
        {
            // infinite incoming requests
            var incomingRequests = GetRequests();

            Console.WriteLine("=== Executing .Result'ed Requests ===");
            Console.WriteLine();

            var processedEvents = 0; // keep track of how many events we process
            var tasks = new List<Task>();
            _stopWatch.Restart();
            // infinite loop that processes events
            foreach (var request in incomingRequests)
            {
                tasks.Add(
                    ProcessRequestUsingResult(request) // this is where we actually call to process the request
                        .ContinueWith(RespondToRequest(request)) // web server responds to request when processing is done
                );

                processedEvents++;
                if (processedEvents >= 5)
                {
                    break; // only process 5 events so this demo last until the end of time
                }
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult(); // wait for all tasks to complete

            Console.WriteLine();
            Console.WriteLine("=== Executing Awaited Requests ===");
            Console.WriteLine();

            processedEvents = 0;
            tasks.Clear();
            _stopWatch.Restart();
            // infinite loop that processes events
            foreach (var request in incomingRequests)
            {
                tasks.Add(
                    ProcessRequestUsingAwait(request) // this is where we actually call to process the request
                        .ContinueWith(RespondToRequest(request)) // web server responds to request when processing is done
                );

                processedEvents++;
                if (processedEvents >= 5)
                {
                    break; // only process 5 events so this demo last until the end of time
                }
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult(); // wait for all tasks to complete
            _stopWatch.Stop();
        }

        static async Task<string> ProcessRequestUsingResult(int i)
        {
            var fileContents = DoDataAccessAsync().Result; // notice using .Result here
            return fileContents.Replace("Here", i.ToString());
        }

        static async Task<string> ProcessRequestUsingAwait(int i)
        {
            var fileContents = await DoDataAccessAsync(); // notice using await here
            return fileContents.Replace("Here", i.ToString());
        }

        static async Task<string> DoDataAccessAsync()
        {
            var fileAccessTime = 1 * 1000;
            await Task.Delay(fileAccessTime); // Simulate file access or any other IO
            return "File Contents Here";
        }

        static IEnumerable<int> GetRequests()
        {
            var i = 0;
            while (true)
            {
                yield return i++;
            }
        }

        static Action<Task<string>> RespondToRequest(int request)
        {
            return response =>
            {
                if (response.IsCompleted)
                {
                    if (response.IsCompletedSuccessfully)
                    {
                        Console.WriteLine($"= REQUEST {request} COMPLETED at {_stopWatch.ElapsedMilliseconds}=");
                        Console.WriteLine($"Response: {response.Result}"); // using .Result here because response task is definitely completed
                    }
                    else
                    {
                        Console.WriteLine($"= REQUEST {request} FAILED at {_stopWatch.ElapsedMilliseconds}=");
                        Console.WriteLine("Error processing request.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Method was called on unfinished task.");
                }
            };
        }
    }
}
