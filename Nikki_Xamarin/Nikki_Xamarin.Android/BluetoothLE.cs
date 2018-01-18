using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NTU.CrossPlatformInterfaces.BLE;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Java.Util;
using System.Threading.Tasks;

namespace Genuino101_Xamarin.Droid
{
    public class BluetoothLE : BluetoothGattCallback, IBluetoothLE
    {
        public event CharacteristicValueChanged OnBLECharacteristicChanged;
        public event DeviceChangeDelegate OnBLEConnectionStateChanged;

        private class BLE_Scanner : ScanCallback
        {
            public delegate void NewConnectionDelegate(ScanResult device);
            public event NewConnectionDelegate GattConnecting;
            private string _findname;
            private bool _cancelling = false;
            
            public BLE_Scanner()
            {
                
            }

            public BLE_Scanner(string finddevicewithname)
            {
                _findname = finddevicewithname;

            }
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                if (!_cancelling)
                {
                    string name = result.Device.Name;

                    if (name != null)
                    {
                        if (name == _findname)
                        {
                            this.GattConnecting.Invoke(result);
                            this._cancelling = true;
                        }
                    }
                }
            }
        }

        public List<IBluetoothCharacteristic> BLEKeysToMonitor { get; internal set; }

        private Context _context;

        public BLEState BluetoothState { get; internal set; }

        private string _searchfordevicenamed;

        private BLE_Scanner _blescannerresult;

        private BluetoothLeScanner _scanner;

        private BluetoothGatt _gattdevice;

        /// <summary>
        /// Gatt Async Writing Queue
        /// </summary>
        private Stack<BluetoothGattDescriptor> _gattdescqueue;
        private bool _gattdescwritebusy = false;

        private bool _enablelegacy = false;
        
        /// <summary>
        /// Initialise the new Bluetooth Connection
        /// </summary>
        /// <param name="c"></param>
        public BluetoothLE(Context c)
        {
            this._context = c;
            this.BLEKeysToMonitor = new List<IBluetoothCharacteristic>();
        }

        
        /// <summary>
        /// Start the BLE connection and seek the passed in device
        /// </summary>
        /// <param name="devicename">Device to search for</param>
        /// <returns>true on scan started</returns>
        public bool StartBLE(string devicename)
        {
            if (this.BluetoothState == BLEState.IDLE)
            {
                // Start
                BluetoothManager btm = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);

                if (btm != null)
                {
                    BluetoothAdapter adaptor = btm.Adapter;

                    if (adaptor == null)
                    {
                        return false;
                    }

                    // Bluetooth Available?
                    if (!adaptor.IsEnabled)
                    {
                        return false;
                    }

                    // Search for device name
                    _searchfordevicenamed = devicename;

                        this._blescannerresult = new BLE_Scanner(_searchfordevicenamed);
                        this._blescannerresult.GattConnecting += _blescanner_GattConnecting;

                        this._scanner = adaptor.BluetoothLeScanner;
                        this._scanner.StartScan(this._blescannerresult);

                        // Initialise Characteristics
                        this._gattdescqueue = new Stack<BluetoothGattDescriptor>();
                        this._gattdescwritebusy = false;

                        // State Change
                        this.OnBLEConnectionStateChanged.Invoke(this, BLEConnectionStatusEnum.SCANNING);

                        // Set to scanning
                        this.BluetoothState = BLEState.SCANNING;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        {
            base.OnDescriptorWrite(gatt, descriptor, status);

            // Remove last item and write the next
            this._gattdescwritebusy = false;
            this.SendNextGattDescriptor();
        }
        
        /// <summary>
        /// GattConnecting event
        /// </summary>
        /// <param name="device">Resulting device</param>
        private void _blescanner_GattConnecting(ScanResult device)
        {
                // Connecting to the device
                this._gattdevice = device.Device.ConnectGatt(this._context, true, this);

            // Stop the scanner
            this._scanner.StopScan(this._blescannerresult);
        }

        /// <summary>
        /// Stop the bluetooth connection if active
        /// </summary>
        /// <returns></returns>
        public bool StopBLE()
        {
            switch (this.BluetoothState)
            {
                case BLEState.IDLE:
                    return true;

                case BLEState.SCANNING:
                    this._scanner.StopScan(this._blescannerresult);
                    this._scanner.Dispose();
                    this._blescannerresult.Dispose();
                    break;

                case BLEState.CONNECTED:
                    this._gattdevice.Close();
                    this._gattdevice.Dispose();
                    this._gattdevice = null;
                    break;
            }

            // Clean, dispose
            if (this._scanner != null)
            {
                this._scanner.Dispose();
                this._scanner = null;
            }

            if (this._blescannerresult != null)
            {
                this._blescannerresult.Dispose();
                this._blescannerresult = null;
            }

            // Reset
            this._gattdescqueue.Clear();
            this._gattdescwritebusy = false;

            // Reset state
            this.BluetoothState = BLEState.IDLE;
            return true;
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            //base.OnCharacteristicRead(gatt, characteristic, status);

            if (status == GattStatus.Success)
            {
                // Write to memory
                byte[] data = characteristic.GetValue();

                IBluetoothCharacteristic c = this.BLEKeysToMonitor.Where(x => x.UUID.ToLower() == characteristic.Uuid.ToString()).First();
                
                if(c != null)
                {
                    c.SetValue(data);
                    this.OnBLECharacteristicChanged.Invoke(c.UUID);
                }
                
            }
            
        }

        #region Overrides
        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            //base.OnCharacteristicChanged(gatt, characteristic);
            string id = characteristic.Uuid.ToString();
            
            IBluetoothCharacteristic c = this.BLEKeysToMonitor.Where(x => x.UUID.ToLower() == id.ToLower()).FirstOrDefault();

            if (c != null)
            {
                // Yep, set the value
                c.SetValue(characteristic.GetValue());

                // Fire the change event
                if (this.OnBLECharacteristicChanged != null)
                {
                    this.OnBLECharacteristicChanged.Invoke(id);
                }
            }
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            switch (status)
            {
                case GattStatus.Success:
                    this.BluetoothState = BLEState.CONNECTED;

                    // Connection state has changed fire the event
                    if (this.OnBLEConnectionStateChanged != null)
                    {
                        this.OnBLEConnectionStateChanged.Invoke(this, BLEConnectionStatusEnum.CONNECTED);
                    }

                    gatt.DiscoverServices();
                    break;

                case GattStatus.Failure:
                    // Connection state has changed fire the event
                    if (this.OnBLEConnectionStateChanged != null)
                    {
                        this.OnBLEConnectionStateChanged.Invoke(this, BLEConnectionStatusEnum.FAILED);
                    }

                    this.StopBLE();
                    break;
            }
        }
        
        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);

