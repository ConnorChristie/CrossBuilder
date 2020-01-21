#include <signal.h>
#include <iostream>
#include <fstream>
#include <cassert>
#include <string>

#include "executor.h"
#include "execution_object.h"
#include "execution_object_pipeline.h"
#include "configuration.h"

using namespace tidl;
using std::string;
using std::unique_ptr;
using std::vector;

Executor* CreateExecutor(DeviceType dt, int num, const Configuration& c)
{
    if (num == 0) return nullptr;

    DeviceIds ids;
    for (int i = 0; i < num; i++)
        ids.insert(static_cast<DeviceId>(i));

    return new Executor(dt, ids, c);
}

int main(int argc, char* argv[])
{
    std::cout << "Hello world" << std::endl;

    Configuration c;
    uint32_t num_eve = Executor::GetNumDevices(DeviceType::EVE);
    uint32_t num_dsp = Executor::GetNumDevices(DeviceType::DSP);
    // unique_ptr<Executor> e_dsp(CreateExecutor(DeviceType::DSP, num_dsp, c));

    std::cout << "Num DSPs: " << num_dsp << ", Num EVEs: " << num_eve << std::endl;

    return 0;
}
