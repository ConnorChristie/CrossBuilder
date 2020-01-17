cmake_minimum_required(VERSION 2.8.9)
project(hello CXX)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -Wall -Werror -Wno-error=ignored-attributes")

add_executable(hello src/main.cpp)

target_link_libraries(hello TIOpenCL)
target_include_directories(hello SYSTEM PUBLIC ${CMAKE_PREFIX_PATH}/usr/share/ti/opencl/)
target_compile_options(hello PRIVATE)
