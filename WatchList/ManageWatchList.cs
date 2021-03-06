﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AppRecoder.Model;
using System.Diagnostics;
using System.Linq;

namespace WatchList
{
	class ManageWatchList
	{
		private readonly CancellationTokenSource cts = new CancellationTokenSource();

		private readonly string fullPath = @"C:\Program Files\AppRecoder\processes.txt";

		private List<ProcessModel> processList = new List<ProcessModel>();

		private readonly string ignorePath = @"C:\Program Files\AppRecoder\IgnoreProcess.txt";

		private List<string> ignoreProcesses = new List<string>();

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
			cts.Cancel();
			Console.WriteLine("プログラムの終了。");
		}

		private async void UpdateListAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("リストの更新を開始。");

			while (!cancellationToken.IsCancellationRequested)
			{
				Console.WriteLine("all ...登録済みの監視プロセスを全て表示");
				Console.WriteLine("add ...監視するプロセスを実行中プロセスから追加");
				Console.WriteLine("rm ...プロセスをリストから削除。");
				Console.WriteLine("iall...無視するプロセス一覧を表示");
				Console.WriteLine("iadd ...プロセスを無視するリストに追加");
				Console.WriteLine("irm ...無視するリストから除外");
				var key = Console.ReadLine();
				Console.WriteLine();

				try
				{
					switch (key)
					{
						case "all":
							ViewAll();
							break;
						case "add":
							Add();
							break;
						case "rm":
							Remove();
							break;
						case "iall":
							ViewAllIgnore();
							break;
						case "iadd":
							AddIgnore();
							break;
						case "irm":
							RemoveIgnore();
							break;
						default:
							Console.WriteLine($"不正な入力です。:{key}");
							break;
					}

					SaveAll();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
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
			foreach (var process in processList)
			{
				Console.WriteLine(process.ProcessName);
			}
		}

		private void ViewAllIgnore()
		{
			foreach(var name in ignoreProcesses)
			{
				Console.WriteLine(name);
			}
		}

		private void Add()
		{
			var currentProcesses = GetCurrentProcess();

			foreach (var process in currentProcesses.ToArray())
			{
				if (processList.Any(p => p.ProcessName == process.ProcessName))
				{
					currentProcesses.Remove(process);
					continue;
				}
				Console.WriteLine(process.ProcessName);
			}
			Console.WriteLine("プロセス名を入力。");

			string name = Console.ReadLine();
			Console.WriteLine();

			if (!currentProcesses.Any(p => p.ProcessName == name))
			{
				throw new Exception($"不正な入力。{name}");
			}

			ProcessModel addProcess = currentProcesses.Where(p => p.ProcessName == name).ToArray()[0];
			if (!processList.Any(p => p == addProcess))
			{
				processList.Add(addProcess);
			}
		}

		private List<ProcessModel> GetCurrentProcess()
		{
			Process[] processes = null;
			try
			{
				processes = Process.GetProcesses();
			}
			catch
			{
				return new List<ProcessModel>();
			}

			List<ProcessModel> currentProcesses = new List<ProcessModel>();
			foreach (var process in processes)
			{
				try
				{
					var tmp = new ProcessModel
					{
						ProcessName = process.ProcessName,
						ProcessPath = process.MainModule.FileName,
					};

					if (currentProcesses.Count(p => p.ProcessName == tmp.ProcessName) == 0)
					{
						currentProcesses.Add(tmp);
					}
				}
				catch
				{ }
			}

			// 無視するリストのものを除外。
			currentProcesses = currentProcesses
					.Where(p => !ignoreProcesses.Contains(p.ProcessName))
					.ToList();

			return currentProcesses.OrderBy(p => p.ProcessName).ToList();
		}

		private void Remove()
		{
			Console.WriteLine("プロセス名を入力。");
			string name = Console.ReadLine();
			Console.WriteLine();

			processList = processList.Where(p => p.ProcessName != name).ToList();
		}

		private void AddIgnore()
		{
			var currentProcesses = GetCurrentProcess();

			foreach (var process in currentProcesses.ToArray())
			{
				if (processList.Any(p => p.ProcessName == process.ProcessName))
				{
					currentProcesses.Remove(process);
					continue;
				}
				Console.WriteLine(process.ProcessName);
			}
			Console.WriteLine("プロセス名を入力。");

			string name = Console.ReadLine();
			Console.WriteLine();

			if (!currentProcesses.Any(p => p.ProcessName == name))
			{
				throw new Exception($"不正な入力。{name}");
			}

			ProcessModel addProcess = currentProcesses.Where(p => p.ProcessName == name).ToArray()[0];
			if (!ignoreProcesses.Contains(addProcess.ProcessName))
			{
				ignoreProcesses.Add(addProcess.ProcessName);
			}
		}

		private void RemoveIgnore()
		{
			foreach(var process in ignoreProcesses)
			{
				Console.WriteLine(process);
			}
			Console.WriteLine("プロセス名を入力。");
			string name = Console.ReadLine();
			Console.WriteLine();

			ignoreProcesses.Remove(name);
		}

		private void RoadAll()
		{
			string listText;

			listText = File.ReadAllText(fullPath);
			processList = JsonConvert.DeserializeObject<List<ProcessModel>>(listText);

			listText = File.ReadAllText(ignorePath);
			ignoreProcesses = JsonConvert.DeserializeObject<List<string>>(listText);
		}

		private void SaveAll()
		{
			string listText = JsonConvert.SerializeObject(processList);
			File.WriteAllText(fullPath, listText);

			listText = JsonConvert.SerializeObject(ignoreProcesses);
			File.WriteAllText(ignorePath, listText);
		}
	}
}