            if (descriptor.Characteristic.Properties.HasFlag(GattProperty.Notify))
            {
                // Setup notify
                gatt.SetCharacteristicNotification(descriptor.Characteristic, true);
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                // Add to the queue before trying to send it (Queued?)
                this._gattdescqueue.Push(descriptor);
                this.SendNextGattDescriptor();
            }
        }

        /// <summary>
        /// Not needed on Android
        /// </summary>
        public void BeginNotify() { }


        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            if (status == GattStatus.Success)
            {
                foreach (BluetoothGattService s in gatt.Services)
                {
                    foreach (BluetoothGattCharacteristic c in s.Characteristics)
                    {
                        IBluetoothCharacteristic bc = this.BLEKeysToMonitor.Where(x => x.UUID.ToLower() == c.Uuid.ToString().ToLower()).FirstOrDefault();
                        if (bc != null)
                        {
                            if (c.Properties.HasFlag(GattProperty.Notify))
                            {
                                // Enable the characteristic notifications
                                gatt.SetCharacteristicNotification(c, true);

                                // Add to the GATT register stack (Async needs to wait for each write before trying to write a new one)
                                // We need to register with the Descriptor for Notifications (Fixed ID below)

                                BluetoothGattDescriptor d;

                                d = c.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));

                                // Store for reading on request
                                (bc as BluetoothCharacteristicAndroid).Characteristic = c;

                                if (d != null)
                                {

                                    // IMPORTANT. This is Async so multiple fast calls to write descriptor will not work as it will overwrite the critical
                                    // value before it can complete the operation. Its ok for this as we are only writing one value but you will need to
                                    // implement some wait writing if your registering multiple Characteristic notifications
                                    d.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                                    // Add to the queue before trying to send it (Queued?)
                                    this._gattdescqueue.Push(d);
                                    this.SendNextGattDescriptor();
                                }
                                else
                                {
                                    BluetoothGattCharacteristic characteristic = gatt.GetService(UUID.FromString("180F")).GetCharacteristic(UUID.FromString("0000EEEE-0000-1000-8000-00805F9B35AF"));
                                    d = c.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                                    // Store for reading on request
                                    (bc as BluetoothCharacteristicAndroid).Characteristic = c;

                                    // IMPORTANT. This is Async so multiple fast calls to write descriptor will not work as it will overwrite the critical
                                    // value before it can complete the operation. Its ok for this as we are only writing one value but you will need to
                                    // implement some wait writing if your registering multiple Characteristic notifications
                                    d.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                                    // Add to the queue before trying to send it (Queued?)
                                    this._gattdescqueue.Push(d);
                                    this.SendNextGattDescriptor();
                                }
                            }
                        }
                    }
                }

            }
        }
        
        /// <summary>
        /// Write the next Gatt notification descriptor
        /// </summary>
        private void SendNextGattDescriptor()
        {
            if (!this._gattdescwritebusy)
            {
                
                if (this._gattdescqueue.Count > 0)
                {
                    BluetoothGattDescriptor d = this._gattdescqueue.Pop();
                    this._gattdevice.WriteDescriptor(d);
                    this._gattdescwritebusy = true;
                }
            }
        }

        public void AddCharacteristic(string id, string name, BLEVariableType type)
        {
            this.BLEKeysToMonitor.Add(new BluetoothCharacteristicAndroid(id, name, type));
        }

        public void AddCharacteristic(string id, string name, IBluetoothPacket formatter)
        {
            this.BLEKeysToMonitor.Add(new BluetoothCharacteristicAndroid(id, name, formatter));
        }
        #endregion
    }
}