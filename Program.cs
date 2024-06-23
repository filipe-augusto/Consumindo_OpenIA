using System.Text;
using System.Text.Json;

while (true)
{
    Console.WriteLine("Digite sua pergunta:");
    var prompt = Console.ReadLine();

    if (prompt.ToLower() == "sair")
        break;

    if (prompt.ToLower().StartsWith("imagem"))
        await Imagem(prompt.Substring(7)); // Remove "imagem " do prompt
    else
        await Pergunta(prompt);
}

async Task Pergunta(string prompt)
{
    if (string.IsNullOrWhiteSpace(prompt))
        return;

    string apiKey = "your_key";

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 1.0,
                max_tokens = 1024
            }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Resposta data = JsonSerializer.Deserialize<Resposta>(responseString);
            Console.ForegroundColor = ConsoleColor.Red;

            Array.ForEach(data.choices.ToArray(), (item) => Console.WriteLine(item.message.content.Replace("\n", "")));

            Console.ResetColor();
        }
        else
        {
            Console.Clear();
            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ocorreu um erro ao enviar a pergunta. Detalhes: {responseString}");
        }
    }
}

async Task Imagem(string prompt)
{
    if (string.IsNullOrWhiteSpace(prompt))
        return;

    string apiKey = "your_key";

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new
            {
                prompt = prompt,
                n = 1,
                size = "1024x1024"
            }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/images/generations", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Resposta resposta = JsonSerializer.Deserialize<Resposta>(responseString);
            Console.ForegroundColor = ConsoleColor.Red;

            Array.ForEach(resposta.data.ToArray(), (item) => Console.WriteLine(item.url.Replace("\n", "")));

            Console.ResetColor();
        }
        else
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ocorreu um erro ao enviar a pergunta. Detalhes: {responseString}");
        }
    }
}

class Resposta
{
    public List<Choice> choices { get; set; }
    public Data[] data { get; set; }
    public class Choice
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string content { get; set; }
    }

    public class Data
    {
        public string url { get; set; }
    }
}
