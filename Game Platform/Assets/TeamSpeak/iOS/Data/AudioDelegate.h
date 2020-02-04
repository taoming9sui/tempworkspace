/*
 * TeamSpeak 3 client sample
 *
 * Copyright (c) 2007-2013 TeamSpeak Systems GmbH
 */


#import <UIKit/UIKit.h>

#import "AudioIO.h"

@interface AudioDelegate : NSObject <AudioIODelegate>

@property (nonatomic, retain) AudioIO *remoteIO;
@property (nonatomic) BOOL devicesOpen;

@end
