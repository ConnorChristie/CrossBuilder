set(CMAKE_SYSTEM_NAME Linux)

set(CMAKE_C_COMPILER "D:/Toolchains/gcc-linaro-6.5.0-2018.12-i686-mingw32_arm-linux-gnueabihf/bin/arm-linux-gnueabihf-gcc.exe")
set(CMAKE_CXX_COMPILER "D:/Toolchains/gcc-linaro-6.5.0-2018.12-i686-mingw32_arm-linux-gnueabihf/bin/arm-linux-gnueabihf-g++.exe")

set(CMAKE_SYSROOT "D:/Toolchains/beaglebone-sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf")
set(CMAKE_PREFIX_PATH "D:/Toolchains/beaglebone-sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf")

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -L=/usr/lib -L=/lib -L=/usr/lib/arm-linux-gnueabihf")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -L=/usr/lib -L=/lib -L=/usr/lib/arm-linux-gnueabihf")
