using Integration.Service;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var service = new ItemIntegrationService();
		/* Single Server
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
		Thread.Sleep(500);

		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("f"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("g"));

		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("f"));
		ThreadPool.QueueUserWorkItem(_ => service.SaveItem("g"));
		*/

		// Distributed System
		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("a"));
		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("b"));
		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("c"));
		Thread.Sleep(500);

		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("a"));
		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("b"));
		ThreadPool.QueueUserWorkItem(async _ => await service.SaveItem("c"));

		Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
}