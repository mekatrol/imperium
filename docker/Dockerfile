# Use official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

ENV DEBIAN_FRONTEND=noninteractive

# Expected arguments
ARG SSH_USER_NAME
ARG SSH_USER_PASSWORD
ARG HOSTNAME
ARG TIMEZONE


# Set working directory
WORKDIR /app

# Install git and openssh-server
RUN apt-get update && apt-get install -y \
    sudo \
    syslog-ng \
    nano \
    tzdata \
    git \
    openssh-server

# Create the SSH user and set a password
RUN useradd -m -s /bin/bash $SSH_USER_NAME && echo "$SSH_USER_NAME:$SSH_USER_PASSWORD" | chpasswd

# Add the SSH user to the sudo group
RUN usermod -aG sudo $SSH_USER_NAME

# Create SSH run file directory
RUN mkdir /var/run/sshd 

# Create imperium directory
RUN mkdir /imperium 

# Clone the repo
RUN git clone https://github.com/mekatrol/imperium.git /imperium && \
    rm /imperium/code/Imperium/Imperium.Server/appsettings.Development.json

# Allow password auth
RUN sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/' /etc/ssh/sshd_config && \
    sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config

# Expose SSH and application ports (adjust app port if needed)
EXPOSE 22
EXPOSE 5000

# Build and publish the .NET app
RUN dotnet restore /imperium/code/Imperium/Imperium.Server/Imperium.Server.csproj && \
    dotnet publish /imperium/code/Imperium/Imperium.Server/Imperium.Server.csproj -c Release -o /app/publish    

RUN cat <<EOF > /entry-point.sh
#!/bin/bash

# Start the .NET app in background
dotnet /app/publish/Imperium.Server.dll &

# Start the SSH daemon
/usr/sbin/sshd -D
EOF

# COPY entry-point.sh /entry-point.sh
RUN chmod +x /entry-point.sh

# Start SSH and the .NET app
CMD ["/entry-point.sh"]
