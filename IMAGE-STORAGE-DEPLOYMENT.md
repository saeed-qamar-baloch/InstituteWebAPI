# Persistent image storage on Hetzner

The API now uses one configurable image root for uploads, public website images,
the institute logo, and private student/teacher photos.

In production, keep that root outside the directory replaced by each deployment.
The examples below assume the systemd service runs as `rozhn`; replace that user
and the service name with the values used on the server.

```bash
sudo install -d -o rozhn -g rozhn /var/lib/rozhn-api/images/{Website,Institute,Students,Teachers}
```

Set the path in the systemd service (or its environment file):

```ini
[Service]
Environment=Images__RootPath=/var/lib/rozhn-api/images
```

If the service has filesystem hardening, also allow writes to the directory:

```ini
ReadWritePaths=/var/lib/rozhn-api/images
```

Then reload and restart:

```bash
sudo systemctl daemon-reload
sudo systemctl restart YOUR_API_SERVICE
```

## One-time migration

Before switching the setting, copy (do not move) the current image folders into
the persistent root and preserve timestamps/permissions:

```bash
sudo rsync -a /CURRENT/API/PATH/Images/ /var/lib/rozhn-api/images/
sudo chown -R rozhn:rozhn /var/lib/rozhn-api/images
```

The database stores image URLs, not image bytes. Any URL whose corresponding
file is already missing must be recovered from a backup or uploaded again.
After recovery, verify one public and one authenticated private request:

```bash
curl -I https://api.rozhn.org/images/Website/FILE.jpg
curl -I -H "Authorization: Bearer TOKEN" \
  https://api.rozhn.org/api/secure-image/Students/FILE.jpg
```

Expected responses are `200`. A nonexistent public file now correctly returns
`404` rather than the misleading `401` produced by the global auth fallback.
