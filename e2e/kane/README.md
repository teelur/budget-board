# End-to-end (e2e) tests for web

Browser tests written in plain English for [Kane CLI](https://www.testmuai.com/kane-cli/).

| Test | What it covers |
|---|---|
| `register_test.md` | Registering a local account and logging in to the dashboard |
| `spending_test.md` | Adding a manual account and a grocery purchase, then finding it on the Budgets page |

## Run

Each test starts from an empty database. From the repository root, start the stack on port 4940:

```bash
CUSTOM_PORT=4940 docker compose --env-file compose.env up -d --wait
npm install -g @testmuai/kane-cli
kane-cli login
kane-cli testmd run e2e/kane/register_test.md
```

Reset with `docker compose --env-file compose.env down -v` and start again before each run.
