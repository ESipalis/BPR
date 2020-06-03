/*
* main.c
* Author : IHA
*
* Example main file including LoRaWAN setup
* Just for inspiration :)
*/

#include <stdio.h>
#include <avr/io.h>
#include <avr/sfr_defs.h>

#include <hal_defs.h>
#include <ihal.h>

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>

#include <FreeRTOSTraceDriver.h>
#include <stdio_driver.h>
#include <serial.h>

// Needed for LoRaWAN
#include <lora_driver.h>

#include <rcServo.h>
#include "drivers/hcsr04driver.h"

#define LORA_appEUI "d78039d42ee6237a"
#define LORA_appKEY "99f3755169ee604cf8f0472c4a99daf7"
#define LORA_JOIN_NETWORK_MAX_TRIES 7

#include "util/debug_util.h"
#include "detectionSystem/tasks/uplink_handler.h"
#include "detectionSystem/tasks/downlink_handler.h"
#include "detectionSystem/models/uplink_message.h"

void lora_init_task(void *pvParameters);
void servo_control_task(void *pvParameters);
void hcsr04_control_task(void *pvParameters);

QueueHandle_t uplink_message_queue;
MessageBufferHandle_t downlink_message_buffer;
configuration_t configuration;

/*-----------------------------------------------------------*/
void initialize_freertos_stuff(void) {
    uplink_message_queue = xQueueCreate(10, sizeof(uplink_message_t));
    downlink_message_buffer = xMessageBufferCreate(10 * (sizeof(lora_payload_t) + sizeof(size_t)));
    configuration = configuration_create();

    static uplink_handler_task_parameters uplinkHandlerTaskParameters;
    uplinkHandlerTaskParameters.uplink_message_queue = uplink_message_queue;
    uplinkHandlerTaskParameters.loraAppEui = LORA_appEUI;
    uplinkHandlerTaskParameters.loraAppKey = LORA_appKEY;
    uplinkHandlerTaskParameters.maxJoinNetworkTries = LORA_JOIN_NETWORK_MAX_TRIES;
    freertos_task_parameters taskParameters = {"", configMINIMAL_STACK_SIZE, 3};
    create_uplink_handler_task(taskParameters, &uplinkHandlerTaskParameters, NULL);

    static downlink_handler_task_parameters downlinkHandlerTaskParameters;
    downlinkHandlerTaskParameters.downlink_message_buffer = downlink_message_buffer;
    downlinkHandlerTaskParameters.configuration = configuration;
    create_downlink_handler_task(taskParameters, &downlinkHandlerTaskParameters, NULL);
}

void servo_control_task(void *pvParameters) {
    for (;;) {
        rcServoSet(0, -100);
        debugPrint("Turned 1...\n");
        vTaskDelay(200);

        rcServoSet(0, 0);
        debugPrint("Turned 2...\n");
        vTaskDelay(200);

        rcServoSet(0, 100);
        debugPrint("Turned 3...\n");
        vTaskDelay(200);
    }
}

static void hcsr04_measured(uint8_t sensorNo, uint32_t timerTicksPassed) {
    uint16_t distance = hcsr04_timer_ticks_to_centimeters(timerTicksPassed, 16000000);
    debugPrint("Distance measured(%d): %u\n", sensorNo, distance);
}

void hcsr04_control_task(void *pvParameters) {
    for (;;) {
        hcsr04_initiate_measurement(2, hcsr04_measured);
        hcsr04_initiate_measurement(1, hcsr04_measured);
        vTaskDelay(pdMS_TO_TICKS(1000));
    }
}

/*-----------------------------------------------------------*/
void initialiseSystem() {
    // Make it possible to use stdio on COM port 0 (USB) on Arduino board - Setting 57600,8,N,1
    stdioCreate(ser_USART0);

    initialize_freertos_stuff();

    // Initialise the HAL layer and use 5 for LED driver priority
    hal_create(4);
    // Initialise the LoRaWAN driver without down-link buffer
    lora_driver_create(LORA_USART, downlink_message_buffer);

//     rcServoCreate();
//    hcsr04_create(2);
//    hcsr04_add(1, &PORTC, PC0, PK7);
//    hcsr04_add(2, &PORTC, PC1, PK5);
//    hcsr04_power_up();
}

/*-----------------------------------------------------------*/
int main(void) {
    initialiseSystem(); // Must be done as the very first thing!!
    debugPrint("Program Started!!\n");
    vTaskStartScheduler(); // Initialise and run the freeRTOS scheduler. Execution should never return from here.

    /* Replace with your application code */
    while (1) {
    }
}

