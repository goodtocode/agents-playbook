# Administrative Application UX/UI Style Guide

## Purpose

This guide defines the user experience and user interface philosophy for administrative applications.

Examples include:

- Administration portals
- Operations dashboards
- Management consoles
- Orchestration platforms
- Runtime management systems
- Configuration portals
- Asset management systems
- Monitoring applications
- Workflow management platforms

This guide intentionally focuses on administrative users rather than consumer users.

Administrative applications should optimize for:

- Understanding
- Visibility
- Traceability
- Provenance
- Control
- Diagnostics
- Efficiency

Administrative applications should not primarily optimize for:

- Marketing experiences
- Consumer experiences
- Storytelling
- Guided onboarding
- Wizard-first interactions
- Hidden complexity

Administrative software exists to help users understand and manage complex systems.

---

# Design Philosophy

Administrative software serves a different purpose than consumer software.

The primary objective is not delight.

The primary objective is understanding.

Users should quickly understand:

- What exists
- What is selected
- What is related
- What is running
- What changed
- What failed
- What can be done next

The interface should reveal system behavior rather than hiding it.

---

# Core UX Principles

## Principle 1: Record First

Administrative applications begin with facts.

Always answer:

What exists?

before asking:

What would you like to create?

Preferred order:

```text
Existing Records
Selected Record
Related Records
Create/Edit Actions
```

Creation workflows are secondary to visibility.

---

## Principle 2: Selection Drives Context

The selected object is the center of the workspace.

Users should always know:

What is selected?

Everything else derives from that selection.

Preferred pattern:

```text
Collection
    ↓
Selected Object
    ↓
Relationships
    ↓
Children
    ↓
History
```

---

## Principle 3: Navigation Reflects Architecture

Navigation teaches the system.

A user should understand:

- Where am I?
- What exists?
- What can I do next?

without documentation.

The UI should expose architectural boundaries rather than hiding them.

---

## Principle 4: One Workspace, One Responsibility

A workspace should have one purpose.

Avoid combining unrelated concerns into a single page.

Avoid:

```text
Administration
Reports
Monitoring
Configuration
```

on the same page.

Prefer:

```text
One Navigation Node
One Responsibility
One Working Surface
```

---

## Principle 5: Progressive Detail

Information should move from broad to specific.

Preferred sequence:

```text
Collection
Selection
Details
Relationships
History
Diagnostics
```

Never reverse the flow.

---

## Principle 6: Provenance Is a First-Class Citizen

Administrative applications manage assets, configurations, workflows, executions, and operational state.

Users must understand:

- What happened?
- Why?
- When?
- Against which version?
- Under which configuration?

Version lineage, execution history, relationships, and dependencies should remain visible throughout the experience.

---

## Principle 7: Runtime Visibility Over Configuration

Most administrative systems over-emphasize configuration.

Administrative experiences should emphasize:

```text
Current State
Executions
Diagnostics
History
Relationships
```

before:

```text
Settings
Configuration
Forms
```

Configuration is important.

Visibility is more important.

---

## Principle 8: Context Before Action

Users should understand context before being asked to act.

Preferred sequence:

```text
View
Understand
Select
Modify
Execute
```

Never:

```text
Create
Configure
Hope
```

---

# Core Administrative Questions

Every administrative screen should answer one or more of the following:

- What exists?
- What is selected?
- What is related?
- What changed?
- What executed?
- What version executed?
- What failed?
- What should I do next?

If a screen cannot answer at least one of these questions, it likely should not exist.

---

# Navigation Philosophy

Navigation is not merely a routing mechanism.

Navigation is a teaching mechanism.

Users should understand the structure of the system by looking at navigation alone.

Navigation should:

- Reflect architectural boundaries
- Expose major capabilities
- Minimize hidden functionality
- Remain visible whenever practical

Users should rarely need documentation to discover functionality.

---

# Home Page Philosophy

The home page is:

- A dashboard

The home page is:

```text
A Navigation Center
A landing page
A marketing page
```

Its purpose is to answer:

- What capabilities exist?
- What is the recent transactions in each capability?
- Call to action to admin that capability?
- Call to action to execute that capability?

The home page provides a instant summary of what has happened and how to use that capability.

---

# Workspace Philosophy

A workspace is the primary administrative surface.

Each workspace should focus on a single responsibility.

A workspace should clearly expose:

```text
Existing Records
Selected Record
Relationships
History
Actions
```

Users should not be required to navigate across multiple pages to understand an object's context.

---

# Administrative Information Hierarchy

Information should appear in the following priority order:

```text
1. Existing Records

2. Selected Record

3. Relationships

4. Child Records

5. Runtime Information

6. History

7. Actions

8. Create/Edit Forms
```

This hierarchy should remain consistent across the application.

---

# Record-First Design Standard

Administrative screens should begin with visibility.

Preferred structure:

```text
Existing Records

Selected Record

Relationships

Child Records

Edit Actions

Create Actions
```

Avoid leading with:

```text
Create Forms
Setup Wizards
Configuration Panels
```

---

# Selection Standard

Selection should drive the workspace.

Whenever practical:

- Records are selectable
- The current selection is visually obvious
- The selected record drives downstream context

Preferred experience:

```text
Data Grid
    ↓
Selected Record
    ↓
Relationships
    ↓
History
```

---

# Dashboard Philosophy

Dashboards answer:

- What exists?
- What is active?
- What needs attention?
- What happened recently?

Dashboards should not exist primarily for:

- Creating records
- Editing records
- Configuration tasks

Dashboards should direct users toward workspaces where those activities occur.

---

# Dashboard Information Priority

Present information in the following order:

```text
Health

Attention Required

Recent Activity

Inventory

Details
```

Avoid prioritizing:

```text
Configuration
Create Forms
Edit Forms
```

on dashboards.

---

# Relationship Visibility Standard

Relationships are often more important than the record itself.

Users should be able to understand:

- Parent relationships
- Child relationships
- Dependencies
- Execution links
- Version lineage

without navigating through multiple pages.

---

# Version and Provenance Standard

Version visibility should be automatic.

Users should always be able to determine:

- Current version
- Previous versions
- Active version
- Executed version
- Historical lineage

Version information should never be hidden behind secondary navigation.

---

# History Standard

History should be discoverable from the current workspace.

Users should be able to answer:

- What happened?
- When did it happen?
- Who changed it?
- What did it affect?

without leaving the current context.

---

# Runtime Visibility Standard

Runtime information is a first-class administrative concern.

Users should be able to determine:

- What is running?
- What executed?
- What completed?
- What failed?
- What is waiting?

without switching between multiple workspaces.

---

# UX Success Criteria

An administrative screen is successful when a user can determine:

- What exists
- What is selected
- What is related
- What executed
- What changed
- What failed
- What can be done next

within a few seconds of arriving on the page.

Every design decision should support this goal.

---

# Guiding Principle

Administrative applications prioritize understanding over aesthetics.

The interface should reveal the system.

The interface should explain the system.

The interface should help users make informed decisions with confidence.

Clarity always wins.