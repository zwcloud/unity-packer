# UnityPacker

UnityPacker is a collection of a library and small command line tools that can create, unpack and inspect `UnityPackage` files without a Unity installation. It is great for automated builds of Unity tools.

*Tested on Unity 2018.3 and 2019.1.0f2*

> This repository was forked from [FatihBAKIR/UnityPacker](https://github.com/FatihBAKIR/UnityPacker) and refactored to fix path issues, usage and compatibility.

## How to create .unitypackage

    ./UnityPack mode pack folder "" package "Samples" root "Assets/Plugins/Product/Samples" ignore "(Plugins\\.*|unitypackage|exe|exe|cmd)$"
    
This example will produce a `Samples.unitypackage` from the contents current directory recursively in the current directory.

> It can omit files or directories from regex more using the `ignore` option.

> Meta files are kept or generated if missing.

## How to extract .unitypackage

    .//UnityPacker mode unpack package "Samples.unitypackage" destination "."

This example will unpack the `Samples.unitypackage` to the working directory, with proper directory structure.
