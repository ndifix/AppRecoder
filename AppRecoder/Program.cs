using System;
using System.Threading;

namespace AppRecoder
{
	class Program
	{
		static void Main()
		{
			var service = new ManageService();
			service.StartService();

			Console.CancelKeyPress += (sender, e) =>
			{
				service.StopService();
				Environment.Exit(0);
			};
			while (true)
			{
				Thread.Sleep(10);
			}
		}
	}
}
