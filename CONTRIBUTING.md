# Contributing to Budget Board

Budget Board welcomes contributions from the community.

Contributing comes in many forms, whether it's reporting bugs, suggesting features, improving documentation, suggesting translations, or submitting code changes. This document provides guidelines and instructions for contributing to the project.

## Ways to Contribute

- **Bug reports** — Open an [issue](https://github.com/teelur/budget-board/issues/new?template=bug_report.md) with information about the bug, steps to reproduce it, and any relevant screenshots or logs.
- **Feature requests** — Start a [discussion](https://github.com/teelur/budget-board/discussions/categories/feature-requests) or add to existing requests. I will typicaly prioritize features that have more community interest, so upvoting and commenting on existing discussions is a great way to help drive the direction of the project.
- **Documentation** — If you find any gaps or inaccuracies in the documentation, you can open a PR in the [documentation repo](https://github.com/teelur/budget-board-docs).
- **Translations** — Help translate the app at [Weblate](https://hosted.weblate.org/engage/budget-board/). No code required.
- **Code contributions** — See below.

## Code Contributions

### Before You Start

Before you start working on code changes, please create a feature request discussion first. This helps to align on the approach and ensure that your efforts will be in line with the project's goals and roadmap. Indicate you are willing to work on the feature, and an issue will be created and assigned to you, when it's ready to be worked on.

For simple tweaks or bugfixes, you can skip the discussion step and open a PR directly. But for larger features or changes, it's important to follow the process to ensure your work will not be rejected.

## Development Setup

### Client Changes

#### Prerequisites

You will need to install the following software to run the client locally:

- Node.js
- npm
- yarn

#### Running the Client Locally

1. Clone the repository and navigate to the client directory.
2. Run `yarn install` to install dependencies.
3. Run `yarn run dev` to start the development server.
4. The client will be available at `http://localhost:5173`.

### Server Changes

#### Prerequisites

You will need to install the following software to run the server locally:

- .NET 10 SDK
- PostgreSQL

#### Running the Server Locally

1. Clone the repository and navigate to the server directory.
2. You will need to setup a local PostgreSQL database and get the connection string for it.
3. Update the .NET secrets with the connection details for your local database. See below for the required secrets.
4. Run `dotnet run` to start the server.

#### Secrets

You will need to add these variables to you .NET secrets file for the BudgetBoard.WebAPI project.

## Required Environment Variables

| Variable            | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| `CLIENT_ADDRESS`    | URL of the frontend for CORS (e.g., `http://localhost:5173`) |
| `POSTGRES_HOST`     | PostgreSQL host (e.g., `localhost`)                          |
| `POSTGRES_DATABASE` | PostgreSQL database name (e.g., `budgetboard`)               |
| `POSTGRES_USER`     | PostgreSQL username                                          |
| `POSTGRES_PASSWORD` | PostgreSQL password                                          |
| `POSTGRES_PORT`     | PostgreSQL port (default: `5432`)                            |
