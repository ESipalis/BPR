#include "processing_handler.h"

#include <stdlib.h>
#include "detectionSystem/models/scanning_result.h"
#include "util/debug_util.h"
#include "detectionSystem/models/uplink_message_util.h"

void processing_handler_function(void* vParameters);

void create_processing_handler_task(freertos_task_parameters task_parameters, processing_handler_task_parameters* function_parameters, TaskHandle_t* task_handle);

uplink_message_t uplinkMessage;

void callback(e_LoRa_return_code_t loRaReturnCode) {
    debugPrint("Processing handler: callbacked with return code %s\n", lora_driver_map_return_code_to_text(loRaReturnCode));
    free(uplinkMessage);
}

void processing_handler_function(void* vParameters) {
    processing_handler_task_parameters* parameters = vParameters;
    scanning_result_struct* scanningResult;
    for(;;) {
        debugPrint("Processing handler: taking scanning result\n");
        xQueueReceive(parameters->scanning_result_queue, &scanningResult, portMAX_DELAY);
        debugPrint("Processing handler: scanning result received\n");
        uplinkMessage = create_object_detection_message(scanningResult->width, callback);
        xQueueSendToBack(parameters->uplink_message_queue, uplinkMessage, portMAX_DELAY);
        debugPrint("Processing handler: uplink message sent\n");
    }
}
