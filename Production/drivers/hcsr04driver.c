#include "hcsr04driver.h"
#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdio.h>
#include <stdlib.h>

#include "../util/debug_util.h"

// 48 <=> 176: PIN 3 = 128 = 2^7;;; PIN 3 = PK7
// 160 <=> 128: PIN 5 = 32 = 2^5;;; PIN 5 = PK5
// Echo PORT definitions. If a different PORT is to be used these values have to be changed:
// PCINT values refer to the pin change interrupt index and vect, e.g. PORTK has PCINT index of 2 and its vector is PCINT2_vect
#define PORT_ECHO PORTK
#define PCINT_REGISTER_ECHO 2
#define PCINT_VECT_ECHO PCINT2_vect

// These values do not have to be changed
#define PIN_ECHO *(&PORT_ECHO - 2)
#define PCMSK_ECHO *(&PCMSK0 + PCINT_REGISTER_ECHO)
#define PCIE_ECHO PCINT_REGISTER_ECHO
// Guess the line below doesn't work because _VECTOR has to take a literal number and cannot perform an operation?
//#define PCINT_VECT_ECHO _VECTOR(PCINT0_vect_num + PCINT_REGISTER_ECHO)


// Cannot make ECHO port choosable as then interrupt routines have to be implemented for all possible choices.
// Basically disabling interrupts for all other purposes as you cannot make multiple interrupt service routines.
// It can be possible to implement the interrupt routines only when certain definitions are defined.
struct hcsr04 {
    volatile uint8_t* trigger_port;
    uint8_t trigger_pin;
    uint8_t echo_pin;
    // volatile because fields are shared between main thread and ISR.
    void (* volatile callback)(uint8_t, uint32_t);
    volatile uint8_t numberOfTimerOverflows;
    volatile uint16_t timerValue;
};

static hcsr04_t *sensors;
static uint8_t sensor_count;

static volatile uint8_t numberOfTimerOverflows = 0;
static volatile uint8_t previousPIN_ECHO;


void hcsr04_create(uint8_t sensorCount) {
    sensors = calloc(sensorCount, sizeof(*sensors)); // Allocate enough space for the array of pointers.
    sensor_count = sensorCount;

    TIMSK1 = _BV(TOIE1); // Enable timer overflow interrupt
}

void hcsr04_add(uint8_t sensor_number, volatile uint8_t* trigger_port, uint8_t trigger_pin, uint8_t echo_pin) {
    if(sensor_number > sensor_count) {
        debugPrint("Sensor number cannot be higher than sensor count: %d\n", sensor_number);
        return;
    }
    if(sensors[sensor_number - 1] != NULL) {
        debugPrint("Sensor already registered at this number: %d\n", sensor_number);
        return;
    }

    hcsr04_t sensor = malloc(sizeof(*sensor));
    sensor->trigger_port = trigger_port;
    sensor->trigger_pin = trigger_pin;
    sensor->echo_pin = echo_pin;

    *(trigger_port-1) |= _BV(trigger_pin); // Set trigger pin as output.
    *trigger_port &= ~_BV(trigger_pin);

    *(&PORT_ECHO - 1) &= ~_BV(echo_pin); // Set as input
    PORT_ECHO |= _BV(echo_pin); // Pull up
    PCMSK_ECHO |= _BV(echo_pin); // Enable PCINT interrupt.

    sensors[sensor_number - 1] = sensor;
}

void hcsr04_power_up() {
    TCCR1B |= _BV(CS10); // Enable timer
    PCICR |= _BV(PCIE_ECHO); // Enable PCINT Register vector
    debugPrint("HCSR04 Power up\n");
}

void hcsr04_power_down() {
    TCCR1B &= ~_BV(CS10); // Disable timer
    PCICR &= ~_BV(PCIE_ECHO); // Disable PCINT Register vector
    debugPrint("HCSR04 Power down\n");
}

// Putting callback in this method instead of the constructor, it allows to reuse the same sensor for different purposes.
void hcsr04_initiate_measurement(uint8_t sensorNumber, void (*callback)(uint8_t, uint32_t)) {
    hcsr04_t sensor = sensors[sensorNumber - 1];
    if(sensor == NULL) {
        debugPrint("Sensor %d not added\n", sensorNumber);
        return;
    }

    if (PIN_ECHO & _BV(sensor->echo_pin)) {
        debugPrint("Sensor %d already measuring\n", sensorNumber);
        return;
    }
    sensor->callback = callback;
    // Set trigger pin HIGH for 10us.
    *sensor->trigger_port |= _BV(sensor->trigger_pin);
    _delay_us(10);
    *sensor->trigger_port &= ~_BV(sensor->trigger_pin);
}

uint16_t hcsr04_timer_ticks_to_centimeters(uint32_t timerTicksMeasurement, uint32_t timerTicksPerSecond) {
    return (uint16_t)(timerTicksMeasurement / 58.f) / (timerTicksPerSecond / 1000000.f);
}

uint8_t hcsr04_is_measurement_valid(uint32_t timerTicks, uint32_t timerTicksPerSecond) {
    // 400 * 58 = 23200
    return timerTicks > 23200 * (timerTicksPerSecond / 1000000.f);
}

ISR(TIMER1_OVF_vect) {
    numberOfTimerOverflows++;   /* Increment Timer Overflow count */
}

ISR(PCINT_VECT_ECHO) {
    uint8_t changedPins = PIN_ECHO ^ previousPIN_ECHO;
    for (int a = 0; a < sensor_count; a++) {
        if (sensors[a] != NULL && (changedPins & _BV(sensors[a]->echo_pin))) { // Sensor a pin changed.
            hcsr04_t sensor = sensors[a];
            if (PIN_ECHO & _BV(sensor->echo_pin)) { // HIGH
                sensor->numberOfTimerOverflows = numberOfTimerOverflows;
                sensor->timerValue = TCNT1;
            } else { // LOW
                uint16_t overflows = (256 + (uint16_t)numberOfTimerOverflows - (uint16_t)sensor->numberOfTimerOverflows) % 256;
                uint16_t timerValue = TCNT1;
                int32_t timerDifference = (int32_t)timerValue - (int32_t)sensor->timerValue;
                uint32_t timerTicksPassed = 65536 * overflows + timerDifference;
//                debugPrint("O1: %u\n", numberOfTimerOverflows);
//                debugPrint("O2: %u\n", sensor->numberOfTimerOverflows);
//                debugPrint("O: %u\n", overflows);
//                debugPrint("T1: %u\n", timerValue);
//                debugPrint("T2: %u\n", sensor->timerValue);
//                debugPrint("T: %ld\n", timerDifference);
//                debugPrint("R: %lu\n", timerTicksPassed);

                if (sensor->callback != NULL) {
                    sensor->callback(a + 1, timerTicksPassed);
                }
            }
        }
    }
    previousPIN_ECHO = PIN_ECHO;
}
