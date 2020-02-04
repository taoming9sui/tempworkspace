// Generated by Apple Swift version 3.1 (swiftlang-802.0.53 clang-802.0.42)
#pragma clang diagnostic push

#if defined(__has_include) && __has_include(<swift/objc-prologue.h>)
# include <swift/objc-prologue.h>
#endif

#pragma clang diagnostic ignored "-Wauto-import"
#include <objc/NSObject.h>
#include <stdint.h>
#include <stddef.h>
#include <stdbool.h>

#if !defined(SWIFT_TYPEDEFS)
# define SWIFT_TYPEDEFS 1
# if defined(__has_include) && __has_include(<uchar.h>)
#  include <uchar.h>
# elif !defined(__cplusplus) || __cplusplus < 201103L
typedef uint_least16_t char16_t;
typedef uint_least32_t char32_t;
# endif
typedef float swift_float2  __attribute__((__ext_vector_type__(2)));
typedef float swift_float3  __attribute__((__ext_vector_type__(3)));
typedef float swift_float4  __attribute__((__ext_vector_type__(4)));
typedef double swift_double2  __attribute__((__ext_vector_type__(2)));
typedef double swift_double3  __attribute__((__ext_vector_type__(3)));
typedef double swift_double4  __attribute__((__ext_vector_type__(4)));
typedef int swift_int2  __attribute__((__ext_vector_type__(2)));
typedef int swift_int3  __attribute__((__ext_vector_type__(3)));
typedef int swift_int4  __attribute__((__ext_vector_type__(4)));
typedef unsigned int swift_uint2  __attribute__((__ext_vector_type__(2)));
typedef unsigned int swift_uint3  __attribute__((__ext_vector_type__(3)));
typedef unsigned int swift_uint4  __attribute__((__ext_vector_type__(4)));
#endif

#if !defined(SWIFT_PASTE)
# define SWIFT_PASTE_HELPER(x, y) x##y
# define SWIFT_PASTE(x, y) SWIFT_PASTE_HELPER(x, y)
#endif
#if !defined(SWIFT_METATYPE)
# define SWIFT_METATYPE(X) Class
#endif
#if !defined(SWIFT_CLASS_PROPERTY)
# if __has_feature(objc_class_property)
#  define SWIFT_CLASS_PROPERTY(...) __VA_ARGS__
# else
#  define SWIFT_CLASS_PROPERTY(...)
# endif
#endif

#if defined(__has_attribute) && __has_attribute(objc_runtime_name)
# define SWIFT_RUNTIME_NAME(X) __attribute__((objc_runtime_name(X)))
#else
# define SWIFT_RUNTIME_NAME(X)
#endif
#if defined(__has_attribute) && __has_attribute(swift_name)
# define SWIFT_COMPILE_NAME(X) __attribute__((swift_name(X)))
#else
# define SWIFT_COMPILE_NAME(X)
#endif
#if defined(__has_attribute) && __has_attribute(objc_method_family)
# define SWIFT_METHOD_FAMILY(X) __attribute__((objc_method_family(X)))
#else
# define SWIFT_METHOD_FAMILY(X)
#endif
#if defined(__has_attribute) && __has_attribute(noescape)
# define SWIFT_NOESCAPE __attribute__((noescape))
#else
# define SWIFT_NOESCAPE
#endif
#if defined(__has_attribute) && __has_attribute(warn_unused_result)
# define SWIFT_WARN_UNUSED_RESULT __attribute__((warn_unused_result))
#else
# define SWIFT_WARN_UNUSED_RESULT
#endif
#if !defined(SWIFT_CLASS_EXTRA)
# define SWIFT_CLASS_EXTRA
#endif
#if !defined(SWIFT_PROTOCOL_EXTRA)
# define SWIFT_PROTOCOL_EXTRA
#endif
#if !defined(SWIFT_ENUM_EXTRA)
# define SWIFT_ENUM_EXTRA
#endif
#if !defined(SWIFT_CLASS)
# if defined(__has_attribute) && __has_attribute(objc_subclassing_restricted)
#  define SWIFT_CLASS(SWIFT_NAME) SWIFT_RUNTIME_NAME(SWIFT_NAME) __attribute__((objc_subclassing_restricted)) SWIFT_CLASS_EXTRA
#  define SWIFT_CLASS_NAMED(SWIFT_NAME) __attribute__((objc_subclassing_restricted)) SWIFT_COMPILE_NAME(SWIFT_NAME) SWIFT_CLASS_EXTRA
# else
#  define SWIFT_CLASS(SWIFT_NAME) SWIFT_RUNTIME_NAME(SWIFT_NAME) SWIFT_CLASS_EXTRA
#  define SWIFT_CLASS_NAMED(SWIFT_NAME) SWIFT_COMPILE_NAME(SWIFT_NAME) SWIFT_CLASS_EXTRA
# endif
#endif

