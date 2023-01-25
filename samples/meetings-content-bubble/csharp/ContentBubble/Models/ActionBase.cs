using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Content_Bubble_Bot.Models
{
    public class ActionBase
    {
        /// <summary>
        ///  Model entity for identifying action type of card.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///  Model entity for extracting user's choiceset.
        /// </summary>
        public string Choice { get; set; }
    }
}
