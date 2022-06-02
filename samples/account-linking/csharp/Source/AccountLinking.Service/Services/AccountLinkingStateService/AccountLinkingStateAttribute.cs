namespace Microsoft.Teams.Samples.AccountLinking.Service.State;

[AttributeUsage(AttributeTargets.Method)]
public class AccountLinkingStateAttribute : Attribute
{
    public string QueryStringParameterName { get; set; }

    public AccountLinkingStateAttribute(string queryStringParameterName)
    {
        QueryStringParameterName = queryStringParameterName;
    }
}