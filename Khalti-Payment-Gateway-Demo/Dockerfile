#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Khalti-Payment-Gateway-Demo/Khalti-Payment-Gateway-Demo.csproj", "Khalti-Payment-Gateway-Demo/"]
RUN dotnet restore "Khalti-Payment-Gateway-Demo/Khalti-Payment-Gateway-Demo.csproj"
COPY . .
WORKDIR "/src/Khalti-Payment-Gateway-Demo"
RUN dotnet build "Khalti-Payment-Gateway-Demo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Khalti-Payment-Gateway-Demo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Khalti-Payment-Gateway-Demo.dll"]