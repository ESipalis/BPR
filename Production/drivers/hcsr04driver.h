#ifndef BPR_HCSR04DRIVER_H
#define BPR_HCSR04DRIVER_H

#include <stdint.h>

void hcsr04_create();
void hcsr04_initiateMeasurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t));
float hcsr04_timerTicksToCentimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond);

#endif //BPR_HCSR04DRIVER_H
