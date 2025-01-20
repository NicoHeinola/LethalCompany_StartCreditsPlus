# Changelog

## [1.2.1] 2025-01-20

### Removed
- Testing logs that I forgot to remove from `1.2.0`.

### Changed
- `1.2` -> `1.2.0` in changelog.

## [1.2.0] 2025-01-20

### Added
- New terminal commands! (help, reload)

### Fixed
- If someone (somehow?) was able to join during day 0 when ship had already landed, it would still add credits if "Dynamic Start Credits" was enabled. This is now fixed.

## [1.1.1] 2025-01-15

### Changed
- Updated README.md.
- Renamed words: "unmodded" -> "vanilla".

## [1.1.0] 2025-01-15

### Added
- Setting to allow you to regain credits if you save and exit and then re-host the lobby on day 0. In vanilla Lethal Company, you lose credits and don't keep bought items on day 0, if you save and exit and then rejoin. This setting allows you to regain the lost credits.

### Optimized
- Minor optimizations related to loading save data. It is now done only once for this mod.

## [1.0.0] 2025-01-12
Initial release.

### Added
- Static start credit configuration.
- Random start credit configuration.
- Dynamic start credit configuration based on player amount.
