using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTU.CrossPlatformInterfaces.BLE
{
    public delegate void ValueChangedDelegate(BluetoothCharacteristicBase o);

    public enum BLEVariableType
    {
        INTEGER,
        FLOAT,
        STRING,
        FLOATCOLLECTION,
        INTEGERCOLLECTION,
        CUSTOMMASK
    }
    
    public interface IBluetoothCharacteristic
    {
        
        event ValueChangedDelegate ValueChanged;
        
        String DisplayName { get; set; }
        String UUID { get; }
        BLEVariableType VariableType { get; }

        object Value { get; set; }
       
        string StringValue { get; }
        
        string VariableTypeName { get; }
        
        void SetValue(byte[] value);
      
    }

    public class BluetoothCharacteristicBase: IBluetoothCharacteristic
    {
        #region Values
        public event ValueChangedDelegate ValueChanged;
        #endregion

        #region Variables
        private object _value = null;

        #endregion

        #region Properties
        public String DisplayName { get; set; }
        public String UUID { get; private set; }
        public BLEVariableType VariableType { get; private set; }

        private IBluetoothPacket _customdatamask;
        
        public object Value
        {
            get
            {
                // Get the value
                return _value;
            }
            set
            {
                this._value = value;
            }
        }

        public string StringValue
        {
            get
            {
                if (_value == null)
                {
                    return "NA";
                }
                else
                {
                    return _value.ToString();
                }
            }
        }

        public string VariableTypeName
        {
            get
            {
                switch (this.VariableType)
                {
                    case BLEVariableType.FLOAT:
                        return "Float";

                    case BLEVariableType.INTEGER:
                        return "Integer";
                    case BLEVariableType.STRING:
                        return "String";
                    case BLEVariableType.FLOATCOLLECTION:
                        return "Float Array";
                    case BLEVariableType.INTEGERCOLLECTION:
                        return "Int Array";
                    default:
                        return "Unknown";
                }
            }
        }

        #endregion

        #region Constructors
        public BluetoothCharacteristicBase(string uuid, string name, BLEVariableType type)
        {
            this.UUID = uuid;
            this.DisplayName = name;
            this._value = null;
            this.VariableType = type;
        }

        public BluetoothCharacteristicBase(string uuid, string name, IBluetoothPacket datamask)
        {
            this.UUID = uuid;
            this.DisplayName = name;
            this._value = null;
            this.VariableType = BLEVariableType.CUSTOMMASK;
            this._customdatamask = datamask;
        }
        #endregion

        #region Functions

        public void SetValue(byte[] value)
        {
            // Set the new value
            switch (this.VariableType)
            {
                case BLEVariableType.FLOAT:
                    this._value = System.BitConverter.ToSingle(value, 0);
                    break;

                case BLEVariableType.INTEGER:
                    this._value = System.BitConverter.ToInt32(value, 0);
                    break;

                case BLEVariableType.STRING:
                    List<Char> chars = new List<char>(value.Length / 2);

                    for (int x = 0; x < value.Length; x += 2)
                    {
                        chars.Add(System.BitConverter.ToChar(value, x));
                    }

                    this._value = new String(chars.ToArray());
                    break;

                case BLEVariableType.FLOATCOLLECTION:
                    float[] a = new float[5];

                    a[0] = System.BitConverter.ToSingle(value, 0);
                    a[1] = System.BitConverter.ToSingle(value, 4);
                    a[2] = System.BitConverter.ToSingle(value, 8);
                    a[3] = System.BitConverter.ToSingle(value, 12);
                    a[4] = System.BitConverter.ToSingle(value, 16);

                    this.Value = a;
                    break;

                case BLEVariableType.INTEGERCOLLECTION:
                    int[] i = new int[10];

                    i[0] = System.BitConverter.ToInt16(value, 0);
                    i[1] = System.BitConverter.ToInt16(value, 2);
                    i[2] = System.BitConverter.ToInt16(value, 4);
                    i[3] = System.BitConverter.ToInt16(value, 8);
                    i[4] = System.BitConverter.ToInt16(value, 10);

                    this.Value = i;
                    break;

                case BLEVariableType.CUSTOMMASK:
                    _customdatamask.DecodePacket(value);
                    _value = _customdatamask;
                    break;
            }

            // Attached event handler?
            if (this.ValueChanged != null)
            {
                // Fire change event
                this.ValueChanged.Invoke(this);
            }
        }
        #endregion
    }
}
