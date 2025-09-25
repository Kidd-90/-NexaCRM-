# Login Hosting Notes

## Purpose
- Ensure the NexaCRM Blazor WebAssembly client always lands on the login experience when the site is first loaded from a hosted environment.
- Prevent hard refresh navigation to `/login` that can break on static hosting platforms without custom rewrite rules.

## Implementation Details
- `LoginPage.razor` now declares both `/login` and `/` routes. Anonymous visitors landing on the site root immediately render the full login layout instead of a temporary redirect shell, while authenticated users continue to be forwarded to their dashboards during initialization.
- `App.razor` uses the standard `AuthorizeRouteView` pipeline with a lightweight `LoadingScreen` for the authorizing state and `RedirectToLogin` for unauthorized attempts. This keeps the router logic simple while still pushing protected routes back to the login experience.
- `Pages/_Imports.razor` continues to apply `[Authorize]` to every page by default while the authentication flows (`LoginPage`, `FindIdPage`, `PasswordResetPage`, and `UserRegistrationPage`) opt-in to `[AllowAnonymous]`, preventing unauthenticated users from landing on internal dashboards when the app first renders.

## Testing Guidance
- Build the solution with `dotnet build --configuration Release`.
- Run the UI test suite with `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`.
- After publishing or hosting, open the site root (`/`) and verify the login layout renders immediately. Confirm that submitting valid credentials transitions to the appropriate dashboard without a full page reload or 404.
