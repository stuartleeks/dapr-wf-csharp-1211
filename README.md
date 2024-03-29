# dapr-wf-csharp-1211

This repo is a repro for https://github.com/dapr/dotnet-sdk/issues/1211.

## Running the repro

To run and validate that the code is working correctly, run:

1. Open in a dev container in VS Code
2. Run `dapr init`
3. Run `dapr run -f .` to start the app
4. Run ` echo '[ "test1", "hello" ]' | ./scripts/run-workflow.sh` to trigger a workflow and display the output


With the code in the initial state, the above should run correctly and output something similar to the output below:

```bash
Refreshing workflow status...
Status: Completed - done
Workflow complete
{
  "id": "f837bbf8-f8d6-4efe-91f8-149d286024b3",
  "status": "Completed",
  "result": [
    "uftu1",
    "ifmmp"
  ]
}
```

To reproduce the issue, comment out the `ToList()` call in `ProcessingWorkflow.cs` and re-run.
With the `ToList()` commented out, the workflow no longer completes.
