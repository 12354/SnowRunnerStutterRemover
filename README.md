## TL;DR:

I fixed the stuttering issue where your throttle cuts off:

[Download SnowRunnerStutterRemover.zip](https://github.com/12354/SnowRunnerStutterRemover/releases/latest)

Start SnowRunnerStutterRemover.exe.

It will ask you for the location of your SnowRunner.exe, start the game for you and fix the issue. You need to start SnowRunner.exe using this fix every time you start the game.

To run this, .NET Framework 4.7.2 is needed.

## Whats the issue:

After playing this game for a while I encountered the issue where your game stutters for a second and you lose your throttle([example post](https://old.reddit.com/r/snowrunner/comments/gk38ez/stutter_and_throttle_cut_off/)).

Other types of stuttering will not be fixed by this, but this one was the most annoying and long stutter I had in this game.

## What causes it?

Windows send a notification to the game that some device has changed (maybe new device plugged in, maybe some device has issues where it reconnects every minute). When SnowRunner receives this notification it enumerates all input devices(takes a long time => stutter) and recreates it's direct input device(this loses keypresses => cut off throttle). You can try this by plugging in a usb device.

## How did you fix it?

By preventing the game from receiving the "device changed notification", we can prevent this issue altogether. This is done by hooking into game internals and preventing the notification from reaching the game loop. This means you will probably not be able to plug in a new usb controller while the game is running, and you need to restart the game.
