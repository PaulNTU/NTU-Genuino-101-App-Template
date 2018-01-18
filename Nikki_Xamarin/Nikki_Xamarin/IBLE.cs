using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRAME.CrossPlatformInterfaces.BLE
{
    public enum BLEState
    {
        IDLE,
        CONNECTED,
        SCANNING
    }

    public enum BLEConnectionStatusEnum
    {
        CONNECTED,
        FAILED,
        CLOSED,
        SCANNING
    }

    public delegate void CharacteristicValueChanged(string uuid);
    public delegate void DeviceChangeDelegate(IBluetoothLE device, BLEConnectionStatusEnum status);

    public interface IBluetoothLE
    {
        List<IBluetoothCharacteristic> BLEKeysToMonitor { get; }

        bool StartBLE(string devicename);

        bool StopBLE();

        void AddCharacteristic(string id, string name, BLEVariableType type);

        void AddCharacteristic(string id, string name, IBluetoothPacket formatter);

        event CharacteristicValueChanged OnBLECharacteristicChanged;
        
        event DeviceChangeDelegate OnBLEConnectionStateChanged;

        void BeginNotify();
    }
}
