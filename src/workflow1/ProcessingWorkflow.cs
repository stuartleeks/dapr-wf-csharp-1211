using System.Diagnostics;
using Dapr.Workflow;

namespace Workflow1
{

	public class ProcessingWorkflow : Workflow<string[], string[]>
	{
		public async override Task<string[]> RunAsync(WorkflowContext context, string[] inputs)
		{
			try
			{
				if (!context.IsReplaying)
				{
					Console.WriteLine($"Starting workflow: {context.InstanceId}");
				}

				// Process each value in inputs in parallel activities
				var activityTasks = inputs.Select(input =>
						context.CallActivityAsync<string>(nameof(ProcessValueActivity), input)
				)
				// Comment out the ToList below to repro https://github.com/dapr/dotnet-sdk/issues/1211
				.ToList()
				;
				await Task.WhenAll(activityTasks);

				var result = activityTasks.Select(t=>t.Result).ToArray();

				
				await context.CallActivityAsync<bool>(
					name: nameof(SaveStateActivity),
					 input: result);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
				throw;
			}
		}
	}

}