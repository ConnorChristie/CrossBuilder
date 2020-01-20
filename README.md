# CrossBuilder
This contains several utilities for cross compiling native projects on a Windows based host.

## Highlights
* Dependency fetcher that emulates `apt-get`
    * Downloads packages directly to the host computer instead of downloading them first to the target and copying them over.
    * Prevents having to install dev packages on the target machine.
* File catcher that tracks file modifications when using commands such as `make install`
    * Most cmake projects don't respect the install directory and instead write their files to all different directories.
    * Allows you to copy captured files to your sysroot or staging directory.

## Finding a sysroot to use

1. On the physical device, you'll want to find out what version of GCC you have. Do so by typing `gcc -v` and you should see something similar to the following:

    ```sh
    debian@beaglebone:~$ gcc -v
    Using built-in specs.
    COLLECT_GCC=gcc
    COLLECT_LTO_WRAPPER=/usr/lib/gcc/arm-linux-gnueabihf/6/lto-wrapper
    ---> Target: arm-linux-gnueabihf
    Configured with: ../src/configure -v --with-pkgversion='Debian 6.3.0-18+deb9u1' --with-bugurl=file:///usr/share/doc/gcc-6/README.Bugs --enable-languages=c,ada,c++,java,go,d,fortran,objc,obj-c++ --prefix=/usr --program-suffix=-6 --program-prefix=arm-linux-gnueabihf- --enable-shared --enable-linker-build-id --libexecdir=/usr/lib --without-included-gettext --enable-threads=posix --libdir=/usr/lib --enable-nls --with-sysroot=/ --enable-clocale=gnu --enable-libstdcxx-debug --enable-libstdcxx-time=yes --with-default-libstdcxx-abi=new --enable-gnu-unique-object --disable-libitm --disable-libquadmath --enable-plugin --enable-default-pie --with-system-zlib --disable-browser-plugin --enable-java-awt=gtk --enable-gtk-cairo --with-java-home=/usr/lib/jvm/java-1.5.0-gcj-6-armhf/jre --enable-java-home --with-jvm-root-dir=/usr/lib/jvm/java-1.5.0-gcj-6-armhf --with-jvm-jar-dir=/usr/lib/jvm-exports/java-1.5.0-gcj-6-armhf --with-arch-directory=arm --with-ecj-jar=/usr/share/java/eclipse-ecj.jar --with-target-system-zlib --enable-objc-gc=auto --enable-multiarch --disable-sjlj-exceptions --with-arch=armv7-a --with-fpu=vfpv3-d16 --with-float=hard --with-mode=thumb --enable-checking=release --build=arm-linux-gnueabihf --host=arm-linux-gnueabihf --target=arm-linux-gnueabihf
    Thread model: posix
    ---> gcc version 6.3.0 20170516 (Debian 6.3.0-18+deb9u1)
    ```

2. Now, if you have an ARM device this next step is easy since Linaro has a slew of sysroots available as well as compilers for all different platforms. To find the one you need, navigate to https://releases.linaro.org/components/toolchain/binaries/ and you'll see a list of GCC versions.

3. Choose the version closest to the version installed on the physical device, in my case the version on my device is `gcc version 6.3.0 20170516 (Debian 6.3.0-18+deb9u1)` and the closest version from Linaro is `6.3-2017.05`.

4. Now select the appropriate target where mine is `arm-linux-gnueabihf` as determined by the GCC version command above.

5. Download and extract the GCC compiler that's appropriate for your host machine. In my case, I would download `gcc-linaro-6.3.1-2017.05-i686-mingw32_arm-linux-gnueabihf.tar.xz` since I am running windows and using mingw. (TODO: how to determine the exact binaries and setup for mingw)

6. Download and extract the sysroot, in my case it's `sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf.tar.xz`.

