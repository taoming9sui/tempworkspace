/*
 * TeamSpeak 3 client sample
 *
 * Copyright (c) 2007-2013 TeamSpeak Systems GmbH
 */

#import "AudioIO.h"
#import "CAAudioBufferList.h"

@interface AudioIO ()

- (void) enableRecording;
- (void) enablePlayback;
- (void) initAudioFormat;
- (void) initCallbacks;
- (void) allocateInputBuffers:(UInt32)inNumberFrames;

- (void) setupRemoteIO;

@end

static OSStatus	AudioInCallback(void *inRefCon,
                                AudioUnitRenderActionFlags *ioActionFlags,
                                const AudioTimeStamp *inTimeStamp,
                                UInt32 inBusNumber,
                                UInt32 inNumberFrames,
                                AudioBufferList *ioData)
{
	AudioIO *audioIO = (AudioIO *)inRefCon;
    
    if (audioIO.inputBufferList == NULL)
    {
        [audioIO allocateInputBuffers:inNumberFrames];
    }
    
    // fill buffer list with recorded samples
    OSStatus status = AudioUnitRender(audioIO.audioUnit, 
                                      ioActionFlags, 
                                      inTimeStamp, 
                                      inBusNumber, 
                                      inNumberFrames, 
                                      audioIO.inputBufferList);
    if (status != noErr)
    {
        // -- error codes --
        // paramErr = -50,
        //NSLog(@"AudioInCallback could not render audio unit: status = %lu\n", status);
        return status;
    }
    
    // Inform our delegate we got new audio
    [audioIO.delegate processAudioFromMicrophone:audioIO.inputBufferList];
			
	return noErr;
}

static OSStatus	AudioOutCallback(void *inRefCon,
                                 AudioUnitRenderActionFlags *ioActionFlags,
                                 const AudioTimeStamp *inTimeStamp,
                                 UInt32 inBusNumber,
                                 UInt32 inNumberFrames,
                                 AudioBufferList *ioData)
{
	AudioIO *audioIO = (AudioIO *)inRefCon;
	
	[audioIO.delegate processAudioToSpeaker:ioData];
			
	return 0;
}

#pragma mark -
#pragma mark AudioIO

@implementation AudioIO

@synthesize audioUnit;
@synthesize delegate;
@synthesize started;
@synthesize inputBufferList;

- (id)init 
{
	if ((self = [super init]))
    {
        started = NO;
        
		AVAudioSession *session = [AVAudioSession sharedInstance];
		NSError *errRet = nil;
		
		AVAudioSessionCategoryOptions options = AVAudioSessionCategoryOptionMixWithOthers;//AVAudioSessionCategoryOptionMixWithOthers | AVAudioSessionCategoryOptionAllowBluetooth | AVAudioSessionCategoryOptionDefaultToSpeaker;
		
		[session setCategory:AVAudioSessionCategoryPlayAndRecord withOptions:options error:&errRet];
		if (errRet)
			NSLog(@"Error in AudioIO::init::setCategory %@",[errRet localizedDescription]);

#if USE_IOS_VOICE_PROCESSING
		[session setMode:AVsetPreferredIOBufferDurationAudioSessionModeVoiceChat error:&errRet];
		if (errRet)
			NSLog(@"Error in AudioIO::init::setMode %@",[errRet localizedDescription]);

        [session setActive:YES error:&errRet];
        [session setPreferredIOBufferDuration:0.02 error:nil];
#endif
		
		// Try to set our preferred sample rate or the SDK will resample to 48kHz
		// and it's more efficient to let iOS do it.
		[session setPreferredSampleRate:AUDIO_SAMPLE_RATE error:&errRet];
		if (errRet)
			NSLog(@"Error in AudioIO::init::setPreferredSampleRate %@",[errRet localizedDescription]);
		
				
		[session setActive:YES error:&errRet];
        [session setPreferredIOBufferDuration:0.02 error:nil];
       
		if (errRet)
			NSLog(@"Error in AudioIO::init::setActive %@",[errRet localizedDescription]);
		
        
        
        [self setupRemoteIO];
		
        
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(routeChangeHandler:)
													 name:AVAudioSessionRouteChangeNotification
												   object:[AVAudioSession sharedInstance]];
		
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(interruptionHandler:)
													 name:AVAudioSessionInterruptionNotification
												   object:[AVAudioSession sharedInstance]];
        
	}
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
	
    AudioComponentInstanceDispose(audioUnit);
    
    AudioUnitUninitialize(audioUnit);
    
    // free input buffer list used to retrieve recorded data
    if (inputBufferList)
    {
        for (UInt32 i = 0; i < inputBufferList->mNumberBuffers; i++)
        {
            free (inputBufferList->mBuffers[i].mData);
        }
        CAAudioBufferList::Destroy(inputBufferList);
        inputBufferList = NULL;
    }
    
    [super dealloc];
}

