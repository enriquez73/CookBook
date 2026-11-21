namespace Cookbook.Api.Configuration;

public static class ApiRoutes
{
    public const string Api = "/api";

    public const string Version = "v1";

    public const string Root = $"{Api}/{Version}";

    public static class Recipes
    {
        public const string Base = $"{Root}/recipes";

        public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Ingredients
    {
        public const string Base = $"{Root}/ingredients";

        public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Categories
    {
        public const string Base = $"{Root}/categories";

        public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Tags
    {
        public const string Base = $"{Root}/tags";

        public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Equipments
    {
        public const string Base = $"{Root}/equipments";

        public const string ById = $"{Base}/{{id:guid}}";
    }
}