# Login Error Handling

The NexaCRM Web client categorises Supabase sign-in failures so that users receive clear guidance during authentication.

## Failure Categories

- **Missing credentials**: Client-side validation returns immediately with guidance to enter the missing username or password.
- **User not found**: Supabase responses that indicate an unknown account return "입력하신 아이디를 찾을 수 없습니다." and log a warning for administrators.
- **Invalid password**: Known credential errors surface "비밀번호가 일치하지 않습니다" along with a console warning so support teams can trace repeated failures.
- **Pending approval**: Accounts without an approved `organization_users` row are signed out and informed that administrator approval is required.
- **Unknown errors**: Any unexpected Supabase or network issue records a structured log entry and shows the generic retry message.

## Implementation Notes

- `CustomAuthStateProvider.SignInAsync` now returns a `LoginResult` that includes a failure reason enum and localised message.
- When Supabase returns ambiguous credential errors, the provider performs a lightweight profile lookup to determine whether the username exists before classifying the failure.
- All failure paths ensure the Supabase session is cleared and log context-rich warnings for operational visibility.
