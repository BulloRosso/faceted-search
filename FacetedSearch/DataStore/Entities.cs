using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacetedSearch.DataStore
{
    /// <summary>
    /// Fake data store to translate entity keys (in real life this would be some mongoDB stuff or search terms would have to be denormalized into language specific search indices)
    /// </summary>
    public class Entities
    {
        public enum EntityTypes {
            Language,
            Topic
        }

        public static string GetTitle(EntityTypes EntityType, string EntityId) {

            EntityId = EntityId.ToLower();

            switch (EntityType)
            {
                case EntityTypes.Language:

                    switch (EntityId)
                    {
                        case "de":
                            return "German";
                        case "en":
                            return "English";
                        case "fr":
                            return "French";
                        case "it":
                            return "Italian";
                        default:
                            return "*Typo*";
                    }

                case EntityTypes.Topic:

                    switch (EntityId)
                    {
                        case "geek":
                            return "Geek stuff";
                        case "it":
                            return "Information Technology";
                        case "social":
                            return "Social Media";
                        case "music":
                            return "Music & Entertainment";
                        case "fashion":
                            return "Fashion & Lifestyle";
                        default:
                            return "*Typo*";
                    }

            }

            return "*unknown entity*";

        }
    }
}