# GitHub Copilot Instructions for SSH.NET

## What this repository does

SSH.NET is a .NET library for SSH-2 protocol communication, optimized for parallelism. It provides:

- **SSH command execution** (synchronous and async)
- **SFTP** file operations (synchronous and async)
- **SCP** file transfers
- **Port forwarding** (local, remote, dynamic/SOCKS)
- **Interactive shell** via `ShellStream`
- **NetConf** protocol client
- **Multi-factor and certificate-based authentication**

Primary entry points are `SshClient`, `SftpClient`, `ScpClient`, and `NetConfClient`, all extending `BaseClient`.

---

## Core technologies

| Area | Technology |
|---|---|
| Language | C# (`LangVersion=latest`) with `#nullable enable` everywhere |
| Runtimes | .NET Framework 4.6.2, .NET Standard 2.0, .NET 8+, .NET 9+, .NET 10+ |
| Cryptography | BouncyCastle (`BouncyCastle.Cryptography`) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` (`ILogger`/`ILoggerFactory`) |
| Testing | MSTest v4, Moq, Testcontainers (Docker for integration tests) |
| Build tooling | .NET SDK 10.0.100, Nerdbank.GitVersioning |
| Static analysis | StyleCop.Analyzers, Meziantou.Analyzer, SonarAnalyzer.CSharp |

---

## Code organization

```
src/Renci.SshNet/
├── Channels/          # SSH channel types (session, forwarded, X11…)
├── Common/            # Shared utilities, extension methods, custom exceptions
├── Compression/       # Zlib compression support
├── Connection/        # Socket connectors, proxy support (SOCKS4/5, HTTP), key exchange
├── Messages/          # SSH protocol message types
│   ├── Transport/
│   ├── Authentication/
│   └── Connection/
├── Security/          # Algorithms, key-exchange, cryptography wrappers
│   └── Cryptography/  # Ciphers, signatures, key types
├── Sftp/              # SFTP session, requests, responses
├── Netconf/           # NetConf client
└── Abstractions/      # Platform and I/O abstractions
```

Large classes are split into **partial class files** per concern (e.g., `PrivateKeyFile.PKCS1.cs`, `PrivateKeyFile.OpenSSH.cs`).

---

## Naming and style conventions

| Element | Convention | Example |
|---|---|---|
| Classes, methods, properties | PascalCase | `SftpClient`, `ListDirectory()`, `IsConnected` |
| Private fields | `_camelCase` | `_isDisposed`, `_sftpSession` |
| Interfaces | `I` prefix + PascalCase | `ISftpClient`, `IChannel` |
| Constants / static readonly | PascalCase | `MaximumSshPacketSize` |
| Local variables | camelCase | `operationTimeout`, `connectionInfo` |

**StyleCop is enforced.** Follow existing file conventions:

- `#nullable enable` at the top of every `.cs` file.
- `using` directives **outside** the namespace block, grouped with `System.*` first, then a blank line, then other namespaces.
- 4-space indentation (spaces, not tabs).
- XML doc comments (`///`) on all public and internal members; use `<inheritdoc/>` when inheriting from an interface.
- Describe exception conditions in `/// <exception cref="…">` tags.
- No Hungarian notation.

---

## Error handling

Use the existing exception hierarchy; do not throw `Exception` or `ApplicationException` directly.

```
SshException
├── SshConnectionException       # connection-level failures
├── SshAuthenticationException   # auth failures
├── SshOperationTimeoutException # operation timed out
├── SshPassPhraseNullOrEmptyException
├── ProxyException
├── SftpException
│   ├── SftpPermissionDeniedException
│   └── SftpPathNotFoundException
├── ScpException
└── NetConfServerException
```

- Annotate new exception classes with `#if NETFRAMEWORK [Serializable] #endif` and add the protected serialization constructor inside the same `#if` block, matching the pattern in `SshException.cs`.
- Surface async errors by storing them in a `TaskCompletionSource` or re-throwing via `ExceptionDispatchInfo.Throw()` — never swallow exceptions silently.
- Raise `ErrorOccurred` events on long-running components (e.g., `ForwardedPort`, `Session`) rather than propagating the exception across thread boundaries.

---

## API design

- **Every public blocking method should have a `…Async(CancellationToken cancellationToken = default)` counterpart.** Keep both in the same partial class file or co-located partial file.
- Validate all arguments at the top of public methods; prefer `ArgumentNullException`, `ArgumentException`, `ArgumentOutOfRangeException` with descriptive messages.
- Return `IEnumerable<T>` for sequences that are already materialized; use `IAsyncEnumerable<T>` when data streams asynchronously.
- Prefer `ReadOnlyMemory<byte>` / `Memory<byte>` over `byte[]` in new protocol-layer code.
- Do not expose mutable collections directly; use `.AsReadOnly()` or copy-on-return.

---

## Async and concurrency

