#include "uplink_message.h"

#include <stdlib.h>

struct uplink_message {
    lora_payload_t* lora_payload;
    uplink_message_callback_function callback;
};


uplink_message_t uplink_message_create(lora_payload_t* lora_payload, uplink_message_callback_function callback) {
    uplink_message_t self = calloc(1, sizeof(*self));
    self->lora_payload = lora_payload;
    self->callback = callback;
    return self;
}

lora_payload_t* uplink_message_get_lora_payload(uplink_message_t uplinkMessage) {
    return uplinkMessage->lora_payload;
}

uplink_message_callback_function uplink_message_get_callback(uplink_message_t uplinkMessage) {
    return uplinkMessage->callback;
}
