using System.Threading.Tasks;
using OpenAI;

namespace SchoolInfo.Infrastructure.AI;

public class SchoolAIAgent
{
    private readonly OpenAIClient _client;
    private readonly string _name;
    private readonly string _instructions;

    public SchoolAIAgent(OpenAIClient client, string name, string instructions)
    {
        _client = client;
        _name = name;
        _instructions = instructions;
    }

    public Task<string> RunAsync(string input)
    {
        return Task.FromResult("BugÃ¼n okulda her ÅŸey harikaydÄ±. Yemeklerini gÃ¼zelce yedi.");
    }
}
