A .NET wrapper for the [Developer Image Library](http://openil.sourceforge.net/) (DevIL). It employs P/Invoke and targets **DevIL 1.7.8**.

This is still a work in progress! When the API is complete I will be releasing the binaries, for now if you're interested feel free to compile the source yourself. The solution is for Visual Studio 2010 (.NET 4.0) and contains x86 and x64 configurations. There are no special build instructions.

DevIL.NET is a sister wrapper to the [Assimp.NET](http://code.google.com/p/assimp-net/) project. Like that project, a more "high level" API is planned (but not yet completed). This means you don't have to fiddle with the P/Invoke layer yourself!

**Update (5/7/2012)**: Most aspects of the high level API are in - namely "image states" that allow you to more intuitively control the various (and often confusing) enable caps/other settings in DevIL, as well as a "ManagedImage" object that should be helpful for reading an image's mipmap/face data.