using System;
using System.Threading;

namespace WatchList
{
	class Program
	{
		static void Main()
		{
			var service = new ManageWatchList();
			service.OnStart();

			Console.CancelKeyPress += (sender, e) =>
			{
				service.OnStop();
				Environment.Exit(0);
			};
			while (true)
			{
				Thread.Sleep(10);
			}
		}
	}
}
