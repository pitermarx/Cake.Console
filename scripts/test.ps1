$csproj = "src/Cake.Console.Tests/Cake.Console.Tests.csproj"
function test($arguments)
{
    $fileName = $arguments.Replace(" ", "_");
    $fileNameSnapshot = "snapshots/$filename.verified.txt";
    $fileNameResult = "snapshots/$filename.result.txt";
    $cmd = "dotnet run -p $csproj -c=Release --no-build -- $arguments"

    Write-Host $cmd
    $err = ""
    try {
        $output = iex "$cmd 2>&1" -ErrorAction SilentlyContinue
        if ($output -eq $null) { $output = "" }

    } catch {
        $err = $_
    }

    $output > $fileNameResult
    $err >> $fileNameResult

    if (!(Test-Path $fileNameSnapshot))
    {
        mv $fileNameResult $fileNameSnapshot
        Write-Host "    > Snapshot file written"
        return;
    }

    # Ignore time diferences in report
    $diff = Compare-Object $(Get-Content $fileNameSnapshot) $(Get-Content $fileNameResult) |
        ? { !$_.InputObject.Contains("00:00:") }

    if ($diff)
    {
        Write-Host "    > FAIL"
        $diff
        return;
    }

    rm $fileNameResult;
    Write-Host "    > SUCCESS"
}

dotnet build $csproj -c=Release

test("unknown")
test("host")
test("cli")
test("cli --target=task2")
test("cli --target=task1 -v=Diagnostic")
test("cli --target=task1")
test("cli --target=taskB")
test("cli --target=taskB --exclusive")
test("cli --description")
test("cli --dryrun")
test("cli --dryrun --target=TaskB")
test("cli --dryrun --target=TaskB --exclusive")
test("cli --version")
test("cli --info")
test("cli --target=task1 -v=Diagnostic")
test("cli --tree")
test("cli --help")
test("cli -h")