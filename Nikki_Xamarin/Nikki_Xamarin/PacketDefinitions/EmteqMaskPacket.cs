using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRAME.CrossPlatformInterfaces.BLE.PacketDefinitions
{
    public class EmteqMaskPacket : IBluetoothPacket
    {
        /// <summary>
        /// Bit packets indicating mask contact values
        /// </summary>
        private uint _liftdetect;

        /// <summary>
        /// Packet received number
        /// </summary>
        public uint PacketNumber { get; set; }
        
        /// <summary>
        /// Channel one of received RMS
        /// </summary>
        public float Channel1 { get; set; }

        /// <summary>
        /// Channel two of received RMS
        /// </summary>
        public float Channel2 { get; set; }

        /// <summary>
        /// Channel three of received RMS
        /// </summary>
        public float Channel3 { get; set; }

        /// <summary>
        /// Channel four of received RMS
        /// </summary>
        public float Channel4 { get; set; }

        /// <summary>
        /// Decoded lift detect channel one
        /// </summary>
        public byte LiftDetectChannel1
        {
            get
            {
                uint bits = 7;
                uint value = _liftdetect & bits;

                return (byte)value;
            }
        }

        /// <summary>
        /// Decoded lift detect channel one
        /// </summary>
        public byte LiftDetectChannel2
        {
            get
            {
                uint bits = 56;
                uint value = _liftdetect & bits;

                return (byte)value;
            }
        }

        /// <summary>
        /// Decoded lift detect channel one
        /// </summary>
        public byte LiftDetectChannel3
        {
            get
            {
                uint bits = 448;
                uint value = _liftdetect & bits;

                return (byte)value;
            }
        }

        /// <summary>
        /// Decoded lift detect channel one
        /// </summary>
        public byte LiftDetectChannel4
        {
            get
            {
                uint bits = 3584;
                uint value = _liftdetect & bits;

                return (byte)value;
            }
        }

        /// <summary>
        /// Is the packet even or odd?
        /// </summary>
        public bool IsEven
        {
            get
            {
                return this.PacketNumber % 2 == 0;
            }
        }
        
        /// <summary>
        /// Decode the packet
        /// </summary>
        /// <param name="data"></param>
        public void DecodePacket(byte[] data)
        {
            this.PacketNumber = BitConverter.ToUInt16(data, 0);
            this._liftdetect = BitConverter.ToUInt16(data, 2);

            this.Channel1 = BitConverter.ToSingle(data, 4);
            this.Channel2 = BitConverter.ToSingle(data, 8);
            this.Channel3 = BitConverter.ToSingle(data, 12);
            this.Channel4 = BitConverter.ToSingle(data, 16);
        }

        /// <summary>
        /// No need to encode in the convertor. Fires Not implemented
        /// </summary>
        /// <returns></returns>
        public byte[] EncodePacket()
        {
            throw new NotImplementedException();
        }
    }
}
