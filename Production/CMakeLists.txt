cmake_minimum_required(VERSION 3.15)
project(BPR C)

set(CMAKE_C_STANDARD 11)
add_compile_definitions(__AVR_ATmega2560__)

include_directories(.
        "C:/Program Files (x86)/Atmel/Studio/7.0/toolchain/avr8/avr8-gnu-toolchain/avr/include")

include_directories(FreeRTOS/src)
include_directories(iot_drivers)


add_executable(BPR
        main.c drivers/hcsr04driver.c drivers/hcsr04driver.h)
