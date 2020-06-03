#ifndef BPR_CONFIGURATION_H
#define BPR_CONFIGURATION_H
#include <stdint.h>

typedef struct configuration* configuration_t;

configuration_t configuration_create();
void set_scan_minute_of_the_day(configuration_t self, uint16_t scan_minute_of_the_day);
uint16_t get_scan_minute_of_the_day(configuration_t self);
void set_heartbeat_period_days(configuration_t self, uint8_t heartbeat_period_days);
uint8_t get_heartbeat_period_days(configuration_t self);


#endif //BPR_CONFIGURATION_H
