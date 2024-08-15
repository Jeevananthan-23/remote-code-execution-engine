# Use the official Ubuntu base image
FROM ubuntu:20.04

# Set environment variables for non-interactive apt installs
ENV DEBIAN_FRONTEND=noninteractive

# Install dependencies
RUN apt-get update -y && \
    apt-get install -y \
    curl \
    vim \
    sudo \
    zlib1g-dev \
    build-essential \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    software-properties-common

# Install the .NET SDK (version 8.0)
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg
RUN add-apt-repository https://packages.microsoft.com/ubuntu/$(lsb_release -rs)/prod
RUN apt-get update && \
    apt-get install -y dotnet-sdk-8.0 && \
    apt-get install -y dotnet-runtime-8.0

# Install the Node
RUN curl -sL https://deb.nodesource.com/setup_16.x | bash
RUN apt-get install nodejs -y
# RUN npm install typescript ts-node


# Install the default JDK (OpenJDK 11)
RUN apt-get install -y default-jdk

# Install GCC (for C/C++ compilation)
RUN apt-get install -y gcc g++

# Verify installations
RUN dotnet --version
RUN java -version
RUN gcc --version
RUN node -v
RUN npm -v

# Create a directory for your application and copy the project files
WORKDIR /src
COPY ["workers/Workers.csproj", "workers/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "./workers/./Workers.csproj"
COPY . .
WORKDIR "/src/workers"
RUN dotnet build "./Workers.csproj" -c Release -o /app/build

# Restore and build the project
RUN dotnet publish "Workers.csproj" -c Release -o /app/publish

# Create and set permissions for the temp directory
RUN rm -rf temp
RUN mkdir temp
RUN chmod 700 /
RUN chmod 755 -R temp/

# Create a user for running the service securely
RUN adduser --disabled-password --gecos "" judge

# Switch to the new user
USER judge

# Set the working directory and entrypoint
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "Workers.dll"]
