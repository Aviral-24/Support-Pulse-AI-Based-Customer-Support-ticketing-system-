# # Build Stage
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src
# COPY ["Backend.csproj", "./"]
# RUN dotnet restore "Backend.csproj"
# COPY . .
# RUN dotnet publish "Backend.csproj" -c Release -o /app/publish

# # Runtime Stage
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# WORKDIR /app
# COPY --from=build /app/publish .
# ENTRYPOINT ["dotnet", "Backend.dll"]

# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 🔥 FIX: Install Fonts and FontConfig for QuestPDF in the Linux container
RUN apt-get update && apt-get install -y \
    fontconfig \
    libfreetype6 \
    fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend.csproj", "./"]
RUN dotnet restore "./Backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Backend.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend.dll"]