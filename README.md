# ShoutboxSPA

This is a DNN 8 module using the SPA pattern. Written using AngularJS this module creates a shoutbox/chat style module. 

Features include
- Profanity Check on all post
- Spam control on the new posts, replies and voting
- Support for anonymous posting
- Uses the Quicksetting DNN pattern
- Support for DNN profile images or gravatar images
- Supports exporting/importing
- Supports searchable data

# Getting The Module
The releases page contains the latest package which you can just install in your DNN 8+ portal.

#Compiling The Code
The code is written in C# using Visual Studio 2013, you can download the source from here. The source code should be placed in the /DesktopModules folder of 
your DNN8 install. From there you can open the project in VS and compile.

Additionally you can also create a package directly using the NANT commandline build file. You need to install NANT and NANT Contrib to get this
to work. 
In the /build folder double click nant64.bat then type `nant package` to create the zip package in the folder /package

You can also just run `nant build` to have the dll compiled.
