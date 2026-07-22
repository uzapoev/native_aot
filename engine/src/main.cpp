#include <iostream>
#include <windows.h>

#if ANTIVEAOT_STATIC_LIB
extern "C" 
{
    int csharp_init();
    void csharp_register_update(void* callback);
    void csharp_update(float dt);
    void csharp_shutdown();
}
#else
    typedef int (*csharp_init_pfn)();
    typedef void (*csharp_register_update_pfn)(void* callback);
    typedef void (*csharp_update_pfn)(float dt);
    typedef void (*csharp_shutdown_pfn)();
#endif

void MyUpdateCallback(float deltaTime)
{
    std::cout << "[C++] Update called, dt = " << deltaTime << "\n";
}

int main()
{
    std::cout << "=== NativeAOT Engine Starting ===\n";

    auto handle                 = LoadLibrary("../external/MyGameScripting.dll");
    auto csharp_init            = (csharp_init_pfn)GetProcAddress(handle,"csharp_init");
    auto csharp_register_update = (csharp_register_update_pfn)GetProcAddress(handle,"csharp_register_update");
    auto csharp_update          = (csharp_update_pfn)GetProcAddress(handle,"csharp_update");
    auto csharp_shutdown        = (csharp_shutdown_pfn)GetProcAddress(handle,"csharp_shutdown");
    
    csharp_init();
    
    csharp_register_update(MyUpdateCallback);
    
    for (int i = 0; i < 5; ++i)
    {
        csharp_update(0.016f);
    }

    csharp_shutdown();

    return 0;
}