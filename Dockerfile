FROM microsoft-docker-remote.artifactory.platform.vwfs.io/dotnet/sdk:6.0@sha256:b1ac02d48e170b007d2d2392119fa037e531f1ad93cf8433d911f402d00c743a AS build-env

WORKDIR /src
COPY ["PCLDeliveryAPI/PCLDeliveryAPI.csproj", "PCLDeliveryAPI/"]
COPY ["PorscheComponent/PorscheComponent.csproj", "PorscheComponent/"]
COPY ["PorscheDataAccess/PorscheDataAccess.csproj", "PorscheDataAccess/"]
COPY ["PorscheUtilities/PorscheUtilities.csproj", "PorscheUtilities/"]
RUN dotnet restore "PCLDeliveryAPI/PCLDeliveryAPI.csproj"
COPY . .
WORKDIR "/src/PCLDeliveryAPI"
RUN dotnet build "PCLDeliveryAPI.csproj" -c Release -o /app/build
RUN dotnet publish "PCLDeliveryAPI.csproj" \ 
  -c Release \ 
  -o /app/publish \
  -r alpine-x64 \
  --self-contained true \
  /p:PublishReadyToRun=true \
  /p:PublishReadyToRunShowWarnings=true \
  /p:PublishSingleFile=true

FROM microsoft-docker-remote.artifactory.platform.vwfs.io/dotnet/runtime-deps:6.0-alpine@sha256:e86ce2bf9b77a93075dbe03bc5a3ba4eeae93bdfc5415a9354c22ed3504d46a8

# The user the app should run as
ENV APP_USER=app
# The home directory
ENV APP_DIR="/$APP_USER"

# default directory is /app
WORKDIR $APP_DIR

# Harden docker image
COPY build/harden.sh .
RUN chmod +x harden.sh && \
  sh -xe harden.sh && \
  rm harden.sh 

# Copy application and chown all app files
COPY --from=build-env --chown=$APP_USER:$APP_USER /app/publish .

RUN mkdir -p /tmp
VOLUME /tmp

RUN mkdir -p /var/lib/amazon
VOLUME /var/lib/amazon

RUN mkdir -p /var/log/amazon
VOLUME /var/log/amazon

RUN mkdir -p /$APP_DIR/TempFolder
RUN chown $APP_USER:$APP_USER /$APP_DIR/TempFolder
VOLUME /$APP_DIR/TempFolder

RUN mkdir -p /$APP_DIR/wwwroot
RUN chown $APP_USER:$APP_USER /$APP_DIR/wwwroot
VOLUME /$APP_DIR/wwwroot

ENV DOTNET_RUNNING_IN_CONTAINER=true \
  ASPNETCORE_URLS=http://+:8080

# Run some post install hardening commands
COPY build/post-install.sh .
RUN chmod +x post-install.sh && \
  sh -xe post-install.sh PCLDeliveryAPI && \
  rm post-install.sh 

# Run app as non root user
USER $APP_USER
EXPOSE 8080
ENTRYPOINT ["./PCLDeliveryAPI"]
