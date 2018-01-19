#include <CurieBLE.h>

// Bluetooth
BLEPeripheral bledevice; // BLE device

// Services
BLEService informationService("713D0000-503E-4C75-BA94-3148F18D941E");

BLEIntCharacteristic ReedSwitch("713D0002-503E-4C75-BA94-3148F18D941E", BLERead | BLENotify);

int val = 0;
void setup() {

  Serial.begin(9600);
  delay(1000);
  
  // put your setup code here, to run once:
  Serial.println("Initialising Bluetooth...");
  
  // Bluetooth Setup
  bledevice.setLocalName("PepsiCo");
  bledevice.setDeviceName("PepsiCo");

  // Add the characteristic
  bledevice.addAttribute(informationService);

  // Put all sensor values into one characteristic for speed enhancement
  bledevice.addAttribute(ReedSwitch);

  // Advertise  
  bledevice.setAdvertisedServiceUuid(informationService.uuid());
  bledevice.setConnectionInterval(0, 500);
  // Start the service running
  bledevice.begin();
}

void DoBluetoothOne()
{
      Serial.println("Bluetooth Sent");

     if(val == 0) 
     {
      val = 1;
     }
     else
     {
      val = 0;
     }
     
      // Send
      ReedSwitch.setValue(val);

      digitalWrite(7, 1);
}

void loop() {
    // Connected?
  BLECentral central = bledevice.central();

  if(central)
  {
    digitalWrite(7, 1);
    if(central.connected())
    {
      DoBluetoothOne();
    }
  }
  else
  {
    digitalWrite(7, 0);
  }
  delay(2000);
}
