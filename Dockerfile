FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj .
RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get -y update && apt-get -y upgrade && apt-get install ffmpeg --no-install-recommends -y
COPY --from=build /app ./
COPY --from=build /source/yt/yt-dlp_linux ./yt/
ENTRYPOINT [ "dotnet", "youtube-dl-api.dll" ]