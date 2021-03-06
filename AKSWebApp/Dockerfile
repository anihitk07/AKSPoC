#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


ARG VERSION=5.0-alpine3.12
FROM mcr.microsoft.com/dotnet/sdk:$VERSION AS build-env
WORKDIR /build
COPY ["AKSWebApp/AKSWebApp.csproj", "AKSWebApp/"]

RUN dotnet restore "AKSWebApp/AKSWebApp.csproj"
COPY . .

WORKDIR "/build/AKSWebApp"

RUN dotnet publish "AKSWebApp.csproj" \
  -r alpine-x64 \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true \
  /p:PublishReadyToRun=true \
  /p:PublishReadyToRunShowWarnings=true \
  -c Release \
  -o ./output

RUN chmod u+x,o+x ./output/AKSWebApp

ENV TINI_VERSION v0.19.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini-static ./tini
RUN chmod +x ./tini

FROM mcr.microsoft.com/dotnet/runtime-deps:$VERSION

RUN echo "ipv6" >> /etc/modules;
RUN echo "http://dl-cdn.alpinelinux.org/alpine/v3.12/community/" >> /etc/apk/repositories; \
    echo "http://dl-cdn.alpinelinux.org/alpine/v3.12/main/" >> /etc/apk/repositories;

RUN apk update --no-cache
RUN apk add --no-cache icu-libs

ENV APP_USER=app
ENV APP_DIR="/$APP_USER"

WORKDIR "/build/AKSWebApp"

RUN apk add dos2unix --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/community/ --allow-untrusted

COPY --from=build-env /build/AKSWebApp/harden.sh .
RUN chmod +x harden.sh && \
  dos2unix harden.sh && \
  sh harden.sh && \
  rm harden.sh

COPY --from=build-env /build/AKSWebApp/post-install.sh .
RUN chmod +x post-install.sh && \
  dos2unix post-install.sh && \
  sh post-install.sh AKSWebApp && \
  rm post-install.sh

WORKDIR $APP_DIR
COPY --from=build-env --chown=$APP_USER:$APP_USER /build/AKSWebApp/output .
COPY --from=build-env /build/AKSWebApp/tini .

ENV DOTNET_RUNNING_IN_CONTAINER=true \
  ASPNETCORE_URLS=http://+:8080 \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  COMPlus_EnableDiagnostics=0

USER $APP_USER
EXPOSE 8080
ENTRYPOINT ["./tini", "--", "./AKSWebApp"]