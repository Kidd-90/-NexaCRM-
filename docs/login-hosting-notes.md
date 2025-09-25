# Login Hosting Notes

## Purpose
- Ensure the NexaCRM Blazor WebAssembly client always lands on the login experience when the site is first loaded from a hosted environment.
- Prevent hard refresh navigation to `/login` that can break on static hosting platforms without custom rewrite rules.

## Implementation Details
- `App.razor` now uses Blazor's `Router.OnNavigateAsync` callback to enforce authentication before any protected page renders. The callback redirects root requests (`/`, `/index`, `/index.html`) directly to `/login` and blocks navigation to any authenticated route until the user has a valid session.
- `RedirectToLogin.razor` remains as the shared UI used while an anonymous user is being navigated to the login experience and during the `Authorizing`/`NotAuthorized` states emitted by `AuthorizeRouteView`.
- `Pages/_Imports.razor` continues to apply `[Authorize]` to every page by default while the authentication flows (`LoginPage`, `FindIdPage`, `PasswordResetPage`, and `UserRegistrationPage`) opt-in to `[AllowAnonymous]`, preventing unauthenticated users from landing on internal dashboards when the app first renders.
- The login page (`LoginPage.razor`) still performs an authentication check on initialization to send already authenticated users to the appropriate dashboard, ensuring the new redirect path does not interfere with existing role-based routing.

## Testing Guidance
- Build the solution with `dotnet build --configuration Release`.
- Run the UI test suite with `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`.
- After publishing or hosting, open the site root (`/`) and verify the browser is redirected to `/login` without a full page reload or 404.
