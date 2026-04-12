# Agent instructions for SSH.NET

## Build

Always build in **both** Debug and Release before finalizing changes. Release mode sets `TreatWarningsAsErrors=true`, which will catch StyleCop and analyzer violations that are silent in Debug.

```bash
dotnet build
dotnet build -c Release
```

## Tests

Run both the unit tests and the integration tests:

```bash
dotnet test test/Renci.SshNet.Tests/
dotnet test test/Renci.SshNet.IntegrationTests/
```

Integration tests use **Testcontainers** (Docker required) to spin up an Alpine SSH server automatically.

**On Linux**, skip the `net48` (netfx) TFM — it requires .NET Framework and won't run:

```bash
dotnet test test/Renci.SshNet.Tests/ --framework net9.0
dotnet test test/Renci.SshNet.IntegrationTests/ --framework net9.0
```

For inner-loop speed, testing a single modern .NET target (e.g. `--framework net9.0`) is acceptable when a change affects all targets uniformly.

### Expected pass rate

The baseline is **100% passing**. Occasional failures due to timing or network flakiness are acceptable; treat any failure that could plausibly relate to your change as a real failure and investigate.

### Known failures in network-restricted environments

The following integration tests require outbound internet access (they connect to `www.google.com:80` or similar) and will fail in network-restricted environments such as the Copilot agent sandbox. These failures are **not** regressions — ignore them:

- `Ssh_DynamicPortForwarding_IPv4`
- `Ssh_DynamicPortForwarding_DomainName`
- `Ssh_DynamicPortForwarding_DisposeSshClientWithoutStoppingPort`
- `Ssh_LocalPortForwarding`
- `Ssh_LocalPortForwardingCloseChannels`
- `OldIntegrationTests/ForwardedPortLocalTest` (all methods)

## Coding style

Style is enforced by **StyleCop** and **Meziantou** analyzers and becomes build errors in Release mode. Follow `.editorconfig` and `stylecop.json` at the repo root. Key rules:

- `#nullable enable` at the top of every `.cs` file.
- `using` directives **outside** the namespace block; `System.*` namespaces first, blank line, then other namespaces.
- 4-space indentation (spaces, not tabs).
- XML doc comments (`///`) on all public and internal members; `<inheritdoc/>` when implementing an interface.
- Private fields use `_camelCase`; everything else uses `PascalCase`.
