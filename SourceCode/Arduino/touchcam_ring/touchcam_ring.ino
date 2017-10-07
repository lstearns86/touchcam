#include <Wire.h>
#include <SPI.h>
#include <Adafruit_LSM9DS0.h>
#include <Adafruit_Sensor.h>  // not used in this demo but required!

/*
 * Note: requires modifying the Wire library in order to achieve the maximum possible frame rate.
 * Open twi.h, which on Windows is located in (e.g.) AppData/Local/Arduino15/packages/arduino/hardware/avr/#.#.##/libraries/Wire/src/utility
 * Modify the following line, changing:
 *    #define TWI_FREQ 100000L
 *    to
 *    #define TWI_FREQ 400000L
 * Save the file and then re-compile the Arduino sketch
 */

// for MUX
extern "C" { 
#include "utility/twi.h"  // from Wire library, so we can do bus scanning
}
#define TCAADDR 0x70

void tcaselect(uint8_t i) {
  if (i > 7) return;
 
  Wire.beginTransmission(TCAADDR);
  Wire.write(1 << i);
  Wire.endTransmission();  
}

// i2c
Adafruit_LSM9DS0 lsm = Adafruit_LSM9DS0((int32_t)0);
Adafruit_LSM9DS0 lsm2 = Adafruit_LSM9DS0((int32_t)1);

int LED_pin = 9;
int IR1_pin = A0;
int IR2_pin = A1;

int IR1_THRESHOLD = 915;
int IR2_THRESHOLD = 925;

int BAUD_RATE = 38400;

int LED_MAX = 255;
byte ledBrightness = 0;
byte numSensors = 1;
bool imuInitialized = false;

long lastUpdate;
int frameCounter;

void setupIMU(Adafruit_LSM9DS0 *lsm)
{
  // 1.) Set the accelerometer range
  lsm->setupAccel(lsm->LSM9DS0_ACCELRANGE_8G);
  
  // 2.) Set the magnetometer sensitivity
  lsm->setupMag(lsm->LSM9DS0_MAGGAIN_12GAUSS);

  // 3.) Setup the gyroscope
  lsm->setupGyro(lsm->LSM9DS0_GYROSCALE_500DPS);
}

void setup() {
  Serial.begin(BAUD_RATE);

  Wire.begin();
  
  pinMode(LED_pin, OUTPUT);

  delay(1000);
  delayedSetup();
  
  lastUpdate = millis();
  frameCounter = 0;
}

void delayedSetup() {
  // Try to initialize
  tcaselect(0);
  if (!lsm.begin())
  {
    while (1);
  }
  setupIMU(&lsm);

// uncomment if you want to support two IMUs
//  tcaselect(1);
//  if (!lsm2.begin())
//  {
//    while (1);
//  }
//  setupIMU(&lsm2);

  imuInitialized = true;
}

void write(int v) {
  byte *b = (byte*)&v;
  for(byte i = 0; i < sizeof(int); i++) Serial.write(b[i]);

  // use this instead for debugging
// Serial.print(v);
// Serial.print(",");
}

void writeLSM(Adafruit_LSM9DS0 lsm) {
  lsm.read();
  write(lsm.accelData.x);
  write(lsm.accelData.y);
  write(lsm.accelData.z);
  write(lsm.magData.x);
  write(lsm.magData.y);
  write(lsm.magData.z);
  write(lsm.gyroData.x);
  write(lsm.gyroData.y);
  write(lsm.gyroData.z);
}

void loop() {
  int ir1 = analogRead(IR1_pin);
  int ir2 = analogRead(IR2_pin);

  while(Serial.available() > 0) 
  {
    byte command = Serial.read();
    ledBrightness = (command & 0x7f) * 2;
    numSensors = command > 127 ? 2 : 1;
  }
  analogWrite(LED_pin, ledBrightness);
  digitalWrite(13, ledBrightness == 0 ? LOW : HIGH);

  frameCounter += 1;

  if(imuInitialized) {
    // write all sensor readings to Serial (in byte form)
    tcaselect(0);
    write(ir1);
    write(ir2);
    writeLSM(lsm);

    // uncomment if using two IMUs
  //  if(numSensors >= 2)
  //  {
  //    tcaselect(1);
  //    writeLSM(lsm2);
  //  }
  //  else
  //  {
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //    write((float)0);
  //  }
    Serial.print("\r\n\r\n");
  }
}
