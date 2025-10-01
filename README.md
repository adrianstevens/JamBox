![JamBox GitHub Banner](/Assets/Images/jambox-banner.jpg)

# JamBox

JamBox is a modern music media player client designed to connect seamlessly with your JellyFin server. It allows you to browse, stream, and play your music library with an intuitive interface, providing a smooth listening experience across platforms.

![JamBox GitHub Banner](/Assets/Images/jambox-screens.png)

## Software Stack

<div align="center">
  <a href="https://dotnet.microsoft.com/en-us/"><img width="10%" src="/assets/images/tile-net.png" alt=".NET"></a>
  <a href="https://avaloniaui.net/"><img width="10%" src="/assets/Images/tile-avalonia.png.png" alt="Avalonia"/></a>
  <a href="https://www.videolan.org/vlc/"><img width="10%" src="/assets/Images/tile-vlc.png" alt="VLC"/></a>
  <a href="https://velopack.io/"><img width="10%" src="/assets/Images/tile-velopack.png" alt="Velopack"/></a>
  <a href="https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-9.0/"><img width="10%" src="/assets/Images/tile-json.png" alt="Json"></a>
</div>

## Linux (tested with Debian)

### Installing VLC

#### Update the Package Index

Open your terminal and run:

```
sudo apt update
```

If you only need the VLC player libraries (runtime only):

```
sudo apt install libvlc5 libvlccore9
```

If you also need development headers for building applications with VLC:

```
sudo apt install libvlc-dev
```


## Support

Finding bugs or weird behaviors? File an [issue](https://github.com/adrianstevens/JamBox/issues) with repro steps.