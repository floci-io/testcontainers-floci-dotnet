## Summary

<!-- Briefly describe what this PR does and why. -->

## Related issue

<!-- Link the issue this PR addresses, e.g. "Fixes #123" or "Closes #456". -->
<!-- If there is no issue, explain why the change is needed. -->

## Type of change

<!-- Check the one that applies. -->

- [ ] Bug fix (`fix:` commit)
- [ ] New feature (`feat:` commit)
- [ ] Breaking change (`feat!:` commit or `BREAKING CHANGE:` footer)
- [ ] Documentation or chore

## Checklist

- [ ] `dotnet build Testcontainers.Floci.slnx -c Release -warnaserror` is clean (0 warnings)
- [ ] `dotnet test` passes (unit always; integration if Docker-affecting — needs Docker)
- [ ] New or changed behaviour is covered by tests
- [ ] Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `chore:`, etc.)