- Use `async Task` / `async ValueTask` with `CancellationToken` for all new async methods.
- The socket receive loop and keep-alive timer run on dedicated background threads; protect shared state with `lock` or the custom internal `Lock` type used in `Session`.
- Prefer `TaskCompletionSource<T>` to bridge event-driven or callback-based code into the async model.
- Never block inside async code with `.Result` or `.Wait()` — this can cause deadlocks on synchronization-context-bound callers.
- Use `ConfigureAwait(false)` in library code (this is a class library, not an app).

---

## Security-sensitive areas

These areas require extra care:

- **`src/Renci.SshNet/Security/`** — key exchange, algorithm negotiation. Algorithm choices have direct security implications.
- **`src/Renci.SshNet/PrivateKeyFile*.cs`** — key format parsing (PKCS#1, PKCS#8, OpenSSH, PuTTY, ssh.com). Input is untrusted; validate lengths and offsets before indexing.
- **`src/Renci.SshNet/Connection/`** — host key verification. Never bypass host key checking silently.
- **Sensitive data in memory**: clear key material as soon as it is no longer needed; do not log private key bytes or plaintext passwords even at `Debug` level.
- **`SshNetLoggingConfiguration.WiresharkKeyLogFilePath`** is a Debug-only diagnostic that writes session keys to disk. It must never be enabled in production builds.
- **Cryptographic primitives** come from BouncyCastle. Prefer existing wrappers over direct calls to BouncyCastle APIs; adding new algorithms requires corresponding unit tests and algorithm negotiation entries.

---

## Testing

### Unit tests (`test/Renci.SshNet.Tests/`)

- Framework: **MSTest** (`[TestClass]`, `[TestMethod]`, `[TestInitialize]`, `[TestCleanup]`)
- Mocking: **Moq** — use `Mock<T>`, `.Setup(…)`, `.Verify(…)`
- File naming: `{ClassName}Test[_{Scenario}].cs` (e.g., `SftpClientTest.ConnectAsync.cs`)
- Each scenario lives in its own partial class file inside `Classes/`
- Base classes: `TestBase`, `SessionTestBase`, `BaseClientTestBase` — extend the appropriate base
- Arrange-Act-Assert style; each test method is focused on a single observable behaviour

```csharp
[TestMethod]
public void MethodName_Scenario_ExpectedOutcome()
{
    // Arrange
    var connectionInfo = new PasswordConnectionInfo("host", 22, "user", "pwd");
    var target = new SftpClient(connectionInfo);

    // Act
    var actual = target.SomeProperty;

    // Assert
    Assert.AreEqual(expected, actual);
}
```

### Integration tests (`test/Renci.SshNet.IntegrationTests/`)

- Require **Docker** (via Testcontainers); a running Docker daemon is a prerequisite.
- Run with `dotnet test` like any other project once Docker is available.

### Running tests

```bash
# Unit tests only
dotnet test test/Renci.SshNet.Tests/

# All tests (requires Docker)
dotnet test

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### What to test

- Add unit tests for every new public method and every non-trivial internal method.
- Test both the happy path and error/edge cases (e.g., `null` arguments, disposed state, cancellation).
- For async methods, test cancellation via `CancellationToken` and timeout behaviour.
- Do **not** remove or weaken existing tests; add new tests instead.

---

## Performance considerations

- SSH.NET is designed for parallelism; avoid introducing `lock` contention on hot paths.
- Prefer `ArrayPool<byte>.Shared` or `MemoryPool<byte>.Shared` over allocating new `byte[]` in protocol encoding/decoding.
- The `BufferSize` on `SftpClient` controls read/write payload sizes; keep protocol overhead in mind when changing defaults.
- Benchmark significant changes with `test/Renci.SshNet.Benchmarks/` using BenchmarkDotNet.

---

## Making changes safely

1. **Run the unit tests first** (`dotnet test test/Renci.SshNet.Tests/`) to establish a clean baseline.
2. **Keep changes focused.** Small, targeted PRs merge faster. Separate refactoring from behaviour changes.
3. **Multi-targeting**: the library targets .NET Framework 4.6.2, .NET Standard 2.0, and modern .NET. Use `#if NETFRAMEWORK` / `#if NET` guards for platform-specific code; confirm all TFMs build with `dotnet build`.
4. **Public API changes**: adding new overloads or members is generally safe. Removing or changing existing signatures is a breaking change — discuss in an issue first.
5. **Cryptographic changes**: consult existing algorithm registration lists (in `ConnectionInfo.cs` and `Session.cs`) before adding or removing algorithms.
6. **StyleCop and analyzers**: the build treats analyzer warnings as errors in Release/CI mode. Run `dotnet build -c Release` locally to catch these before pushing.
7. **CI note**: some integration tests can flake due to timing or networking. If an existing (unrelated) test fails intermittently in CI but passes locally, it is likely a known flake.
