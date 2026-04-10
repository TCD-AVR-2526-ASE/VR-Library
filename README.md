# VR-Library

## Running it in Unity
The easiest way to use the project is just to open it in Unity and hit Play.

If you do that on a normal setup, it will run in desktop mode automatically. If you have a VR headset connected and ready, the project should switch over to VR mode on its own.

So for most testing, you do not need to build anything first.

## Desktop Build
If you want to make a desktop build instead of running it in the editor:

1. Open the project in Unity.
2. Go to `File > Build Profiles` or `File > Build Settings` depending on your Unity version.
3. Select your desktop target platform:
   - `Windows`
   - `Mac`
   - `Linux`
4. Make sure the correct scene(s) are included in the build.
5. Click `Build`.
6. Choose an output folder.
7. Once Unity finishes, open the generated executable and run it normally.

## VR Build
If you want to run the project on a VR headset:

1. Open the project in Unity.
2. Go to `File > Build Profiles` or `File > Build Settings`.
3. Select `Android` as the target platform.
4. Click `Switch Platform` if Unity has not already switched to Android.
5. Make sure the correct scene(s) are included.
6. Build the project as an `.apk`.
7. After the `.apk` is created, upload it to your headset using something like `SideQuest` or adjacent.
8. Install it on the headset and launch it from there.

## Quick Summary
- Press Play in Unity for the fastest way to test.
- Desktop mode works out of the box in-editor.
- VR mode should activate automatically if your headset is connected.
- For a standalone VR version, build an Android APK and sideload it to the headset.