#pragma mark - AVAudioSession Notifications

- (void)routeChangeHandler:(NSNotification *)notification
{
    
    UInt8 reasonValue = [[notification.userInfo valueForKey:AVAudioSessionRouteChangeReasonKey] intValue];
    
    if (AVAudioSessionRouteChangeReasonNewDeviceAvailable == reasonValue)
	{
		NSLog(@"AVAudioSessionRouteChangeNotification: New Device Available");
	}
	else if (AVAudioSessionRouteChangeReasonOldDeviceUnavailable == reasonValue)
	{
		NSLog(@"AVAudioSessionRouteChangeNotification: Old Device Unavailable");
    }
	
	AVAudioSession *session = [AVAudioSession sharedInstance];
	
	NSError *errRet = nil;
	
	AVAudioSessionCategoryOptions options = AVAudioSessionCategoryOptionMixWithOthers;
	
	[session setCategory:AVAudioSessionCategoryPlayAndRecord withOptions:options error:&errRet];
	if (errRet)
		NSLog(@"Error in AudioIO::routeChangeHandler::setCategory %@",[errRet localizedDescription]);
	
    
#if USE_IOS_VOICE_PROCESSING
	[session setMode:AVAudioSessionModeVoiceChat error:&errRet];
	if (errRet)
		NSLog(@"Error in AudioIO::routeChangeHandler::setMode %@",[errRet localizedDescription]);
#endif
	
	[session setActive:YES error:&errRet];
	if (errRet)
		NSLog(@"Error in AudioIO::routeChangeHandler::setActive %@",[errRet localizedDescription]);
	
    [session setPreferredIOBufferDuration.002 error:nil];
    
}

- (void)interruptionHandler:(NSNotification *)notification
{
    
    UInt8 typeValue = [[notification.userInfo valueForKey: AVAudioSessionInterruptionTypeKey] intValue];
	
	// Adding audio session code to handle interruptions ensures that your application’s audio continues behaving gracefully
	// when a phone call arrives or a Clock or Calendar alarm sounds.
	
	// An audio interruption is the deactivation of your application’s audio session—which immediately stops or pauses your audio,
	// depending on which technology you are using. Interruptions happen when a competing audio session from a built-in application
	// activates and that session is not categorized by the system to mix with yours.
	// After your session goes inactive, the system sends a “you were interrupted” message which you can respond to by saving state,
	// updating the user interface, and so on.
	
	// Your application may get shut down following an interruption. This happens when a user decides to accept a phone call.
	// If a user instead elects to ignore a call, or dismisses an alarm, the system issues an interruption-ended message
	// and your application continues running. For your audio to resume, your audio session must be reactivated.
    
	AVAudioSession *session = [AVAudioSession sharedInstance];
	NSError *errRet = nil;
	
	if (AVAudioSessionInterruptionTypeBegan == typeValue)
    {
       	NSLog(@"AVAudioSession interruption began.");
        
		// Your application’s audio session has just been interrupted, such as by a phone call.
		// This might get called when the user unplugs headphones.
		
		if ([[UIApplication sharedApplication] applicationState] == UIApplicationStateBackground)
		{
			// http://lists.apple.com/archives/coreaudio-api/2010/Aug/msg00164.html
			// If you are interrupted while in the background, then you could decide to become "mixable"
			// (as a non-mixable app you cannot re-activate yourself in the background),
			// and just cooperate with the hardware settings as they are
			// (because now an app with a more recent presence than yours is with the user)
			
			// For us the Sample Rate Conversion may kick in since the hardware sample rate may no longer be 48kHz as TeamSpeak likes.
			AVAudioSessionCategoryOptions options = AVAudioSessionCategoryOptionMixWithOthers;// | AVAudioSessionCategoryOptionDefaultToSpeaker;
			
			[session setCategory:AVAudioSessionCategoryPlayAndRecord withOptions:options error:&errRet];
			if (errRet)
				NSLog(@"Error in AudioIO::interruptionHandler::setCategory %@",[errRet localizedDescription]);
			
#if USE_IOS_VOICE_PROCESSING
			[session setMode:AVAudioSessionModeVoiceChat error:&errRet];
			if (errRet)
				NSLog(@"Error in AudioIO::interruptionHandler::setMode %@",[errRet localizedDescription]);
#endif
			
			// Try to set our preferred sample rate or the SDK will resample to 48kHz
			// and it's more efficient to let iOS do it.
			[session setPreferredSampleRate:AUDIO_SAMPLE_RATE error:&errRet];
			if (errRet)
				NSLog(@"Error in AudioIO::interruptionHandler::setPreferredSampleRate %@",[errRet localizedDescription]);
			
			// make sure we are again the active session
			[session setActive:YES error:&errRet];
			if (errRet)
				NSLog(@"Error in AudioIO::interruptionHandler::setActive %@",[errRet localizedDescription]);
		}
    }
	else if (AVAudioSessionInterruptionTypeEnded == typeValue) // For a phone call, may not get this.
	{
       	NSLog(@"AVAudioSession interruption ended.");
        
		// make sure we are again the active session
		[session setActive:YES error:&errRet];
		if (errRet)
			NSLog(@"Error in AudioIO::interruptionHandler::setActive %@",[errRet localizedDescription]);
	}
        [session setPreferredIOBufferDuration:0.02 error:nil];
}

