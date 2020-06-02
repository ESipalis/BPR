#ifndef BPR_FREERTOS_UTIL_H
#define BPR_FREERTOS_UTIL_H
#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>

#define mutexSection(...) xSemaphoreTake(self->mutex, portMAX_DELAY); __VA_ARGS__ xSemaphoreGive(self->mutex);

typedef struct task_parameters {
    const char* const pcName;
    const uint16_t stackDepth;
    UBaseType_t priority;
} freertos_task_parameters;

void freertos_create_task(freertos_task_parameters task_parameters, TaskFunction_t function, void* const function_parameters, TaskHandle_t* const task_handle);

#endif //BPR_FREERTOS_UTIL_H
