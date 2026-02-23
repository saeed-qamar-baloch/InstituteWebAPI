# Copilot Instructions

## General Guidelines
- Prioritize Razor Pages over Blazor or MVC when relevant, as this workspace targets .NET 8 and includes a Razor Pages project. 

## Requirements

### 1. Deduplication
- Check if an instruction with the **same semantic meaning** already exists.
- If found, **enhance** the existing instruction rather than adding a duplicate.
- If the new memory is more specific or comprehensive, replace the old one.
- If they complement each other, merge them into a single, cohesive instruction.

### 2. Structure
- If the file is empty, create a clean structure starting with a heading (e.g., "# Copilot Instructions").
- Organize related instructions under appropriate headings (##, ###).
- Use bullet points (-) for instruction lists.
- Maintain consistent indentation and spacing.

### 3. Placement
- Group semantically related instructions together.
- Place general instructions before specific ones.
- If multiple sections exist, add to the most relevant section.
- Create new sections only when the instruction doesn't fit existing categories.

### 4. Clarity
- Keep instructions concise and actionable.
- Use imperative mood ("Use X", not "You should use X").
- Avoid redundant phrases.
- Preserve existing formatting conventions (code blocks, emphasis, links).

### 5. Output Format
- Return ONLY the complete merged Markdown content.
- Do NOT wrap output in markdown code fences (```).
- Do NOT add meta-commentary like "Here is the result".
- Preserve all existing content unless merging/deduplicating.