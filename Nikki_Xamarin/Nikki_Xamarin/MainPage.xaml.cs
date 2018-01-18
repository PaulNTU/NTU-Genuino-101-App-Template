using NTU.CrossPlatformInterfaces.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Genuino101_Xamarin
{
	public partial class MainPage : ContentPage
	{
        private IBluetoothLE _bluetooth;
        private int _currentstate;

        public MainPage(IBluetoothLE bluetooth)
		{
			InitializeComponent();

            _bluetooth = bluetooth;

            _bluetooth.OnBLECharacteristicChanged += Bluetooth_OnBLECharacteristicChanged;
            _bluetooth.OnBLEConnectionStateChanged += Bluetooth_OnBLEConnectionStateChanged;

            _bluetooth.AddCharacteristic("713D0002-503E-4C75-BA94-3148F18D941E", "REED", BLEVariableType.INTEGER);

            _bluetooth.StartBLE("PepsiCo");

		}

        private void Bluetooth_OnBLEConnectionStateChanged(IBluetoothLE device, BLEConnectionStatusEnum status)
        {
            switch(status)
            {
                case BLEConnectionStatusEnum.SCANNING:

                    UpdateHeaderText("Scanning For Device: PepsiCo");
                    break;

                case BLEConnectionStatusEnum.CONNECTED:
                    UpdateHeaderText("Connected To Device...Awaiting Value");
                    break;
            }
        }

        private void UpdateHeaderText(string text)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                HeaderLabel.Text = text;
            });
        }

        private void ShowEmpty()
        {
            StackLayout content = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.Center };
            content.Children.Add(new Label() { Text = "Out Of Stock", HorizontalTextAlignment = TextAlignment.Center, FontSize = 32 });
            content.Children.Add(new Image() { Source = "empty" });
            this.Content = content;
        }

        private void ShowFull()
        {
            StackLayout content = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.Center };
            content.Children.Add(new Label() { Text = "In Stock", HorizontalTextAlignment = TextAlignment.Center, FontSize = 32 });
            content.Children.Add(new Image() { Source = "stocked" });
            this.Content = content;
        }

        private void Bluetooth_OnBLECharacteristicChanged(string uuid)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                IBluetoothCharacteristic c = _bluetooth.BLEKeysToMonitor.Where(x => x.UUID.ToString().ToLower() == uuid.ToLower()).FirstOrDefault();

                if (c != null)
                {
                    int value = (int)c.Value;

                    if (value != _currentstate)
                    {
                        // Keep track of the last received state so that we only refresh when needed
                        _currentstate = value;
                        if (value == 0)
                        {
                            ShowEmpty();
                        }
                        else
                        {
                            ShowFull();
                        }
                    }
                }
            });
        }
    }
}
