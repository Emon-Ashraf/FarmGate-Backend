# Image uploads

Verified scope: product creation/editing and farmer/customer avatar updates. Existing `ImageUrl` / `ProfileImageUrl` strings and DTOs remain compatible; no database migration is required. No cloud image provider or deployment volume was configured in this repository.

## Hosting configuration

The filesystem adapter is intended for a **single API process** with a durable volume. Configure these non-secret settings outside source control:

```text
Images__Root=/absolute/path/on/persistent-volume/farmgate-images
Images__PublicBaseUrl=https://your-api.example/api/images
```

Replace the example host with the actual externally reachable API origin, including any reverse-proxy path prefix. Production requires HTTPS. For local development only, the public base URL may use HTTP and must use a host reachable from the Android device/emulator. Set Android's ignored `local.properties` `FARMGATE_BASE_URL` to the same API origin (trailing slash), then rebuild. `10.0.2.2` is only for an emulator reaching its host.

The process needs read/write access to the image root. Keep it outside the application checkout and webroot; back it up together with the database and ownership sidecars. Do not mount it under a static-files server: JSON sidecars and partial files must not be publicly accessible. Configure proxy request limits to allow 5 MiB plus multipart overhead. A generated JPEG is publicly readable by opaque URL, consistent with existing public product/profile imagery. Uploading is authenticated and rate limited; anonymous uploads are prohibited.

If storage is unconfigured, upload returns 503 while existing APIs continue to work. There is no invented storage credential or cloud dependency. This adapter does **not** establish durability on ephemeral container disks, coordinate across multiple writers, or provide a quota. Before production, confirm the volume, backups, capacity limits, and ingress/rate controls. For a replicated/ephemeral deployment, implement an object-storage adapter and transactional ownership/retention metadata instead; do not deploy this filesystem adapter across multiple processes. The existing JWT and PostgreSQL configuration are still required.

## API and lifecycle

`POST /api/images/product` or `/api/images/avatar`, multipart field `file`, returns `{ "url": "https://…/api/images/<generated-id>.jpg" }`. Product uploads require an active farmer; avatars require an active farmer/customer. Request body <= 5 MiB plus envelope, file <= 5 MiB, decoded raster <= 24 megapixels. Accept only JPEG/PNG/WebP with matching detected content, decode completely, orient, fit within 1600/800 pixels without upscaling, and re-encode pixels as JPEG without original metadata. Decoding concurrency is bounded to two per process. The original filename is never used. Errors use ProblemDetails for upload validation/storage failures; existing domain save errors keep their established shape.

Saving a managed URL checks owner and purpose. Existing HTTP(S) external URLs still work; `content://` is rejected. Image selection/upload does not itself save a profile/product. A failed form save remains a failed save and can be retried with the uploaded URL.

Images become claimed before their URL is persisted, avoiding a cleanup-versus-save race. Unclaimed uploads expire after seven days via hourly cleanup. Claimed images (including replaced images and uploads claimed before a failed database write) are intentionally retained: neither clients nor the cleanup worker can delete an image that another record might still reference. A future retention job must check database references while writes are stopped or use transactional metadata. This conservative retention policy can consume disk; monitor capacity. Crashes can leave partial files that require operator cleanup after confirming no writer is using them.

## Dependencies and verification

SkiaSharp and SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.1 support net8.0, and the published NuGet metadata declares MIT. They replace ImageSharp, whose current packages explicitly require a valid license for Release builds. No ImageSharp dependency or licensing bypass remains. Retain SkiaSharp's bundled third-party notices when distributing native assets. Linux decoding is exercised by tests; verify native assets for a different deployment OS/architecture.

Run `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet build -c Release` against `FarmGate Backend Api.sln`. Tests cover decoded content/MIME, limits, filenames/traversal, ownership/purpose, legacy URLs, pending cleanup, real HTTP JWT authentication, and upload/read/profile-save persistence with an isolated in-memory test database. PostgreSQL deployment and actual device traffic require external configuration and are not proved by these tests. Existing AutoMapper 16.1.1 advisories are unrelated to this change and remain visible in NuGet auditing.
