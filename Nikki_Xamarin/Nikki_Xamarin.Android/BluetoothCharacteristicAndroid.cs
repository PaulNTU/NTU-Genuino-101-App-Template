using Android.Bluetooth;
using FRAME.CrossPlatformInterfaces.BLE;

namespace Nikki_Xamarin.Droid
{
    public class BluetoothCharacteristicAndroid: BluetoothCharacteristicBase, IBluetoothCharacteristic
    {
        public BluetoothGattCharacteristic Characteristic { get; set; }

        public BluetoothCharacteristicAndroid(string uuid, string name, BLEVariableType type):base(uuid, name, type)
        {

        }

        public BluetoothCharacteristicAndroid(string uuid, string name, IBluetoothPacket formatter) : base(uuid, name, formatter)
        {

        }
    }
}