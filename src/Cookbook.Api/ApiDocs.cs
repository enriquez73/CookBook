static class ApiDocs
{
    public const string SwaggerUi = """
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Cookbook API - Swagger UI</title>
        <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css"></head>
        <body><div id="ui"></div>
        <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
        <script>SwaggerUIBundle({ url: '/openapi/v1.json', dom_id: '#ui' });</script>
        </body></html>
        """;

    public const string Redoc = """
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Cookbook API - Redoc</title></head>
        <body><redoc spec-url="/openapi/v1.json"></redoc>
        <script src="https://cdn.redoc.ly/redoc/latest/bundles/redoc.standalone.js"></script>
        </body></html>
        """;
}
