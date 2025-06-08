using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class TaskSchedulerService
{
    private readonly ConcurrentQueue<(Action Task, DateTime ExecuteAt)> _tasks = new();
    private readonly CancellationTokenSource _cts = new();

    public TaskSchedulerService()
    {
        Task.Run(ProcessTasks);
    }

    public void Schedule(Action task, TimeSpan delay)
    {
        _tasks.Enqueue((task, DateTime.UtcNow.Add(delay)));
    }

    private async Task ProcessTasks()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (_tasks.TryPeek(out var scheduledTask))
            {
                var now = DateTime.UtcNow;
                if (scheduledTask.ExecuteAt <= now)
                {
                    _tasks.TryDequeue(out var taskToRun);
                    taskToRun.Task.Invoke();
                }
            }
            await Task.Delay(500); // Check every 0.5 seconds
        }
    }

    public void Stop() => _cts.Cancel();
}

// Example usage
public class Program
{
    public static void Main()
    {
        var scheduler = new TaskSchedulerService();

        scheduler.Schedule(() => Console.WriteLine("Task 1 executed!"), TimeSpan.FromSeconds(3));
        scheduler.Schedule(() => Console.WriteLine("Task 2 executed!"), TimeSpan.FromSeconds(5));

        Console.WriteLine("Tasks scheduled. Waiting...");
        Thread.Sleep(7000);
        scheduler.Stop();
    }
}
