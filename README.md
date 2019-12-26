# UnityPacker

UnityPacker is a collection of a library and small command line tools that can create, unpack and inspect `UnityPackage` files without a Unity installation. It is great for automated builds of Unity tools.    

![Screenshot](https://raw.githubusercontent.com/ogxd/unity-packer/master/Demo/screenshot.gif)

*Tested on Unity 2018.3 and 2019.1.0f2*

> This repository was initially forked from [FatihBAKIR/UnityPacker](https://github.com/FatihBAKIR/UnityPacker) and then heavily refactored.

## How to create .unitypackage

    ./UnityPacker mode=pack folder="." package="Samples" root="Assets/Plugins/Product/Samples" ignore="(Plugins\\.*|unitypackage|exe|exe|cmd)$"
    
This example will produce a `Samples.unitypackage` from the contents current directory recursively in the current directory.

> It can omit files or directories from regex more using the `ignore` option.

> Meta files are kept or generated if missing.

## How to extract .unitypackage

    ./UnityPacker mode=unpack package="Samples.unitypackage" destination="."

This example will unpack the `Samples.unitypackage` to the working directory, with proper directory structure.