#if !defined(SWIFT_PROTOCOL)
# define SWIFT_PROTOCOL(SWIFT_NAME) SWIFT_RUNTIME_NAME(SWIFT_NAME) SWIFT_PROTOCOL_EXTRA
# define SWIFT_PROTOCOL_NAMED(SWIFT_NAME) SWIFT_COMPILE_NAME(SWIFT_NAME) SWIFT_PROTOCOL_EXTRA
#endif

#if !defined(SWIFT_EXTENSION)
# define SWIFT_EXTENSION(M) SWIFT_PASTE(M##_Swift_, __LINE__)
#endif

#if !defined(OBJC_DESIGNATED_INITIALIZER)
# if defined(__has_attribute) && __has_attribute(objc_designated_initializer)
#  define OBJC_DESIGNATED_INITIALIZER __attribute__((objc_designated_initializer))
# else
#  define OBJC_DESIGNATED_INITIALIZER
# endif
#endif
#if !defined(SWIFT_ENUM)
# define SWIFT_ENUM(_type, _name) enum _name : _type _name; enum SWIFT_ENUM_EXTRA _name : _type
# if defined(__has_feature) && __has_feature(generalized_swift_name)
#  define SWIFT_ENUM_NAMED(_type, _name, SWIFT_NAME) enum _name : _type _name SWIFT_COMPILE_NAME(SWIFT_NAME); enum SWIFT_COMPILE_NAME(SWIFT_NAME) SWIFT_ENUM_EXTRA _name : _type
# else
#  define SWIFT_ENUM_NAMED(_type, _name, SWIFT_NAME) SWIFT_ENUM(_type, _name)
# endif
#endif
#if !defined(SWIFT_UNAVAILABLE)
# define SWIFT_UNAVAILABLE __attribute__((unavailable))
#endif
#if !defined(SWIFT_UNAVAILABLE_MSG)
# define SWIFT_UNAVAILABLE_MSG(msg) __attribute__((unavailable(msg)))
#endif
#if !defined(SWIFT_AVAILABILITY)
# define SWIFT_AVAILABILITY(plat, ...) __attribute__((availability(plat, __VA_ARGS__)))
#endif
#if !defined(SWIFT_DEPRECATED)
# define SWIFT_DEPRECATED __attribute__((deprecated))
#endif
#if !defined(SWIFT_DEPRECATED_MSG)
# define SWIFT_DEPRECATED_MSG(...) __attribute__((deprecated(__VA_ARGS__)))
#endif
#if defined(__has_feature) && __has_feature(modules)
@import ObjectiveC;
#endif

#import "/Volumes/developer/jenkins/workspace/SDK/SDK/l/macbuild/s/deps/teamspeak_client_lib/src/sound/backends/coreaudio_ios/coreaudio-Bridging-Header.h"

#pragma clang diagnostic ignored "-Wproperty-attribute-mismatch"
#pragma clang diagnostic ignored "-Wduplicate-method-arg"

