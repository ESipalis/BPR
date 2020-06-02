#include "downlink_handler.h"
#include "detectionSystem/models/uplink_message.h"
#include "util/debug_util.h"
#include "detectionSystem/models/configuration.h"

void downlink_handler_function(void* vParameters) {
    downlink_handler_task_parameters* parameters = vParameters;
    lora_payload_t loraPayload;
    uint16_t scanMinuteOfTheDay;
    uint8_t heartbeatPeriodDays;
    for(;;) {
        debugPrint("Downlink handler: waiting for downlink message\n");
        xMessageBufferReceive(parameters->downlink_message_buffer, &loraPayload, sizeof(loraPayload), portMAX_DELAY);
        debugPrint("Downlink handler: payload size: %d\n", loraPayload.len);
        scanMinuteOfTheDay = (loraPayload.bytes[0] << 8) + loraPayload.bytes[1];
        heartbeatPeriodDays = loraPayload.bytes[2];
        debugPrint("Scan minute of the day: %u; Hearbeat period days: %u;\n", scanMinuteOfTheDay, heartbeatPeriodDays);
        set_scan_minute_of_the_day(parameters->configuration, scanMinuteOfTheDay);
        set_heartbeat_period_days(parameters->configuration, heartbeatPeriodDays);
    }
}

void create_downlink_handler_task(freertos_task_parameters task_parameters, downlink_handler_task_parameters* function_parameters, TaskHandle_t* task_handle) {
    freertos_create_task(task_parameters, downlink_handler_function, function_parameters, task_handle);
}
