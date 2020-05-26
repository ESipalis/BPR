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
#define PORT_ECHO PORTK
#define DDR_ECHO DDRK
#define PIN_ECHO PINK
#define PIN_ECHO_N1 PK5
#define PIN_ECHO_N2 PK7
#define PCMSK_ECHO PCMSK2
#define PCINT_ECHO_N1 PCINT21
#define PCINT_ECHO_N2 PCINT23


static volatile uint8_t numberOfTimerOverflows = 0;

static void (* volatile sensor1Callback)(uint8_t, uint32_t);

static volatile uint8_t sensor1NumberOfTimerOverflows;
static volatile uint16_t sensor1TimerValue;
static volatile uint8_t previousPIN_ECHO;

// 48 <=> 176: PIN 3 = 128 = 2^7;;; PIN 3 = PK7
// 160 <=> 128: PIN 5 = 32 = 2^5;;; PIN 5 = PK5

void hcsr04_create() {
//    DDR_TRIGGER |= _BV(PIN_TRIGGER_N1) | _BV(PIN_TRIGGER_N2); // Set as output
//    PORT_TRIGGER &= ~(_BV(PIN_TRIGGER_N1) | _BV(PIN_TRIGGER_N2)); // Make sure it's LOW

    DDRK &= ~(_BV(PK5) | _BV(PK7)); // Set as input
    PORTK |= _BV(PK5) | _BV(PK7); // Pull-up
//    DDR_ECHO &= ~(_BV(PIN_ECHO_N1) | _BV(PIN_ECHO_N2)); // Set as input
//    PORT_ECHO |= _BV(PIN_ECHO_N1) | _BV(PIN_ECHO_N2); // Pull-up

    PCMSK2 |= _BV(PCINT21) | _BV(PCINT23); // Enable PCINTs
    PCICR |= _BV(PCMSK2); // Enable PCINT Register 2 vector
//    PCMSK_ECHO |= _BV(PCINT_ECHO_N1) | _BV(PCINT_ECHO_N2); // Enable PCINTs
//    PCICR |= _BV(PCMSK_ECHO); // Enable PCINT Register 2 vector

    TIMSK1 = _BV(TOIE1); // Enable timer overflow interrupt


    DDRA = 0xFF;
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
        printf("M\n");
//        if (sensor1Measuring) {
//            printf("Already measuring...\n");
//            return;
//        }
        sensor1Callback = callback;
        PORT_TRIGGER |= _BV(PIN_TRIGGER_N1);
        _delay_us(20);
        PORT_TRIGGER &= ~_BV(PIN_TRIGGER_N1);
    }
}

float hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond) {
    return 34300 * ((float)timerTicksMeasurement / timerTicksPerSecond) / 2;
}

ISR(TIMER1_OVF_vect) {
    numberOfTimerOverflows++;   /* Increment Timer Overflow count */
}

ISR(PCINT2_vect) {
    printf("I%d;%d\n", PINK, previousPIN_ECHO);
    PORTA = PINK;
//    if ((PIN_ECHO ^ previousPIN_ECHO) & _BV(PIN_ECHO_N1)) { // Echo pin 1 was the one that was changed
//        if(!(PIN_ECHO & _BV(PIN_ECHO_N1))) { // If it's now HIGH
//            sensor1NumberOfTimerOverflows = numberOfTimerOverflows;
//            sensor1TimerValue = TCNT1;
//        } else { // If it's now LOW
//            uint16_t overflows = (256 + numberOfTimerOverflows - sensor1NumberOfTimerOverflows) % 255;
//            uint32_t timerTicksPassed = 65536 * overflows + (TCNT1 - sensor1TimerValue);
//            if (sensor1Callback != 0) {
//                sensor1Callback(numberOfTimerOverflows, timerTicksPassed);
//            }
//        }
//    } else {
//
//    }
    previousPIN_ECHO = PINK;
}
