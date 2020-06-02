cmake_minimum_required(VERSION 3.15)
project(BPR C)

set(CMAKE_C_STANDARD 11)
add_compile_definitions(__AVR_ATmega2560__)

include_directories(.
        "C:/Program Files (x86)/Atmel/Studio/7.0/toolchain/avr8/avr8-gnu-toolchain/avr/include")
#"D:/Atmel Studio/7.0/toolchain/avr8/avr8-gnu-toolchain/avr/include")

include_directories(FreeRTOS/src)
include_directories(iot_drivers)


add_executable(BPR
        main.c drivers/hcsr04driver.c drivers/hcsr04driver.h util/debug_util.h util/debug_util.c detectionSystem/models/time.c detectionSystem/models/time.h detectionSystem/models/configuration.c detectionSystem/models/configuration.h detectionSystem/models/scanning_result.c detectionSystem/models/scanning_result.h detectionSystem/tasks/execution_handler.c detectionSystem/tasks/execution_handler.h util/freertos_util.c util/freertos_util.h detectionSystem/models/uplink_message.c detectionSystem/models/uplink_message.h detectionSystem/models/uplink_message_util.c detectionSystem/models/uplink_message_util.h detectionSystem/tasks/scanning_handler.c detectionSystem/tasks/scanning_handler.h detectionSystem/tasks/processing_handler.c detectionSystem/tasks/processing_handler.h detectionSystem/tasks/uplink_handler.c detectionSystem/tasks/uplink_handler.h detectionSystem/tasks/downlink_handler.c detectionSystem/tasks/downlink_handler.h)
