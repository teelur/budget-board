# AI Disclosure

Given the sensitivity of the data that is handled by the project, and the rise of vibe-coded apps that are sloppily built by AI tools without much human oversight, I want to be transparent about how I use AI tools in this project.

While this project is human-designed, developed, reviewed, and tested, I do use AI tools as part of my workflow, and I have put together this document to break down some of the areas where I use AI tools and where I explicitly do not use them.

## General Philosophy

This project predates the widespread availability of LLM-based AI tools, and thus a majority of the core architecture of the project was created without any AI assistance. Since early 2025, I have used AI tools as part of my workflow, mostly due to some of the benefits I have found in my professional work as a developer.

My general philosophy is to treat AI tools as an overly eager intern/junior developer. They can provide interesting suggestions and can be helpful with some hand-holding, but should not be trusted with high-stakes features. I've found it to be pretty helpful with smaller, well-defined tasks, but larger, more complex tasks tend to require too much iteration to be worth it.

---

## Usage Breakdown

There are a few areas where I do use AI tools and where I explicitly do not.

### Where AI is Helpful

#### Brainstorming

Typically at the start of a more complex feature, I will use AI tools to generate markdown documents that list out my thoughts on how to approach the implementation. It's super helpful to be able to gather my thoughts in a conversational format and then take that conversation and boil it down into a concise design doc that I can refer back to as I implement the feature.

#### UI Prototyping

When I'm designing a new UI component, I like to be able to quickly put together a basic prototype to get a feel for how the interaction with the component will work. AI tools are helpful for throwing together some basic code to get something to show up with some garbage data, which I can then use to build out the backend logic.

#### UI Styling

I work on .NET enterprise software professionally, so UI design isn't my strong suit. I generally have an idea in mind of what I want things to look like, but struggle to make the CSS do what I want. AI tools are very helpful in figuring out the right CSS to get it done.

#### PR Reviews

Adding the Copilot reviewer to GitHub PRs has been pretty helpful in getting another set of "eyes" on changelists. I will always do self-review of the code, but as a solo maintainer, it's nice to have some additional feedback on the code.

#### Unit Test Scaffolding

Unit tests are like 80% boilerplate and 20% actual brain power. AI is very helpful in scaffolding up a bunch of tests, and then going back through and cleaning them up.

#### Other Misc Repetitive or Low-Complexity Tasks

There are a lot of other small, repetitive tasks that come up in development that start to make this feel like a job rather than something I work on for fun. Adding features with prior art, updating similar changes across several components, and other low-risk, high-effort things. AI tools can be helpful in minimizing the time spent on those tasks.

### Where AI is Not Helpful

#### High-Stakes Code

I avoid leaning on AI tools for things like authentication and authorization logic, database schema design, and other critical paths. There are a couple of reasons:

- The cost of mistakes here is very high, so I want to ensure that it is done right the first time.
- It's important to have a good understanding of what the underlying logic looks like, so that debugging issues later is easier.
- There typically isn't a lot of prior art to scaffold from, so I need to be sure it is designed in a way that is effective and maintainable.

I will typically still use AI to ask questions and get ideas, but I won't rely on it to generate the actual code for these areas.

#### Documentation Content

I have tried to use AI tools to generate documentation content in the past, mostly because I don't particularly enjoy writing it. I've found that it doesn't really make the process any easier, just different. The generated content is often overly verbose and requires a lot of editing to get it to not be slop, and at that point, it is just as easy to write it myself.

It is helpful for grammar and spelling checks, but I don't let it mess with phrasing.

_Last updated: 2026-06-08_
