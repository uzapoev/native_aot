cmake_minimum_required(VERSION 3.22)
project(NativeAOT_Engine LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# platform 
# ios-arm64 for ios 
# android-arm64
if(WIN32)
    set(RID "win-x64")
    set(LIB_NAME "MyGameScripting.dll")
elseif(APPLE AND NOT IOS)
    set(RID "osx-x64")
    set(LIB_NAME "libMyGameScripting.dylib")
elseif(IOS)
    set(RID "ios-arm64")
    set(LIB_NAME "libMyGameScripting.dylib")
elseif(ANDROID)
    set(RID "android-arm64")
    set(LIB_NAME "libMyGameScripting.so")
else()
    set(RID "linux-x64")
    set(LIB_NAME "libMyGameScripting.so")
endif()

# autobuild C#
add_custom_target(BuildNativeAOT ALL
    COMMAND dotnet publish "${CMAKE_CURRENT_SOURCE_DIR}/scripting/MyGameScripting" 
            -c Release -r ${RID} -o "${CMAKE_CURRENT_SOURCE_DIR}/external"
    COMMENT "Building NativeAOT C# library..."
)

# import
add_library(MyGameScripting SHARED IMPORTED GLOBAL)
set_target_properties(MyGameScripting PROPERTIES
    IMPORTED_LOCATION "${CMAKE_CURRENT_SOURCE_DIR}/external/${LIB_NAME}"
    INTERFACE_INCLUDE_DIRECTORIES "${CMAKE_CURRENT_SOURCE_DIR}/scripting/MyGameScripting"
)
add_dependencies(MyGameScripting BuildNativeAOT)

# engine
add_executable(MyEngine engine/src/main.cpp)
#target_link_libraries(MyEngine PRIVATE MyGameScripting)


add_custom_command(TARGET MyEngine POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${CMAKE_CURRENT_SOURCE_DIR}/external/${LIB_NAME}"
        $<TARGET_FILE_DIR:MyEngine>
)