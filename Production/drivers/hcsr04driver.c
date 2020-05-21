#include "hcsr04driver.h"
#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>

#define TRIGGER_PIN PK4
#define ECHO_PIN PK2

static uint8_t numberOfTimerOverflows = 0;

static void (*sensor1Callback)(uint8_t, uint32_t);

static uint8_t sensor1NumberOfTimerOverflows;
static uint16_t sensor1TimerValue;
static uint8_t sensor1Measuring = 0;

void hcsr04_create() {
    DDRK |= _BV(TRIGGER_PIN); // Set as output
    PORTK |= _BV(ECHO_PIN); // Pull-up

    TIMSK1 = _BV(TOIE1); // Enable timer overflow interrupt
    TCCR1B |= _BV(CS10); // Enable timer

    PCMSK2 |= _BV(PCINT20); // Enable PCINT20
    PCICR |= _BV(PCIE2); // Enable PCINT Register 2 vector
}

void hcsr04_initiateMeasurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t)) {
    if (sensorNumber == 1) {
        sensor1Callback = callback;
        sensor1Measuring = 0;
        PORTK |= _BV(TRIGGER_PIN);
        _delay_us(10);
        PORTK &= ~_BV(TRIGGER_PIN);
    }
}

float hcsr04_timerTicksToCentimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond) {
    return 34300 * ((float)timerTicksMeasurement / timerTicksPerSecond) / 2;
}

ISR(TIMER1_OVF_vect) {
    numberOfTimerOverflows++;   /* Increment Timer Overflow count */
}

ISR(PCINT2_vect) {
    if(!sensor1Measuring) {
        sensor1NumberOfTimerOverflows = numberOfTimerOverflows;
        sensor1TimerValue = TCNT1;
        sensor1Measuring = 1;
    } else {
        int16_t overflows = numberOfTimerOverflows - sensor1NumberOfTimerOverflows;
        overflows = overflows > 0 ? overflows : overflows + 255;
        uint32_t timerTicksPassed = 65536 * (overflows > 0) + (TCNT1 - sensor1TimerValue);
        sensor1Measuring = 0;
        sensor1Callback(1, timerTicksPassed);
    }

}
