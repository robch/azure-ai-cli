<#@ template hostspecific="true" #>
<#@ output extension=".py" encoding="utf-8" #>
<#@ parameter type="System.String" name="ClassName" #>
from openai import AzureOpenAI

class <#= ClassName #>:
    def __init__(self, openai_api_version, openai_endpoint, openai_key, openai_chat_deployment_name, openai_system_prompt):
        self.openai_system_prompt = openai_system_prompt
        self.openai_chat_deployment_name = openai_chat_deployment_name
        self.client = AzureOpenAI(
            api_key=openai_key,
            api_version=openai_api_version,
            azure_endpoint = openai_endpoint
            )
        self.clear_conversation()

    def clear_conversation(self):
        self.messages = [
            {'role': 'system', 'content': self.openai_system_prompt}
        ]

    def get_chat_completions(self, user_input, callback):
        self.messages.append({'role': 'user', 'content': user_input})

        complete_content = ''
        response = self.client.chat.completions.create(
            model=self.openai_chat_deployment_name,
            messages=self.messages,
            stream=True)

        for chunk in response:

            choice0 = chunk.choices[0] if hasattr(chunk, 'choices') and chunk.choices else None
            delta = choice0.delta if choice0 and hasattr(choice0, 'delta') else None
            content = delta.content if delta and hasattr(delta, 'content') else ''

            finish_reason = choice0.finish_reason if choice0 and hasattr(choice0, 'finish_reason') else None
            if finish_reason == 'length':
                content += f"{content}\nERROR: Exceeded max token length!"

            if content is None: continue

            complete_content += content
            callback(content)

        self.messages.append({'role': 'assistant', 'content': complete_content})
        return complete_content