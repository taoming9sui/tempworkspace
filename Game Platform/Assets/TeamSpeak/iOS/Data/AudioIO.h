/*
 * TeamSpeak 3 client sample
 *
 * Copyright (c) 2007-2013 TeamSpeak Systems GmbH
 */

//
// Note: This file is excluded from ARC by setting the -fno-objc-arc compiler flag
// for this file in the build settings.
//

#import <UIKit/UIKit.h>
#import <AVFoundation/AVFoundation.h>

static const Float64 AUDIO_SAMPLE_RATE              = 48000.f;
static const int     AUDIO_NUM_CHANNELS             = 2;
static const int     AUDIO_BIT_DEPTH_IN_BYTES       = 2;
static const int     AUDIO_BIT_DEPTH                = 16; // 8 * AUDIO_BIT_DEPTH_IN_BYTES
static const int     AUDIO_FORMAT_IS_NONINTERLEAVED = FALSE;

#define kWaveDeviceID			"iOS_WaveDeviceId"
#define kWaveDeviceDisplayName	"iOS AudioIO Device"

static const int AUDIO_OUTPUT_BUS = 0;
static const int AUDIO_INPUT_BUS = 1;

@class AudioIO;

@protocol AudioIODelegate

@required

-(void) processAudioToSpeaker:(AudioBufferList *)ioData;
-(void) processAudioFromMicrophone:(AudioBufferList *)ioData;
-(void) audioWillStart:(AudioIO *)audioIO;
-(void) audioWillStop:(AudioIO *)audioIO;

@end

@interface AudioIO : NSObject
{
	
	AudioUnit					audioUnit;	
	AudioStreamBasicDescription	audioFormat;
	id<AudioIODelegate>		    delegate;
    BOOL                        started;
    AudioBufferList             *inputBufferList; // For microphone audio
}

@property AudioUnit									audioUnit;
@property (getter=isStarted) BOOL					started;
@property (nonatomic, retain) id<AudioIODelegate>	delegate;
@property (nonatomic) AudioBufferList				*inputBufferList;

-(void)start;
-(void)stop;

@end

