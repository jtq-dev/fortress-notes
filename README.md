# fortress-notes 🛡️
**Policy-as-Code guardrails (OPA/Rego + Conftest + Terraform/Kubernetes examples)**

`fortress-notes` is a practical collection of **OPA/Rego policies** and **Conftest tests** to enforce security and reliability standards in Infrastructure-as-Code (IaC) and Kubernetes manifests.

It’s meant to be:
- **Fast to run locally**
- **Easy to plug into CI**
- **Strict enough to prevent common mistakes**
- **Documented so recruiters can understand in 2 minutes**

---

## What’s inside

### ✅ Policy checks (examples)
- **Deny Docker images using `:latest`**
- **Require Kubernetes probes** (liveness/readiness)
- **Require resource requests/limits**
- **Deny overly permissive Terraform rules** (e.g., `0.0.0.0/0` ingress)
- **Basic guardrails** for safer deployments

You can apply these checks to:
- Kubernetes YAML (Deployments, Services, etc.)
- Terraform plans/configs (depending on how you structure inputs)

---

## Tech stack
- **OPA / Rego** (policy language)
- **Conftest** (runs policies as tests)
- Optional: **Terraform**, **Kubernetes manifests**
- Optional: CI with **GitHub Actions**

---

## Quickstart

### 1) Install Conftest
**macOS (Homebrew)**
```bash
brew install conftest
