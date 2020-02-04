#!/usr/bin/python

import sys
import shutil
import os

from mod_pbxproj import XcodeProject

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('Start of python script\n')
f.close()

XcodePath = sys.argv[3]
XcodeFrameworksPath = XcodePath + '/System/Library/Frameworks/'

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('Framework path:\n')
f.write(XcodeFrameworksPath)
f.write('\n')
f.close()
	
			
projectPath = sys.argv[1]

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('project path:\n')
f.write(projectPath)
f.write('\n')
f.close()

project = XcodeProject.Load(projectPath + '/Unity-iPhone.xcodeproj/project.pbxproj')

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('Project loaded:\n')
f.close()

shutil.rmtree(projectPath + '/Classes/TeamSpeak', True);
shutil.copytree(sys.argv[2] + '/Data', projectPath + '/Classes/TeamSpeak',ignore = shutil.ignore_patterns('*.meta', '*.zip','*.a'));
	
teamSpeakGroup = project.get_or_create_group('TeamSpeak')
currentGroup = teamSpeakGroup
for root, dirs, files in os.walk(projectPath + '/Classes/TeamSpeak'):
	baseName = os.path.basename(root)
	if baseName != 'TeamSpeak':
		currentGroup = project.get_or_create_group(baseName, parent=teamSpeakGroup)
	for f in files:
		if not f.endswith('.a'):
			results = project.add_file_if_doesnt_exist(os.path.join(root,f), parent=currentGroup)
			for result in results:
				if result.get('isa') == 'PBXBuildFile':
					result.add_compiler_flag('-fno-objc-arc')

project.add_header_search_paths('"$(SRCROOT)/Classes/TeamSpeak"', recursive=False);

#shutil.copyfile(sys.argv[2] + '/Data/libts3client_ios_sdk_device.a' , projectPath + '/libts3client_ios_sdk_device.a');

# shutil.copyfile(sys.argv[2] + '/Data/libts3support_device.a' , projectPath + '/libts3support_device.a');

if sys.argv[4] == '1':
	shutil.rmtree(projectPath + '/boost.framework', True);
	unzipCommand = 'unzip ' + sys.argv[2] + '/Data/boost.framework.zip -d ' + projectPath;
	os.system(unzipCommand);
	project.add_file_if_doesnt_exist(projectPath + '/boost.framework', tree='SDKROOT')
	project.add_framework_search_paths(projectPath, False)
	f = open('TeamSpeakBuildLogFile.txt','a')
	f.write('Added boost framework:\n')
	f.close()
		

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('copied libts3client_ios_sdk_device.a \n')
f.close()

#project.add_file_if_doesnt_exist(projectPath + '/libts3client_ios_sdk_device.a', tree="SDKROOT");
# project.add_file_if_doesnt_exist(projectPath + '/libts3support_device.a', tree="SDKROOT");

f = open('TeamSpeakBuildLogFile.txt','a')
f.write('added ' + sys.argv[3] + ':\n')
f.close()

project.remove_other_ldflags("-all_load");
f = open('TeamSpeakBuildLogFile.txt','a')
f.write('removed ld flag all_load \n')
f.close()

project.add_other_ldflags("-lc++");
f = open('TeamSpeakBuildLogFile.txt','a')
f.write('added ld flag lc++\n')
f.close()


project.save()
f = open('TeamSpeakBuildLogFile.txt','a')
f.write('Saved project:\n')
f.close()
