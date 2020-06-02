#ifndef BPR_UPLINK_HANDLER_H
#define BPR_UPLINK_HANDLER_H

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include <queue.h>
#include "util/freertos_util.h"

typedef struct uplink_handler_task_parameters {
    QueueHandle_t uplink_message_queue;
    const char* loraAppEui;
    const char* loraAppKey;
    uint8_t maxJoinNetworkTries;
} uplink_handler_task_parameters;

void create_uplink_handler_task(freertos_task_parameters task_parameters, uplink_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

#endif //BPR_UPLINK_HANDLER_H
