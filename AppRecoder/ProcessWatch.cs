using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppRecoder.Model;

namespace AppRecoder
{
	public class ProcessWatch
	{
		private readonly List<ProcessModel> currentProcess = new List<ProcessModel>();

		/// <summary>
		/// 実行中の全てのプロセスを取得します。
		/// </summary>
		public async Task WatchProcessAync(CancellationToken cancellationToken)
		{
			Console.WriteLine("プロセスの監視スレッドを開始。");

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					WatchProcessAsyncImpl();
					await Task.Delay(60 * 1000, cancellationToken);
				}
				catch (TaskCanceledException)
				{ }
				catch (Exception ex)
				{
					Console.WriteLine($"プロセスの監視スレッドで未知の例外。{ex.Message}");
				}
			}

			Console.WriteLine("プロセスの監視スレッドを終了。");
		}

		private void WatchProcessAsyncImpl()
		{
			Process[] processes = Process.GetProcesses();

			lock (currentProcess)
			{
				currentProcess.Clear();

				foreach (var process in processes)
				{
					try
					{
						var tmp = new ProcessModel
						{
							ProcessName = process.ProcessName,
							ProcessPath = process.MainModule.FileName,
						};
						currentProcess.Add(tmp);
					}
					catch
					{ }
				}
			}
		}
	}
}
