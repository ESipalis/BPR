#include "execution_handler.h"
#include "detectionSystem/models/uplink_message_util.h"
#include "util/debug_util.h"

void task_function(void *vParameters);

void create_execution_handler_task(freertos_task_parameters task_parameters, execution_handler_task_parameters *function_parameters, TaskHandle_t *task_handle) {
    freertos_create_task(task_parameters, task_function, function_parameters, task_handle);
}

void task_function(void *vParameters) {
    debugPrint("Execution handler: starting...\n");
    execution_handler_task_parameters *parameters = vParameters;
    uplink_message_t heartbeatMessage = create_heartbeat_message(NULL);
    TickType_t currentTicks = xTaskGetTickCount();
    while (!is_initialized(parameters->current_time)) {
        xQueueSendToBack(parameters->uplink_message_queue, heartbeatMessage, (TickType_t)portMAX_DELAY);
        debugPrint("Current time not initialized, enqued heartbeat message\n");
        vTaskDelayUntil(&currentTicks, pdMS_TO_TICKS(60000));
    }

    for(;;) {
        xSemaphoreGive(parameters->scanning_semaphore);
        debugPrint("Execution handler: gave scanning semaphore, going to sleep.\n");
        vTaskDelayUntil(&currentTicks, pdMS_TO_TICKS(60000));
    }

}
