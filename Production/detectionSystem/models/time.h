#ifndef BPR_TIME_H
#define BPR_TIME_H
#include <stdint.h>

typedef struct time* detection_system_time_t;

detection_system_time_t time_create();
void set_minute_of_the_day(detection_system_time_t time, uint16_t minuteOfTheDay);
uint16_t get_minute_of_the_day(detection_system_time_t time);

void set_initialized(detection_system_time_t time, uint8_t initialized);
uint8_t is_initialized(detection_system_time_t time);

void set_day(detection_system_time_t time, uint8_t day);
uint8_t get_day(detection_system_time_t time);

#endif //BPR_TIME_H
