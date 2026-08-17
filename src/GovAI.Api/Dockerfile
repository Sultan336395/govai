# Depo kökünden derlenir:  docker build -f src/GovAI.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce yalnızca proje dosyalarını kopyalayıp restore ediyoruz; kaynak değişse de
# NuGet katmanı önbellekte kalır ve derleme belirgin şekilde hızlanır.
COPY Directory.Build.props global.json ./
COPY src/GovAI.Domain/GovAI.Domain.csproj src/GovAI.Domain/
COPY src/GovAI.Application/GovAI.Application.csproj src/GovAI.Application/
COPY src/GovAI.Persistence/GovAI.Persistence.csproj src/GovAI.Persistence/
COPY src/GovAI.Infrastructure/GovAI.Infrastructure.csproj src/GovAI.Infrastructure/
COPY src/GovAI.Api/GovAI.Api.csproj src/GovAI.Api/
RUN dotnet restore src/GovAI.Api/GovAI.Api.csproj

COPY src/ src/
RUN dotnet publish src/GovAI.Api/GovAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# QuestPDF metin ölçümü için sistem fontlarına ihtiyaç duyar; aspnet imajında bunlar yoktur.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core curl \
    && rm -rf /var/lib/apt/lists/*

# Root olmayan kullanıcı; kapsayıcı güvenliğinin temel gereği.
RUN useradd --uid 10001 --create-home --shell /usr/sbin/nologin govai
COPY --from=build --chown=govai:govai /app/publish ./
RUN mkdir -p /app/logs && chown govai:govai /app/logs

USER govai
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "GovAI.Api.dll"]
