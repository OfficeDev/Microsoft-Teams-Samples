namespace MeetingTranscription.Services
{
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Card Factory contract.
    /// Card Factory Service exposes methods to render Adaptive Card or Adaptive Cards as attachment.
    /// </summary>
    public interface ICardFactory
    {
        /// <summary>
        ///  Returns Attachment rendered as Adaptive Card. Additionaly takes in Object to be rendered into Card.
        /// </summary>
        /// <param name="dataObj">Object to be rendered into Card.</param>
        /// <returns>Attachment.</returns>
        public Attachment CreateAdaptiveCardAttachement(object dataObj);
    }
}
