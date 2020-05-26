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

// Prototype for LoRaWAN handler
void lora_handler_create(UBaseType_t lora_handler_task_priority);

void lora_init_task(void *pvParameters);

void servo_control_task(void *pvParameters);

void hcsr04_control_task(void *pvParameters);

/*-----------------------------------------------------------*/
void create_tasks_and_semaphores(void) {
//    xTaskCreate(
//            lora_init_task, (const portCHAR *) "Lora"  // A name just for humans
//            , configMINIMAL_STACK_SIZE  // This stack size can be checked & adjusted by reading the Stack Highwater
//            , NULL, 3  // Priority, with 3 (configMAX_PRIORITIES - 1) being the highest, and 0 being the lowest.
//            , NULL);

//    xTaskCreate(
//            servo_control_task, (const portCHAR *) "Lora"  // A name just for humans
//            , configMINIMAL_STACK_SIZE  // This stack size can be checked & adjusted by reading the Stack Highwater
//            , NULL, 3  // Priority, with 3 (configMAX_PRIORITIES - 1) being the highest, and 0 being the lowest.
//            , NULL);

    xTaskCreate(
            hcsr04_control_task, (const portCHAR *) "HCSR04"  // A name just for humans
            , configMINIMAL_STACK_SIZE  // This stack size can be checked & adjusted by reading the Stack Highwater
            , NULL, 3  // Priority, with 3 (configMAX_PRIORITIES - 1) being the highest, and 0 being the lowest.
            , NULL);

}

void servo_control_task(void *pvParameters) {
    for (;;) {
        rcServoSet(0, -100);
        printf("Turned 1...\n");
        vTaskDelay(200);

        rcServoSet(0, 0);
        printf("Turned 2...\n");
        vTaskDelay(200);

        rcServoSet(0, 100);
        printf("Turned 3...\n");
        vTaskDelay(200);
    }
}

static void hcsr04_measured(uint8_t sensorNo, uint32_t timerTicksPassed) {
//    float distanceFloat = hcsr04_timerTicksToCentimeters(timerTicksPassed, 16000000);
//    uint16_t distance = distanceFloat;
    printf("Distance measured(%d): %d\n", sensorNo, timerTicksPassed);
}

void hcsr04_control_task(void *pvParameters) {
    for(;;) {
        hcsr04_initiate_measurement(1, hcsr04_measured);
        vTaskDelay(pdMS_TO_TICKS(1000));
    }
}

void lora_init_task(void *pvParameters) {
    lora_driver_reset_rn2483(1); // Activate reset line
    vTaskDelay(2);
    lora_driver_reset_rn2483(0); // Release reset line
    vTaskDelay(150); // Wait for tranceiver module to wake up after reset
    lora_driver_flush_buffers(); // get rid of first version string from module after reset!

    e_LoRa_return_code_t factory_reset_result = lora_driver_rn2483_factory_reset();
    printf("Factory reset: %s\n", lora_driver_map_return_code_to_text(factory_reset_result));

    e_LoRa_return_code_t configure_to_eu868_result = lora_driver_configure_to_eu868();
    printf("Configure to EU868: %s\n", lora_driver_map_return_code_to_text(configure_to_eu868_result));

    static char dev_eui[17];
    e_LoRa_return_code_t get_hweui_result = lora_driver_get_rn2483_hweui(dev_eui);
    printf("Get HWEUI: %s; HWEUI: %s\n", lora_driver_map_return_code_to_text(get_hweui_result), dev_eui);

    e_LoRa_return_code_t set_deveui_result = lora_driver_set_device_identifier(dev_eui);
    printf("Set DevEUI => %s: %s\n", dev_eui, lora_driver_map_return_code_to_text(set_deveui_result));

    e_LoRa_return_code_t set_otaa_identity_result = lora_driver_set_otaa_identity(LORA_appEUI, LORA_appKEY, dev_eui);
    printf("Set OTAA Identity (appEUI: %s, appKEY: %s): %s\n", LORA_appEUI, LORA_appKEY, lora_driver_map_return_code_to_text(set_otaa_identity_result));

    e_LoRa_return_code_t set_adaptive_data_rate_result = lora_driver_set_adaptive_data_rate(LoRa_ON);
    printf("Set adaptive data rate => ON: %s\n", lora_driver_map_return_code_to_text(set_adaptive_data_rate_result));

    e_LoRa_return_code_t set_receive_delay_result = lora_driver_set_receive_delay(500);
    printf("Set receive delay => 500ms: %s\n", lora_driver_map_return_code_to_text(set_receive_delay_result));

    e_LoRa_return_code_t save_mac_result = lora_driver_save_mac();
    printf("Save to mac: %s\n", lora_driver_map_return_code_to_text(save_mac_result));

    uint8_t tries = 1;
    e_LoRa_return_code_t join_result;
    while ((join_result = lora_driver_join(LoRa_OTAA)) != LoRa_ACCEPTED && tries <= LORA_JOIN_NETWORK_MAX_TRIES) {
        printf("Join network (try #%d): %s\n", tries++, lora_driver_map_return_code_to_text(join_result));
    }

    if (join_result != LoRa_ACCEPTED) {
        printf("Couldn't join the network, shutting down LoRaWAN handler task.\n");
        vTaskDelete(NULL);
        return;
    }
}

/*-----------------------------------------------------------*/
void initialiseSystem() {

    // Set output ports for leds used in the example
    DDRA |= _BV(DDA0) | _BV(DDA7);
    // Initialise the trace-driver to be used together with the R2R-Network
    trace_init();
    // Make it possible to use stdio on COM port 0 (USB) on Arduino board - Setting 57600,8,N,1
    stdioCreate(ser_USART0);
    // Let's create some tasks
    create_tasks_and_semaphores();

    // vvvvvvvvvvvvvvvvv BELOW IS LoRaWAN initialisation vvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    // Initialise the HAL layer and use 5 for LED driver priority
    hal_create(4);
    // Initialise the LoRaWAN driver without down-link buffer
//    lora_driver_create(LORA_USART, NULL);

//     rcServoCreate();
    hcsr04_create();
    hcsr04_power_up();
}

/*-----------------------------------------------------------*/
int main(void) {
    initialiseSystem(); // Must be done as the very first thing!!
    printf("Program Started!!\n");
    vTaskStartScheduler(); // Initialise and run the freeRTOS scheduler. Execution should never return from here.

    /* Replace with your application code */
    while (1) {
    }
}

