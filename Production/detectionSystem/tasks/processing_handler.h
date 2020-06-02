#ifndef BPR_PROCESSING_HANDLER_H
#define BPR_PROCESSING_HANDLER_H

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include <queue.h>
#include "util/freertos_util.h"

typedef struct processing_handler_task_parameters {
    QueueHandle_t scanning_result_queue;
    QueueHandle_t uplink_message_queue;
} processing_handler_task_parameters;

void create_processing_handler_task(freertos_task_parameters task_parameters, processing_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

#endif //BPR_PROCESSING_HANDLER_H
