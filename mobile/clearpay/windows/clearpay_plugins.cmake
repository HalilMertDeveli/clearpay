# T-075 / T-091: do not build firebase_core / firebase_auth / cloud_firestore
# on Windows. CMake extracts firebase_cpp_sdk_windows_*.zip (~1GB) and fails
# ARCHIVE_EXTRACT. Android still uses those plugins. Dart initClearPayFirebase
# already catches missing options (T-065); Firestore ping fail-open (T-091).
# JWT / SQL ledger unchanged.

list(APPEND FLUTTER_PLUGIN_LIST
  flutter_secure_storage_windows
)

list(APPEND FLUTTER_FFI_PLUGIN_LIST
  jni
)

set(PLUGIN_BUNDLED_LIBRARIES)

foreach(plugin ${FLUTTER_PLUGIN_LIST})
  add_subdirectory(flutter/ephemeral/.plugin_symlinks/${plugin}/windows plugins/${plugin})
  target_link_libraries(${BINARY_NAME} PRIVATE ${plugin}_plugin)
  list(APPEND PLUGIN_BUNDLED_LIBRARIES $<TARGET_FILE:${plugin}_plugin>)
  list(APPEND PLUGIN_BUNDLED_LIBRARIES ${${plugin}_bundled_libraries})
endforeach(plugin)

foreach(ffi_plugin ${FLUTTER_FFI_PLUGIN_LIST})
  add_subdirectory(flutter/ephemeral/.plugin_symlinks/${ffi_plugin}/windows plugins/${ffi_plugin})
  list(APPEND PLUGIN_BUNDLED_LIBRARIES ${${ffi_plugin}_bundled_libraries})
endforeach(ffi_plugin)