#pragma mark -

- (void)setupRemoteIO
{
    // Describe audio component
    AudioComponentDescription desc;
    desc.componentType = kAudioUnitType_Output;
    desc.componentSubType = kAudioUnitSubType_RemoteIO;
    desc.componentFlags = 0;
    desc.componentFlagsMask = 0;
    desc.componentManufacturer = kAudioUnitManufacturer_Apple;
    
    // Get component
    AudioComponent inputComponent = AudioComponentFindNext(NULL, &desc);
    
    // Get audio unit
    OSStatus status = AudioComponentInstanceNew(inputComponent, &audioUnit);
    if (status != noErr)
    {
        printf("AudioIO could not create new audio component: status = %lu\n", status);
    }
    
    [self enableRecording];
    [self enablePlayback];
    [self initAudioFormat];
    [self initCallbacks];
    
    // initialize
    status = AudioUnitInitialize(audioUnit);
    if (status != noErr)
    {
        printf("AudioIO could not initialize audio unit: status = %lu\n", status);
    }    
}

- (void)enableRecording
{
    // Enable IO for recording
    UInt32 flag = 1;
    OSStatus status = AudioUnitSetProperty(audioUnit, 
                                           kAudioOutputUnitProperty_EnableIO, 
                                           kAudioUnitScope_Input, 
                                           AUDIO_INPUT_BUS,
                                           &flag, 
                                           sizeof(flag));
    if (status != noErr)
    {
        printf("AudioIO enable_recording failed: status = %lu\n", status);
    }
}

- (void)enablePlayback
{
    // enable IO for playback
    UInt32 flag = 1;
    OSStatus status = AudioUnitSetProperty(audioUnit, 
                                           kAudioOutputUnitProperty_EnableIO, 
                                           kAudioUnitScope_Output, 
                                           AUDIO_OUTPUT_BUS,
                                           &flag, 
                                           sizeof(flag));
    if (status != noErr)
    {
        printf("AudioIO enable_playback failed: status = %lu\n", status);
    }
}

