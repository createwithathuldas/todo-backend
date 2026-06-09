FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Todo-backend.csproj", "."]


RUN dotnet restore "Todo-backend.csproj"

COPY . .

RUN dotnet build "Todo-backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Todo-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=publish /app/publish .


EXPOSE 5143

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5143

ENTRYPOINT ["dotnet", "Todo-backend.dll"]
