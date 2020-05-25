#ifndef BPR_HCSR04DRIVER_H
#define BPR_HCSR04DRIVER_H

#include <stdint.h>

void hcsr04_create();
void hcsr04_power_up();
void hcsr04_power_down();
void hcsr04_initiate_measurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t));
float hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond);

#endif //BPR_HCSR04DRIVER_H
