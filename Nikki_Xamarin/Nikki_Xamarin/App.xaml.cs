using NTU.CrossPlatformInterfaces.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Genuino101_Xamarin
{
	public partial class App : Application
	{
		public App (IBluetoothLE bluetooth)
		{
			InitializeComponent();

			MainPage = new Genuino101_Xamarin.MainPage(bluetooth);
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
