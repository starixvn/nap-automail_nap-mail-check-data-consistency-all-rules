# ARG CI_REGISTRY_IMAGE
# FROM $CI_REGISTRY_IMAGE:builder as deps
# RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

# ENV NUGET_PACKAGES=/src/packages
# COPY --from=deps $NUGET_PACKAGES/. $NUGET_PACKAGES/


COPY ./Fanex.CheckDataConsistency.Service/Fanex.CheckDataConsistency.Business ./Fanex.CheckDataConsistency.Business
COPY ./Fanex.CheckDataConsistency.Service/Fanex.CheckDataConsistency.Service/Fanex.CheckDataConsistency.Service.csproj ./Fanex.CheckDataConsistency.Service/Fanex.CheckDataConsistency.Service.csproj

RUN ls /src -l
WORKDIR /src/Fanex.CheckDataConsistency.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n -r linux-x64 

COPY ./Fanex.CheckDataConsistency.Service/Fanex.CheckDataConsistency.Service .

RUN dotnet publish -r linux-x64 --self-contained false -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-nap-mail-core

# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.CheckDataConsistency.Service.dll"]
