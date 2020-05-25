#include "hcsr04driver.h"
#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdio.h>

#define TRIGGER_PIN PK4
#define ECHO_PIN PK2

static volatile uint8_t numberOfTimerOverflows = 0;

static void (* volatile sensor1Callback)(uint8_t, uint32_t);

static volatile uint8_t sensor1NumberOfTimerOverflows;
static volatile uint16_t sensor1TimerValue;
static volatile uint8_t sensor1Measuring = 0;

void hcsr04_create() {
    DDRK |= _BV(TRIGGER_PIN); // Set as output
    PORTK &= ~_BV(TRIGGER_PIN); // Make sure it's LOW

    DDRK &= ~_BV(ECHO_PIN); // Set as input
    PORTK |= _BV(ECHO_PIN); // Pull-up

    TIMSK1 = _BV(TOIE1); // Enable timer overflow interrupt

    PCMSK2 |= _BV(PCINT18); // Enable PCINT18, PK2
    PCICR |= _BV(PCIE2); // Enable PCINT Register 2 vector
}

void hcsr04_power_up() {
    TCCR1B |= _BV(CS10); // Enable timer
}

void hcsr04_power_down() {
    TCCR1B &= ~_BV(CS10); // Disable timer
}

void hcsr04_initiate_measurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t)) {
    if (sensorNumber == 1) {
        sensor1Measuring = 0;
//        PORTK |= _BV(ECHO_PIN); // Check if ISR triggers.
//        if (sensor1Measuring) {
//            printf("Already measuring...\n");
//            return;
//        }
        sensor1Callback = callback;
        PORTK |= _BV(TRIGGER_PIN);
        _delay_us(12);
        PORTK &= ~_BV(TRIGGER_PIN);
    }
}

float hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond) {
    return 34300 * ((float)timerTicksMeasurement / timerTicksPerSecond) / 2;
}

ISR(TIMER1_OVF_vect) {
    numberOfTimerOverflows++;   /* Increment Timer Overflow count */
}

ISR(PCINT2_vect) {
    printf("T\n");
    if(!sensor1Measuring) {
        sensor1NumberOfTimerOverflows = numberOfTimerOverflows;
        sensor1TimerValue = TCNT1;
        sensor1Measuring = 1;
    } else {
        uint16_t overflows = (256 + numberOfTimerOverflows - sensor1NumberOfTimerOverflows) % 255;
        uint32_t timerTicksPassed = 65536 * overflows + (TCNT1 - sensor1TimerValue);
        sensor1Measuring = 0;
        sensor1Callback(numberOfTimerOverflows, timerTicksPassed);
    }

}
