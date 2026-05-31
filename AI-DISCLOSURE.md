# AI Disclosure

This project uses AI-assisted tooling during development. Given the sensitivity of the data handled by this application, I don't feel a simple statement in the README is enough to be transparent about the role of AI tools in the development process.

I've put together this disclosure document to break down the different areas where AI tools are used, to what extent they are used, and where I explicitly avoid using AI tools.

## General Philosophy

This project pre-dates the widespread availability of AI tools, and was initially architected and developed without them. As such, the overall design, architecture, and critical code paths of the application were created by human developers without AI assistance. I have slowly integrated AI tools into my workflow to help with the more tedious parts of development, and have found them to be useful in some areas.

While AI tools can be incredibly helpful for certain tasks, particularly mechanical tasks such as generating boilerplate, there are a lot of areas where they are notoriously unreliable and can introduce subtle bugs or security issues if used in a hands-off "vibe-coding" way.

**AI tools should be used as they are, tools, not as co-developers. They can be an accelerator for developers, but they are not a replacement for human judgment, experience, and good software engineering principles.**

For this reason, the general guidance is to treat AI tools as an overly eager intern/junior developer. They are generally helpful for simple tasks like scaffolding features with established patterns or low stakes code like UI components, but it is still expected that a human developer is responsible for the overall design, architecture, and critical code paths of the application.

**AI tools may generate code, but ultimately the human developer is responsible for that code, and should understand, review, and test it just as thoroughly (if not more thoroughly) as any human-authored code.**

---

## Usage Breakdown By Area

AI tool usage differs across the different areas of the codebase, so I have provided a small description of the general approach for each area.

### Server (`server/`)

A lot of the server code is what I would consider "high-stakes" code, in that bugs or security issues in this code could result in both data loss and data compromise.

For this reason, it is important that code in this component is deliberate, reviewed, and tested. This naturally limits the use of AI tools, but there are still some small tasks where they can be helpful.

| AI Used For                       | AI Not Used For                                                                                                    |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Debugging unexpected behavior     | User authorization and authentication logic                                                                        |
| Scaffolding unit tests            | Constructive and destructive operations (e.g. database writes, API calls)                                          |
| Brainstorming high-level concepts | Novel features without clear prior art or patterns to scaffold from (e.g. new algorithms, complex data processing) |

---

### Client (`client/`)

While there are some aspects of the client code that are high-stakes, such as the authentication flows, some of the state management, and data parsing, there are also a lot of areas where the stakes are lower, such as UI components, styling, and some of the more mechanical aspects of React development.

| AI Used For                                | AI Not Used For                            |
| ------------------------------------------ | ------------------------------------------ |
| Brainstorming UI styling                   | User authentication and authorization code |
| Debugging React build issues               | Constructive and destructive operations    |
| Implementing features with prior art       |                                            |
| Scaffolding code with established patterns |                                            |

---

### Build Infrastructure

The build infrastructure (CI/CD pipelines, Dockerfiles, etc.) was largely set up before AI tools were widely available, and as such are mostly human-authored.

| AI Used For                                                                 | AI Not Used For                     |
| --------------------------------------------------------------------------- | ----------------------------------- |
| Debugging build issues                                                      | Setting up the build infrastructure |
| Finding appropriate documentation for build tools and configuration options | Creating Dockerfiles                |
| PR feedback                                                                 |                                     |

---

### Documentation

While I have experimented with using AI tools to generate documentation, I have found that it is more work to get the generated documentation to not be overly-verbose slop than it is to just write it myself.

| AI Used For                                    | AI Not Used For           |
| ---------------------------------------------- | ------------------------- |
| Some of the styling and formatting in the wiki | All documentation content |
| Debugging Docusaurus build issues              |                           |

---

_Last updated: 2026-05-31_
