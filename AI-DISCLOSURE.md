# AI Disclosure

Given the sensitivity of the data that is handled by the project, and the rise of vibe-coded apps that are sloppily built by AI tools without much human oversight, I want to be transparent about how I use AI tools in this project.

While this project is human-designed, developed, reviewed, and tested, I do use AI tools as part of my workflow, and I have put together this document to break down some of the areas where I use AI tools and where I explicitly do not use them.

## General Philosophy

This project predates the widespread availability of LLM-based AI tools, and thus a majority of the core architecture of the project was created without any AI assistance. Since early 2025, I have used AI tools as part of my workflow, mostly due to some of the benefits I have found in my professional work.

AI tools can be incredibly helpful for certain tasks, but they cannot replace human judgment. I generally will lean on AI tools for tasks like:

- Debugging unexpected behavior
- Brainstorming high-level concepts
- PR review feedback
- Scaffolding code with established patterns
- Other high-effort, low-complexity tasks

My general philosophy is to treat AI tools as an overly eager intern/junior developer. They can provide interesting suggestions and can be helpful with some hand-holding, but should not be trusted with high-stakes features.

---

## Usage Breakdown By Area

The usage of AI tools varies across the different areas of the project. I've broken down each area in the sections below. This list isn't comprehensive, but should give a general idea of what my workflow looks like.

### Server (`server/`)

A lot of the server code is high-stakes, since it handles data persistence, user authentication and authorization, and other critical paths. For this reason, I don't typically use AI tools much for the server side.

| AI Used For                        | AI Not Used For                                                                                                  |
| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| Scaffolding unit tests             | User authorization and authentication logic                                                                      |
| Misc high-effort, repetitive tasks | Constructive and destructive operations (e.g. database writes, API calls)                                         |
|                                    | New features without clear prior art or patterns to scaffold from (e.g. new algorithms, complex data processing)  |
|                                    | Database schema                                                                                                  |

---

### Client (`client/`)

While there are some aspects of the client code that are high-stakes, such as the authentication flows, most of the client code is visual, so I tend to lean on AI tools more here.

| AI Used For                             | AI Not Used For                            |
| --------------------------------------- | ------------------------------------------ |
| Prototyping UI components               | User authentication and authorization code |
| Debugging React build issues            | Constructive and destructive operations    |
| Brainstorming high-level UI/UX concepts | New features without clear prior art       |
| UI styling and CSS                      |                                            |

---

### Build Infrastructure

The build infrastructure (CI/CD pipelines, Dockerfiles, etc.) was largely set up before AI tools were widely available, so it is mostly human-authored.

| AI Used For                                                                 | AI Not Used For                     |
| --------------------------------------------------------------------------- | ----------------------------------- |
| Debugging build issues                                                      | Setting up the build infrastructure |
| Finding appropriate documentation for build tools and configuration options | Creating Dockerfiles                |

---

### Documentation

While I have experimented with using AI tools to generate documentation, I have found that it is more work to get the generated documentation to not be overly-verbose slop than it is to just write it myself.

| AI Used For                                    | AI Not Used For           |
| ---------------------------------------------- | ------------------------- |
| Some of the styling and formatting in the wiki | All documentation content |
| Debugging Docusaurus build issues              |                           |

---

_Last updated: 2026-05-31_
