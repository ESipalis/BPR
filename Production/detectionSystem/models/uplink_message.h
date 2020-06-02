#ifndef BPR_UPLINK_MESSAGE_H
#define BPR_UPLINK_MESSAGE_H

#include "iot_drivers/lora_driver.h"

typedef struct uplink_message* uplink_message_t;
typedef void (*uplink_message_callback_function)(e_LoRa_return_code_t);

uplink_message_t uplink_message_create(lora_payload_t* lora_payload, void (*callback) (e_LoRa_return_code_t));

#endif //BPR_UPLINK_MESSAGE_H
