FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HotelOS.Backend.sln", "./"]
COPY ["src/HotelOS.Domain/*.csproj", "src/HotelOS.Domain/"]
COPY ["src/HotelOS.Contracts/*.csproj", "src/HotelOS.Contracts/"]
COPY ["src/HotelOS.Application/*.csproj", "src/HotelOS.Application/"]
COPY ["src/HotelOS.Infrastructure/*.csproj", "src/HotelOS.Infrastructure/"]
COPY ["src/HotelOS.Api/*.csproj", "src/HotelOS.Api/"]
COPY ["src/HotelOS.Workers/*.csproj", "src/HotelOS.Workers/"]

RUN dotnet restore "HotelOS.Backend.sln"

COPY . .
WORKDIR "/src/src/HotelOS.Api"
RUN dotnet build "HotelOS.Api.csproj" -c Release -o /app/build
RUN dotnet publish "HotelOS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HotelOS.Api.dll"]
