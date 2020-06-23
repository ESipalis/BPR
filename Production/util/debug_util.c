#include <stdarg.h>
#include <stdio.h>

#include <ATMEGA_FreeRTOS.h>
#include <semphr.h>

SemaphoreHandle_t printMutex;

void debugPrint(const char *format, ...) {
#ifdef DEBUG_PRINT
    if(printMutex == NULL) {
        printMutex = xSemaphoreCreateMutex();
    }
    xSemaphoreTake(printMutex, portMAX_DELAY);
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
    xSemaphoreGive(printMutex);
#endif
}
