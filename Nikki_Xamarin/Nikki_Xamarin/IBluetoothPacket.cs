using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTU.CrossPlatformInterfaces.BLE
{
    public interface IBluetoothPacket
    {
        void DecodePacket(byte[] data);
        byte[] EncodePacket();
    }
}
