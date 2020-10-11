using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppRecoder.Model;
using Newtonsoft.Json;

namespace AppRecoder
{
	public class ProcessWatch
	{
		private readonly string fullPath = @"../../../../processes.txt";

		private readonly List<string> currentProcess = new List<string>();

		private readonly List<ProcessModel> watchList = new List<ProcessModel>();

		public ProcessWatch()
		{
			string listText;

			listText = File.ReadAllText(fullPath);
			watchList = JsonConvert.DeserializeObject<List<ProcessModel>>(listText);
		}

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
						currentProcess.Add(process.ProcessName);
					}
					catch
					{ }
				}
			}
		}

		public async Task RecordProcess(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					RecordProcessImpl();
					SaveAll();
					await Task.Delay(60 * 1000, cancellationToken);
				}
				catch (TaskCanceledException)
				{ }
				catch (Exception ex)
				{
					Console.WriteLine($"プロセスの記録スレッドで未知の例外。{ex.Message}");
				}
			}
		}

		private void RecordProcessImpl()
		{

			foreach (ProcessModel process in watchList)
			{
				lock (currentProcess)
				{
					if (currentProcess.Contains(process.ProcessName))
					{
						process.AccumulatedMinute++;
					}
				}
			}
		}

		private void SaveAll()
		{
			string listText = JsonConvert.SerializeObject(watchList);
			File.WriteAllText(fullPath, listText);
		}
	}
}
