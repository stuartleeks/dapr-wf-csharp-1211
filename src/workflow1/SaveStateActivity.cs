using Dapr.Client;
using Dapr.Workflow;

namespace Workflow1
{

	public class SaveStateActivity : WorkflowActivity<string[], bool>
	{
		private readonly DaprClient _daprClient;

		public SaveStateActivity(DaprClient daprClient)
		{
			_daprClient = daprClient;
		}
		public async override Task<bool> RunAsync(WorkflowActivityContext context, string[] input)
		{
			Console.WriteLine($"Starting {nameof(SaveStateActivity)}: {context.InstanceId}");

			await _daprClient.SaveStateAsync(storeName: "statestore", key: context.InstanceId, value: input);

			return true;
		}
	}
}