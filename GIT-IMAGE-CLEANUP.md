# Git Image Cleanup Instructions

504 student/teacher photos are currently tracked in this repository.
They must be removed from git tracking because they are PII (personally
identifiable photos of students and teachers).

⚠️  Read every step before running anything on the production repo.

---

## Step 1 — Untrack files in the current working tree (safe, reversible)

This removes the files from git's index but leaves them on disk.
Run this from the repo root:

```bash
git rm --cached "Images/Students/" -r
git rm --cached "Images/Teachers/" -r
git commit -m "chore: stop tracking student/teacher photos (PII)"
git push
```

After this commit, the files are gone from the latest HEAD but still
exist in every previous commit's history.  If the history is private
(internal self-hosted git), stop here — Step 1 is sufficient.

---

## Step 2 — Rewrite history to erase all traces (public or sensitive repos)

⚠️  History rewriting is destructive.  All collaborators must re-clone
after this.  Coordinate with your team before proceeding.

### Option A — git-filter-repo (recommended)

```bash
# Install
pip install git-filter-repo

# Run from repo root (creates a backup branch automatically)
git filter-repo --path "Images/Students/" --invert-paths
git filter-repo --path "Images/Teachers/" --invert-paths

# Force-push all branches and tags
git push --force --all
git push --force --tags
```

### Option B — BFG Repo Cleaner (faster on large repos)

```bash
# Download BFG jar from https://rtyley.github.io/bfg-repo-cleaner/
# Then run (from the parent directory of the repo):
java -jar bfg.jar --delete-folders Students --delete-folders Teachers --no-blob-protection rozhn-repo/
cd rozhn-repo
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force --all
```

---

## Step 3 — Re-add the images to the server (outside git)

The images are NOT deleted from your local disk by `git rm --cached`.
They stay in `Images/Students/` and `Images/Teachers/` and continue to
be served by the API at runtime.

For production:
- Copy images directly to the server's `Images/` folder via SFTP / RDP.
- Or store them in blob storage and update the image serving code.

---

## Step 4 — Tell all collaborators

```
⚠️  Git history has been rewritten.
Please delete your local clone and re-clone:
  git clone <repo-url>
Do NOT git pull — it will fail with non-fast-forward errors.
```

---

## Why the institute logo is kept

`Images/Institute/logo.jpg` and `Images/Institute/logo.png` are
non-sensitive branding assets.  They remain tracked in git.
