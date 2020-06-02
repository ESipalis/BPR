#include "time.h"
#include <stdlib.h>
#include "ATMEGA_FreeRTOS.h"
#include "semphr.h"

struct time {
    uint8_t initialized;
    uint8_t minute_of_the_day;
    uint8_t day;
    SemaphoreHandle_t mutex;
};

detection_system_time_t create() {
    detection_system_time_t time = calloc(1, sizeof(*time));
    time->mutex = xSemaphoreCreateMutex();
}

void set_minute_of_the_day(detection_system_time_t time, uint16_t minuteOfTheDay) {
    xSemaphoreTake(time->mutex, portMAX_DELAY);
    time->minute_of_the_day = minuteOfTheDay;
    xSemaphoreGive(time->mutex);
}
uint16_t get_minute_of_the_day(detection_system_time_t time) {
    xSemaphoreTake(time->mutex, portMAX_DELAY);
    uint16_t minuteOfTheDay = time->minute_of_the_day;
    xSemaphoreGive(time->mutex);
    return minuteOfTheDay;
}

void set_initialized(detection_system_time_t time, uint8_t initialized) {
//    xSemaphoreTake(time->mutex, portMAX_DELAY);
    time->initialized = initialized;
//    xSemaphoreGive(time->mutex);
}

uint8_t is_initialized(detection_system_time_t time) {
    return time->initialized;
}

void set_day(detection_system_time_t time, uint8_t day) {
    time->day = day;
}

uint8_t get_day(detection_system_time_t time) {
    return time->day;
}
