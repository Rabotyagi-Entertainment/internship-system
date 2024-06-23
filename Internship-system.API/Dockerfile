FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Internship-system.API/Internship-system.API.csproj", "Internship-system.API/"]
COPY ["Internship-system.BLL/Internship-system.BLL.csproj", "Internship-system.BLL/"]
COPY ["Internship-system.DAL/Internship-system.DAL.csproj", "Internship-system.DAL/"]
COPY ["internship-system.Common/internship-system.Common.csproj", "internship-system.Common/"]
RUN dotnet restore "Internship-system.API/Internship-system.API.csproj"
COPY . .
WORKDIR "/src/Internship-system.API"
RUN dotnet build "Internship-system.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Internship-system.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Internship-system.API/Src/PracticeDiary_CourseWork.docx", "Src/"]
COPY ["Internship-system.API/Src/PracticeDiary_Default.docx", "Src/"]
ENTRYPOINT ["dotnet", "Internship-system.API.dll"]
