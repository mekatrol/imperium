#!/bin/bash

# Start the .NET app in background
dotnet /app/publish/Imperium.Server.dll &

# Start the SSH daemon
/usr/sbin/sshd -D
