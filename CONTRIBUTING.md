Contributions in the form of issues, pull requests (PRs) and discussions are welcome to this repository. Please consider that the library is currently maintained as a hobby by a small number of individuals. As such, depending on the weather, work, private lives etc., your PR may wait an indeterminate amount of time before being addressed. Generally speaking, the more targeted and better tested the change, the quicker it can be merged.

## Building

The library has no special requirements to build, other than an up-to-date .NET SDK, and can be built from within the IDE or with `dotnet build` at the command line.

This repository also hosts the source for https://sshnet.github.io/SSH.NET/, which is built using [docfx](https://dotnet.github.io/docfx/index.html) and whose source files are in the `docfx/` directory. In order to build the site, install the docfx dotnet tool with `dotnet tool update -g docfx` and then run `docfx docfx/docfx.json --serve` from the root of the repository. When it completes, you should see e.g.

```
Serving "E:\github\SSH.NET\docfx\_site" on http://localhost:8080. Press Ctrl+C to shut down.
```

from which you can view the local version of the site. When making iterative changes, run `docfx docfx/docfx.json` from a separate command line and refresh the browser.

## Testing

The library has a test project for unit tests and a test project for integration tests. The latter uses [Testcontainers](https://dotnet.testcontainers.org/) which has a dependency on Docker. Practically, on Windows, an installation of Docker Desktop is all that is required, without any additional configuration. With Docker Desktop running, the integration tests can run like normal tests from within the IDE or with `dotnet test` at the command line.

Code coverage information can be generated for all test projects at once or for individual test projects. From the root of the repository or from the individual test project directory, run `dotnet test --collect:"XPlat Code Coverage"`.

The coverage information can be visualised using e.g. [ReportGenerator](https://reportgenerator.io/). Install the ReportGenerator dotnet tool with `dotnet tool update -g dotnet-reportgenerator-globaltool` and then run

```
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:TestResults/CoverageReport -assemblyfilters:+Renci.SshNet
```

View the report by opening TestResults/CoverageReport/index.html in the browser.

Before subsequent coverage collections, delete the previous collections with `git clean -fX test/` to prevent previous coverage files from being included in the subsequent generated report.

## CI

The repository makes use of continuous integration (CI) on [AppVeyor](https://ci.appveyor.com/project/drieseng/ssh-net/history) to validate builds and tests on PR branches and non-PR branches. At the time of writing, some tests can occasionally fail in CI due to a dependency on timing or a dependency on networking/socket code. If you see an existing test which is unrelated to your changes occasionally failing in CI but passing locally, you probably don't need to worry about it. If you see one of your newly-added tests failing, it is probably worth investigating why and whether it can be made more stable.

## Good to know

### TraceSource logging

The Debug build of SSH.NET contains rudimentary logging functionality via `System.Diagnostics.TraceSource`. See `Renci.SshNet.Abstractions.DiagnosticAbstraction` for usage examples.

### Wireshark

Wireshark is able to dissect initial connection packets, such as key exchange, before encryption happens. Enter "ssh" as the display filter. See https://wiki.wireshark.org/SSH.md for more information.
