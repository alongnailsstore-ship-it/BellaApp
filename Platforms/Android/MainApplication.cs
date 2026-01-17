using Android.App;
using Android.Runtime;
using System;

namespace BellaLink.App
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		protected override MauiApp CreateMauiApp()
		{
			// === ARMADILHA PARA ERROS NATIVOS ===
			AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
			{
				// Isso vai imprimir o erro real na janela de SAÍDA do Visual Studio
				System.Diagnostics.Debug.WriteLine("==================================================");
				System.Diagnostics.Debug.WriteLine("🔴 CRASH FATAL (ANDROID):");
				System.Diagnostics.Debug.WriteLine(args.Exception.ToString());
				System.Diagnostics.Debug.WriteLine("==================================================");
			};

			// === ARMADILHA PARA ERROS .NET ===
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
			{
				System.Diagnostics.Debug.WriteLine("==================================================");
				System.Diagnostics.Debug.WriteLine("🔴 CRASH FATAL (.NET):");
				System.Diagnostics.Debug.WriteLine(args.ExceptionObject.ToString());
				System.Diagnostics.Debug.WriteLine("==================================================");
			};

			return MauiProgram.CreateMauiApp();
		}
	}
}