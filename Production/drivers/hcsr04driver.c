#include "hcsr04driver.h"
#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdio.h>

// Trigger pin definitions, assuming both trigger pins are connected to the same port
#define DDR_TRIGGER DDRC
#define PORT_TRIGGER PORTC
#define PIN_TRIGGER_N1 PC0
#define PIN_TRIGGER_N2 PC1


// Echo pin definitions, assuming both echo pins are connected to the same port.
// 48 <=> 176: PIN 3 = 128 = 2^7;;; PIN 3 = PK7
// 160 <=> 128: PIN 5 = 32 = 2^5;;; PIN 5 = PK5
#define PORT_ECHO PORTK
#define DDR_ECHO DDRK
#define PIN_ECHO PINK
#define PIN_ECHO_N1 PK7 // Refers to pin 5 on PORTK
#define PIN_ECHO_N2 PK5 // Refers to pin 3 on PORTK
#define PCMSK_ECHO PCMSK2
#define PCIE_ECHO PCIE2
#define PCINT_ECHO_N1 PCINT23 // PK7
#define PCINT_ECHO_N2 PCINT21 // PK5


static volatile uint8_t numberOfTimerOverflows = 0;
static volatile uint8_t previousPIN_ECHO;

static void (* volatile sensor1Callback)(uint8_t, uint32_t);
static volatile uint8_t sensor1NumberOfTimerOverflows;
static volatile uint16_t sensor1TimerValue;

static void (* volatile sensor2Callback)(uint8_t, uint32_t);
static volatile uint8_t sensor2NumberOfTimerOverflows;
static volatile uint16_t sensor2TimerValue;

void hcsr04_create() {
    DDR_TRIGGER |= _BV(PIN_TRIGGER_N1) | _BV(PIN_TRIGGER_N2); // Set as output
    PORT_TRIGGER &= ~(_BV(PIN_TRIGGER_N1) | _BV(PIN_TRIGGER_N2)); // Make sure it's LOW

    DDR_ECHO &= ~(_BV(PIN_ECHO_N1) | _BV(PIN_ECHO_N2)); // Set as input
    PORT_ECHO |= _BV(PIN_ECHO_N1) | _BV(PIN_ECHO_N2); // Pull-up

    PCMSK_ECHO |= _BV(PCINT_ECHO_N1) | _BV(PCINT_ECHO_N2); // Enable PCINTs
    PCICR |= _BV(PCIE_ECHO); // Enable PCINT Register 2 vector

    TIMSK1 = _BV(TOIE1); // Enable timer overflow interrupt

    DDRA = 0xFF; // Enable LEDs
    printf("HCSR04 Create\n");
}

void hcsr04_power_up() {
    TCCR1B |= _BV(CS10); // Enable timer
}

void hcsr04_power_down() {
    TCCR1B &= ~_BV(CS10); // Disable timer
}

void hcsr04_initiate_measurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t)) {
    if (sensorNumber == 1) {
        if (PIN_ECHO & _BV(PIN_ECHO_N1)) {
            printf("Already measuring1...\n");
            return;
        }
        sensor1Callback = callback;
        PORT_TRIGGER |= _BV(PIN_TRIGGER_N1);
        _delay_us(10);
        PORT_TRIGGER &= ~_BV(PIN_TRIGGER_N1);
    } else if (sensorNumber == 2) {
        if (PIN_ECHO & _BV(PIN_ECHO_N2)) {
            printf("Already measuring2...\n");
            return;
        }
        sensor2Callback = callback;
        PORT_TRIGGER |= _BV(PIN_TRIGGER_N2);
        _delay_us(10);
        PORT_TRIGGER &= ~_BV(PIN_TRIGGER_N2);
    }
}

float hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond) {
//    return 34300 * ((float)timerTicksMeasurement / timerTicksPerSecond) / 2;
    return 17150.0 * ((double)timerTicksMeasurement / (double)timerTicksPerSecond);
}

ISR(TIMER1_OVF_vect) {
    numberOfTimerOverflows++;   /* Increment Timer Overflow count */
}

// Do we need volatile here for parameters? Think not, but not sure.
static void calculate_timer_ticks_passed_and_call_callback(uint8_t sensorNo, uint8_t sensorNumberOfTimerOverflows, uint16_t sensorTimerValue, void (*sensorCallback)(uint8_t, uint32_t)) {
    uint16_t overflows = (256 + numberOfTimerOverflows - sensorNumberOfTimerOverflows) % 255;
    uint32_t timerTicksPassed = 65536 * overflows + (TCNT1 - sensorTimerValue);
    if (sensorCallback != 0) {
        sensorCallback(sensorNo, timerTicksPassed);
    }
}

ISR(PCINT2_vect) {
    PORTA = PIN_ECHO;
    if ((PIN_ECHO ^ previousPIN_ECHO) & _BV(PIN_ECHO_N1)) { // Echo pin 1 was the one that was changed
        if(PIN_ECHO & _BV(PIN_ECHO_N1)) { // HIGH
            sensor1NumberOfTimerOverflows = numberOfTimerOverflows;
            sensor1TimerValue = TCNT1;
        } else { // LOW
            calculate_timer_ticks_passed_and_call_callback(1, sensor1NumberOfTimerOverflows, sensor1TimerValue, sensor1Callback);
        }
    } else {
        if(PIN_ECHO & _BV(PIN_ECHO_N2)) { // HIGH
            sensor2NumberOfTimerOverflows = numberOfTimerOverflows;
            sensor2TimerValue = TCNT1;
        } else { // LOW
            calculate_timer_ticks_passed_and_call_callback(2, sensor2NumberOfTimerOverflows, sensor2TimerValue, sensor2Callback);
        }
    }
    previousPIN_ECHO = PIN_ECHO;
}
