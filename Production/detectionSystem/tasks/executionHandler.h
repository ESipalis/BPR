#ifndef BPR_EXECUTIONHANDLER_H
#define BPR_EXECUTIONHANDLER_H

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>
#include <queue.h>
#include <detectionSystem/models/time.h>
#include <detectionSystem/models/configuration.h>
#include "util/freertos_util.h"

typedef struct execution_handler_task_parameters {
    SemaphoreHandle_t scanning_semaphore;
    detection_system_time_t current_time;
    configuration_t configuration;
    QueueHandle_t uplink_message_queue;
} execution_handler_task_parameters;

void create_execution_handler_task(freertos_task_parameters task_parameters, execution_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

#endif //BPR_EXECUTIONHANDLER_H
