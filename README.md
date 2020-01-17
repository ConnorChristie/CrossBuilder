# CrossBuilder
This contains several utilities for cross compiling native projects on a Windows based host.

## Highlights:
* Dependency fetcher that emulates `apt-get`
    * Downloads packages directly to the host computer instead of downloading them first to the target and copying them over.
    * Prevents having to install dev packages on the target machine.
* File catcher that tracks written files when using commands such as `make install`
    * Most cmake projects don't respect the install directory and instead write their files to all different directories.
    * Allows you to copy captured files to your sysroot or staging directory.

## Building OpenCV

1. Install dependencies

    ```sh
    CrossBuilder.exe install -s <sysroot> libavcodec-dev libavformat-dev libswscale-dev libgstreamer1.0-0 gstreamer1.0-plugins-base gstreamer1.0-plugins-good gstreamer1.0-plugins-bad gstreamer1.0-plugins-ugly gstreamer1.0-libav gstreamer1.0-tools libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libusb-1.0-0-dev libgtk-3-dev ffmpeg libgtk2.0-dev
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

## Troubleshooting:

### undefined reference to `__dlsym'
    Probably don't have libdl.so or librt.so

### ld.exe: cannot find /lib/arm-linux-gnueabihf/libc.so.6 inside
    Probably didn't update and remove absolute paths in .so files

### libm.a: error adding symbols: Bad value
    Cannot find libm.so and resorts to using libm.a which is BAD. Create libm.so