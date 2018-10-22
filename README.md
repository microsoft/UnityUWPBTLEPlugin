# A UWP based BTLE plugin which can be used in UWP application or within Unity to talk to BTLE devices in windows.



## Requirements
Minimum MSVS 2017

The UWP MSVS solution "UnityUWPBTLEPlugin.sln" can be found in the root of the enlistment.  You open that file from
explorer or open MSVS and open the file.

## UnityUWPBTLEPlugin

The plug in component project directory

## UnityUWPBTLEPlugin\BluethoothLEHelper

Location of the source code for the plug in

## TestApp

A UWP based test application used to ensure core functionality of the plug in.

## UnityUWPBTLEPlugin\Bin

Once the project is built this is where the UnityUWPBTLEPlugin.winmd file will be locate that you will need to move into the unity project to use it.

example \UnityUWPPlugin\UnityUWPBTLEPlugin\bin\x64\Release\UnityUWPBTLEPlugin.winmd


# Build Instructions
Open the solution in MSVS
Select Release | Debug and x86 | x64 for your depending on your target.

Once the build is finished you can test it using the TestApp or you can use the UnityUWPBTLEPlugin.winmd in another project.

