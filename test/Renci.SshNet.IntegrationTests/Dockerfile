FROM mcr.microsoft.com/dotnet/sdk:7.0.403-bullseye-slim AS build
ENV DEBIAN_FRONTEND=noninteractive
ENV DEBCONF_NOWARNINGS="yes"
ARG BUILD_CONFIGURATION=Debug
ENV http_proxy=http://proxy-dmz.intel.com:912/
ENV https_proxy=http://proxy-dmz.intel.com:912/
ENV no_proxy=intel.com,localhost

RUN curl -fsSL https://get.docker.com -o get-docker.sh \
    && sh get-docker.sh

WORKDIR /src

COPY ["test/Directory.Build.props", "test/"]
COPY ["Directory.Build.props", "."]
COPY ["test/Renci.SshNet.IntegrationTests/Dockerfile.TestServer", "test/Renci.SshNet.IntegrationTests/"]
COPY ["test/Renci.SshNet.IntegrationTests/Renci.SshNet.IntegrationTests.csproj", "test/Renci.SshNet.IntegrationTests/"]
COPY ["test/Renci.SshNet.TestTools.OpenSSH/Renci.SshNet.TestTools.OpenSSH.csproj", "test/Renci.SshNet.TestTools.OpenSSH/"]
COPY ["src/Renci.SshNet/Renci.SshNet.csproj", "src/Renci.SshNet/"]
RUN dotnet restore "./test/Renci.SshNet.IntegrationTests/./Renci.SshNet.IntegrationTests.csproj"

COPY . .

run ls test/Renci.SshNet.IntegrationTests/

WORKDIR "/src/test/Renci.SshNet.IntegrationTests"
RUN dotnet build "./Renci.SshNet.IntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/src/test/Renci.SshNet.IntegrationTests"
ENV https_proxy=
ENV http_proxy=
CMD ["dotnet", "test", "--no-restore", "--no-build", "-o", "/app/build", "--results-directory", "artifacts", "--logger", "console;verbosity=detailed", "--logger", "liquid.md;LogFileName=TestReport.md", "-p:CollectCoverage=true", "-p:CoverletOutputFormat=cobertura", "-p:CoverletOutput=artifacts/coverage.xml", "Renci.SshNet.IntegrationTests.csproj"]