- (void)initAudioFormat
{
    // describe format
    FillOutASBDForLPCM(audioFormat, AUDIO_SAMPLE_RATE, AUDIO_NUM_CHANNELS, AUDIO_BIT_DEPTH, AUDIO_BIT_DEPTH, false, false, AUDIO_FORMAT_IS_NONINTERLEAVED);
    
    // Apply output format
    OSStatus status = AudioUnitSetProperty(audioUnit, 
                                           kAudioUnitProperty_StreamFormat, 
                                           kAudioUnitScope_Output, 
                                           AUDIO_INPUT_BUS, 
                                           &audioFormat, 
                                           sizeof(audioFormat));
    if (status != noErr)
    {
        printf("AudioIO init_audio_format could not set output format: status = %lu\n", status);
    }
    
    // Apply input format
    status = AudioUnitSetProperty(audioUnit, 
                                  kAudioUnitProperty_StreamFormat, 
                                  kAudioUnitScope_Input, 
                                  AUDIO_OUTPUT_BUS, 
                                  &audioFormat, 
                                  sizeof(audioFormat));
    if (status != noErr)
    {
        printf("AudioIO init_audio_format could not set input format: status = %lu\n", status);
    }
}

- (void)initCallbacks
{
    // Set input callback
    AURenderCallbackStruct callbackStruct;
    
    callbackStruct.inputProc = AudioInCallback;
    callbackStruct.inputProcRefCon = self;
    
    OSStatus status = AudioUnitSetProperty(audioUnit, 
                                           kAudioOutputUnitProperty_SetInputCallback, 
                                           kAudioUnitScope_Global, 
                                           AUDIO_INPUT_BUS, 
                                           &callbackStruct, 
                                           sizeof(callbackStruct));
    if (status != noErr)
    {
        printf("AudioIO init_callbacks could not set input callback: status = %lu\n", status);
    }
    
    // Set output callback
    callbackStruct.inputProc = AudioOutCallback;
    callbackStruct.inputProcRefCon = self;
    
    status = AudioUnitSetProperty(audioUnit, 
                                  kAudioUnitProperty_SetRenderCallback, 
                                  kAudioUnitScope_Global, 
                                  AUDIO_OUTPUT_BUS,
                                  &callbackStruct, 
                                  sizeof(callbackStruct));
    if (status != noErr)
    {
        printf("AudioIO init_callbacks could not set output callback: status = %lu\n", status);
    }
}

- (void) allocateInputBuffers:(UInt32)inNumberFrames
{
    printf("AudioIO allocate_input_buffers: inNumberFrames = %lu\n", inNumberFrames);
    
    UInt32 bufferSizeInBytes = inNumberFrames * (AUDIO_FORMAT_IS_NONINTERLEAVED ? AUDIO_BIT_DEPTH_IN_BYTES :  (AUDIO_BIT_DEPTH_IN_BYTES * AUDIO_NUM_CHANNELS));
    
    // allocate buffer list
    inputBufferList = CAAudioBufferList::Create(inNumberFrames);
    
    inputBufferList->mNumberBuffers = AUDIO_FORMAT_IS_NONINTERLEAVED ? AUDIO_NUM_CHANNELS : 1;
    
    for (UInt32 i = 0; i < inputBufferList->mNumberBuffers; i++)
    {
        printf("AudioIO allocate_input_buffers: index = %lu, bufferSizeInBytes = %lu\n", i, bufferSizeInBytes);
        inputBufferList->mBuffers[i].mNumberChannels = AUDIO_FORMAT_IS_NONINTERLEAVED ? 1 : AUDIO_NUM_CHANNELS;
        inputBufferList->mBuffers[i].mDataByteSize = bufferSizeInBytes;
        inputBufferList->mBuffers[i].mData = malloc(bufferSizeInBytes);
    }
}

#pragma mark -

- (void)start 
{
    OSStatus status = AudioOutputUnitStart(audioUnit);
    if (status != noErr)
    {
        NSLog(@"AudioIO start: An error occured, status = %lu", status);
    }
    else
    {
        started = YES;
    }
}

- (void)stop 
{
    OSStatus status = AudioOutputUnitStop(audioUnit);
    
    if (status != noErr)
    {
        NSLog(@"AudioIO stop: An error occured, status = %lu", status);
    }
    else
    {
        // Clear the input buffer list
        if (inputBufferList)
        {
            for (UInt32 i = 0; i < inputBufferList->mNumberBuffers; i++)
            {
                free (inputBufferList->mBuffers[i].mData);
            }
            CAAudioBufferList::Destroy(inputBufferList);
            inputBufferList = NULL;
        }
        started = NO;
    }
}

@end
