FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SmartHospitalAPI.csproj", "./"]
RUN dotnet restore "SmartHospitalAPI.csproj"

COPY . .
RUN dotnet publish "SmartHospitalAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SmartHospitalAPI.dll"]