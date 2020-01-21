cmake_minimum_required(VERSION 3.13)
project(hello CXX)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -Wall -Werror -Wno-error=ignored-attributes")
set(PROJECT_NAME hello)

add_executable(${PROJECT_NAME} src/main.cpp)
target_link_libraries(${PROJECT_NAME} PUBLIC python3.5m)

target_link_options(${PROJECT_NAME} PRIVATE "${CMAKE_PREFIX_PATH}/usr/share/ti/tidl/tidl_api/tidl.so")
target_link_options(${PROJECT_NAME} PRIVATE "${CMAKE_PREFIX_PATH}/usr/share/ti/tidl/tidl_api/tidl_api.a")
target_link_options(${PROJECT_NAME} PRIVATE "${CMAKE_PREFIX_PATH}/usr/share/ti/tidl/tidl_api/tidl_imgutil.a")

include_directories(${CMAKE_PREFIX_PATH}/usr/share/ti/opencl)
include_directories(${CMAKE_PREFIX_PATH}/usr/share/ti/tidl/tidl_api/inc)
