# Use official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0

ENV DEBIAN_FRONTEND=noninteractive

# Expected arguments
ARG SSH_USER_NAME
ARG SSH_USER_PASSWORD
ARG MQTT_HOST_NAME
ARG MQTT_USER_NAME
ARG MQTT_USER_PASSWORD
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

# Allow password auth
RUN sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/' /etc/ssh/sshd_config && \
    sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config

# Set timezone
RUN  ln -fs /usr/share/zoneinfo/$TIMEZONE /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata


# Expose SSH and application ports (adjust app port if needed)
EXPOSE 22
EXPOSE 5000

RUN cat <<EOF > /entry-point.sh
#!/bin/bash

# Start the SSH daemon
service ssh start

# Start the .NET app in background
cd /app/publish
dotnet ./Imperium.Server.dll &
cd /

# Hang tailing a log we know exists
tail -f /var/log/faillog
EOF

# Make executable
RUN chmod +x /entry-point.sh

RUN curl -fsSL https://deb.nodesource.com/setup_22.x | sudo bash - && \
    apt-get install -y nodejs 

# Clone the app
RUN git clone https://github.com/mekatrol/imperium.git /imperium

# Build and publish the app
RUN dotnet restore /imperium/code/Imperium/Imperium.Server/Imperium.Server.csproj 
RUN cd /imperium/code/Imperium/imperium.dashboard && \
    npm i && \
    npm run compile && \
    cd /
RUN dotnet publish /imperium/code/Imperium/Imperium.Server/Imperium.Server.csproj -c Release -o /app/publish    

# MQTT  settings
RUN sed -i "s/\"MqttServer\": *\"\"/\"MqttServer\": \"${MQTT_HOST_NAME}\"/" /app/publish/appsettings.json && \
    sed -i "s/\"MqttUser\": *\"\"/\"MqttUser\": \"${MQTT_USER_NAME}\"/" /app/publish/appsettings.json && \
    sed -i "s/\"MqttPassword\": *\"\"/\"MqttPassword\": \"${MQTT_USER_PASSWORD}\"/" /app/publish/appsettings.json

# Start SSH and the .NET app
CMD ["/entry-point.sh"]
