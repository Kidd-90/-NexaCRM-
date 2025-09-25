# Login Hosting Notes

## Purpose
- Ensure the NexaCRM Blazor WebAssembly client always lands on the login experience when the site is first loaded from a hosted environment.
- Prevent hard refresh navigation to `/login` that can break on static hosting platforms without custom rewrite rules.

## Implementation Details
- `App.razor` renders `RedirectToLogin` whenever the router resolves the landing routes (`/`, `/index`, `/index.html`). The component performs a client-side redirect to `/login` using SPA navigation so the first-render experience always shows the login form without triggering a hard refresh.
- `AuthorizeRouteView` remains wrapped around every protected page to display `RedirectToLogin` during the `Authorizing` and `NotAuthorized` states, ensuring unauthorized users are pushed back to the login flow before any dashboard content renders.
- `Pages/_Imports.razor` continues to apply `[Authorize]` to every page by default while the authentication flows (`LoginPage`, `FindIdPage`, `PasswordResetPage`, and `UserRegistrationPage`) opt-in to `[AllowAnonymous]`, preventing unauthenticated users from landing on internal dashboards when the app first renders.
- The login page (`LoginPage.razor`) still performs an authentication check on initialization to send already authenticated users to the appropriate dashboard, ensuring the redirect path does not interfere with existing role-based routing.

## Testing Guidance
- Build the solution with `dotnet build --configuration Release`.
- Run the UI test suite with `dotnet test ./tests/BlazorWebApp.Tests --configuration Release`.
- After publishing or hosting, open the site root (`/`) and verify the browser is redirected to `/login` without a full page reload or 404.
