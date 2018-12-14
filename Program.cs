using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace event_hub_sender
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.CursorVisible = false;

            // might want to run something async at some point, stick it on the thread pool
            var source = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                source.Cancel();
            };

            try
            {
                return MainAsync(args, source.Token).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 1223;
        }
        private static async Task<int> MainAsync(string[] args, CancellationToken token)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["EventHub"].ConnectionString;
            
            Console.WriteLine($"Creating event hub client for {connectionString}");
            EventHubClient client = EventHubClient.CreateFromConnectionString(connectionString);
            int x = 0;
            while (token.IsCancellationRequested == false)
            {
                EventData data = new EventData(System.Text.Encoding.UTF8.GetBytes($"{DateTime.Now.ToString("HH:mm:ss.fff")}"));
                await client.SendAsync(data, "FixedPartition");

                // Thread.Sleep(100);
                Console.Write("🗣 ");
                if (++x == 50)
                {
                    x = 0;
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("Interrupted, stopping send.");

            return 0;
        }
    }
}