SWIFT_CLASS("_TtC25coreaudio_ts_soundbackend17Audio_Device_Info")
@interface Audio_Device_Info : NSObject
@property (nonatomic, copy) NSString * _Nonnull deviceID;
@property (nonatomic, copy) NSString * _Nonnull deviceName;
- (nonnull instancetype)init OBJC_DESIGNATED_INITIALIZER;
@end


SWIFT_CLASS("_TtC25coreaudio_ts_soundbackend8Audio_IO")
@interface Audio_IO : NSObject
@property (nonatomic) void (* _Null_unspecified captureCallback)(void * _Nullable, int8_t const * _Nullable, struct ts3soundbackend_captureParams const * _Nullable, struct ts3soundbackend_captureParams const * _Nullable);
@property (nonatomic) int32_t (* _Null_unspecified renderCallback)(void * _Nullable, int8_t const * _Nullable, struct ts3soundbackend_playbackParams const * _Nullable);
@property (nonatomic) void (* _Null_unspecified notifyCallback)(void * _Nullable, int32_t, int8_t const * _Nullable, int32_t);
@property (nonatomic) void (* _Null_unspecified messageCallback)(void * _Nullable, int8_t const * _Nullable, int8_t const * _Nullable, int32_t);
@property (nonatomic) void * _Null_unspecified callbackParam;
@property (nonatomic, copy) NSArray<Audio_Device_Info *> * _Nonnull audioDevices;
@property (nonatomic, strong) Audio_Device_Info * _Nullable defaultDevice;
@property (nonatomic) BOOL lock;
+ (BOOL)isSupported SWIFT_WARN_UNUSED_RESULT;
- (int32_t)openDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId channels:(int * _Nonnull)channels bitsPerSample:(int * _Nonnull)bitsPerSample samplesPerSecond:(int * _Nonnull)samplesPerSecond channelMask:(unsigned int * _Nonnull)channelMask SWIFT_WARN_UNUSED_RESULT;
- (int32_t)closeDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (int32_t)startDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (int32_t)stopDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (nonnull instancetype)initWithCallbacks:(struct ts3soundbackend_globalCallbacks const * _Nonnull)callbacks OBJC_DESIGNATED_INITIALIZER;
- (nonnull instancetype)init SWIFT_UNAVAILABLE;
@end

@class NSNotification;

SWIFT_CLASS("_TtC25coreaudio_ts_soundbackend11Audio_IO_v3")
@interface Audio_IO_v3 : Audio_IO
@property (nonatomic, readonly) BOOL canCapture;
@property (nonatomic, readonly) BOOL canRender;
@property (nonatomic) BOOL captureActive;
@property (nonatomic) BOOL renderActive;
+ (BOOL)isSupported SWIFT_WARN_UNUSED_RESULT;
- (int32_t)openDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId channels:(int * _Nonnull)channels bitsPerSample:(int * _Nonnull)bitsPerSample samplesPerSecond:(int * _Nonnull)samplesPerSecond channelMask:(unsigned int * _Nonnull)channelMask SWIFT_WARN_UNUSED_RESULT;
- (int32_t)closeDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (int32_t)startDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (int32_t)stopDeviceWithDeviceType:(int8_t)deviceType deviceId:(char const * _Nonnull)deviceId contextId:(unsigned int)contextId SWIFT_WARN_UNUSED_RESULT;
- (void)onInterruptNotificationWithNotification:(NSNotification * _Nonnull)notification;
- (void)onRouteChangedNotificationWithNotification:(NSNotification * _Nonnull)notification;
- (void)onMediaServicesWereResetWithNotification:(NSNotification * _Nonnull)notification;
- (nonnull instancetype)initWithCallbacks:(struct ts3soundbackend_globalCallbacks const * _Nonnull)callbacks OBJC_DESIGNATED_INITIALIZER;
@end

#pragma clang diagnostic pop
