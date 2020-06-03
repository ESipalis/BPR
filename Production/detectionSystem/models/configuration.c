#include "configuration.h"
#include <stdlib.h>
#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include "util/freertos_util.h"

struct configuration {
    uint16_t scan_minute_of_the_day;
    uint8_t heartbeat_period_days;
    SemaphoreHandle_t mutex;
};

configuration_t configuration_create() {
    configuration_t configuration = calloc(1, sizeof(*configuration));
    configuration->mutex = xSemaphoreCreateMutex();
    return configuration;
}

void set_scan_minute_of_the_day(configuration_t self, uint16_t scan_minute_of_the_day) {
    mutexSection(
            self->scan_minute_of_the_day = scan_minute_of_the_day;
    )
}

uint16_t get_scan_minute_of_the_day(configuration_t self) {
    mutexSection(
            uint16_t scan_minute_of_the_day = self->scan_minute_of_the_day;
    )
    return scan_minute_of_the_day;
}

void set_heartbeat_period_days(configuration_t self, uint8_t heartbeat_period_days) {
    self->heartbeat_period_days = heartbeat_period_days;
}

uint8_t get_heartbeat_period_days(configuration_t self) {
    return self->heartbeat_period_days;
}
