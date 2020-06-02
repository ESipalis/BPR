#include "scanning_handler.h"
#include "detectionSystem/models/scanning_result.h"
#include "util/debug_util.h"

void scanning_handler_function(void *vParameters);

static scanning_result_struct scanningResults[] = {{10, 255},
                                            {11, 0},
                                            {12, -1}
};

void create_scanning_handler_task(freertos_task_parameters task_parameters, scanning_handler_task_parameters *function_parameters, TaskHandle_t *task_handle) {
    freertos_create_task(task_parameters, scanning_handler_function, function_parameters, task_handle);
}

void scanning_handler_function(void *vParameters) {
    scanning_handler_task_parameters *parameters = vParameters;
    uint8_t scanning_result_index = 0;
    for (;;) {
        debugPrint("Scanning handler: waiting for scanning semaphore\n");
        xSemaphoreTake(parameters->scanning_semaphore, portMAX_DELAY);
        debugPrint("Scanning handler: scanning semaphore taken, sending scanning result\n");
        xQueueSendToBack(parameters->scanning_result_queue, &scanningResults[scanning_result_index], portMAX_DELAY);
        debugPrint("Scanning handler: scanning result sent\n");
    }
}
