using System.Net;
using Dapr.Client;
using Dapr.Workflow;

namespace Workflow1
{
	public class ProcessValueActivity : WorkflowActivity<string, string>
	{
		public override Task<string> RunAsync(WorkflowActivityContext context, string input)
		{
			Console.WriteLine($"{context.InstanceId}: {context.Name}: ⚡ triggered");

			return Task.FromResult(CaesarShift(input, 1));
		}

		private string CaesarShift(string plainText, int shift)
		{
			var cipherText = string.Empty;
			foreach (char ch in plainText)
			{
				if (char.IsLetter(ch))
				{
					char d = char.IsUpper(ch) ? 'A' : 'a';
					cipherText += (char)(((ch + shift - d) % 26) + d);
				}
				else
				{
					cipherText += ch;
				}
			}
			return cipherText;
		}
	}
}