using RockLib.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

while (true)
{
	await new Tester().Test();

	var tasks = new List<Task>();

	for (var i = 0; i < 8; i++)
	{
		tasks.Add(Task.Run(async () => await new Tester().Test()));
	}

	await Task.WhenAll(tasks);

	Console.WriteLine(Tester._total);
	Console.ReadLine();
}

public class Tester
{
	private static int _counter = 0;
	public static int _total = 0;

	public async Task Test()
	{
		for (int i = 0; i < 10000; i++)
		{
			var count = i;
			Log(count);
		}

		await Task.CompletedTask;
	}

	public interface ILogger : IDisposable { }

	public class Logger : ILogger
	{
		public Logger()
		{

		}
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		//~Logger()
		//{
		//    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//    Dispose(disposing: false);
		//}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public void Log(int i)
	{
		Interlocked.Increment(ref _counter);

		var config = Config.Root.GetCompositeSection("RockLib_Logging", "RockLib.Logging");
		var defaultTypes = new DefaultTypes();
		if (!defaultTypes.TryGet(typeof(ILogger), out var dummy))
			defaultTypes.Add(typeof(ILogger), typeof(Logger));

		ILogger logger = null;
		try
		{
			logger = config.CreateReloadingProxy<ILogger>(defaultTypes, null, null);
		}
		finally
		{
			//logger?.Dispose();
			(logger as ConfigReloadingProxy<ILogger>)?.Dispose();
		}
		Interlocked.Decrement(ref _counter);
		Interlocked.Increment(ref _total);

		int count = _total;

		if (count % 1000 == 0)
			Console.WriteLine(count);
	}
}