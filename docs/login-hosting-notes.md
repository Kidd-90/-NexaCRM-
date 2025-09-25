# Login Hosting Notes

## Purpose
- Ensure the NexaCRM Blazor WebAssembly client always lands on the login experience when the site is first loaded from a hosted environment.
- Prevent hard refresh navigation to `/login` that can break on static hosting platforms without custom rewrite rules.

## Implementation Details
- `App.razor` inspects the current relative URI and renders the `RedirectToLogin` helper when the app is opened on the root path or the generated `index.html` variants.
- `RedirectToLogin.razor` performs an in-app navigation to `/login` using Blazor's `NavigationOptions` to replace the history entry, preserving any query string or fragment during the redirect.
- `Pages/_Imports.razor` now applies `[Authorize]` to every page by default while the authentication flows (`LoginPage`, `FindIdPage`, `PasswordResetPage`, and `UserRegistrationPage`) opt-in to `[AllowAnonymous]`, preventing unauthenticated users from landing on internal dashboards when the app first renders.
- The login page (`LoginPage.razor`) still performs an authentication check on initialization to send already authenticated users to the appropriate dashboard, ensuring the new redirect path does not interfere with existing role-based routing.

## Testing Guidance
- Build the solution with `dotnet build --configuration Release`.
- Run the UI test suite with `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`.
- After publishing or hosting, open the site root (`/`) and verify the browser is redirected to `/login` without a full page reload or 404.
