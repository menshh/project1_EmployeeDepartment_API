FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Project1.csproj", "./"]
RUN dotnet restore "./Project1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Project1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Project1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeDepartment.dll"]
