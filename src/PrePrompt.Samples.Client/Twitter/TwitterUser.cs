using System.Runtime.Serialization;

namespace PrePrompt.Samples.Client.Twitter
{
    [DataContract]
    public class TwitterUser
    {
        [DataMember(Name = "id")]
        public int Id;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "screen_name")]
        public string ScreenName;
        [DataMember(Name = "location")]
        public string Location;
        [DataMember(Name = "description")]
        public string Description;
        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl;
        [DataMember(Name = "url")]
        public string Url;
        [DataMember(Name = "protected")]
        public bool IsProtected;
        [DataMember(Name = "followers_count")]
        public int FollowersCount;
        [DataMember(Name = "profile_background_color")]
        public string ProfileBackgroundColor;
        [DataMember(Name = "profile_text_color")]
        public string ProfileTextColor;
        [DataMember(Name = "profile_line_color")]
        public string ProfileLinkColor;
        [DataMember(Name = "profile_sidebar_fill_color")]
        public string ProfileSidebarFillColor;
        [DataMember(Name = "profile_sidebar_border_color")]
        public string ProfileSidebarBorderColor;
        [DataMember(Name = "friends_count")]
        public int FriendsCount;
        [DataMember(Name = "created_at")]
        public string CreatedDate;
        [DataMember(Name = "favourites_count")]
        public int FavouritesCount;
        [DataMember(Name = "utc_offset")]
        public string UtcOffset;
        [DataMember(Name = "time_zone")]
        public string TimeZone;
        [DataMember(Name = "profile_background_image_url")]
        public string ProfileBackgroundImageUrl;
        [DataMember(Name = "profile_background_tile")]
        public bool IsProfileBackgroundTiled;
        [DataMember(Name = "profile_use_background_image")]
        public bool PorfileUsesBackgroundImage;
        [DataMember(Name = "geo_enabled")]
        public bool IsGeoEnabled;
        [DataMember(Name = "verified")]
        public bool IsVerified;
        [DataMember(Name = "statuses_count")]
        public int StatusesCount;
        [DataMember(Name = "lang")]
        public string Language;
        [DataMember(Name = "listed_count")]
        public int ListedCount;
    }
}