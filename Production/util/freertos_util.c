#include "freertos_util.h"

void freertos_create_task(freertos_task_parameters task_parameters, TaskFunction_t function, void* const function_parameters, TaskHandle_t* const task_handle) {
    xTaskCreate(function, task_parameters.pcName, task_parameters.stackDepth, function_parameters, task_parameters.priority, task_handle);
}
