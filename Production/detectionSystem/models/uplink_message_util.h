#ifndef BPR_UPLINK_MESSAGE_UTIL_H
#define BPR_UPLINK_MESSAGE_UTIL_H
#include "uplink_message.h"
#include <stdlib.h>

uplink_message_t create_heartbeat_message(uplink_message_callback_function callback);
uplink_message_t create_object_detection_message(uint16_t widthCentimeters, uplink_message_callback_function callback);

#endif //BPR_UPLINK_MESSAGE_UTIL_H
