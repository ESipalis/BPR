#include <stdarg.h>
#include <stdio.h>

void debugPrint(const char *format, ...) {
#ifdef DEBUG_PRINT
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
#endif
}
