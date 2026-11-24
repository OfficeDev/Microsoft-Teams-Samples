# EJS in React sample build tools

The React Scripts 4 samples with an `@surma/rollup-plugin-off-main-thread` override
intentionally use EJS **3.1.10**, rather than the plugin's original EJS 2 range.
Use **npm 8.3 or later** so that npm applies these overrides. Keep the override
when regenerating their lockfiles; do not replace it with a direct application
dependency or a React Scripts major-version upgrade.

EJS versions below 3.1.7 are affected by GHSA-phwq-j96m-2c2q, and versions below
3.1.10 are affected by GHSA-ghr5-ch3p-vcr6. Updating only to 3.1.7 is insufficient.
The signing sample already accepts EJS 3, so its lockfile uses a compatible
3.1.10 update without an override.

The plugin renders its bundled AMD loader with `ejs.render(template, options)`.
Its templates use standard interpolation and conditionals, without removed
EJS 2 include syntax. EJS 2.7.4 and 3.1.10 produce identical loader output for
both `useEval` settings and default/custom AMD function names. The pinned
plugin/Rollup combinations (1.4.2/1.32.1 and 2.2.3/2.67.0) were also checked
with shared modules, workers, and dynamic imports.

Lockfiles retain their existing formats. Regeneration also reconciles existing
manifest/lock drift for the sidepanel HTTP proxy, audio-state/tag Teams SDKs,
and C# incoming-webhook React dependencies. The bulk-meetings manifest matches
its existing React Scripts 4 lockfile and webpack 4 build tools; migrating it
to React Scripts 5 is separate work. The sidepanel lockfile previously
contained duplicate JSON keys, which npm normalizes during regeneration.

Some legacy samples have pre-existing peer conflicts. When needed, regenerate
and install with `--legacy-peer-deps`; this does not change the EJS override.
Use `--ignore-scripts` for dependency-only validation.
