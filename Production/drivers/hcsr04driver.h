#ifndef BPR_HCSR04DRIVER_H
#define BPR_HCSR04DRIVER_H

#include <stdint.h>

typedef struct hcsr04 *hcsr04_t;

void hcsr04_create(uint8_t sensor_count);

void hcsr04_add(uint8_t sensor_number, volatile uint8_t* trigger_port, uint8_t trigger_pin, uint8_t echo_pin);

void hcsr04_power_up();

void hcsr04_power_down();

void hcsr04_initiate_measurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t));

uint16_t hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond);

uint8_t hcsr04_is_measurement_valid(uint32_t timerTicks, uint32_t timerTicksPerSecond);

#endif //BPR_HCSR04DRIVER_H
