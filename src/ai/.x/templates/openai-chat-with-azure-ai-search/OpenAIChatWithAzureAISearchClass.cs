<#@ template hostspecific="true" #>
<#@ output extension=".cs" encoding="utf-8" #>
<#@ parameter type="System.String" name="ClassName" #>
<#@ parameter type="System.String" name="OpenAIEndpoint" #>
<#@ parameter type="System.String" name="OpenAIDeploymentName" #>
<#@ parameter type="System.String" name="SearchEndpoint" #>
<#@ parameter type="System.String" name="SearchApiKey" #>
<#@ parameter type="System.String" name="SearchIndexName" #>
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class <#= ClassName #>
{
    private static string _openAIEndpoint = "<#= OpenAIEndpoint #>";
    private static string _openAIDeploymentName = "<#= OpenAIDeploymentName #>";
    private static string _searchEndpoint = "<#= SearchEndpoint #>";
    private static string _searchApiKey = "<#= SearchApiKey #>";
    private static string _searchIndexName = "<#= SearchIndexName #>";

    public async Task ChatUsingYourOwnData()
    {
        var client = new OpenAIClient(new Uri(_openAIEndpoint), new DefaultAzureCredential());

        AzureCognitiveSearchChatExtensionConfiguration contosoExtensionConfig = new()
        {
            SearchEndpoint = new Uri(_searchEndpoint),
            IndexName = _searchIndexName,
        };

        contosoExtensionConfig.SetSearchKey(_searchApiKey);

        ChatCompletionsOptions chatCompletionsOptions = new()
        {
            DeploymentName = _openAIDeploymentName,
            Messages =
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant that answers questions about the Contoso product database."),
                new ChatMessage(ChatRole.User, "What are the best-selling Contoso products this month?")
            },

            AzureExtensionsOptions = new AzureChatExtensionsOptions()
            {
                Extensions = { contosoExtensionConfig }
            }
        };

        Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
        ChatMessage message = response.Value.Choices[0].Message;

        Console.WriteLine($"{message.Role}: {message.Content}");

        Console.WriteLine("Citations and other information:");

        foreach (ChatMessage contextMessage in message.AzureExtensionsContext.Messages)
        {
            Console.WriteLine($"{contextMessage.Role}: {contextMessage.Content}");
        }
    }
}