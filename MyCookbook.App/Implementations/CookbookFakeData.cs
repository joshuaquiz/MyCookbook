using System;
using System.Collections.Generic;
using System.Linq;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Implementations;

public static class CookbookFakeData
{
    public static PopularItem GetPopularItem(
        UserProfileModel? userProfile = null)
    {
        var popularItems = new List<PopularItem>
        {
            new(
                Guid.NewGuid(),
                new Uri(
                    "https://www.allrecipes.com/thmb/SpLbvOKqRtr6U3iodmNcJ5FgnAw=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/49943-grilled-peanut-butter-and-jelly-sandwich-4x3-0309-085648b2dc5f421da0fbef9292a89ff0.jpg",
                    UriKind.Absolute),
                "PB&J",
                new Uri("https://www.wilsoncenter.org/sites/default/files/media/images/person/james-person-1.jpg",
                    UriKind.Absolute),
                "FoodNetwork",
                TimeSpan.FromMinutes(15),
                new Uri(
                    "https://www.allrecipes.com/thmb/SpLbvOKqRtr6U3iodmNcJ5FgnAw=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/49943-grilled-peanut-butter-and-jelly-sandwich-4x3-0309-085648b2dc5f421da0fbef9292a89ff0.jpg",
                    UriKind.Absolute)),
            new(
                Guid.NewGuid(),
                new Uri(
                    "https://www.foodandwine.com/thmb/dMG6keGBcEF7XF8LZdR2y5dPrxc=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/jamaican-jerk-chicken-FT-RECIPE0918-eabbd55da31f4fa9b74367ef47464351.jpg",
                    UriKind.Absolute),
                "Jerk Chicken",
                new Uri("https://www.wilsoncenter.org/sites/default/files/media/images/person/james-person-1.jpg",
                    UriKind.Absolute),
                "FoodNetwork",
                TimeSpan.FromHours(1.5),
                new Uri(
                    "https://www.allrecipes.com/thmb/SpLbvOKqRtr6U3iodmNcJ5FgnAw=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/49943-grilled-peanut-butter-and-jelly-sandwich-4x3-0309-085648b2dc5f421da0fbef9292a89ff0.jpg",
                    UriKind.Absolute))
        };
        return popularItems[Random.Shared.Next(0, popularItems.Count)] with
        {
            AuthorName = userProfile is null
                ? "FoodNetwork"
                : userProfile.Value.FirstName + " " + userProfile.Value.LastName
        };
    }

    public static UserProfileModel GetAppUsersProfile()
    {
        var userProfile = new UserProfileModel(
            Guid.NewGuid(),
            new Uri(
                "https://www.allrecipes.com/thmb/SpLbvOKqRtr6U3iodmNcJ5FgnAw=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/49943-grilled-peanut-butter-and-jelly-sandwich-4x3-0309-085648b2dc5f421da0fbef9292a89ff0.jpg",
                UriKind.Absolute),
            new Uri("https://www.wilsoncenter.org/sites/default/files/media/images/person/james-person-1.jpg",
                UriKind.Absolute),
            "Jim",
            "Smith",
            "USA",
            "Columbus",
            59,
            30,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\n\nUt enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
            true,
            false,
            []);
        return userProfile with
        {
            RecentRecipes = Enumerable.Range(0, Random.Shared.Next(0, 15))
                .Select(_ => GetPopularItem(userProfile))
                .ToList()
        };
    }
}