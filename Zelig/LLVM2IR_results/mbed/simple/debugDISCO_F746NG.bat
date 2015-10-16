@echo off

IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

ECHO.
ECHO Trying to start openOCD for Windows

START /I \openocd\bin\openocd -f \openocd\share\openocd\scripts\board\stm32f7discovery.cfg -s \openocd\share\openocd\scripts\

PAUSE

ECHO.
ECHO Trying to attach to remote target on port 3333
"%GCC_BIN%arm-none-eabi-gdb.exe" DISCO_F746NG\mbed_simple.elf %* -ex "target remote :3333" -ex "monitor halt reset" -ex "ni" 

exit /b