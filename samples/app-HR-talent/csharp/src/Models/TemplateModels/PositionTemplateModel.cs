using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Models.TemplateModels
{
    public class PositionTemplateModel : BaseTemplateModel<Position>
    {
        public object ButtonActions { get; set; }
    }
}
