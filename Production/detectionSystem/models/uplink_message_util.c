#include "uplink_message_util.h"


uplink_message_t create_object_detection_message(uint16_t widthCentimeters, uplink_message_callback_function callback) {
    lora_payload_t *loraPayload = calloc(1, sizeof(*loraPayload));
    loraPayload->len = 2;
    loraPayload->port_no = 1;
    loraPayload->bytes[0] = widthCentimeters >> 8;
    loraPayload->bytes[1] = widthCentimeters & 0xFF;
    uplink_message_t uplinkMessage = uplink_message_create(loraPayload, callback);
    return uplinkMessage;
}

uplink_message_t create_heartbeat_message(uplink_message_callback_function callback) {
    lora_payload_t *loraPayload = calloc(1, sizeof(*loraPayload));
    loraPayload->len = 0;
    loraPayload->port_no = 1;
    uplink_message_t uplinkMessage = uplink_message_create(loraPayload, callback);
    return uplinkMessage;
}
