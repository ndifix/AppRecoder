using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AppRecoder.Model;

namespace WatchList
{
	class ManageWatchList
	{
		private CancellationTokenSource cts = new CancellationTokenSource();

		private readonly string fullPath = @"../../../../processes.txt";

		private List<ProcessModel> processList = new List<ProcessModel>();

		public void OnStart()
		{
			Console.WriteLine("プログラムの起動");
			try
			{
				RoadAll();
				if (processList == null)
				{
					throw new Exception("");
				}
				UpdateListAsync(cts.Token);
			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
				{
					Console.WriteLine($"ファイルが存在しません。指定されたパス：{fullPath}");
				}
				else
				{
					Console.WriteLine($"設定ファイルの読み込みに失敗。{ex.Message}");
				}

				Console.WriteLine("Ctrl + c で終了します。");
			}
		}

		public void OnStop()
		{
			SaveAll();
			cts.Cancel();
			Console.WriteLine("プログラムの終了。");
		}

		private async void UpdateListAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("リストの更新を開始。");

			while (!cancellationToken.IsCancellationRequested)
			{
				Console.WriteLine("l ...登録済みの監視プロセスを全て表示");
				var key = Console.ReadKey();
				Console.WriteLine();

				switch (key.KeyChar)
				{
					case 'l':
						ViewAll();
						break;
					default:
						Console.WriteLine("不正な入力です。");
						break;
				}

				try
				{
					await Task.Delay(100, cancellationToken);
				}
				catch (TaskCanceledException)
				{ }
				catch (Exception ex)
				{
					Console.WriteLine($"リストの更新で未知の例外。{ex.Message}");
				}
			}

			Console.WriteLine("リストの更新を終了。");
		}

		private void ViewAll()
		{
			foreach(var process in processList)
			{
				Console.WriteLine(process.ProcessName);
			}
		}

		private void RoadAll()
		{
			string listText;

			listText = File.ReadAllText(fullPath);
			processList = JsonConvert.DeserializeObject<List<ProcessModel>>(listText);
		}

		private void SaveAll()
		{
			string listText = JsonConvert.SerializeObject(processList);
			File.WriteAllText(fullPath, listText);
		}
	}
}
