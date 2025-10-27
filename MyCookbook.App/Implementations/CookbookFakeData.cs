using System;
using System.Collections.Generic;
using System.Linq;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Enums;

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

    public static RecipeModel GetRecipe()
    {
        return new RecipeModel(
            Guid.NewGuid(),
            new Uri(
                "https://www.foodandwine.com/thmb/dMG6keGBcEF7XF8LZdR2y5dPrxc=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/jamaican-jerk-chicken-FT-RECIPE0918-eabbd55da31f4fa9b74367ef47464351.jpg",
                UriKind.Absolute),
            "Jerk Chicken",
            TimeSpan.Zero,
            TimeSpan.FromSeconds(86),
            1,
            "A yummy sandwhich",
            new UserProfileModel(
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
                []),
            [],
            [
                new(
                    Guid.NewGuid(),
                    1,
                    new Uri(
                        "https://www.mashed.com/img/gallery/the-untold-truth-of-the-peanut-butter-and-jelly-sandwich/intro-1592327549.jpg",
                        UriKind.Absolute),
                    "Spread the peanut butter and jelly on the bread.",
                    [
                        new(
                            Guid.NewGuid(),
                            new IngredientModel(
                                Guid.NewGuid(),
                                new Uri(
                                    "https://i.guim.co.uk/img/media/680e32cbca5c3ef1b8570dab45baa9272972acbb/0_231_5760_3456/master/5760.jpg?width=1200&quality=85&auto=format&fit=max&s=8fd7c391da7edc664eec28db8163a806",
                                    UriKind.Absolute),
                                "Chicken"),
                            "2",
                            Measurement.Piece,
                            null),

                        new(
                            Guid.NewGuid(),
                            new IngredientModel(
                                Guid.NewGuid(),
                                new Uri(
                                    "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bc/PeanutButter.jpg/640px-PeanutButter.jpg",
                                    UriKind.Absolute),
                                "Water"),
                            "1/4",
                            Measurement.Cup,
                            null),

                        new(
                            Guid.NewGuid(),
                            new IngredientModel(
                                Guid.NewGuid(),
                                new Uri(
                                    "https://i2.wp.com/practicalselfreliance.com/wp-content/uploads/2019/01/grape-jam-4.jpg?resize=480%2C360&ssl=1",
                                    UriKind.Absolute),
                                "Spices"),
                            "1/4",
                            Measurement.Cup,
                            null)
                    ]),

                new(
                    Guid.NewGuid(),
                    2,
                    new Uri(
                        "https://bushcooking.com/wp-content/uploads/2017/06/Toasted-Penut-Butter-and-Jelly-Sandwich-3a.jpg",
                        UriKind.Absolute),
                    "Spread a thin layer of butter on sandwich and grill lightly.",
                    [
                        new(
                            Guid.NewGuid(),
                            new IngredientModel(
                                Guid.NewGuid(),
                                new Uri(
                                    "https://www.sumptuousspoonfuls.com/wp-content/uploads/2020/04/Homemade-Spreadable-Butter-3.jpg",
                                    UriKind.Absolute),
                                "Spreadable butter"),
                            "1/8",
                            Measurement.Cup,
                            null)
                    ])
            ]);
    }
}