namespace msgcard_actest;
using AdaptiveCards.Templating;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        var acTemplateJson = File.ReadAllText("actemplate.json");
        var messageCardInput = File.ReadAllText("msgcard.json");
        // var messageCardInput = File.ReadAllText("msgCardSharePointRequest.json");
        // var messageCardInput = File.ReadAllText("msgcardWithoutSections.json");
         // var messageCardInput = File.ReadAllText("msgcardTwitter.json");

        // Create a Template instance from the template payload
        AdaptiveCardTemplate template = new AdaptiveCardTemplate(acTemplateJson);

        // You can use any serializable object as your messageCardInput
        var messageCardData = JsonConvert.DeserializeObject(messageCardInput);
        // "Expand" the template - this generates the final Adaptive Card payload
        string adaptiveCardJson = template.Expand(messageCardData);
        Console.WriteLine(adaptiveCardJson);
        //Optional: Convert to Adaptive Card object
        // AdaptiveCardParseResult result = AdaptiveCard.FromJson(adaptiveCardJson);
        //result.Card has the AC output       
    }
}
