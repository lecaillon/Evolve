FROM checkmarx.jfrog.io/docker/chainguard/dotnet-sdk:8.0.8-r0--07dd9a5170a52f
USER root

# Install ReSharper GlobalTools
#RUN dotnet tool install --global JetBrains.ReSharper.GlobalTools --version 2024.1.4

# Restore
COPY build/common.props /build/
WORKDIR /src
COPY /src/Nuget.config /src/*.sln /src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore *.sln --configfile Nuget.config

COPY /src/ .

# Run ReSharper GlobalTools
#RUN /root/.dotnet/tools/jb inspectcode *.sln --build -o=output.txt --severity=WARNING --verbosity=WARN -f=Text -a\
#    && grep -E " {5}" output.txt | tee issues.txt && if [ -s issues.txt ]; then echo "Please fix the above warnings." && exit 1; fi

# Uninstall ReSharper GlobalTools
#RUN rm -rf /root/.dotnet/tools/.store/jetbrains.resharper.globaltools

# Check for vulnerable packages
#RUN dotnet list package --vulnerable --include-transitive | tee vulnerability_check.txt && \
#    if grep -q "has the following vulnerable packages" vulnerability_check.txt; then \
#      echo "Vulnerable packages found. Aborting build."; \
#      exit 1; \
#    fi

# Test
RUN dotnet test *.sln

# Pack
ARG VERSION=0.0.1

RUN dotnet pack *.sln -c Release -o /out -p:PackageVersion=$VERSION \
    -p:DefineConstants="JETBRAINS_ANNOTATIONS" -p:DefineConstants="CONTRACTS_FULL" -p:NoWarn=1591 -p:GenerateDocumentationFile=true

# Publish
WORKDIR /out
CMD ["-c", "find . -maxdepth 1 -type f -exec dotnet nuget push {} -s https://baget.lumodev.com/v3/index.json \\;"]
