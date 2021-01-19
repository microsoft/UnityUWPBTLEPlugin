# Note: This repo is now archived. It is still available READ ONLY for forking or historical interest.

# UNITYUWPPlugin
A UWP based BTLE plugin which can be used in UWP application or within Unity to talk to BTLE devices in windows.

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

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

