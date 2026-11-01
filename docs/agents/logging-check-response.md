From the repository root, run unit tests with the local SDK:

```sh
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet test backend/InvoiceApp.sln
```

One concrete logger limitation: `read_events()` skips a final transcript line without a trailing newline. That event is omitted until the line is completed and another sync runs.

Read both files. No files edited, tests executed, agents spawned, or commits created.