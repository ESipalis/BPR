#include "uplink_handler.h"
#include "iot_drivers/lora_driver.h"
#include "detectionSystem/models/uplink_message.h"
#include "util/debug_util.h"

uint8_t lora_initialized = 0;

void initialize_lora(const char* loraAppEui, const char* loraAppKey, uint8_t maxJoinNetworkTries);

void uplink_handler_function(void* vParameters) {
    uplink_handler_task_parameters* parameters = vParameters;
//    uplink_message_t uplinkMessage;
//    for(;;) {
//        debugPrint("Uplink handler: waiting for uplink message\n");
//        xQueueReceive(parameters->uplink_message_queue, &uplinkMessage, portMAX_DELAY);
//        debugPrint("Uplink handler: received uplink message\n");
//        e_LoRa_return_code_t returnCode = lora_driver_sent_upload_message(true, uplink_message_get_lora_payload(uplinkMessage));
//        uplink_message_callback_function callback = uplink_message_get_callback(uplinkMessage);
//        if(callback != NULL) {
//            debugPrint("Uplink handler: calling callback with return code: %s\n", lora_driver_map_return_code_to_text(returnCode));
//            callback(returnCode);
//        }
//    }
    static lora_payload_t uplink_payload;
    static uint8_t payload_index = 0;
    uplink_payload.port_no = 1;
    uplink_payload.len = 2;

    TickType_t currentTicks = xTaskGetTickCount();
    for(;;) {
        if (!lora_initialized) {
            initialize_lora(parameters->loraAppEui, parameters->loraAppKey, parameters->maxJoinNetworkTries);
        }
        if(!lora_initialized) {
            debugPrint("Lora not initialized...Continuing...\n");
            continue;
        }
        switch (payload_index) {
            case 0: // 0 width
                uplink_payload.bytes[0] = 0;
                uplink_payload.bytes[1] = 0;
                break;
            case 1: // 200 width
                uplink_payload.bytes[0] = 0;
                uplink_payload.bytes[1] = 200;
                break;
            case 2: // max width (meaning no estimated size)
                uplink_payload.bytes[0] = 0xFF;
                uplink_payload.bytes[1] = 0xFF;
                break;
        }
        payload_index = (payload_index + 1) % 3;
		debugPrint("Sending message with payload index - %d\n", payload_index);

        e_LoRa_return_code_t send_message_result = lora_driver_sent_upload_message(true, &uplink_payload);
        if(send_message_result == LoRa_NOT_JOINED) {
            lora_initialized = 0;
            initialize_lora(parameters->loraAppEui, parameters->loraAppKey, parameters->maxJoinNetworkTries); // Also called here for faster reconnection.
        }
        vTaskDelayUntil(&currentTicks, pdMS_TO_TICKS(60000));
    }
}

void create_uplink_handler_task(freertos_task_parameters task_parameters, uplink_handler_task_parameters* function_parameters, TaskHandle_t* task_handle) {
    freertos_create_task(task_parameters, uplink_handler_function, function_parameters, task_handle);
}

void initialize_lora(const char* loraAppEui, const char* loraAppKey, uint8_t maxJoinNetworkTries) {
    lora_driver_reset_rn2483(1); // Activate reset line
    vTaskDelay(2);
    lora_driver_reset_rn2483(0); // Release reset line
    vTaskDelay(150); // Wait for tranceiver module to wake up after reset
    lora_driver_flush_buffers(); // get rid of first version string from module after reset!

    e_LoRa_return_code_t factory_reset_result = lora_driver_rn2483_factory_reset();
    debugPrint("Factory reset: %s\n", lora_driver_map_return_code_to_text(factory_reset_result));

    e_LoRa_return_code_t configure_to_eu868_result = lora_driver_configure_to_eu868();
    debugPrint("Configure to EU868: %s\n", lora_driver_map_return_code_to_text(configure_to_eu868_result));

    static char dev_eui[17];
    e_LoRa_return_code_t get_hweui_result = lora_driver_get_rn2483_hweui(dev_eui);
    debugPrint("Get HWEUI: %s; HWEUI: %s\n", lora_driver_map_return_code_to_text(get_hweui_result), dev_eui);

    e_LoRa_return_code_t set_deveui_result = lora_driver_set_device_identifier(dev_eui);
    debugPrint("Set DevEUI => %s: %s\n", dev_eui, lora_driver_map_return_code_to_text(set_deveui_result));

    e_LoRa_return_code_t set_otaa_identity_result = lora_driver_set_otaa_identity(loraAppEui, loraAppKey, dev_eui);
    debugPrint("Set OTAA Identity (appEUI: %s, appKEY: %s): %s\n", loraAppEui, loraAppKey, lora_driver_map_return_code_to_text(set_otaa_identity_result));

    e_LoRa_return_code_t set_spreading_factor_result = lora_driver_set_spreading_factor(12);
    debugPrint("Set spreading factor (%u) result: %s\n", 12, lora_driver_map_return_code_to_text(set_spreading_factor_result));

//    e_LoRa_return_code_t set_adaptive_data_rate_result = lora_driver_set_adaptive_data_rate(LoRa_ON);
//    debugPrint("Set adaptive data rate => ON: %s\n", lora_driver_map_return_code_to_text(set_adaptive_data_rate_result));

    e_LoRa_return_code_t set_receive_delay_result = lora_driver_set_receive_delay(500);
    debugPrint("Set receive delay => 500ms: %s\n", lora_driver_map_return_code_to_text(set_receive_delay_result));

    e_LoRa_return_code_t save_mac_result = lora_driver_save_mac();
    debugPrint("Save to mac: %s\n", lora_driver_map_return_code_to_text(save_mac_result));

    uint8_t tries = 1;
    e_LoRa_return_code_t join_result;
    while ((join_result = lora_driver_join(LoRa_OTAA)) != LoRa_ACCEPTED && tries <= maxJoinNetworkTries) {
        debugPrint("Join network (try #%d): %s\n", tries++, lora_driver_map_return_code_to_text(join_result));
    }

    if (join_result == LoRa_ACCEPTED) {
        lora_initialized = 1;
    }
}
