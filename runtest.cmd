rem vstest.console src\Renci.SshNet.Tests\bin\Debug\net40\Renci.SshNet.Tests.dll "/TestCaseFilter:TestCategory=Gert

vstest.console src\Renci.SshNet.Tests\bin\Debug\net40\Renci.SshNet.Tests.dll /TestCaseFilter:"TestCategory!=integration&TestCategory!=LongRunning"