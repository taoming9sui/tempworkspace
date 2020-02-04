#include "AudioDelegate.h"

#pragma mark - Application lifecycle
//Do this in wrapper
extern "C"{

	AudioDelegate* teamSpeakRemoteIODelegate;
	
	void teamSpeakRemoteIOInit(){
		//Only call this once!
		NSLog(@"*** Creating remoteIO ***");
        
        teamSpeakRemoteIODelegate = [[AudioDelegate alloc] init];
    }

}