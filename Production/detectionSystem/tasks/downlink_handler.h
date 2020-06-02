#ifndef BPR_DOWNLINK_HANDLER_H
#define BPR_DOWNLINK_HANDLER_H

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include <queue.h>
#include <message_buffer.h>
#include "util/freertos_util.h"
#include "detectionSystem/models/configuration.h"

typedef struct downlink_handler_task_parameters {
    MessageBufferHandle_t downlink_message_buffer;
    configuration_t configuration;
} downlink_handler_task_parameters;

void create_downlink_handler_task(freertos_task_parameters task_parameters, downlink_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

#endif //BPR_DOWNLINK_HANDLER_H
