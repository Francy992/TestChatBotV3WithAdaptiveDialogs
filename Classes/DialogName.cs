namespace QnABot.Classes
{
    public static class DialogName
    {
        public const string LuisDialog = "luis-dialog";
        public const string HandleMeteo = "meteo-dialog";
        public const string HandleBookingRestaurant = "restaurant-dialog";
        public const string HandleTravelDialog = "travel-dialog";
        public const string HandleTravelBookingDialog = "travel-booking-dialog";
        public const string QnAMakerDialog = nameof(Microsoft.Bot.Builder.AI.QnA.Dialogs.QnAMakerDialog);

        public const string ResultFromLuis = "resultFromLuis";
    }
}