7. Create a CMake toolchain for your new compiler and sysroot. An example one is shown below for my environment:

    ```cmake
    set(CMAKE_SYSTEM_NAME Linux)

    set(CMAKE_C_COMPILER "D:/Toolchains/gcc-linaro-6.5.0-2018.12-i686-mingw32_arm-linux-gnueabihf/bin/arm-linux-gnueabihf-gcc.exe")
    set(CMAKE_CXX_COMPILER "D:/Toolchains/gcc-linaro-6.5.0-2018.12-i686-mingw32_arm-linux-gnueabihf/bin/arm-linux-gnueabihf-g++.exe")

    set(CMAKE_SYSROOT "D:/Toolchains/sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf")
    set(CMAKE_PREFIX_PATH "D:/Toolchains/sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf")

    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -L=/usr/lib -L=/lib")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -L=/usr/lib -L=/lib")
    ```

## Building OpenCV

1. Install dependencies

    ```sh
    CrossBuilder.exe install -s "D:\Toolchains\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf" libavcodec-dev libavformat-dev libswscale-dev libgstreamer1.0-0 gstreamer1.0-plugins-base gstreamer1.0-plugins-good gstreamer1.0-plugins-bad gstreamer1.0-plugins-ugly gstreamer1.0-libav gstreamer1.0-tools libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libusb-1.0-0-dev libgtk-3-dev ffmpeg libgtk2.0-dev
    ```

2. Update ld scripts and remove absolute paths:

    * `<sysroot>\lib\libgcc_s.so`
    * `<sysroot>\usr\lib\arm-linux-gnueabihf\libc.so`
    * `<sysroot>\usr\lib\arm-linux-gnueabihf\libpthread.so`
    * `<sysroot>\usr\lib\gcc\arm-linux-gnueabihf\6\libgcc_s.so`
    * `<sysroot>\usr\lib\libc.so`
    * `<sysroot>\usr\lib\libpthread.so`

    TODO: Need to have CrossBuilder do this automatically (search for "GNU ld script")

3. Setup extra dependencies "dl m pthread rt" inside `/lib`

    * Manually create `libm.so` inside `/lib`
    * Manually create `libdl.so` inside `/lib`
    * Manually create `librt.so` inside `/lib`
    * Manually create `libpthread.so` inside `/lib`

4. Clone the [OpenCV](https://github.com/opencv/opencv) project and run CMake inside build directory:

    ```sh
    cmake -G "Unix Makefiles" -DCMAKE_TOOLCHAIN_FILE=D:\Git\CrossBuilder\armhf.toolchain.cmake -DCMAKE_BUILD_TYPE=Release -DBUILD_EXAMPLES=OFF -DBUILD_DOCS=OFF -DBUILD_PERF_TESTS=OFF -DBUILD_TESTS=OFF -DWITH_GSTREAMER=ON -DWITH_GTK=ON ..
    ```

5. Build the project:

    ```sh
    make -j10
    ```

5. Capture the installed files to be used in the sysroot:

    ```sh
    FileCatcher.exe make install
    ```

## Building RealSense

1. Install dependencies

    ```sh
    CrossBuilder.exe install -s "D:\Toolchains\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf" libusb-1.0-0-dev libglfw3-dev
    ```

2. Clone the [librealsense](https://github.com/IntelRealSense/librealsense) project and run CMake inside build directory:

    ```sh
    cmake -G "Unix Makefiles" -DCMAKE_TOOLCHAIN_FILE=D:\Git\CrossBuilder\armhf.toolchain.cmake -DCMAKE_BUILD_TYPE=Release -DBUILD_EXAMPLES=OFF -DBUILD_DOCS=OFF -DBUILD_PERF_TESTS=OFF -DBUILD_TESTS=OFF -DWITH_GSTREAMER=ON -DWITH_GTK=ON ..
    ```

3. Build the project:

    ```sh
    make -j10
    ```

5. Capture the installed files to be used in the sysroot:

    ```sh
    FileCatcher.exe make install
    ```

## Troubleshooting

### undefined reference to `__dlsym'
    Probably don't have libdl.so or librt.so

### ld.exe: cannot find /lib/arm-linux-gnueabihf/libc.so.6 inside
    Probably didn't update and remove absolute paths in .so files

### libm.a: error adding symbols: Bad value
    Cannot find libm.so and resorts to using libm.a which is BAD. Create libm.so
