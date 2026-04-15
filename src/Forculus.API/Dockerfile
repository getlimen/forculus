FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Forculus.slnx ./
COPY src/ ./src/
RUN dotnet publish src/Forculus.API/Forculus.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y wireguard-tools iproute2 iptables && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
EXPOSE 3004 51820/udp
ENV ASPNETCORE_URLS=http://+:3004
ENTRYPOINT ["dotnet", "Forculus.API.dll"]
