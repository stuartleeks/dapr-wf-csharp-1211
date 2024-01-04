# dapr-workflow-processor-csharp

This is a project to explore ideas for Dapr workflows calling rate-limited processing services in C#

- [dapr-workflow-processor-csharp](#dapr-workflow-processor-csharp)
	- [Components](#components)
		- [processor](#processor)
		- [workflow1](#workflow1)
		- [processor-sender](#processor-sender)
	- [Running workflow1](#running-workflow1)
		- [workflow1 - no retries](#workflow1---no-retries)


## Components

| Component | Description                                                                                                        |
| --------- | ------------------------------------------------------------------------------------------------------------------ |
| processor | Simple service to fake a long-running, rate-limited API (cheaper than CogSvcs and easier to run locally 😉)         |
| workflow1 | Contains an HTTP endpoint for submitting jobs and a workflow that processes them by invoking the processor service |

### processor

The `processor` is an HTTP endpoint that can be invoked by as a Dapr service.
The service is configured with a chance of random failure and a rate limit.
The service returns the submitted content shifted via a caesar cipher.

Calls to `processor` are rate-limited (to 1 call per second)`

Configuration options:
- `PORT` - the port to listen on (default 8001), use this to override the port when running multiple instances of the service locally
- `DELAY` - how long the service should sleep for to simulate doing work (default 2s)
- `FAILURE_CHANCE` - what chance of failure there is for an invocation of the service, expressed as a percentage (default 30, i.e. 30% chance of failure)
- `SHIFT_AMOUNT` - the caesar shift amount to use when encoding the content (default 1)

### workflow1

The `workflow1` service contains a Dapr workflow and an HTTP endpoint that can be invoked to submit a job to the workflow.

There are sample jobs in `submit_jobs.http` in the repo root that show the format of the job:

```json
{
	"steps": [
		{
			"name": "parallel_step",
			"actions" : [
				{
					"action": "processor1",
					"content" : "Hello World"
				},
				{
					"action": "processor1",
					"content" : "Do stuff"
				},
				{
					"action": "processor1",
					"content" : "Do more stuff"
				}
			]
		},
			{
			"name": "final_step",
			"actions" : [
				{
					"action": "processor1",
					"content" : "Finale"
				}
			]
		}
	]
}
```

A job consists of a number of steps, each of which contains a number of actions.
Each step is executed in sequence, but the actions within a step are executed in parallel.
The `action` value indicates which service to invoke (in this case `processor1` which is a deployment of the `processor` service).

The result from the workflow is in the format shown below:

```json
{
	"id": "60eea8fa-514f-473f-a7ff-0d66330d0220",
	"status": "Completed",
	"steps": [
		{
			"actions": [
				{
					"action": "processor1",
					"attempt_count": 1,
					"content": "Hello World",
					"result": {
						"result": "Ifmmp Xpsme",
						"success": true
					}
				},
				{
					"action": "processor1",
					"attempt_count": 1,
					"content": "Do stuff",
					"result": {
						"result": "Ep tuvgg",
						"success": true
					}
				},
				{
					"action": "processor1",
					"attempt_count": 1,
					"content": "Do more stuff",
					"result": {
						"result": "Ep npsf tuvgg",
						"success": true
					}
				}
			],
			"name": "parallel_step"
		},
		{
			"actions": [
				{
					"action": "processor1",
					"attempt_count": 1,
					"content": "Finale",
					"result": {
						"result": "Gjobmf",
						"success": true
					}
				}
			],
			"name": "final_step"
		}
	]
}
```

The `status` property indicates whether the job completed successfully or not.
The `steps` property contains the results of each step, including the results of each action within the step.
Each item under `actions` also includes an `attempt_count` property which indicates how many times the action was attempted which is relevant in some of the retry configurations.


### processor-sender

The `processor-sender` service was mostly added as a quick way to test the behaviour of the `processor` service.


## Running workflow1

All of the scenarios assume that you are running in the dev container and have run `dapr init`.

For all of the workflow1 scenarios, there are two instances of the `processor` service running.
Service `processor1` uses a shift value of 1 (`Hello` becomes `Ifmmp`) and service `processor2` uses a shift value of 2 (`Hello` becomes `Jgnnq`).


To send requests to the workflow you can use `submit_jobs.http` or the justfile recipes.

To use `submit_jobs.http`, open the file in VS Code and use the `Send Request` button to send the request to the workflow.
The file contains requests for a job with single step + action, a job with multiple steps + actions, as well as requests to query the job status.

To use the justfile recipes run `just submit-job-simple`, `just submit-job-multi-step`, or `just submit-job-multi-step-with-processor2` from the terminal.

### workflow1 - no retries

Starting the services:

```bash
dapr run -f dapr-workflow1-no-retries.yaml
```

The above commands start `processor1` with a 30% chance of failure and a 1 RPS rate limit.

When running the multi-step/-action job you will see that when multiple requests are sent in parallel the processor returns status 429 responses (too many requests) for some of the requests due to the rate-limiting configured.
You will also likely see some status 400 responses due to the failure chance configured for `processor1`.

For ease of identifying the `invoke_processor` results in the logs the completion log message includes an emoji:

- ✅ - success
- ❌ - failure
- ⏳ - rate-limited
