#ifndef BPR_SCANNING_HANDLER_H
#define BPR_SCANNING_HANDLER_H

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include <queue.h>
#include "util/freertos_util.h"

typedef struct scanning_handler_task_parameters {
    SemaphoreHandle_t scanning_semaphore;
    QueueHandle_t scanning_result_queue;
} scanning_handler_task_parameters;

void create_scanning_handler_task(freertos_task_parameters task_parameters, scanning_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

#endif //BPR_SCANNING_HANDLER_H
