/*
 * TeamSpeak 3 client sample
 *
 * Copyright (c) 2007-2013 TeamSpeak Systems GmbH
 */


#import "AudioDelegate.h"

#import <AVFoundation/AVFoundation.h>

#import "clientlib.h"
#import "public_errors.h"

@implementation AudioDelegate

@synthesize remoteIO;


-(id)init
{
    remoteIO = [[AudioIO alloc] init];
    [remoteIO setDelegate: self];
	
    // If we haven't yet registered our audio device, start the RemoteIO and register
    NSLog(@"*** Starting remoteIO ***");
    
    [remoteIO start];

    
    if ((self = [super init]))
    {
        _devicesOpen = NO;
		
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(routeChangeHandler:)
													 name:AVAudioSessionRouteChangeNotification
												   object:[AVAudioSession sharedInstance]];
		
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(interruptionHandler:)
													 name:AVAudioSessionInterruptionNotification
												   object:[AVAudioSession sharedInstance]];
	}
	_devicesOpen = YES;
    return self;
}

- (void)dealloc
{
    
	[[NSNotificationCenter defaultCenter] removeObserver:self
                                                    name:AVAudioSessionRouteChangeNotification
												  object:[AVAudioSession sharedInstance]];
    
	[[NSNotificationCenter defaultCenter] removeObserver:self
                                                    name:AVAudioSessionInterruptionNotification
												  object:[AVAudioSession sharedInstance]];
}

#pragma mark - Remote IO Audio Callbacks

- (void)processAudioToSpeaker:(AudioBufferList *)ioData
{
	if (_devicesOpen)
	{
		int numSamples = ioData->mBuffers[0].mDataByteSize / (AUDIO_BIT_DEPTH_IN_BYTES * AUDIO_NUM_CHANNELS);
		short *outData = (short*)(ioData->mBuffers[0].mData); // A single buffer contains interleaved data for AUDIO_NUM_CHANNELS channels
		
		unsigned int error;
        
		/* Get playback data from the client lib */
		if ((error = ts3client_acquireCustomPlaybackData(kWaveDeviceID,
                                                         outData,
                                                         numSamples)) != ERROR_ok)
		{
            for (UInt32 i = 0; i < ioData->mNumberBuffers; i++)
                memset(ioData->mBuffers[i].mData, 0, ioData->mBuffers[i].mDataByteSize);
		}
	}
	else
	{
        for (UInt32 i = 0; i < ioData->mNumberBuffers; i++)
            memset(ioData->mBuffers[i].mData, 0, ioData->mBuffers[i].mDataByteSize);
	}
}

/*
 * Process the audio coming from microphone.
 */

- (void)processAudioFromMicrophone:(AudioBufferList *)ioData
{
    int numSamples = ioData->mBuffers[0].mDataByteSize / (AUDIO_BIT_DEPTH_IN_BYTES * AUDIO_NUM_CHANNELS);
    short *inData = (short*)(ioData->mBuffers[0].mData); // A single buffer contains interleaved data for AUDIO_NUM_CHANNELS channels
    
	if (_devicesOpen)
	{
		unsigned int error;
		/* Send capture data to the client lib */
		if ((error = ts3client_processCustomCaptureData(kWaveDeviceID,
														inData,
														numSamples)) != ERROR_ok)
		{
			NSLog(@"Failed to send capture data");
		}
	}
}

//
// When the user plugs in a pair of headphones or otherwise changes the audio configuration
// the RemoteIO will be restarted, so this gives us a chance to close things down
//
- (void)audioWillStart:(AudioIO *)audioIO
{
    NSLog(@"*** audioWillStart ***");
}

- (void)audioWillStop:(AudioIO *)audioIO
{
    NSLog(@"*** audioWillStop ***");
}

#pragma mark - AVAudioSession Notifications

- (void)routeChangeHandler:(NSNotification *)notification
{
    // Nothing to do here in this example, AudioIO handles most of the work.
}

- (void)interruptionHandler:(NSNotification *)notification
{
    // Nothing to do here in this example, AudioIO handles most of the work.
}

@end
