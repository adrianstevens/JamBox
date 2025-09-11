#!/bin/bash
# install-icons.sh - Installs JamBox icons and .desktop launcher for Linux

APP_NAME="JamBox"
ICON_BASENAME="icon"
ICON_DIR="$(dirname "$0")/Assets/icons"
USER_ICON_DIR="$HOME/.local/share/icons/hicolor"
USER_APP_DIR="$HOME/.local/share/applications"
DESKTOP_FILE="$USER_APP_DIR/jambox.desktop"

# List of icon sizes to install
SIZES=(16 32 48 64 128 256 512)

# Install icons
for SIZE in "${SIZES[@]}"; do
    DEST_DIR="$USER_ICON_DIR/${SIZE}x${SIZE}/apps"
    mkdir -p "$DEST_DIR"
    ICON_SRC="$ICON_DIR/${ICON_BASENAME}-${SIZE}.png"
    if [ -f "$ICON_SRC" ]; then
        cp "$ICON_SRC" "$DEST_DIR/${APP_NAME}.png"
    fi
done

# Create .desktop file
mkdir -p "$USER_APP_DIR"
cat > "$DESKTOP_FILE" <<EOL
[Desktop Entry]
Version=1.0
Type=Application
Name=JamBox
Comment=Development version of JamBox
Exec=dotnet /home/jorgedevs/Projects/JamBox/Source/JamBox.Linux/bin/Debug/net9.0/JamBox.Linux.dll
Icon=JamBox
Terminal=false
Categories=Development;
StartupWMClass=JamBox.Linux
EOL

update-desktop-database "$USER_APP_DIR" 2>/dev/null || true

echo "Icons and launcher installed!"
