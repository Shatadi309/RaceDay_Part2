namespace RaceDay_Part2.Middleware
{//start of namespace
    public class sessionMiddleware
    {//start of class

    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Allow auth + swagger
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/api/auth") || path.StartsWith("/swagger") || path == "/")
            {
                await _next(context);
                return;
            }

            var userId = context.Session.GetInt32("UserID");
            var role = context.Session.GetString("Role");

            if (userId == null || string.IsNullOrEmpty(role))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Please log in.");
                return;
            }

            // Role-based guards
            if (path.StartsWith("/api/events") && context.Request.Method != "GET"
                && role != "Organizer")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: Organizer access required.");
                return;
            }

            if (path.StartsWith("/api/categories") && context.Request.Method != "GET"
                && role != "Organizer")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: Organizer access required.");
                return;
            }

            await _next(context);
        }
    }


}//end of class
}//end of namespace
