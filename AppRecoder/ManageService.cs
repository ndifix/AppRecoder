﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppRecoder
{
	class ManageService
	{
		private CancellationTokenSource cts = new CancellationTokenSource();

		private Task watchProcessTask;

		private Task recordTask;

		public void StartService()
		{
			Console.WriteLine("サービスを開始します。");

			var processWatch = new ProcessWatch();
			watchProcessTask = processWatch.WatchProcessAync(cts.Token);
			recordTask = processWatch.RecordProcess(cts.Token);
		}

		public void StopService()
		{
			Console.WriteLine("サービスを終了します。");

			if (!cts.IsCancellationRequested)
			{
				cts.Cancel();

				try
				{
					if (watchProcessTask != null)
					{
						watchProcessTask.Wait();
					}
					if (recordTask != null)
					{
						recordTask.Wait();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"サービスの停止に失敗。{ex.Message}");
				}
			}
		}
	}
}
