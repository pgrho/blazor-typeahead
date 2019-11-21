namespace Shipwreck.BlazorTypeahead.Demo
{
    public class StateInfo
    {
        public static readonly StateInfo[] All =
        {
            new StateInfo("Alabama", "AL"),
            new StateInfo("Alaska", "AK"),
            new StateInfo("Arizona", "AZ"),
            new StateInfo("Arkansas", "AR"),
            new StateInfo("California", "CA"),
            new StateInfo("Colorado", "CO"),
            new StateInfo("Connecticut", "CT"),
            new StateInfo("Delaware", "DE"),
            new StateInfo("Florida", "FL"),
            new StateInfo("Georgia", "GA"),
            new StateInfo("Hawaii", "HI"),
            new StateInfo("Idaho", "ID"),
            new StateInfo("Illinois", "IL"),
            new StateInfo("Indiana", "IN"),
            new StateInfo("Iowa", "IA"),
            new StateInfo("Kansas", "KS"),
            new StateInfo("Kentucky", "KY"),
            new StateInfo("Louisiana", "LA"),
            new StateInfo("Maine", "ME"),
            new StateInfo("Maryland", "MD"),
            new StateInfo("Massachusetts", "MA"),
            new StateInfo("Michigan", "MI"),
            new StateInfo("Minnesota", "MN"),
            new StateInfo("Mississippi", "MS"),
            new StateInfo("Missouri", "MO"),
            new StateInfo("Montana", "MT"),
            new StateInfo("Nebraska", "NE"),
            new StateInfo("Nevada", "NV"),
            new StateInfo("New Hampshire", "NH"),
            new StateInfo("New Jersey", "NJ"),
            new StateInfo("New Mexico", "NM"),
            new StateInfo("New York", "NY"),
            new StateInfo("North Carolina", "NC"),
            new StateInfo("North Dakota", "ND"),
            new StateInfo("Ohio", "OH"),
            new StateInfo("Oklahoma", "OK"),
            new StateInfo("Oregon", "OR"),
            new StateInfo("Pennsylvania", "PA"),
            new StateInfo("Rhode Island", "RI"),
            new StateInfo("South Carolina", "SC"),
            new StateInfo("South Dakota", "SD"),
            new StateInfo("Tennessee", "TN"),
            new StateInfo("Texas", "TX"),
            new StateInfo("Utah", "UT"),
            new StateInfo("Vermont", "VT"),
            new StateInfo("Virginia", "VA"),
            new StateInfo("Washington", "WA"),
            new StateInfo("West Virginia", "WV"),
            new StateInfo("Wisconsin", "WI"),
            new StateInfo("Wyoming", "WY"),
        };

        private StateInfo(string name, string postal)
        {
            Name = name;
            Postal = postal;
        }

        public string Name { get; }
        public string Postal { get; }
    }
}